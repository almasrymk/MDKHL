using Abp.Extensions;
using Abp.Json;
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
using Volo.Abp;
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
            CameraType = camera.CameraType,
            IsIn=camera.CameraType==CameraType.Exit?false:true,
            

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
        using (DataFilter.Disable<IMultiTenant>())
        {
            var garage = await _garageRepository.FirstOrDefaultAsync(x => x.Id == id && x.Status == Status.Active);
            if (garage == null)
            {
                throw new Exception("This garage is not active");
            }
            return garage;
        }
       
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
        var organization = await GetOrganization(GarageTransaction.OrganizationID);

        int totalHours = CalcTime(EntranceTime.Value, GarageTransaction.AccurateTime.Value,organization.FreeMinutes);
        if (totalHours == 0)
        {
            var garageTransactionDto = await _garageTransactionsAppService.CreateAsync(GarageTransaction);
            return await Task.FromResult(true);
        }

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
                var invoice = await CreateInvoice(garageId, generatedInvoiceNumber, price, organizationId, totalHours, GarageTransaction.PlateNumber, DateTime.UtcNow, EntranceTime, exsitCustomer.IdentityUser.Name, exsitCustomer.ArabicName);

                await _invoiceAppService.CreateAsync(invoice);
                flag = false;
            }
            else
            {


                // customer with package
                var THours = CalculateTimeDependsOnPackage(EntranceTime, GarageTransaction.AccurateTime, packageDto) ;
                if (THours > 0)
                {

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
        return flag;
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
        ,int TotalHours ,string PlateNumber ,DateTime InvoiceTime , DateTime ?EntranceTime , string CustomerEnName , string CustomerArName,Guid? Id =null)
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
            CustomerId = Id
        };

        return invoice;

    }

    public int CalculateTimeDependsOnPackage(DateTime ?entranceTime , DateTime? exitTime , CustomerPackageDto packageDto )
    {
        if (exitTime < entranceTime)
        {
            throw new BusinessException("Leaving date can't be befor entrance date") ;
        }

        DateTime packageStart = entranceTime.Value.Date.AddHours(packageDto.Package.PublishFrom.Hour).AddMinutes(packageDto.Package.PublishFrom.Minute);
        DateTime packageEnd = entranceTime.Value.Date.AddHours(packageDto.Package.PublishTo.Hour).AddMinutes(packageDto.Package.PublishTo.Minute); ;

        double extraHours = 0;

        DateTime currentDay = entranceTime.Value;
        DateTime endDay = exitTime.Value;

        while (currentDay <= endDay)
        {
            DateTime dayPackageStart = currentDay.Date+packageStart.TimeOfDay;
            DateTime dayPackageEnd = currentDay.Date + packageEnd.TimeOfDay;

            if (packageEnd < packageStart)
            {
                dayPackageEnd = dayPackageEnd.AddDays(1);
            }

            DateTime effectiveStart = Max(currentDay, dayPackageStart);
            DateTime effectiveEnd = Min(endDay, dayPackageEnd);

           

            if (currentDay < dayPackageStart)
            {
                var smallestDay = (Min(endDay, dayPackageStart) - currentDay);
                extraHours += smallestDay.Hours + (smallestDay.Minutes>0?1:0);
                currentDay = dayPackageStart;
                
            }

            if (endDay > dayPackageEnd && endDay.Date==currentDay.Date)
            {
                extraHours += (endDay - Max(currentDay, dayPackageEnd)).TotalHours;
            }

            if(endDay.Date > currentDay.Date)
            {
                extraHours += 24 - Math.Abs(dayPackageStart.Hour - dayPackageEnd.Hour) + (Math.Abs(dayPackageStart.Minute - dayPackageEnd.Minute) > 0 ? 1 : 0);
            }
            currentDay = currentDay.AddDays(1);
            if (currentDay.Date == exitTime.Value.Date)
            {
                if (exitTime.Value.Hour < packageStart.Hour)
                {
                    extraHours -= packageStart.Hour - exitTime.Value.Hour;
                }
            }
        }

        // Return result
        return (int) Math.Ceiling( extraHours);
    }

    private static DateTime Max(DateTime a, DateTime b) => a > b ? a : b;

    private static DateTime Min(DateTime a, DateTime b) => a < b ? a : b;
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
        var Customer = await _vehicleRepository.FirstOrDefaultAsync(x => x.ArPlateNumber == plateNumber || x.EnPlateNumber == plateNumber);

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
                .Where(gt => (gt.PlateNumber == null || gt.PlateNumber == plateNumber)&&gt.CameraType==CameraType.Entry && gt.IsIn==true)
                .OrderByDescending(gt => gt.AccurateTime) // or AccurateTime
                .FirstOrDefault();

        return latestTransaction?.AccurateTime;
        }
    }
    private int CalcTime(DateTime startTime, DateTime endTime, int FreeMinutes)
    {


        // Calculate the time difference
        TimeSpan timeDifference = endTime - startTime;

        

        if ((int)timeDifference.TotalHours == 0&& timeDifference.Minutes<=FreeMinutes)
        {
            return 0;
            
        }

        int hoursDiff = (int)Math.Ceiling(timeDifference.TotalHours);

         
        hoursDiff += (timeDifference.Minutes > 0 ? 1 : 0);


        return hoursDiff;
    }
    private async Task<Decimal> CalcInvoice(DateTime entrenceTime, int totalHours, CreateUpdateGarageTransactionDto garageTransaction)
    {
        var organization = await GetOrganization(garageTransaction.OrganizationID);
       
        //4 Cases
        return (organization.IsNumberOfHoursExceeded, organization.IsClose) switch
        {
            (false, false) => calcPriceHours(organization, totalHours),
            (true,false)=> CalculatePriceForExceededHours(totalHours, organization),
            (false,true)=> CalculatePriceForOrganizationIsClosed(totalHours, organization,garageTransaction,entrenceTime),
            (true,true)=> CalculatePriceForOrganizationIsClosedAndExceededHours(totalHours,organization,garageTransaction,entrenceTime), 
        };
    }
    private decimal CalculatePriceForExceededHours(int totalHours,Organization organization)
   {
        decimal price = 0;
        if (totalHours >= organization.NumberOfHours)
        {
            int fullDaysCustomerHasStayed = 0;
            if (totalHours >= 24)
            {
                fullDaysCustomerHasStayed = totalHours / 24;
                totalHours %= 24;
            }
            else
            {
                fullDaysCustomerHasStayed = 1;
                totalHours = 0;
            }
            price += (fullDaysCustomerHasStayed * organization.FullDayPrice);
        }
        price += calcPriceHours(organization, totalHours);
        return price;

   } 
    private decimal CalculatePriceForOrganizationIsClosedAndExceededHours(int totalHours,Organization organization,CreateUpdateGarageTransactionDto garageTransaction , DateTime entrenceTime)
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
    private decimal CalculatePriceForOrganizationIsClosed(int totalHours,Organization organization , CreateUpdateGarageTransactionDto garageTransaction,DateTime entrenceTime)
    {

        //special hours -> hours where organization is close
        //normal hours -> hours where organization is open 
        (double specialHours, double normalHours) = CalculateHours(garageTransaction, entrenceTime, organization);
        var closingHour = Math.Ceiling((organization.ClosingHoursEnd - organization.ClosingHoursStart).TotalHours);
        double price = 0;
        double totalDays = Math.Ceiling(specialHours / closingHour);
        // calculates full days price
        price += totalDays * (double)organization.FullDayPrice;
        //calculates hours
        price += (double)calcPriceHours(organization, (int)normalHours);
        return (decimal)price;
    }
    private decimal calcPriceHours(Organization organization, int hours)
    {
        decimal price = 0;
        price += hours>0? organization.FirstHourPrice:0 ;
        hours--;
        price += hours > 0 ? organization.SecondHourPrice : 0;
        hours--;
        price += hours > 0 ? organization.ThirdHourPrice : 0;
        hours--;
        price += hours > 0 ? (hours * organization.AdditionalHourPrice):0;
        return price;
    }
    private static (double, double) CalculateHours(CreateUpdateGarageTransactionDto garageTransaction, DateTime entrenceTime, Organization organization)
    {
        TimeSpan totalDuration = garageTransaction.AccurateTime.Value - entrenceTime;

        double totalHours = Math.Ceiling(totalDuration.TotalHours);
       

        double specialHours = 0;

        // Loop through each day from entry date to exit date
        DateTime currentDate = entrenceTime.Date;
        while (currentDate <= garageTransaction.AccurateTime.Value.Date)
        {
        DateTime specialStartTime = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, organization.ClosingHoursStart.Hour, organization.ClosingHoursStart.Minute, 0);
        DateTime specialEndTime = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, organization.ClosingHoursEnd.Hour, organization.ClosingHoursEnd.Minute, 0);

            // Handle overnight case where special hours span two dates
          if (specialEndTime.Hour < specialStartTime.Hour)
          {
                specialEndTime = specialEndTime.AddDays(1);

                // Calculate hours before midnight
             if (entrenceTime < specialEndTime && garageTransaction.AccurateTime >= specialStartTime)
             {
                DateTime effectiveStart = entrenceTime < specialStartTime ? specialStartTime : entrenceTime;
                DateTime effectiveEnd = garageTransaction.AccurateTime.Value < specialEndTime ? garageTransaction.AccurateTime.Value : specialEndTime;

                TimeSpan specialDurationBeforeMidnight = effectiveEnd - effectiveStart;
                if (specialDurationBeforeMidnight.TotalHours > 0)
                {
                    specialHours += Math.Ceiling(specialDurationBeforeMidnight.TotalHours); // Round up
                }
             }

                   // Calculate hours after midnight
              if (garageTransaction.AccurateTime > specialEndTime && garageTransaction.AccurateTime >= specialEndTime)
              {
                  DateTime effectiveStart = specialEndTime;
                  DateTime effectiveEnd = garageTransaction.AccurateTime.Value < specialEndTime ? garageTransaction.AccurateTime.Value : specialEndTime;

                  TimeSpan specialDurationAfterMidnight = effectiveEnd - effectiveStart;
                  if (specialDurationAfterMidnight.TotalHours > 0)
                  {
                      specialHours += Math.Ceiling(specialDurationAfterMidnight.TotalHours); // Round up
                  }
              }
            
          }
            else
            {
                // Normal case where special hours are within the same day
                if (entrenceTime < specialEndTime && garageTransaction.AccurateTime > specialStartTime)
                {
                    DateTime effectiveStart = entrenceTime < specialStartTime ? specialStartTime : entrenceTime;
                    DateTime effectiveEnd = garageTransaction.AccurateTime.Value < specialEndTime ? garageTransaction.AccurateTime.Value : specialEndTime;

                    TimeSpan specialDuration = effectiveEnd - effectiveStart;
                    if (specialDuration.TotalHours > 0)
                    {
                        specialHours += Math.Ceiling(specialDuration.TotalHours); // Round up
                    }
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


