using Castle.Core.Resource;
using Microsoft.Extensions.Logging;
using SmartParking.CustomerPackages;
using SmartParking.Entities.Cameras;
using SmartParking.Entities.Customers;
using SmartParking.Entities.GarageTransactions;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Vehicles;
using SmartParking.Enums;
using SmartParking.GarageTransactions;
using SmartParking.Invoices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.GarageTransactionEndPoints;

public class GarageTransactionEndPointsAppService : SmartParkingAppService, IGarageTransactionEndPointsAppService
{
    private readonly IGarageTransactionsAppService _garageTransactionsAppService;
    private readonly IRepository<Camera, int> _cameraRepository;
    private readonly IRepository<Garage, int> _garageRepository;
    private readonly ILogger<GarageTransactionEndPointsAppService> _logger;
    private readonly IRepository<Customer, Guid> _customerRepoistory;
    private readonly IRepository<Vehicle, int> _vehicleRepository;
    private readonly ICustomerPackagesAppService _customerPackagesAppService;
	private readonly IRepository<GarageTransaction, int> _garageTransactionRepoistory;
    private readonly IRepository<Organization, int> _organizationRepoistory;
	private readonly IInvoiceAppService _invoiceAppService;

	public GarageTransactionEndPointsAppService(IGarageTransactionsAppService garageTransactionsAppService, 
        IRepository<Camera, int> cameraRepository, IRepository<Garage, int> garageRepository,
        ILogger<GarageTransactionEndPointsAppService> logger,
        IRepository<Customer,Guid>customerRepoistory,
        IRepository<Vehicle,int> vehicleRepository,
        ICustomerPackagesAppService customerPackagesAppService,
        IRepository<GarageTransaction,int>garageTransactionRepoistory,
        IRepository<Organization,int> organizationRepoistory,
        IInvoiceAppService invoiceAppService)
    {
        _garageTransactionsAppService = garageTransactionsAppService;
        _cameraRepository = cameraRepository;
        _garageRepository = garageRepository;
        _logger = logger;
        _customerRepoistory = customerRepoistory;
        _vehicleRepository = vehicleRepository;
        _customerPackagesAppService = customerPackagesAppService;
			_garageTransactionRepoistory = garageTransactionRepoistory;
        _organizationRepoistory = organizationRepoistory;
			_invoiceAppService = invoiceAppService;
	}
    public async Task<bool> CreateAsync(CreateUpdateGarageTransactionEndPointDto input)
    {
       
        _logger.LogError($"{DateTime.Now} ====>  {JsonSerializer.Serialize( new {input.PlateNumber, input.AccurateTime, input.DeviceID, input.VehiclePictureName})}");
        var camera= GetCamera(input.DeviceID);
        //input.PlateNumber ??= "123456";//input.PlateNnumber;

        _logger.LogError($"{DateTime.Now} ====>  Camera ID {camera.Id}");
        if (camera == null)
            return await Task.FromResult(false);
        var cameraId = camera.Id;
        var garageId =camera.GarageId;
        var organizationId = GetOrganizationId(garageId);
        var GarageTransaction = new CreateUpdateGarageTransactionDto
        {
            AccurateTime=input.AccurateTime,
            SnapTime=input.SnapTime,
            VehiclePicture=input.VehiclePicture,
            PlateNumber = input.PlateNumber,
            CameraId=cameraId,
            GarageId=garageId,
            OrganizationID=organizationId,
            TenantId = camera.TenantId,
            CameraType=camera.CameraType
            
           // Type =camera.Type
        };
        //check if transaction is input
        if(GarageTransaction.CameraType==CameraType.Exit)
        {
            var EntranceTime = await FindEntranceTime(GarageTransaction.PlateNumber);
            if(EntranceTime is null)
            {
					_logger.LogError($"{DateTime.Now} ====>  Entrance Time is null ");

					return await Task.FromResult(false);

				}
				int totalHours = CalcTime(EntranceTime.Value, GarageTransaction.AccurateTime.Value);

            var customer = await CheckCustomer(GarageTransaction.PlateNumber);
            if(customer != null)
            {
                int succes = await CheckActivePackageForCustomer(Guid.Parse(customer), organizationId);
                if(succes==-1)
                {
                    //Customer witouth Active Package

                }
            }
            else
            {
                //Not Customer
                decimal price =await CalcInvoice(EntranceTime.Value, totalHours, GarageTransaction);
					var garageTransactionDto = await _garageTransactionsAppService.CreateAsync(GarageTransaction);
                var invoice = new CreateUpdateInvoiceDto
                {
                    GarageId = garageId,
                    InvoiceNumber = "Not Customer",
                    InvoicePrice = price,
                    InvoiceType = InvoiceType.UnPaid,
                    OrganizationId = GarageTransaction.OrganizationID,
                    TotalHours = totalHours,
                    PlateNumber=GarageTransaction.PlateNumber

                };
                await _invoiceAppService.CreateAsync(invoice);
					return await Task.FromResult(false);

				}
				//int succes= await CheckCustomerPackage(GarageTransaction.OrganizationID,GarageTransaction.PlateNumber);
			

        }
			//Check Garage Capicity

			if (GarageTransaction.CameraType == CameraType.Entry)
			{
				var garage = await _garageRepository.FirstOrDefaultAsync(x => x.Id == garageId);
				if (garage.CurrnetCapicity == garage.MaxCapacity)
					return await Task.FromResult(false);
			}
			try
        {
            var  garageTransactionDto= await _garageTransactionsAppService.CreateAsync(GarageTransaction);
            return await Task.FromResult(true);

        }catch (Exception ex)
        {
				return await Task.FromResult(false);

			}
     

			//_logger.LogError($"{DateTime.Now} ====> transaction created {garageTransactionDto.Id}");

           
        }
        private async Task<Camera> GetCamera(string CameraCode)
        {
            var camera =await _cameraRepository.FirstOrDefaultAsync(x => x.Code.Equals(CameraCode));
           
            return camera;
        }
        private int GetOrganizationId(int GarageId)
        {
            return  _garageRepository.FirstAsync(x=>x.Id== GarageId).Result.OrganizationId;
        }

