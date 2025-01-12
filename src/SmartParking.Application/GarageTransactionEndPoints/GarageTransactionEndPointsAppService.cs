using Abp.Domain.Entities;
using AutoMapper.Internal.Mappers;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using SmartParking.CalculationsEndpoint;
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
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Users;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace SmartParking.GarageTransactionEndPoints;

//[Authorize]
[IgnoreAntiforgeryToken]
public class GarageTransactionEndPointsAppService : SmartParkingAppService, IGarageTransactionEndPointsAppService 
{
    private readonly IGarageTransactionsAppService _garageTransactionsAppService;
    private readonly IUnLinkedTransactionAppService _unlinkedGarageTransactionsAppService;
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
    private readonly ICalculationsEndPointsAppService _calculationsEndPointAppService;
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
       IRepository<UnLinkedTransaction> unLinkedTransactionRepository, IPaymentAppService paymentAppService, ICalculationsEndPointsAppService calculationsEndPointAppService, IUnLinkedTransactionAppService unlinkedGarageTransactionsAppService)
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
        customerEnName = "unknown";
        customerArName = "غير معرف";
        _calculationsEndPointAppService = calculationsEndPointAppService;
        _unlinkedGarageTransactionsAppService = unlinkedGarageTransactionsAppService;
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
        CreateUpdateGarageTransactionDto GarageTransaction = GenerateCreateGarageTransactionDto(input, camera, cameraId, garageId, organizationId);

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
                var packg = await CheckActivePackageForCustomer(Guid.Parse(Id), GarageTransaction.OrganizationID);
                GarageTransaction.CustomerCategory = packg is not null ? (CustomerCategoryStatus)Enum.Parse(typeof(CustomerCategoryStatus), packg.Package.CustomerCategory) : CustomerCategoryStatus.Customer;

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

    private CreateUpdateGarageTransactionDto GenerateCreateGarageTransactionDto(CreateUpdateGarageTransactionEndPointDto input, Camera camera, int cameraId, int garageId, int organizationId)
    {
        return new CreateUpdateGarageTransactionDto
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
            IsMapped = camera.CameraType == CameraType.Entry ? false : true,
        };
    }

    private async Task CreateUnLinkedTransaction(CreateUpdateGarageTransactionEndPointDto input)
    {
        await _unlinkedGarageTransactionsAppService.CreateAsync(input);
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
        garage.CurrnetCapicity--;
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
        if(flag)
          garage.CurrnetCapicity++;
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

        var THoursOutPackage = _calculationsEndPointAppService.CalculateTimeDependsOnPackage(EntranceTime, GarageTransaction.AccurateTime, packageDto, ExpiryDate);

        var price = THoursOutPackage * organization.AdditionalHourPrice;

        CreateUpdateInvoiceDto invoice = await GenrateInvoiceNumberAndInitiateInvoice(EntranceTime, exsitCustomer, GarageTransaction, TotalHours, price, true);

        return invoice;
    }
    private async Task<InvoiceDto> SaveTransactionWithInvoice(CreateUpdateInvoiceDto invoice, CreateUpdateGarageTransactionDto GarageTransaction)
    {
        await _garageTransactionsAppService.CreateAsync(GarageTransaction);
        var resultedInvoice= await _invoiceAppService.CreateAsync(invoice);
        return resultedInvoice;
    }
    private async Task<CreateUpdateInvoiceDto> GenrateInvoiceNumberAndInitiateInvoice(DateTime EntranceTime, Customer exsitCustomer, CreateUpdateGarageTransactionDto GarageTransaction, int TotalHours, decimal price, bool IsPackage)
    {
        var invoice = await CreateInvoice(
          GarageTransaction.GarageId,
          "",
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

        int totalHours = _calculationsEndPointAppService.CalculateTime(EntranceTime, GarageTransaction.AccurateTime.Value, organization.FreeMinutes);
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
        if (EntranceTime is null || EntranceTime>GarageTransaction.AccurateTime)
        {
            GarageTransaction.IsIn = false;
            var garageTransactionDto = await _garageTransactionsAppService.CreateAsync(GarageTransaction);

            _logger.LogError($"{DateTime.Now} ====>  Entrance Time is null  ");
            _logger.LogError($"{DateTime.Now} ====> Get Response false");
            if(EntranceTime > GarageTransaction.AccurateTime)
            {
                _logger.LogError($"{DateTime.Now} ====>  Entrance Time is graeter exit time  ");
                _logger.LogError($"{DateTime.Now} ====> Get Response false");
            }

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

            if (!PassGraceTime)
            {

                _logger.LogTrace($"{DateTime.Now} ====> Get Response true | In Grace period ");

                return await Task.FromResult(true);
            }
            else if (PassGraceTime)
            {
                int Hours = (int)(GarageTransaction.AccurateTime.Value - invoice.Value.InvoiceTime.Value).Hours + (GarageTransaction.AccurateTime.Value-invoice.Value.InvoiceTime.Value ).Minutes > 0 ? 1 : 0;

                _logger.LogTrace($"{DateTime.Now} ====> Get Response false | Exceeded Grace Period must pay invoice");

                var newInvoice= await CalculateInvoiceForCustomerWithoutPackage(EntranceTime.Value, Hours, exsitCustomer, GarageTransaction, CustomerCategoryStatus.Customer);
                var resultedInvoice= await SaveTransactionWithInvoice(newInvoice, GarageTransaction);

                return await Task.FromResult(false);

            }
        }

        int totalHours = _calculationsEndPointAppService.CalculateTime(EntranceTime.Value, GarageTransaction.AccurateTime.Value, organization.FreeMinutes);
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
        var latestTransactions = query
                .Where(gt => (gt.PlateNumber == null || gt.PlateNumber == plateNumber) && gt.IsIn == true)
                .OrderByDescending(gt => gt.AccurateTime)
                .ToList();
        latestTransactions.ForEach(gt =>{ 
            gt.IsIn = false;
            gt.IsMapped = true;
        });
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
    private async Task<Decimal> CalcInvoice(DateTime entrenceTime, int totalHours, CreateUpdateGarageTransactionDto garageTransaction)
    {
        var organization = await GetOrganization(garageTransaction.OrganizationID);

        //4 Cases
        return (organization.IsNumberOfHoursExceeded, organization.IsClose) switch
        {
            (false, false) =>  _calculationsEndPointAppService.CalculatePriceHoursExceededDay(organization, totalHours),
            (true, false) =>_calculationsEndPointAppService.CalculatePriceForExceededHours(totalHours, organization),
            (false, true) => _calculationsEndPointAppService.CalculatePriceForOrganizationIsClosed(totalHours, organization, garageTransaction, entrenceTime),
            (true, true) => _calculationsEndPointAppService.CalculatePriceForOrganizationIsClosedAndExceededHours(totalHours, organization, garageTransaction, entrenceTime),
        };
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
    private async Task<GarageTransaction> GetTransactionById(int garagetransactionId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _garageTransactionRepoistory.GetQueryableAsync();
            query = query.Where(transaction => transaction.Id == garagetransactionId);
            var transaction = await AsyncExecuter.FirstOrDefaultAsync(query);
            if (transaction != null)
            {
                return transaction;
            }
            throw new EntityNotFoundException($"Transaction with id {transaction} is not found");
        }
    }
    public async Task<Result<CreateUpdateInvoiceDto>> CustomerPreCheckoutFromHisMobile(int garageTransactionId)
    {
        var customerID = await GetCurrentCustomerId();
        var Customer = await GetCustomer(customerID.ToString());
        var result= await InitiateGarageTransaction(garageTransactionId);
        var invoice = await CheckoutMobileProcessWithoutSave(customerID, result.Item2.AccurateTime.Value, result.Item1, Customer);
        return invoice;

    }
    public async Task<Result<InvoiceDto>> CustomerCheckoutFromHisMobile(int garageTransactionId)
    {
        var result = await InitiateGarageTransaction(garageTransactionId);
        if (result.Item2.CheckoutMobile != null)
        {
            throw new Exception("Transaction Already Checked-Out");
        }
        var invoice = await CustomerPreCheckoutFromHisMobile(garageTransactionId);
        invoice.Value.InvoiceType = InvoiceType.Paid;
        UpdateEntryTransactionCheckouFromMobile(result.Item2);
        return await SaveTransactionWithInvoice(invoice, result.Item1);
    }



    private async Task<(CreateUpdateGarageTransactionDto,GarageTransaction)> InitiateGarageTransaction(int garageTransactionId)
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
            SnapTime = DateTime.Now,
            TenantId = entryGarageTransaction.TenantId,
            IsIn = true,
            CheckoutMobile = DateTime.Now,
            VehiclePicture = entryGarageTransaction.VehiclePicture,
            CameraType = CameraType.Exit
       };
        return (exitGarageTransaction, entryGarageTransaction);
    }
    public async Task<Result<InvoiceDto>> CustomerCheckoutFromKeeperMobile(int garageTransactionId)
    {
        var result = await InitiateGarageTransaction(garageTransactionId);
        if (result.Item2.CheckoutMobile != null)
        {
            throw new Exception("Transaction Already Checked-Out");
        }
        var invoice = await CustomerPreCheckoutFromKeeperMobile(garageTransactionId);
        invoice.Value.InvoiceType = InvoiceType.Paid;
        await UpdateEntryTransactionCheckouFromMobile(result.Item2);
        
        return await SaveTransactionWithInvoice(invoice, result.Item1);

    }

    private async Task UpdateEntryTransactionCheckouFromMobile(GarageTransaction entryGarageTransaction)
    {
        entryGarageTransaction.CheckoutMobile = DateTime.Now;
        entryGarageTransaction.IsPending = true;
        await _garageTransactionRepoistory.UpdateAsync(entryGarageTransaction,true);

    }

    public async Task<Result<CreateUpdateInvoiceDto>> CustomerPreCheckoutFromKeeperMobile(int garageTransactionId)
    {
        var result = await InitiateGarageTransaction(garageTransactionId);
        var org = await GetOrganization(result.Item1.OrganizationID);
        int totalHours = _calculationsEndPointAppService.CalculateTime(result.Item2.AccurateTime.Value, (DateTime)result.Item1.AccurateTime, org.FreeMinutes);
        var customer = await CheckCustomer(result.Item1.PlateNumber);
        if (customer == null) {

            decimal price = await CalcInvoice(result.Item2.AccurateTime.Value, totalHours, result.Item1);
            return await GenrateInvoiceNumberAndInitiateInvoice(result.Item2.AccurateTime.Value, null, result.Item1, totalHours, price, false);

        }
        var result2 = await GetCustomerWithItsFirstActivePackage(result.Item1, customer);

        return (result2.Item2) switch
        {
            (null) => await CalculateInvoiceForCustomerWithoutPackage(result.Item2.AccurateTime.Value, totalHours, result2.Item1, result.Item1, CustomerCategoryStatus.Customer),
            (not null) => await CalculateInvoiceForCustomerPackage(result.Item2.AccurateTime.Value, org, result2.Item1, result2.Item2, result.Item1, totalHours, (CustomerCategoryStatus)Enum.Parse(typeof(CustomerCategoryStatus), result2.Item2.Package.CustomerCategory)),
        };

    }
}


