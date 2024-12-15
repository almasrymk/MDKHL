using Abp.Domain.Entities;
using JetBrains.Annotations;
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
using SmartParking.Payments;
using SmartParking.Results;
using SmartParking.Vehicles;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
    private readonly IPaymentAppService _paymentAppService;
    private readonly IRepository<GarageTransaction, int> _garageTransactionRepoistory;
    private readonly IRepository<Organization, int> _organizationRepoistory;
    private readonly IInvoiceAppService _invoiceAppService;
    private IDataFilter _dataFilter;
    private readonly ICurrentUser _currentUser; // Inject ICurrentUser
    private readonly IStringLocalizer<SmartParkingResource> _localizer;
    private readonly IRepository<UnLinkedTransaction> _unLinkedTransactionRepository;
    string customerEnName = null;
    string customerArName =null;


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
       IRepository<UnLinkedTransaction> unLinkedTransactionRepository, IPaymentAppService paymentAppService)
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
        _paymentAppService = paymentAppService;
        customerEnName = _localizer["NotCustomer"];
        customerArName = _localizer["NotCustomer"];
    }
    public async Task<bool> CreateAsync(CreateUpdateGarageTransactionEndPointDto input)
    {

        _logger.LogError($"{DateTime.Now} ====>  {JsonSerializer.Serialize(new { input.PlateNumber, input.AccurateTime, input.DeviceID, input.VehiclePictureName })}");
        var camera = await GetCamera(input.DeviceID);

        if (camera == null)
        {
            await CreateUnLinkedTransaction(input);
             _logger.LogError($"{DateTime.Now} ====>  Camera ID is not found , Saved in Unlinked Transaction");
             _logger.LogError($"{DateTime.Now} ====> Get Response false");

            return await Task.FromResult(false);
        }

        var cameraId = camera.Id;
        var garageId = camera.GarageId;

        var organizationId = await GetOrganizationId(garageId);
        if (organizationId == -1)
        {

            await CreateUnLinkedTransaction(input);
            _logger.LogError($"{DateTime.Now} ====>  Organization ID is not found , Saved in Unlinked Transaction");
            _logger.LogError($"{DateTime.Now} ====> Get Response false");
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
            IsIn = camera.CameraType == CameraType.Exit ? false : true,
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
                _logger.LogDebug($"{DateTime.Now} ====> Get Response false | {ex.Message}");
                return await Task.FromResult(false);
            }
        }
        //Check Garage Capicity

        if (GarageTransaction.CameraType == CameraType.Entry)
        {
            Customer customer = null;
            var Id = await CheckCustomer(input.PlateNumber);
            if (Id != null)
            {
               var packg=await CheckActivePackageForCustomer(Guid.Parse(Id), GarageTransaction.OrganizationID);
                GarageTransaction.CustomerCategory = packg is not null ?(CustomerCategoryStatus)Enum.Parse(typeof(CustomerCategoryStatus), packg.Package.CustomerCategory): CustomerCategoryStatus.Customer;

               customer = await GetCustomer(Id);
            }
            else
            {
                customer = null;
            }

            bool canEnter = await HandleCamersEntry(garageId, GarageTransaction.CustomerCategory);
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


    private async Task<bool> HandleCamersEntry(int garageId, CustomerCategoryStatus customer)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            var garage = await GetGarage(garageId);
            if (garage.Status != Status.Active)
            {
                return false;
            }
            var org = await GetOrganization(garage.OrganizationId);
            if (org.Status != Status.Active)
            {
                return false;
            }



            var result = await CheckAvailableSlots(garage, customer);
            await CurrentUnitOfWork.SaveChangesAsync();

            return result;
        }
    }

    private async Task<bool> FreeSlots(Garage garage, CustomerCategoryStatus customer)
    {
        bool flag = false;
        if (customer == CustomerCategoryStatus.Customer)
        {
            garage.CurrnetCapacityForCustomers--;
            flag = true;

        }
        else if (customer == CustomerCategoryStatus.Employee)
        {
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

    private async Task<bool> CheckAvailableSlots(Garage garage, CustomerCategoryStatus customer)
    {
        bool flag = false;
        if ((customer == null || customer == Enums.CustomerCategoryStatus.Customer))
        {
            if (garage.CurrnetCapacityForCustomers + 1 <= garage.MaxSlotsForCustomers)
            {
                garage.CurrnetCapacityForCustomers++;
                flag = true;
            }

        }
        else if (customer == Enums.CustomerCategoryStatus.Employee)
        {

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
    private async Task<CreateUpdateInvoiceDto> CalculateInvoiceForCustomerWithoutPackage(DateTime EntranceTime, int totalHours, Customer exsitCustomer, CreateUpdateGarageTransactionDto GarageTransaction,CustomerCategoryStatus customerCategoryStatus)
    {
        decimal price = await CalcInvoice(EntranceTime, totalHours, GarageTransaction);
        CreateUpdateInvoiceDto invoice = await GenrateInvoiceNumberAndInitiateInvoice(EntranceTime, exsitCustomer, GarageTransaction, totalHours, price, false);
        return invoice;

    }
    private async Task<CreateUpdateInvoiceDto> CalculateInvoiceForCustomerPackage(DateTime EntranceTime, Organization organization, Customer exsitCustomer, CustomerPackageDto packageDto, CreateUpdateGarageTransactionDto GarageTransaction,int TotalHours,CustomerCategoryStatus customerCategory)
    {
        var ExpiryDate = await _customerPackagesAppService.GetExpireyDateForPackageSubscripedByCustomerInSpecificOrganization(organization.Id, exsitCustomer.Id, packageDto.PackageId);

        var THoursOutPackage = CalculateTimeDependsOnPackage(EntranceTime, GarageTransaction.AccurateTime, packageDto, ExpiryDate);

        var price = THoursOutPackage * organization.AdditionalHourPrice;

        CreateUpdateInvoiceDto invoice = await GenrateInvoiceNumberAndInitiateInvoice(EntranceTime, exsitCustomer, GarageTransaction, TotalHours, price, true);

        return invoice;
    }

    private async Task<InvoiceDto> SaveTransactionWithInvoice(CreateUpdateInvoiceDto invoice, CreateUpdateGarageTransactionDto GarageTransaction)
    {
        var garageTransactionDto = await _garageTransactionsAppService.CreateAsync(GarageTransaction);
        var resultedInvoice= await _invoiceAppService.CreateAsync(invoice);
        return resultedInvoice;
    }

    private async Task<CreateUpdateInvoiceDto> GenrateInvoiceNumberAndInitiateInvoice(DateTime EntranceTime, Customer exsitCustomer, CreateUpdateGarageTransactionDto GarageTransaction, int TotalHours, decimal price, bool IsPackage)
    {
        string generatedInvoiceNumber = await _invoiceAppService.GenerateInvoiceNumberAsync(GarageTransaction.OrganizationID);
        var invoice = await CreateInvoice(
          GarageTransaction.GarageId,
          generatedInvoiceNumber,
          price,
          GarageTransaction.OrganizationID,
          TotalHours,
          GarageTransaction.PlateNumber,
          GarageTransaction.AccurateTime.Value,
          EntranceTime,
          exsitCustomer?.IdentityUser.Name ?? customerEnName,
          exsitCustomer?.ArabicName ?? customerArName,
          IsPackage,
          GarageTransaction.VehiclePicture,
          exsitCustomer?.Id ?? null
         
          
        );
        return invoice;
    }

    private async Task<CreateUpdateInvoiceDto> CheckoutMobileProcessWithoutSave(Guid customerId, DateTime EntranceTime, CreateUpdateGarageTransactionDto GarageTransaction, Customer customer)
    {
        var organization = await GetOrganization(GarageTransaction.OrganizationID);

        int totalHours = CalcTime(EntranceTime, GarageTransaction.AccurateTime.Value, organization.FreeMinutes);
        if (totalHours == 0)
        {

          return await GenrateInvoiceNumberAndInitiateInvoice(EntranceTime, customer, GarageTransaction, totalHours, 0, false);

        }
        var packageDto = await CheckActivePackageForCustomer(customerId, GarageTransaction.OrganizationID);

        return packageDto switch
        {
            (null) => await CalculateInvoiceForCustomerWithoutPackage(EntranceTime, totalHours, customer, GarageTransaction,CustomerCategoryStatus.Customer),
            (not null) => await CalculateInvoiceForCustomerPackage(EntranceTime, organization, customer, packageDto, GarageTransaction, totalHours, (CustomerCategoryStatus)Enum.Parse(typeof(CustomerCategoryStatus), packageDto.Package.CustomerCategory)),

        };
       


    }
    private async Task<bool> IfLoggedOutFromMobile(string plateNumber)
    {
        var transaction = await CheckIfCheckoutFromMobile(plateNumber);
        if (transaction is null)
            return false;
        return transaction.CheckoutMobile != null ? true : false;

    }
    private async Task<bool> CheckIfPassGraceTime(DateTime CurrentCheckoutTime,DateTime? lastCheckoutTime , int GraceMinutes)
    {
        if ((int)(CurrentCheckoutTime - lastCheckoutTime).Value.TotalMinutes > GraceMinutes)
        {
            return true;
        }
        return false;


    }
    private async Task<bool> HandleCamersExit(int garageId, int organizationId, CreateUpdateGarageTransactionDto GarageTransaction)
    {
        bool flag = false;
        var EntranceTime = await FindEntranceTime(GarageTransaction.PlateNumber);
        if (EntranceTime is null)
        {
            GarageTransaction.IsIn = false;
            var garageTransactionDto = await _garageTransactionsAppService.CreateAsync(GarageTransaction);

            _logger.LogError($"{DateTime.Now} ====>  Entrance Time is null ");
            _logger.LogError($"{DateTime.Now} ====> Get Response false");

            return await Task.FromResult(false);

        }
        var organization = await GetOrganization(GarageTransaction.OrganizationID);
        var customer = await CheckCustomer(GarageTransaction.PlateNumber);
        var result = await GetCustomerWithItsFirstActivePackage(GarageTransaction, customer);
        Customer exsitCustomer = result.Item1;
        CustomerPackageDto packageDto= result.Item2;

        var CheckIfLoggedOutFromMobile = await IfLoggedOutFromMobile(GarageTransaction.PlateNumber);
        if (CheckIfLoggedOutFromMobile)
        {

            var customerId = customer is not null ? Guid.Parse(customer) : Guid.Empty;

            var invoice = await _invoiceAppService.GetLatestInvoiceForCustomerInOrganizationGarage(GarageTransaction.PlateNumber, organizationId, GarageTransaction.GarageId);
            await CloseCheckoutMobileTransaction(GarageTransaction.PlateNumber);

            _logger.LogTrace($"{DateTime.Now} ====> Customer ID : {customer}");
            var PassGraceTime = await CheckIfPassGraceTime(GarageTransaction.AccurateTime.Value, invoice.Value.InvoiceTime, organization.Graceperiod);
            await CloseEntryTransaction(GarageTransaction.PlateNumber);

            if (invoice.Value.InvoiceType == InvoiceType.Paid.ToString() && !PassGraceTime)
            {

                _logger.LogTrace($"{DateTime.Now} ====> Get Response true | In Grace period ");

                return await Task.FromResult(true);
            }
            else if (invoice.Value.InvoiceType == InvoiceType.Paid.ToString() && PassGraceTime)
            {
                int Hours = (int)(GarageTransaction.AccurateTime.Value - invoice.Value.InvoiceTime.Value).Hours + (GarageTransaction.AccurateTime.Value-invoice.Value.InvoiceTime.Value ).Minutes > 0 ? 1 : 0;

                _logger.LogTrace($"{DateTime.Now} ====> Get Response false | Exceeded Grace Period must pay invoice");

                var newInvoice= await CalculateInvoiceForCustomerWithoutPackage(EntranceTime.Value, Hours, exsitCustomer, GarageTransaction, CustomerCategoryStatus.Customer);
                var resultedInvoice= await SaveTransactionWithInvoice(newInvoice, GarageTransaction);

                return await Task.FromResult(false);

            }
        }

        int totalHours = CalcTime(EntranceTime.Value, GarageTransaction.AccurateTime.Value, organization.FreeMinutes);
        if (totalHours == 0)
        {
            var garageTransactionDto = await _garageTransactionsAppService.CreateAsync(GarageTransaction);
            _logger.LogTrace($"{DateTime.Now} ====> Customer ID : {customer}");

            _logger.LogTrace($"{DateTime.Now} ====> Get Response true | In Free minutes period");
            return await Task.FromResult(true);
        }
        var garage = await GetGarage(garageId);

        //has plate number 
        bool MustPay = false, EmptyPlateNumber = false;
        if (customer != null)
        {

            var invoiceCreateUpdateDto = packageDto switch
            {
                (null) => await CalculateInvoiceForCustomerWithoutPackage(EntranceTime.Value, totalHours, exsitCustomer, GarageTransaction, CustomerCategoryStatus.Customer),
                (not null) => await CalculateInvoiceForCustomerPackage(EntranceTime.Value, organization, exsitCustomer, packageDto, GarageTransaction, totalHours, (CustomerCategoryStatus)Enum.Parse(typeof(CustomerCategoryStatus), packageDto.Package.CustomerCategory)),

            };
            var resultedInvoice = await SaveTransactionWithInvoice(invoiceCreateUpdateDto, GarageTransaction);
            flag =resultedInvoice.InvoicePrice > 0 ? false : true;



        }
        else
        {
            if (GarageTransaction.PlateNumber == null)
            {
                var garageTransactionDto = await _garageTransactionsAppService.CreateAsync(GarageTransaction);
                _logger.LogError($"{DateTime.Now} ====> Get Response false | Plate number is empty");

                flag = false;
                EmptyPlateNumber = true;
            }
            else
            {
                //Not Customer
                decimal price = await CalcInvoice(EntranceTime.Value, totalHours, GarageTransaction);

                var newInvoice = await CalculateInvoiceForCustomerWithoutPackage(EntranceTime.Value, totalHours, exsitCustomer, GarageTransaction, CustomerCategoryStatus.Customer);
                await SaveTransactionWithInvoice(newInvoice, GarageTransaction);
                await FreeSlots(garage, CustomerCategoryStatus.Customer);
                MustPay = true;
                flag = false;
            }
        }
        await CloseEntryTransaction(GarageTransaction.PlateNumber);
        await CurrentUnitOfWork.SaveChangesAsync();
        _logger.LogTrace($"{DateTime.Now} ====> Get Response {flag}");

        switch (flag, MustPay, EmptyPlateNumber)
        {
            case (false, true, _):
                _logger.LogTrace("| Not Customer");
                break;
            case (false, _, false):
                _logger.LogTrace("| Plate Number Is Empty");
                break;
            case (false, _, _):
                _logger.LogTrace("| Enter and Leave outside package time");
                break;
            case (true, _, _):
                _logger.LogTrace("| Enter and Leave in package time");
                break;
        }

        return flag;
    }

    private async Task<(Customer,CustomerPackageDto)> GetCustomerWithItsFirstActivePackage(CreateUpdateGarageTransactionDto GarageTransaction, string? customer)
    {
        Customer exsitCustomer = null;
        CustomerPackageDto packg = null;
        if (customer != null)
        {
            packg = await CheckActivePackageForCustomer(Guid.Parse(customer), GarageTransaction.OrganizationID);
            GarageTransaction.CustomerCategory = packg is not null ? (CustomerCategoryStatus)Enum.Parse(typeof(CustomerCategoryStatus), packg.Package.CustomerCategory) : CustomerCategoryStatus.Customer;

            exsitCustomer = await GetCustomer(customer);

        }

        return (exsitCustomer,packg);
    }

    private async Task<Customer> GetCustomer(string customerId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _customerRepoistory.GetQueryableAsync();
            query = query.Include(x => x.IdentityUser);
            var customer = await query.FirstOrDefaultAsync(x => x.Id == Guid.Parse(customerId));
            return customer;
        }

    }
    private async Task<CreateUpdateInvoiceDto> CreateInvoice(int GarageId, string GeneratedInvoiceNumber, decimal Price, int OrganizationID
        , int TotalHours, string PlateNumber, DateTime InvoiceTime, DateTime? EntranceTime, string CustomerEnName, string CustomerArName, bool IsPackage,string ExitTransactionImage , Guid? customerId = null )
    {
        var entryTransactionImage = await FindEntranceImage(PlateNumber);
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
            CustomerId = customerId,
            IsPackage = IsPackage, 
            ExitTransactionImage = ExitTransactionImage,
            EntryTransactionImage = entryTransactionImage
        };

        return invoice;

    }

    public int CalculateTimeDependsOnPackage(DateTime? entranceTime, DateTime? exitTime, CustomerPackageDto packageDto, DateTime? PackageExpiryDate)
    {
        if (exitTime < entranceTime)
        {
            throw new BusinessException("Leaving date can't be befor entrance date");
        }
        if (packageDto.Package.Published)
        {
            return 0;
        }
        DateTime packageStart = entranceTime.Value.Date.AddHours(packageDto.Package.PublishFrom.Hour).AddMinutes(packageDto.Package.PublishFrom.Minute);
        DateTime packageEnd = entranceTime.Value.Date.AddHours(packageDto.Package.PublishTo.Hour).AddMinutes(packageDto.Package.PublishTo.Minute);

        double extraHours = 0;

        DateTime currentDay = entranceTime.Value;
        DateTime endDay = exitTime.Value;

        while (currentDay <= Min(PackageExpiryDate.Value, exitTime.Value))
        {
            DateTime dayPackageStart = currentDay.Date + packageStart.TimeOfDay;
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
                extraHours += smallestDay.Hours + (smallestDay.Minutes > 0 ? 1 : 0);
                currentDay = dayPackageStart;

            }
            if (currentDay.TimeOfDay > dayPackageEnd.TimeOfDay)
            {
                var hours = currentDay.Hour - dayPackageEnd.Hour;
                currentDay.AddHours(hours);
                extraHours -= hours;
            }

            if (endDay > dayPackageEnd && endDay.Date == currentDay.Date)
            {
                extraHours += (endDay - Max(currentDay, dayPackageEnd)).TotalHours;
                extraHours += (endDay - Max(currentDay, dayPackageEnd)).Minutes > 0 ? 1 : 0;
            }

            if (endDay.Date > currentDay.Date)
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


        if (endDay > PackageExpiryDate)
        {
            extraHours -= packageStart.Hour;
            extraHours += (endDay - PackageExpiryDate).Value.Hours;
            extraHours += (endDay - currentDay).Minutes > 0 ? 1 : 0;
        }

        // Return result
        return (int)extraHours;
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
    private async Task<Camera> GetAnExitCameraByGarageId(int GarageId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var camera = await _cameraRepository.FirstOrDefaultAsync(x => x.GarageId == GarageId && x.CameraType == CameraType.Exit);

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
        using (_dataFilter.Disable<IMultiTenant>())
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

    }
    private async Task<CustomerPackageDto> CheckActivePackageForCustomer(Guid customerId, int organizationId)
    {
        var customerPackage = await _customerPackagesAppService.GetOneActiveCustomerPackageByOrgIdAndCustomerId(organizationId, customerId);
        if (customerPackage == null)
        {
            _logger.LogError($"{DateTime.Now} ====>  customerPackage not found");
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
                    .Where(gt => (gt.PlateNumber == null || gt.PlateNumber == plateNumber) && gt.CameraType == CameraType.Entry && gt.IsIn == true)
                    .OrderByDescending(gt => gt.AccurateTime) // or AccurateTime
                    .FirstOrDefault();

            return latestTransaction?.AccurateTime;
        }
    }
    private async Task<string> FindEntranceImage(string? plateNumber)
    {
        // Get the list of transactions for the provided plate number
        using (DataFilter.Disable<IMultiTenant>())
        {

            var query = await _garageTransactionRepoistory.GetQueryableAsync();


            var latestTransaction = query
                    .Where(gt => (gt.PlateNumber == null || gt.PlateNumber == plateNumber) && gt.CameraType == CameraType.Entry && gt.IsIn == true)
                    .OrderByDescending(gt => gt.AccurateTime) // or AccurateTime
                    .FirstOrDefault();

            return latestTransaction?.VehiclePicture;
        }
    }

    private async Task<GarageTransaction> CheckIfCheckoutFromMobile(string plateNumber)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {

            var query = await _garageTransactionRepoistory.GetQueryableAsync();


            var latestTransaction = query
                    .Where(gt => (gt.PlateNumber == null || gt.PlateNumber == plateNumber) && gt.CameraType == CameraType.Exit && gt.IsIn == true)
                    .OrderByDescending(gt => gt.AccurateTime)
                    .FirstOrDefault();

            return latestTransaction;
        }
    }
    private async Task CloseEntryTransaction(string? plateNumber)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {

            var query = await _garageTransactionRepoistory.GetQueryableAsync();

            await SaveClosedEntryTransaction(plateNumber, query);
        }
    }

    private async Task SaveClosedEntryTransaction(string? plateNumber, IQueryable<GarageTransaction> query)
    {
        var latestTransaction = query
                .Where(gt => (gt.PlateNumber == null || gt.PlateNumber == plateNumber) && gt.IsIn == true)
                .OrderByDescending(gt => gt.AccurateTime)
                .FirstOrDefault();

        latestTransaction.IsIn = false;
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    private async Task CloseCheckoutMobileTransaction(string? plateNumber)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {

            var query = await _garageTransactionRepoistory.GetQueryableAsync();
            query.Where(gt => gt.CameraType == CameraType.Exit);
            await SaveClosedEntryTransaction(plateNumber, query);


        }
    }
    
    private int CalcTime(DateTime startTime, DateTime endTime, int FreeMinutes)
    {


        // Calculate the time difference
        TimeSpan timeDifference = endTime - startTime;



        if ((int)timeDifference.TotalHours == 0 && timeDifference.Minutes <= FreeMinutes)
        {
            return 0;

        }

        int hoursDiff = (int)timeDifference.TotalHours;


        hoursDiff += (timeDifference.Minutes > 0 ? 1 : 0);


        return hoursDiff;
    }
    private async Task<Decimal> CalcInvoice(DateTime entrenceTime, int totalHours, CreateUpdateGarageTransactionDto garageTransaction)
    {
        var organization = await GetOrganization(garageTransaction.OrganizationID);

        //4 Cases
        return (organization.IsNumberOfHoursExceeded, organization.IsClose) switch
        {
            (false, false) => calcPriceHoursExceededDay(organization, totalHours),
            (true, false) => CalculatePriceForExceededHours(totalHours, organization),
            (false, true) => CalculatePriceForOrganizationIsClosed(totalHours, organization, garageTransaction, entrenceTime),
            (true, true) => CalculatePriceForOrganizationIsClosedAndExceededHours(totalHours, organization, garageTransaction, entrenceTime),
        };
    }
    private decimal CalculatePriceForExceededHours(int totalHours, Organization organization)
    {
        decimal price = 0;
        if (totalHours > organization.NumberOfHours)
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
    private decimal CalculatePriceForOrganizationIsClosedAndExceededHours(int totalHours, Organization organization, CreateUpdateGarageTransactionDto garageTransaction, DateTime entrenceTime)
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
    private decimal CalculatePriceForOrganizationIsClosed(int totalHours, Organization organization, CreateUpdateGarageTransactionDto garageTransaction, DateTime entrenceTime)
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
        if ((int)normalHours + (int)specialHours > 24)
        {
            price += (double)calcPriceHoursExceededDay(organization, (int)normalHours + (int)specialHours);
            price -= totalDays * closingHour * (double)organization.AdditionalHourPrice;

        }
        else
        {
            price += (double)calcPriceHoursExceededDay(organization, (int)normalHours);

        }
        return (decimal)price;
    }
    private decimal calcPriceHours(Organization organization, int hours)
    {
        decimal price = 0;
        price += hours > 0 ? organization.FirstHourPrice : 0;
        hours--;
        price += hours > 0 ? organization.SecondHourPrice : 0;
        hours--;
        price += hours > 0 ? organization.ThirdHourPrice : 0;
        hours--;
        price += hours > 0 ? (hours * organization.AdditionalHourPrice) : 0;
        return price;
    }
    private decimal calcPriceHoursExceededDay(Organization organization, int hours)
    {
        decimal price = 0;
        while (hours > 0)
        {
            price += hours > 0 ? organization.FirstHourPrice : 0;
            hours--;
            price += hours > 0 ? organization.SecondHourPrice : 0;
            hours--;
            price += hours > 0 ? organization.ThirdHourPrice : 0;
            hours--;
            price += hours > 0 ? (Math.Min(hours, 21) * organization.AdditionalHourPrice) : 0;
            hours -= 21;

        }
        return price;
    }
    private static (double, double) CalculateHours(CreateUpdateGarageTransactionDto garageTransaction, DateTime entrenceTime, Organization organization)
    {
        TimeSpan totalDuration = garageTransaction.AccurateTime.Value - entrenceTime;

        double totalHours = (int)totalDuration.TotalHours;
        totalHours += totalDuration.Minutes > 0 ? 1 : 0;


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
                        specialHours += (int)specialDurationBeforeMidnight.TotalHours; // Round up
                        specialHours += specialDurationBeforeMidnight.Minutes > 0 ? 1 : 0;
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
                        specialHours += (int)specialDurationAfterMidnight.TotalHours; // Round up
                        specialHours += specialDurationAfterMidnight.Minutes > 0 ? 1 : 0;
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
                        specialHours += (int)specialDuration.TotalHours; // Round up
                        specialHours += specialDuration.Minutes > 0 ? 1 : 0;
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
    private async Task<Guid> GetCurrentCustomerId()
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {

            var customer = await _customerRepoistory.GetQueryableAsync();
            var customerId = customer.Where(s => s.IdentityUserId == CurrentUser.Id).Select(s => s.Id).FirstOrDefault();
            return customerId;
        }
    }

    private async Task<GarageTransactionDto> GetTransactionById(int garagetransactionId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _garageTransactionRepoistory.GetQueryableAsync();
            query = query.Where(transaction => transaction.Id == garagetransactionId);
            var transaction = await AsyncExecuter.FirstOrDefaultAsync(query);
            if (transaction != null)
            {

                var transactionDto = ObjectMapper.Map<GarageTransaction, GarageTransactionDto>(transaction);

                return transactionDto;
            }
            throw new EntityNotFoundException($"Transaction with id {garagetransactionId} is not found");


        }

    }

    public async Task<Result<CreateUpdateInvoiceDto>> CustomerPreCheckoutFromHisMobile(int garageTransactionId)
    {
        var customerID = await GetCurrentCustomerId();
        var Customer = await GetCustomer(customerID.ToString());
        var result= await InitiateGarageTransaction(garageTransactionId);
        var invoice = await CheckoutMobileProcessWithoutSave(customerID, result.Item2, result.Item1, Customer);
        return invoice;

    }
    
    public async Task<Result<InvoiceDto>> CustomerCheckoutFromHisMobile(int garageTransactionId)
    {
        var result = await InitiateGarageTransaction(garageTransactionId);
        var invoice = await CustomerPreCheckoutFromHisMobile(garageTransactionId);
        invoice.Value.InvoiceType = InvoiceType.Paid;
        return await SaveTransactionWithInvoice(invoice,result.Item1);
    }
    private async Task<(CreateUpdateGarageTransactionDto,DateTime)> InitiateGarageTransaction(int garageTransactionId)
    {
        var entryGarageTransaction = await GetTransactionById(garageTransactionId);
        var camera = await GetAnExitCameraByGarageId(entryGarageTransaction.GarageId);

       var  exitGarageTransaction = new CreateUpdateGarageTransactionDto
        {
            AccurateTime = DateTime.Now,
            PlateNumber = entryGarageTransaction.PlateNumber,
            GarageId = entryGarageTransaction.GarageId,
            OrganizationID = entryGarageTransaction.OrganizationID,
            CameraId = camera.Id,
            SnapTime = DateTime.UtcNow,
            TenantId = entryGarageTransaction.TenantId,
            IsIn = true,
            CheckoutMobile = DateTime.Now,
            VehiclePicture = entryGarageTransaction.VehiclePicture,
            CameraType = CameraType.Exit
       };
        return (exitGarageTransaction, entryGarageTransaction.AccurateTime);
    }

    public async Task<Result<InvoiceDto>> CustomerCheckoutFromKeeperMobile(int garageTransactionId)
    {
        var result = await InitiateGarageTransaction(garageTransactionId);
        var invoice = await CustomerPreCheckoutFromKeeperMobile(garageTransactionId);
        return await SaveTransactionWithInvoice(invoice, result.Item1);
    }
    
    public async Task<Result<CreateUpdateInvoiceDto>> CustomerPreCheckoutFromKeeperMobile(int garageTransactionId)
    {
        var result = await InitiateGarageTransaction(garageTransactionId);
        var org = await GetOrganization(result.Item1.OrganizationID);
        int totalHours = CalcTime(result.Item2, (DateTime)result.Item1.AccurateTime, org.FreeMinutes);
        var customer = await CheckCustomer(result.Item1.PlateNumber);
        if (customer == null) {

            decimal price = await CalcInvoice(result.Item2, totalHours, result.Item1);
            return await GenrateInvoiceNumberAndInitiateInvoice(result.Item2, null, result.Item1, totalHours, price, false);

        }
        var result2 = await GetCustomerWithItsFirstActivePackage(result.Item1, customer);

        return (result2.Item2) switch
        {
            (null) => await CalculateInvoiceForCustomerWithoutPackage(result.Item2, totalHours, result2.Item1, result.Item1, CustomerCategoryStatus.Customer),
            (not null) => await CalculateInvoiceForCustomerPackage(result.Item2, org, result2.Item1, result2.Item2, result.Item1, totalHours, (CustomerCategoryStatus)Enum.Parse(typeof(CustomerCategoryStatus), result2.Item2.Package.CustomerCategory)),

        };
        




    }
}