    private async Task<string> CheckCustomer(string plateNumber)
    {
        var Customer = await _vehicleRepository.FirstOrDefaultAsync(x => x.PlateNumber == plateNumber);

        if (Customer == null)
        {
            _logger.LogError($"{DateTime.Now} ====>  CustomerID ");

            return null;
        }
        var CustomerId = Customer.CustomerId;
        return CustomerId.ToString();
    }
    private async Task<int> CheckActivePackageForCustomer(Guid customerId, int organizationId)
    {
			var customerPackage =await _customerPackagesAppService.GetOneActiveCustomerPackageByOrgIdAndCustomerId(organizationId, customerId);
			if (customerPackage == null)
			{
				_logger.LogError($"{DateTime.Now} ====>  customerPackage not found");
            return -1;
				//return customerId.ToString();
			}
			return customerPackage.Id;
		}
	private async Task<DateTime?> FindEntranceTime(string plateNumber)
    {
			// Get the list of transactions for the provided plate number
			//var latestTransactions = await _garageTransactionRepoistory.GetListAsync();
        var query = await _garageTransactionRepoistory.GetQueryableAsync();
        var latestTransaction = query.Where(gt => gt.PlateNumber == plateNumber)
				.OrderByDescending(gt => gt.SnapTime) // or AccurateTime
				.FirstOrDefault();
			// Fetch the latest transaction based on SnapTime or AccurateTime
			
        return latestTransaction?.AccurateTime;
    }
    private int CalcTime(DateTime startTime,DateTime endTime)
	{
			

			// Calculate the time difference
			TimeSpan timeDifference = endTime - startTime;

			// Calculate the hours difference and round up

			int hoursDiff = (int)Math.Ceiling(timeDifference.TotalHours);

			return hoursDiff;
	}
    private async Task<Decimal> CalcInvoice(DateTime entrenceTime,int totalHours, CreateUpdateGarageTransactionDto garageTransaction)
    {
        var organization=await  _organizationRepoistory.FindAsync(garageTransaction.OrganizationID);
        //4 Cases
        if(organization?.IsNumberOfHoursExceeded==false&&organization.IsClose==false)
        {
            //case 1
            decimal price = 0;
            price += (totalHours / 24) * organization.FullDayPrice;
            totalHours -= (totalHours / 24) * 24;
            price += calcPriceHours(organization, totalHours);
            return price;

        }
        else if(organization?.IsNumberOfHoursExceeded==true &&organization.IsClose==false)
        {
                //case 2
                decimal price = 0;
                price += (totalHours / 24) * organization.FullDayPrice;
                totalHours -= (totalHours / 24) * 24;
                if(totalHours>organization.NumberOfHours)
                {
                    price += organization.FullDayPrice;
                    totalHours = 0;
                }
				price += calcPriceHours(organization, totalHours);
				return price;
	    }
        else if(organization?.IsNumberOfHoursExceeded == false && organization.IsClose == true)
        {
                (double specialHours, double normalHours) = CalculateHours(garageTransaction, entrenceTime,organization);
                var closingHour = Math.Ceiling((organization.ClosingHoursEnd - organization.ClosingHoursStart).TotalHours);
                double price = 0;
                double totalDays=Math.Ceiling (specialHours/closingHour);
                price+= totalDays*(double)organization.FullDayPrice;
                price += ((int)(normalHours / 24)) *(double) organization.FullDayPrice;
                normalHours -= ((int)(normalHours / 24)) * 24;
                price +=(double) calcPriceHours(organization,(int) normalHours);
                return (decimal) price;
        }
        else if( organization?.IsNumberOfHoursExceeded == true &&organization.IsClose == true)
        {
                (double specialHours, double normalHours) = CalculateHours(garageTransaction, entrenceTime, organization);
                var closingHour = Math.Ceiling((organization.ClosingHoursEnd - organization.ClosingHoursStart).TotalHours);
                double price = 0;
                double totalDays = Math.Ceiling(specialHours / closingHour);
                price += totalDays * (double)organization.FullDayPrice;
                price += ((int)(normalHours / 24)) * (double)organization.FullDayPrice;
                normalHours -= ((int)(normalHours / 24)) * 24;
                if(normalHours>organization.NumberOfHours)
                {
                    price+= (double)organization.FullDayPrice;
                    normalHours = 0;
                }
                price += (double)calcPriceHours(organization, (int)normalHours);
                return (decimal)price;
        }
            return 0.0m;
    }
    private decimal calcPriceHours(Organization organization,int hours)
    {
            decimal price = 0;
            if(hours==1)
            {
                price += organization.FirstHourPrice; 
            }
            else if(hours==2)
            {
                price += organization.FirstHourPrice;
                price += organization.SecondHourPrice;
            }
            else if(hours==3)
            {
                price += organization.FirstHourPrice;
                price += organization.SecondHourPrice;
                price += organization.ThirdHourPrice;
            }
            else if(hours>3)
            {
				price += organization.FirstHourPrice;
				price += organization.SecondHourPrice;
				price += organization.ThirdHourPrice;
                hours -= 3;
                price += hours * organization.AdditionalHourPrice;
            }
            return price;
    }

