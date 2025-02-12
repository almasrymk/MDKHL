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
using SmartParking.Entities.Invoices;
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
        if (string.IsNullOrEmpty(input.PlateNumber)) {
            return await NotFoundEntityAndExceptionHandling(input, input.PlateNumber);
        }
        input.PlateNumber = input.PlateNumber.Replace(" ", "");

        var camera = await GetCamera(input.DeviceID);

        if (camera == null)
        {
            return await NotFoundEntityAndExceptionHandling(input,input.DeviceID);
        }

        var cameraId = camera.Id;
        var garageId = camera.GarageId;

        var organizationId = await GetOrganizationId(garageId);
        if (organizationId == -1)
        {
            return await NotFoundEntityAndExceptionHandling(input,"Organization");
        }
        CreateUpdateGarageTransactionDto GarageTransaction = GenerateCreateGarageTransactionDto(input, camera, cameraId, garageId, organizationId);

        try
        {
            switch(GarageTransaction.CameraType)
            {
                case CameraType.Exit:
                    return await HandleCamersExit(garageId, organizationId, GarageTransaction);

                case CameraType.Entry:
                    GarageTransaction.CustomerCategory = await GetTransactionCategory(input, GarageTransaction);
                    bool canEnter = await HandleCameraEntry(garageId, GarageTransaction.CustomerCategory);
                    return canEnter ? await _garageTransactionsAppService.CreateAsync(GarageTransaction)is not null : await NotFoundEntityAndExceptionHandling(input, "Can't Enter");
                    
            };
        }
        catch (Exception ex)
        {

            return await NotFoundEntityAndExceptionHandling(input, ex.Message);


        }
        await CreateUnLinkedTransaction(input);
        return await Task.FromResult(false);

    }

    private async Task<CustomerCategoryStatus> GetTransactionCategory(CreateUpdateGarageTransactionEndPointDto input, CreateUpdateGarageTransactionDto GarageTransaction)
    {
        var Id = await CheckVehicle(input.PlateNumber);
        CustomerPackageDto packg = null;
        if (Id != null)
        {
            packg = await CheckActivePackageForPlateNumber(input.PlateNumber, GarageTransaction.OrganizationID,input.AccurateTime.Value);
        }
        return packg is not null ? packg.CustomerCategory : CustomerCategoryStatus.Customer;
    }

    private async Task<bool> NotFoundEntityAndExceptionHandling(CreateUpdateGarageTransactionEndPointDto input,string NotFoundType)
    {
        await CreateUnLinkedTransaction(input);
        _logger.LogError($"{DateTime.Now} ====>  {NotFoundType} is not found , Saved in Unlinked Transaction");
        _logger.LogError($"{DateTime.Now} ====> Get Response false");

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
            return garage;
        }

    }
    private async Task<bool> HandleCameraEntry(int garageId, CustomerCategoryStatus customer)
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
    private async Task<bool> FreeSlots(Garage garage, CustomerCategoryStatus? customer)
    {
        bool flag = false;
        if (customer == CustomerCategoryStatus.Customer||customer==null)
        {
            garage.CurrnetCapacityForCustomers=Math.Max(0,--garage.CurrnetCapacityForCustomers);
            
            flag = true;

        }
        else if (customer == CustomerCategoryStatus.Employee)
        {
            garage.CurrnetCapacityEmployees = Math.Max(0, --garage.CurrnetCapacityEmployees);
            flag = true;
        }
        else
        {
            garage.CurrnetCapacityForFree = Math.Max(0, --garage.CurrnetCapacityForFree);

            flag = true;
        }
        garage.CurrnetCapicity=Math.Max(0,--garage.CurrnetCapicity);
        return await Task.FromResult(flag);

    }
    private async Task<bool> CheckAvailableSlots(Garage garage, CustomerCategoryStatus customer)
    {
        bool flag = false;
        switch (customer)
        {
            case CustomerCategoryStatus.Customer:
                if (garage.CurrnetCapacityForCustomers + 1 <= garage.MaxSlotsForCustomers)
                    garage.CurrnetCapacityForCustomers++;
                break;

            case CustomerCategoryStatus.Employee:
                if (garage.CurrnetCapacityEmployees + 1 <= garage.MaxSlotsForEmployess)
                    garage.CurrnetCapacityEmployees++;
                break;

            case CustomerCategoryStatus.Visitor:
                if (garage.CurrnetCapacityForFree + 1 <= garage.MaxSlotsForFree)
                    garage.CurrnetCapacityForFree++;
                break;

            default:
                if (garage.CurrnetCapacityForCustomers + 1 <= garage.MaxSlotsForCustomers)
                    garage.CurrnetCapacityForCustomers++;
                break;

        }
        if (garage.CurrnetCapicity + 1 <= garage.MaxCapacity)
        {
            garage.CurrnetCapicity++;
            flag = true;
        }
        return await Task.FromResult(flag);

    }


    private async Task<CreateUpdateInvoiceDto> CalculateInvoiceForCustomerWithoutPackage(DateTime EntranceTime, int totalHours, Customer exsitCustomer, CreateUpdateGarageTransactionDto GarageTransaction,CustomerCategoryStatus customerCategoryStatus)
    {
        decimal price = await CalculateInvoiceCases(EntranceTime, totalHours, GarageTransaction);
        CreateUpdateInvoiceDto invoice = await GenrateInvoiceNumberAndInitiateInvoice(EntranceTime, exsitCustomer, GarageTransaction, totalHours, price, false);
        return invoice;

    }
    private async Task<CreateUpdateInvoiceDto> CalculateInvoiceForCustomerPackage(DateTime EntranceTime, Organization organization, Customer exsitCustomer, CustomerPackageDto packageDto, CreateUpdateGarageTransactionDto GarageTransaction,int TotalHours,CustomerCategoryStatus customerCategory)
    {

        var THoursOutPackage = _calculationsEndPointAppService.CalculateTimeDependsOnPackage(EntranceTime, GarageTransaction.AccurateTime, packageDto);

        var price = THoursOutPackage * organization.AdditionalHourPrice;

        CreateUpdateInvoiceDto invoice = await GenrateInvoiceNumberAndInitiateInvoice(EntranceTime, exsitCustomer, GarageTransaction, TotalHours, price, true);

        return invoice;
    }
    private async Task<InvoiceDto> SaveTransactionWithInvoice(CreateUpdateInvoiceDto invoice, CreateUpdateGarageTransactionDto GarageTransaction)
    {
        var garageTransaction =  await _garageTransactionsAppService.CreateAsync(GarageTransaction);
        invoice.GarageTransactionExitId = garageTransaction.Id;
        var resultedInvoice= await _invoiceAppService.CreateAsync(invoice);
        return resultedInvoice;
    }
    private async Task<CreateUpdateInvoiceDto> GenrateInvoiceNumberAndInitiateInvoice(DateTime EntranceTime, Customer exsitCustomer, CreateUpdateGarageTransactionDto GarageTransaction, int TotalHours, decimal price, bool IsPackage)
    {
        string image = GarageTransaction.VehiclePicture;
        if (GarageTransaction.VehiclePicture.Length > 120)
        {
             image = _garageTransactionsAppService.SaveBase64Image(GarageTransaction);
        }

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
          image,
          exsitCustomer?.Id ?? null
         
          
        );
        return invoice;
    }
    private async Task<CreateUpdateInvoiceDto> CheckoutMobileProcessWithoutSave(DateTime EntranceTime, CreateUpdateGarageTransactionDto GarageTransaction, Customer customer)
    {
        var organization = await GetOrganization(GarageTransaction.OrganizationID);

        int totalHours = _calculationsEndPointAppService.CalculateTime(EntranceTime, GarageTransaction.AccurateTime.Value, organization.FreeMinutes);
        if (totalHours == 0)
        {

            return await GenrateInvoiceNumberAndInitiateInvoice(EntranceTime, customer, GarageTransaction, totalHours, 0, false);

        }
        var packageDto = await CheckActivePackageForPlateNumber(GarageTransaction.PlateNumber, GarageTransaction.OrganizationID, GarageTransaction.AccurateTime.Value);
        return await CalculateStayingHoursForCustomer(EntranceTime, GarageTransaction, customer, organization, totalHours, packageDto);
    }

    private async Task<CreateUpdateInvoiceDto> CalculateStayingHoursForCustomer(DateTime EntranceTime, CreateUpdateGarageTransactionDto GarageTransaction, Customer customer, Organization organization, int totalHours, CustomerPackageDto packageDto)
    {
        return (packageDto, packageDto?.Package) switch
        {
            (null, _) or (not null, null) => await CalculateInvoiceForCustomerWithoutPackage(EntranceTime, totalHours, customer, GarageTransaction, CustomerCategoryStatus.Customer),
            (not null, not null) => await CalculateInvoiceForCustomerPackage(EntranceTime, organization, customer, packageDto, GarageTransaction, totalHours, (CustomerCategoryStatus)Enum.Parse(typeof(CustomerCategoryStatus), packageDto.Package.CustomerCategory)),

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
    private async Task ChangeCategoryForEntryTransaction(GarageTransactionDto EntryTransaction)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var createUpdateTransactionDto = ObjectMapper.Map<GarageTransactionDto, CreateUpdateGarageTransactionDto>(EntryTransaction);
            await _garageTransactionsAppService.UpdateAsync(EntryTransaction.Id, createUpdateTransactionDto);
        }
    }
    private async Task<bool> CheckForCategoriesInTransactionsHasBeenChanged(CreateUpdateGarageTransactionDto ExitTransaction, GarageTransactionDto EntryTransaction , Garage garage)
    {
        if(EntryTransaction.CustomerCategory != ExitTransaction.CustomerCategory)
        {
            EntryTransaction.CustomerCategory = ExitTransaction.CustomerCategory;
            await ChangeCategoryForEntryTransaction(EntryTransaction);
            return true;
        }
        return false;

    }
    private async Task<bool> HandleCamersExit(int garageId, int organizationId, CreateUpdateGarageTransactionDto GarageTransaction)
    {
        bool flag = false;
        var EntryTransaction = await FindEntranceTime(GarageTransaction.PlateNumber);
        var EntranceTime = EntryTransaction?.AccurateTime;

        if (EntranceTime is null || EntranceTime > GarageTransaction.AccurateTime)
        {
            await InVailedCasesForTransacrion(GarageTransaction, EntranceTime);
            return await Task.FromResult(false);
        }
        var organization = await GetOrganization(GarageTransaction.OrganizationID);
        var customer = await CheckVehicle(GarageTransaction.PlateNumber);
        var lstCategory = EntryTransaction.CustomerCategory;
        CustomerPackageDto? packageDto = await GetCustomerWithItsFirstActivePackage(GarageTransaction, customer);
        GarageTransaction.CustomerCategory = packageDto is null ? CustomerCategoryStatus.Customer : packageDto.CustomerCategory;

        var garage = await GetGarage(garageId);
        var _categoryHasChanged = await CheckForCategoriesInTransactionsHasBeenChanged(GarageTransaction, EntryTransaction, garage);
        Customer exsitCustomer = customer is not null ? await GetCustomer(customer) : null;

        var CheckIfLoggedOutFromMobile = await IfLoggedOutFromMobile(GarageTransaction.PlateNumber);
        if (CheckIfLoggedOutFromMobile)
        {
            await CheckForCategoryChangement(EntryTransaction, garage, _categoryHasChanged, lstCategory);

            return await CheckGracePeriodScenarios(organizationId, GarageTransaction, EntranceTime, organization, customer, garage, packageDto, exsitCustomer);
        }

        int totalHours = _calculationsEndPointAppService.CalculateTime(EntranceTime.Value, GarageTransaction.AccurateTime.Value, organization.FreeMinutes);

        if (totalHours == 0)
        {
            return await ExitsInFreeMinutesPeriod(GarageTransaction);
        }
        switch (customer)
        {
            case not null:
                flag = await CalculationForRegisteredCustomer(GarageTransaction, flag, EntranceTime, organization, garage, packageDto, exsitCustomer, totalHours);
                await CheckForCategoryChangement(EntryTransaction, garage, _categoryHasChanged, lstCategory);
                break;
            case null:
                await CalculationForUnRegisteredCustomer(GarageTransaction, flag, EntranceTime, garage, exsitCustomer, totalHours);
                break;
        };
        await CloseEntryTransaction(GarageTransaction.PlateNumber);
        await CurrentUnitOfWork.SaveChangesAsync();
        _logger.LogInformation($"{DateTime.Now} ====> Get Response {flag}");

        return flag;
    }

    private async Task CheckForCategoryChangement(GarageTransactionDto? EntryTransacrion, Garage garage, bool _categoryHasChanged,CustomerCategoryStatus lstCategory)
    {
        if (_categoryHasChanged )
        {
            await FreeSlots(garage, lstCategory);
        }
        else
        {
            await FreeSlots(garage, EntryTransacrion.CustomerCategory);
        }
    }

    private async Task<bool> CalculationForUnRegisteredCustomer(CreateUpdateGarageTransactionDto GarageTransaction, bool flag, DateTime? EntranceTime, Garage garage, Customer exsitCustomer, int totalHours)
    {
        var newInvoice = await CalculateInvoiceForCustomerWithoutPackage(EntranceTime.Value, totalHours, exsitCustomer, GarageTransaction, CustomerCategoryStatus.Customer);
        await SaveTransactionWithInvoice(newInvoice, GarageTransaction);
        await FreeSlots(garage, CustomerCategoryStatus.Customer);
        flag = false;
        return flag;
    }

    private async Task<bool> CalculationForRegisteredCustomer(CreateUpdateGarageTransactionDto GarageTransaction, bool flag, DateTime? EntranceTime, Organization organization, Garage garage, CustomerPackageDto? packageDto, Customer exsitCustomer, int totalHours)
    {
        var invoiceCreateUpdateDto = await CalculateStayingHoursForCustomer(EntranceTime.Value, GarageTransaction, exsitCustomer, organization, totalHours, packageDto);
        var resultedInvoice = await SaveTransactionWithInvoice(invoiceCreateUpdateDto, GarageTransaction);
        flag = resultedInvoice.InvoicePrice > 0 ? false : true;
        return flag;
    }

    private async Task<bool> ExitsInFreeMinutesPeriod(CreateUpdateGarageTransactionDto GarageTransaction)
    {
        var garageTransactionDto = await _garageTransactionsAppService.CreateAsync(GarageTransaction);
        _logger.LogInformation($"{DateTime.Now} ====> Close Entry : {GarageTransaction.PlateNumber}");
        await CloseEntryTransaction(GarageTransaction.PlateNumber);
        _logger.LogInformation($"{DateTime.Now} ====> PlateNumber  : {GarageTransaction.PlateNumber}");
        _logger.LogInformation($"{DateTime.Now} ====> Get Response true | In Free minutes period");
        return await Task.FromResult(true);
    }

    private async Task<bool> CheckGracePeriodScenarios(int organizationId, CreateUpdateGarageTransactionDto GarageTransaction, DateTime? EntranceTime, Organization organization, string customer, Garage garage, CustomerPackageDto? packageDto, Customer exsitCustomer)
    {
        var invoice = await _invoiceAppService.GetLatestInvoiceForCustomerInOrganizationGarage(GarageTransaction.PlateNumber, organizationId, GarageTransaction.GarageId);
        await CloseCheckoutMobileTransaction(GarageTransaction.PlateNumber);
        _logger.LogInformation($"{DateTime.Now} ====> Customer ID : {customer}");
        _logger.LogInformation($"{DateTime.Now} ====> PlateNumber  : {GarageTransaction.PlateNumber}");
        var PassGraceTime = await CheckIfPassGraceTime(GarageTransaction.AccurateTime.Value, invoice.Value.InvoiceTime, organization.Graceperiod);
        return PassGraceTime switch
        {
            (false)=> await DoesnotPassGracePeriod(GarageTransaction),
            (true)=> await PassedGracePeriod(GarageTransaction, EntranceTime, exsitCustomer, invoice),
        };
   
    }

    private async Task<bool> PassedGracePeriod(CreateUpdateGarageTransactionDto GarageTransaction, DateTime? EntranceTime, Customer exsitCustomer, Result<InvoiceDto> invoice)
    {
        int Hours =(int) Math.Ceiling((decimal)(GarageTransaction.AccurateTime.Value - invoice.Value.InvoiceTime.Value).TotalHours) ;
        _logger.LogInformation($"{DateTime.Now} ====> Get Response false | Exceeded Grace Period must pay invoice");
        var newInvoice = await CalculateInvoiceForCustomerWithoutPackage(EntranceTime.Value, Hours, exsitCustomer, GarageTransaction, CustomerCategoryStatus.Customer);
        newInvoice.EntranceTime = invoice.Value.InvoiceTime.Value;
        newInvoice.EntryTransactionImage = invoice.Value.EntryTransactionImage;
        await SaveTransactionWithInvoice(newInvoice, GarageTransaction);
        return await Task.FromResult(false);

    }

    private async Task<bool> DoesnotPassGracePeriod(CreateUpdateGarageTransactionDto GarageTransaction)
    {
        _logger.LogInformation($"{DateTime.Now} ====> Get Response true | In Grace period ");
        var garageTransaction = await _garageTransactionsAppService.CreateAsync(GarageTransaction);
        return true;
    }

    private async Task InVailedCasesForTransacrion(CreateUpdateGarageTransactionDto GarageTransaction, DateTime? EntranceTime)
    {
        GarageTransaction.IsIn = false;
        GarageTransaction.IsMapped = false;
        var garageTransactionDto = await _garageTransactionsAppService.CreateAsync(GarageTransaction);

        _logger.LogError($"{DateTime.Now} ====>  Entrance Time is null  ");
        _logger.LogError($"{DateTime.Now} ====> Get Response false");
        if (EntranceTime > GarageTransaction.AccurateTime)
        {
            _logger.LogError($"{DateTime.Now} ====>  Entrance Time is graeter exit time  ");
            _logger.LogError($"{DateTime.Now} ====> Get Response false");
        }
    }

    private async Task<CustomerPackageDto> GetCustomerWithItsFirstActivePackage(CreateUpdateGarageTransactionDto GarageTransaction, string? customer)
    {
        CustomerPackageDto package = null;
        if (customer != null)
        {
            package = await CheckActivePackageForPlateNumber(GarageTransaction.PlateNumber, GarageTransaction.OrganizationID,GarageTransaction.AccurateTime.Value);
            GarageTransaction.CustomerCategory = package is null ? CustomerCategoryStatus.Customer : package.CustomerCategory;

        }

        return package;
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
    private async Task<string> CheckVehicle(string? plateNumber)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var Vehicle = await _vehicleRepository.FirstOrDefaultAsync(x => x.ArPlateNumber == plateNumber || x.EnPlateNumber == plateNumber);

            if (Vehicle == null)
            {
                _logger.LogError($"{DateTime.Now} ====>  Vehicle Not Found ");

                return null;
            }
            var CustomerId = Vehicle.CustomerId;
            return CustomerId.ToString();
        }
    }
    private async Task<CustomerPackageDto> CheckActivePackageForPlateNumber(string PlateNumber, int organizationId,DateTime ExitTransaction)
    {
        var customerPackage = await _customerPackagesAppService.GetOneActiveCustomerPackageByOrganizationIdAndPlateNumber(organizationId, PlateNumber, ExitTransaction);
        if (customerPackage == null)
        {
            _logger.LogError($"{DateTime.Now} ====>  customerPackage not found");
        }
        return customerPackage;
    }
    private async Task<GarageTransactionDto?> FindEntranceTime(string? plateNumber)
    {
        // Get the list of transactions for the provided plate number
        using (DataFilter.Disable<IMultiTenant>())
        {

            var query = await _garageTransactionRepoistory.GetQueryableAsync();

            var latestTransaction = query
                    .Where(gt => gt.PlateNumber == plateNumber && gt.CameraType == CameraType.Entry && gt.IsIn == true)
                    .OrderByDescending(gt => gt.AccurateTime) 
                    .FirstOrDefault();

            return ObjectMapper.Map<GarageTransaction,GarageTransactionDto>(latestTransaction);
        }
    }
    private async Task<string> FindEntranceImage(string? plateNumber)
    {
        // Get the list of transactions for the provided plate number
        using (DataFilter.Disable<IMultiTenant>())
        {

            var query = await _garageTransactionRepoistory.GetQueryableAsync();


            var latestTransaction = query
                    .Where(gt =>  gt.PlateNumber == plateNumber && gt.CameraType == CameraType.Entry && gt.IsIn == true)
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
                    .Where(gt =>  gt.PlateNumber == plateNumber && gt.CameraType == CameraType.Exit && gt.IsIn == true)
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
                .Where(gt =>  gt.PlateNumber == plateNumber && gt.IsIn == true)
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
    private async Task<Decimal> CalculateInvoiceCases(DateTime entrenceTime, int totalHours, CreateUpdateGarageTransactionDto garageTransaction)
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
        var invoice = await CheckoutMobileProcessWithoutSave( result.EntryGarageTransaction.AccurateTime.Value, result.ExitGarageTransaction, Customer);
        return invoice;

    }
    public async Task<Result<InvoiceDto>> CustomerCheckoutFromHisMobile(int garageTransactionId)
    {
        return await CheckoutSequence(garageTransactionId);
    }

    private async Task<Result<InvoiceDto>> CheckoutSequence(int garageTransactionId)
    {
        var result = await InitiateGarageTransaction(garageTransactionId);
        var lstCheckout = result.EntryGarageTransaction.CheckoutMobile.HasValue? result.EntryGarageTransaction.CheckoutMobile.Value:DateTime.MinValue;
        if (lstCheckout != DateTime.MinValue && lstCheckout.AddMinutes(result.EntryGarageTransaction.Organization.Graceperiod) >= DateTime.Now)
        {
            throw new Exception("Transaction Already Checked-Out");
        }
        var invoice = await CustomerPreCheckoutFromHisMobile(garageTransactionId);
        invoice.Value.InvoiceType = InvoiceType.Paid;
        await UpdateEntryTransactionCheckouFromMobile(result.EntryGarageTransaction);
        return await SaveTransactionWithInvoice(invoice, result.ExitGarageTransaction);
    }

    public async Task<Result<InvoiceDto>> CustomerCheckoutFromKeeperMobile(int garageTransactionId)
    {
        return await CheckoutSequence(garageTransactionId);
    }


    private async Task<(CreateUpdateGarageTransactionDto ExitGarageTransaction, GarageTransaction EntryGarageTransaction)> InitiateGarageTransaction(int garageTransactionId)
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
        return (ExitGarageTransaction :exitGarageTransaction, EntryGarageTransaction :entryGarageTransaction);
    }


    private async Task UpdateEntryTransactionCheckouFromMobile(GarageTransaction entryGarageTransaction)
    {
        entryGarageTransaction.CheckoutMobile = DateTime.Now;
        entryGarageTransaction.IsPending = true;
        entryGarageTransaction.CanCheckout = false;
        await _garageTransactionRepoistory.UpdateAsync(entryGarageTransaction,true);

    }

    public async Task<PaymentCheckoutDto> PayInvoiceFromWeb(InvoiceDto invoice)
    {
        var paymentChekout = new PaymentCheckoutRequestDto
        {
            OrganizationId = invoice.OrganizationId,
            PaymentCategory = invoice.IsPackage ? PaymentCategory.Package : PaymentCategory.PaymentParking,
            InvoiceId = invoice.Id,
            PaymentMethod = PaymentMethod.Cash,
            Amount = Math.Round((invoice.InvoicePrice + (invoice.InvoicePrice * 0.15m)), 2),
            CustomerId = invoice.CustomerId
        };
        return await _paymentAppService.CreateCheckoutInvoicePaymentWebAsync(paymentChekout);
    }

    public async Task<Result<CreateUpdateInvoiceDto>> CustomerPreCheckoutFromKeeperMobile(int garageTransactionId)
    {
        var result = await InitiateGarageTransaction(garageTransactionId);
        var org = await GetOrganization(result.ExitGarageTransaction.OrganizationID);
        int totalHours = _calculationsEndPointAppService.CalculateTime(result.EntryGarageTransaction.AccurateTime.Value, (DateTime)result.ExitGarageTransaction.AccurateTime, org.FreeMinutes);
        var customer = await CheckVehicle(result.ExitGarageTransaction.PlateNumber);
        if (customer == null) {

            decimal price = await CalculateInvoiceCases(result.EntryGarageTransaction.AccurateTime.Value, totalHours, result.ExitGarageTransaction);
            return await GenrateInvoiceNumberAndInitiateInvoice(result.EntryGarageTransaction.AccurateTime.Value, null, result.ExitGarageTransaction, totalHours, price, false);

        }
        CustomerPackageDto? packageDto = await GetCustomerWithItsFirstActivePackage(result.ExitGarageTransaction, customer);
        Customer exsitCustomer = await GetCustomer(customer);

        return await CalculateStayingHoursForCustomer(result.EntryGarageTransaction.AccurateTime.Value, result.ExitGarageTransaction,exsitCustomer,org,totalHours,packageDto);
    }
}


