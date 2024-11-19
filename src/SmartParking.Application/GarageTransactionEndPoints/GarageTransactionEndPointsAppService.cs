using Abp.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
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
using SmartParking.Localization;
using SmartParking.Packages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace SmartParking.GarageTransactionEndPoints;

//[Authorize]
[IgnoreAntiforgeryToken]
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
    private IDataFilter _dataFilter;
    private readonly ICurrentUser _currentUser; // Inject ICurrentUser
    private readonly IStringLocalizer<SmartParkingResource> _localizer;
    private readonly IRepository<UnLinkedTransaction> _unLinkedTransactionRepository;

    public GarageTransactionEndPointsAppService(IGarageTransactionsAppService garageTransactionsAppService,
        IRepository<Camera, int> cameraRepository, IRepository<Garage, int> garageRepository,
        ILogger<GarageTransactionEndPointsAppService> logger,
        IRepository<Customer, Guid> customerRepoistory,
        IRepository<Vehicle, int> vehicleRepository,
        ICustomerPackagesAppService customerPackagesAppService,
        IRepository<GarageTransaction, int> garageTransactionRepoistory,
        IRepository<Organization, int> organizationRepoistory,
        IInvoiceAppService invoiceAppService,
        IDataFilter dataFilter, ICurrentUser currentUser,
       IStringLocalizer<SmartParkingResource> localizer,
       IRepository<UnLinkedTransaction> unLinkedTransactionRepository)
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
        _dataFilter = dataFilter;
        _currentUser = currentUser;
        _localizer = localizer;
        _unLinkedTransactionRepository = unLinkedTransactionRepository;
    }
    public async Task<bool> CreateAsync(CreateUpdateGarageTransactionEndPointDto input)
    {

        _logger.LogError($"{DateTime.Now} ====>  {JsonSerializer.Serialize(new { input.PlateNumber, input.AccurateTime, input.DeviceID, input.VehiclePictureName })}");
        var camera = await GetCamera(input.DeviceID);

        if (camera == null)
        {
            await CreateUnLinkedTransaction(input);
            return await Task.FromResult(false);
        }

        // _logger.LogError($"{DateTime.Now} ====>  Camera ID is not found");


        var cameraId = camera.Id;
        var garageId = camera.GarageId;

        var organizationId = await GetOrganizationId(garageId);
        if (organizationId == -1)
        {

            await CreateUnLinkedTransaction(input);
            return await Task.FromResult(false);
        }
        var GarageTransaction = new CreateUpdateGarageTransactionDto
        {
            AccurateTime = input.AccurateTime,
            SnapTime = input.SnapTime,
            VehiclePicture = input.VehiclePicture,
            PlateNumber = input.PlateNumber,
            CameraId = cameraId,
            GarageId = garageId,
            OrganizationID = organizationId,
            TenantId = camera.TenantId,
            CameraType = camera.CameraType
        };
        //check if transaction is input
        if (GarageTransaction.CameraType == CameraType.Exit)
        {
            try
            {

                return await HandleCamersExit(garageId, organizationId, GarageTransaction);
            }
            catch (Exception ex)
            {
                await CreateUnLinkedTransaction(input);
                return await Task.FromResult(false);
            }
        }
        //Check Garage Capicity

        if (GarageTransaction.CameraType == CameraType.Entry)
        {
            Customer customer = null;
            var Id =await CheckCustomer(input.PlateNumber);
            if (Id != null)
            {
               customer = await GetCustomer(Id);
            }
            else
            {
                customer = null;
            }

            bool canEnter = await HandleCamersEntry(garageId , customer);
            if (canEnter)
            {
                try
                {
                    using (DataFilter.Disable<IMultiTenant>())
                    {
                        await _garageTransactionsAppService.CreateAsync(GarageTransaction);


                        return await Task.FromResult(true);
                    }
                }
                catch (Exception ex)
                {

                    await CreateUnLinkedTransaction(input);

                    return await Task.FromResult(false);

                }
            }



        }
        await CreateUnLinkedTransaction(input);
        return await Task.FromResult(false);

    }

    private async Task CreateUnLinkedTransaction(CreateUpdateGarageTransactionEndPointDto input)
    {
        var unLinkedTransaction = ObjectMapper.Map<CreateUpdateGarageTransactionEndPointDto, UnLinkedTransaction>(input);
        await _unLinkedTransactionRepository.InsertAsync(unLinkedTransaction, true);
    }

    private async Task<Garage> GetGarage(int id)
    {
        var garage = await _garageRepository.FirstOrDefaultAsync(x => x.Id == id && x.Status == Status.Active);
        if (garage == null)
        {
            throw new Exception("This garage is not active");
        }
        return garage;
    }
    

    private async Task<bool> HandleCamersEntry(int garageId , Customer customer)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            var garage = await GetGarage(garageId);
           var result= await CheckAvailableSlots(garage, customer);
           await CurrentUnitOfWork.SaveChangesAsync();

            return result;
        }
    }

    private async Task<bool> FreeSlots(Garage garage , Customer customer)
    {
        bool flag = false;
        if ((customer == null || customer.CustomerCategory == Enums.CustomerCategoryStatus.Customer))
        {
                garage.CurrnetCapacityForCustomers--;
            flag = true;

        }
        else if (customer.CustomerCategory == Enums.CustomerCategoryStatus.Employee) {

            
                garage.CurrnetCapacityEmployees--;
            flag = true;


        }
        else
        {
            
           garage.CurrnetCapacityForFree--;
           flag = true;

            

        }

          return await Task.FromResult(flag);
        
    }

    private async Task<bool> CheckAvailableSlots(Garage garage , Customer customer)
    {
        bool flag = false;
        if ((customer == null || customer.CustomerCategory == Enums.CustomerCategoryStatus.Customer))
        {
            if (garage.CurrnetCapacityForCustomers + 1 <= garage.MaxSlotsForCustomers)
            {
                garage.CurrnetCapacityForCustomers++;
                flag = true;
            }

        }
        else if (customer.CustomerCategory == Enums.CustomerCategoryStatus.Employee) {

            if (garage.CurrnetCapacityEmployees + 1 <= garage.MaxSlotsForEmployess)
            {
                garage.CurrnetCapacityEmployees++;
                flag = true;

            }

        }
        else
        {
            if (garage.CurrnetCapacityForFree + 1 <= garage.MaxSlotsForFree)
            {
                garage.CurrnetCapacityForFree++;
                flag = true;

            }

        }

            return await Task.FromResult(flag);
        
    }


    private async Task<bool> HandleCamersExit(int garageId, int organizationId, CreateUpdateGarageTransactionDto GarageTransaction)
    {
        bool flag = false;
        var EntranceTime = await FindEntranceTime(GarageTransaction.PlateNumber);
        if (EntranceTime is null)
        {
            _logger.LogError($"{DateTime.Now} ====>  Entrance Time is null ");

            return await Task.FromResult(false);

        }
        int totalHours = CalcTime(EntranceTime.Value, GarageTransaction.AccurateTime.Value);

        var customer = await CheckCustomer(GarageTransaction.PlateNumber);
        Customer exsitCustomer = null;
        //has plate number 
        if (customer != null)
        {
             exsitCustomer = await GetCustomer(customer);

            var packageDto = await CheckActivePackageForCustomer(Guid.Parse(customer), organizationId);


            if (packageDto == null)
            {
                //Customer witouth Active Package
                decimal price = await CalcInvoice(EntranceTime.Value, totalHours, GarageTransaction);
                var garageTransactionDto = await _garageTransactionsAppService.CreateAsync(GarageTransaction);
                string generatedInvoiceNumber = await _invoiceAppService.GenerateInvoiceNumberAsync(GarageTransaction.OrganizationID);
                var invoice =await CreateInvoice(garageId, generatedInvoiceNumber, price, organizationId, totalHours, GarageTransaction.PlateNumber, DateTime.Now, EntranceTime, exsitCustomer.ArabicName, exsitCustomer.IdentityUser.Name); ;
                await _invoiceAppService.CreateAsync(invoice);
                flag = false;
            }
            else
            {


                // customer with package
                var THours = CalculateTimeDependsOnPackage(EntranceTime, GarageTransaction.AccurateTime, packageDto) ;
                if (THours > 0)
                {

                   var organization =await GetOrganization(GarageTransaction.OrganizationID);
                   var price = THours * organization.AdditionalHourPrice;
                   
                   string generatedInvoiceNumber = await _invoiceAppService.GenerateInvoiceNumberAsync(GarageTransaction.OrganizationID);
                   var invoice = await CreateInvoice(garageId, generatedInvoiceNumber, price, organizationId, THours, GarageTransaction.PlateNumber, DateTime.Now, EntranceTime, exsitCustomer.ArabicName, exsitCustomer.IdentityUser.Name); ;
                   await _invoiceAppService.CreateAsync(invoice);
                    flag = false;
                }




                var garageTransactionDto = await _garageTransactionsAppService.CreateAsync(GarageTransaction);
                flag = true;
            }
        }
        else
        {
            if (GarageTransaction.PlateNumber == null)
            {
                var garageTransactionDto = await _garageTransactionsAppService.CreateAsync(GarageTransaction);
                flag = false;

            }
            else
            {
                string customerEnName = _localizer["NotCustomer"];
                string customerArName = _localizer["NotCustomer"];

                //Not Customer
                decimal price = await CalcInvoice(EntranceTime.Value, totalHours, GarageTransaction);
                var garageTransactionDto = await _garageTransactionsAppService.CreateAsync(GarageTransaction);
                string generatedInvoiceNumber = await _invoiceAppService.GenerateInvoiceNumberAsync(GarageTransaction.OrganizationID);
                //create invoice
                var invoice =await CreateInvoice(garageId, generatedInvoiceNumber, price, organizationId, totalHours, GarageTransaction.PlateNumber, DateTime.UtcNow, EntranceTime, customerEnName, customerArName); ;


                await _invoiceAppService.CreateAsync(invoice);
                flag = false;
            }

        }
        var garage =await GetGarage(garageId);
        var result =await FreeSlots(garage, exsitCustomer);
        return flag&&result;
    }

    private async Task<Customer> GetCustomer(string customerId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _customerRepoistory.GetQueryableAsync();
            query=query.Include(x=>x.IdentityUser);
            var customer =await query.FirstOrDefaultAsync(x=>x.Id==Guid.Parse(customerId));
            return customer;
        }

    }
    private async Task<CreateUpdateInvoiceDto> CreateInvoice(int GarageId,string GeneratedInvoiceNumber ,decimal Price ,int OrganizationID 
        ,int TotalHours ,string PlateNumber ,DateTime InvoiceTime , DateTime ?EntranceTime , string CustomerEnName , string CustomerArName)
    {
        var invoice = new CreateUpdateInvoiceDto
        {
            GarageId = GarageId,
            InvoiceNumber = GeneratedInvoiceNumber,
            InvoicePrice = Price,
            InvoiceType = InvoiceType.UnPaid,
            OrganizationId = OrganizationID,
            TotalHours = TotalHours,
            PlateNumber = PlateNumber,
            InvoiceTime = InvoiceTime,
            EntranceTime = EntranceTime,
            CustomerEnName = CustomerEnName,
            CustomerArName = CustomerArName,
        };

        return invoice;

    }

    private int CalculateTimeDependsOnPackage(DateTime ?EntranceTime , DateTime? ExitTime , CustomerPackageDto packageDto )
    {
        double totalHours = 0;
        DateTime excludeStartTime = EntranceTime.Value.Date.AddHours(packageDto.Package.PublishFrom.Hour).AddMinutes(packageDto.Package.PublishFrom.Minute);
        DateTime excludeEndTime = EntranceTime.Value.Date.AddHours(packageDto.Package.PublishTo.Hour).AddMinutes(packageDto.Package.PublishTo.Minute);
        //if entered before package starts
        totalHours += excludeStartTime.Hour-EntranceTime.Value.Hour > 0 ? excludeStartTime.Hour - EntranceTime.Value.Hour + (excludeStartTime.Minute - EntranceTime.Value.Minute>0?1:0) : 0;

        //enter after package start
        if (totalHours == 0)
        {

            if (EntranceTime > excludeEndTime) {
            
                //if entered within package time 
                //and leave after package ends
               totalHours += ExitTime.Value.Hour - EntranceTime.Value.Hour > 0 ? ExitTime.Value.Hour - EntranceTime.Value.Hour  + (ExitTime.Value.Minute - EntranceTime.Value.Minute > 0 ? 1 : 0) : 0;
            
            }
            else
            {
                if(ExitTime.Value.Hour> excludeEndTime.Hour)
                {

                    totalHours += (ExitTime.Value.Hour - excludeEndTime.Hour) > 0 ? (ExitTime.Value.Hour - excludeEndTime.Hour) : 0;
                    //ceiling minutes to hours
                    totalHours += ExitTime.Value.Minute - excludeEndTime.Minute > 0 ? 1 : 0;

                }
            }
        }
        else
        {
            if (ExitTime > excludeEndTime)
            {
                totalHours += ExitTime.Value.Hour - excludeEndTime.Hour > 0 ? ExitTime.Value.Hour - excludeEndTime.Hour + (ExitTime.Value.Minute - excludeEndTime.Minute > 0 ? 1 : 0) : 0;
            }

        }


        var diffBetweenDates= ExitTime.Value.Subtract(EntranceTime.Value);
        var tDays=diffBetweenDates.Days;
        var totalHoursOutOfPackageInStaying=24-diffBetweenDates.Hours;
        totalHours += tDays * totalHoursOutOfPackageInStaying;

        return (int)Math.Ceiling(totalHours);
    }

    private async Task<Camera> GetCamera(string CameraCode)
    {

        using (_dataFilter.Disable<IMultiTenant>())
        {
            var camera = await _cameraRepository.FirstOrDefaultAsync(x => x.DeviceId == CameraCode);

            return camera;
        }

    }
    private async Task<Organization> GetOrganization(int OrganizationID)
    {

        using (DataFilter.Disable<IMultiTenant>())
        {
           var organization = await _organizationRepoistory.FindAsync(OrganizationID);
            return organization;
        }

    }
    private async Task<int> GetOrganizationId(int GarageId)
    {
        Garage garage = new Garage();
        using (DataFilter.Disable<IMultiTenant>())
        {
            garage = await _garageRepository.FirstOrDefaultAsync(x => x.Id == GarageId);

        }
        if (garage is not null)
        {
            return garage.OrganizationId;

        }
        else
            return -1;
    }
    private async Task<string> CheckCustomer(string? plateNumber)
    {
        var Customer = await _vehicleRepository.FirstOrDefaultAsync(x => x.PlateNumber == null || x.PlateNumber == plateNumber);

        if (Customer == null)
        {
            _logger.LogError($"{DateTime.Now} ====>  CustomerID ");

            return null;
        }
        var CustomerId = Customer.CustomerId;
        return CustomerId.ToString();
    }
    private async Task<CustomerPackageDto> CheckActivePackageForCustomer(Guid customerId, int organizationId)
    {
        var customerPackage = await _customerPackagesAppService.GetOneActiveCustomerPackageByOrgIdAndCustomerId(organizationId, customerId);
        if (customerPackage == null)
        {
            _logger.LogError($"{DateTime.Now} ====>  customerPackage not found");
            return null;
            //return customerId.ToString();
        }
        return customerPackage;
    }
    private async Task<DateTime?> FindEntranceTime(string? plateNumber)
    {
        // Get the list of transactions for the provided plate number
        using (DataFilter.Disable<IMultiTenant>())
        {

            var query = await _garageTransactionRepoistory.GetQueryableAsync();


        var latestTransaction = query
                .Where(gt => (gt.PlateNumber == null || gt.PlateNumber == plateNumber)&&gt.CameraType==CameraType.Entry)
                .OrderByDescending(gt => gt.AccurateTime) // or AccurateTime
                .FirstOrDefault();

        return latestTransaction?.AccurateTime;
        }
    }
    private int CalcTime(DateTime startTime, DateTime endTime)
    {


        // Calculate the time difference
        TimeSpan timeDifference = endTime - startTime;

        // Calculate the hours difference and round up

        int hoursDiff = (int)Math.Ceiling(timeDifference.TotalHours);

        return hoursDiff;
    }
    private async Task<Decimal> CalcInvoice(DateTime entrenceTime, int totalHours, CreateUpdateGarageTransactionDto garageTransaction)
    {
        var organization = await GetOrganization(garageTransaction.OrganizationID);       
        //4 Cases
        if (organization?.IsNumberOfHoursExceeded == false && organization.IsClose == false)
        {
            //case 1
            decimal price = 0;
            price += (totalHours / 24) * organization.FullDayPrice;
            totalHours -= (totalHours / 24) * 24;
            price += calcPriceHours(organization, totalHours);
            return price;

        }
        else if (organization?.IsNumberOfHoursExceeded == true && organization.IsClose == false)
        {
            //case 2
            decimal price = 0;
            price += (totalHours / 24) * organization.FullDayPrice;
            totalHours -= (totalHours / 24) * 24;
            if (totalHours > organization.NumberOfHours)
            {
                price += organization.FullDayPrice;
                totalHours = 0;
            }
            price += calcPriceHours(organization, totalHours);
            return price;
        }
        else if (organization?.IsNumberOfHoursExceeded == false && organization.IsClose == true)
        {
            (double specialHours, double normalHours) = CalculateHours(garageTransaction, entrenceTime, organization);
            var closingHour = Math.Ceiling((organization.ClosingHoursEnd - organization.ClosingHoursStart).TotalHours);
            double price = 0;
            double totalDays = Math.Ceiling(specialHours / closingHour);
            price += totalDays * (double)organization.FullDayPrice;
            price += ((int)(normalHours / 24)) * (double)organization.FullDayPrice;
            normalHours -= ((int)(normalHours / 24)) * 24;
            price += (double)calcPriceHours(organization, (int)normalHours);
            return (decimal)price;
        }
        else if (organization?.IsNumberOfHoursExceeded == true && organization.IsClose == true)
        {
            (double specialHours, double normalHours) = CalculateHours(garageTransaction, entrenceTime, organization);
            var closingHour = Math.Ceiling((organization.ClosingHoursEnd - organization.ClosingHoursStart).TotalHours);
            double price = 0;
            double totalDays = Math.Ceiling(specialHours / closingHour);
            price += totalDays * (double)organization.FullDayPrice;
            price += ((int)(normalHours / 24)) * (double)organization.FullDayPrice;
            normalHours -= ((int)(normalHours / 24)) * 24;
            if (normalHours > organization.NumberOfHours)
            {
                price += (double)organization.FullDayPrice;
                normalHours = 0;
            }
            price += (double)calcPriceHours(organization, (int)normalHours);
            return (decimal)price;
        }
        return 0.0m;
    }
    private decimal calcPriceHours(Organization organization, int hours)
    {
        decimal price = 0;
        hours--;
        price += hours>0? organization.FirstHourPrice:0 ;
        hours--;
        price += hours>0?organization.SecondHourPrice:0;
        hours--;
        price += hours > 0 ? organization.ThirdHourPrice : 0;
        price += (hours * organization.AdditionalHourPrice);
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