    public static (double, double) CalculateHours(CreateUpdateGarageTransactionDto garageTransaction, DateTime entrenceTime, Organization organization)
    {
            // Calculate total time in the garage
            TimeSpan totalDuration = garageTransaction.AccurateTime.Value - entrenceTime;
            double totalHours = Math.Ceiling(totalDuration.TotalHours); // Round up total hours

            // Calculate special hours
            double specialHours = 0;

            // Loop through each day from entry date to exit date
            DateTime currentDate = entrenceTime.Date;

            while (currentDate <= garageTransaction.AccurateTime.Value.Date)
            {
                // Use the organization's closing hours instead of hardcoded values
                DateTime specialStartTime = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, organization.ClosingHoursStart.Hour, organization.ClosingHoursStart.Minute, 0);
                DateTime specialEndTime = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, organization.ClosingHoursEnd.Hour, organization.ClosingHoursEnd.Minute, 0);

                // Check if the car is in the garage during special hours
                if (entrenceTime < specialEndTime && garageTransaction.AccurateTime > specialStartTime)
                {
                    DateTime effectiveStart = entrenceTime < specialStartTime ? specialStartTime : entrenceTime;
                    DateTime effectiveEnd = garageTransaction.AccurateTime.Value > specialEndTime ? specialEndTime : garageTransaction.AccurateTime.Value;

                    TimeSpan specialDuration = effectiveEnd - effectiveStart;
                    if (specialDuration.TotalHours > 0)
                    {
                        specialHours += Math.Ceiling(specialDuration.TotalHours); // Round up
                    }
                }

                // Move to the next day
                currentDate = currentDate.AddDays(1);
            }

            // Normal hours are total hours minus special hours
            double normalHours = totalHours - specialHours;

            return (specialHours, normalHours);
    }


}


