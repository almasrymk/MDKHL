using Abp.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using SmartParking.CalculationsEndpoint;
using SmartParking.Cameras;
using SmartParking.CustomerPackages;
using SmartParking.Entities.Cameras;
using SmartParking.Entities.Customers;
using SmartParking.Entities.GarageTransactions;
using SmartParking.Entities.Lookups;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Vehicles;
using SmartParking.Enums;
using SmartParking.Garages;
using SmartParking.GarageTransactions;
using SmartParking.Invoices;
using SmartParking.Localization;
using SmartParking.Payments;
using SmartParking.Results;
using SmartParking.Utilities;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;

namespace SmartParking.GarageTransactionEndPoints;

//[Authorize]
[IgnoreAntiforgeryToken]
public class GarageTransactionEndPointsAppService : SmartParkingAppService, IGarageTransactionEndPointsAppService 
{
    private readonly IGarageTransactionsAppService _garageTransactionsAppService;
    private readonly IUnLinkedTransactionAppService _unlinkedGarageTransactionsAppService;
    private readonly IGarageTransactionUtilityAppService _garageTransactionUtilityAppService;
    private readonly ILogger<GarageTransactionEndPointsAppService> _logger;
    private readonly IRepository<Customer, Guid> _customerRepoistory;
    private readonly IRepository<Garage, int> _garageRepository;
    private readonly ICustomerPackagesAppService _customerPackagesAppService;
    private readonly IPaymentAppService _paymentAppService;
    private readonly IRepository<Organization, int> _organizationRepoistory;
    private readonly IInvoiceAppService _invoiceAppService;
    private readonly ICalculationsEndPointsAppService _calculationsEndPointAppService;
    private IDataFilter _dataFilter;
    private readonly ICurrentUser _currentUser; // Inject ICurrentUser
    private readonly IStringLocalizer<SmartParkingResource> _localizer;
    private readonly IRepository<UnLinkedTransaction> _unLinkedTransactionRepository;
    private string customerEnName { get; set; } = "unknown";
    private string customerArName { get; set; } = "غير معرف";


    public GarageTransactionEndPointsAppService(
        IGarageTransactionsAppService garageTransactionsAppService,
        ILogger<GarageTransactionEndPointsAppService> logger,
        IRepository<Customer, Guid> customerRepoistory,
        ICustomerPackagesAppService customerPackagesAppService,
        IRepository<Organization, int> organizationRepoistory,
        IInvoiceAppService invoiceAppService,
        IDataFilter dataFilter, ICurrentUser currentUser,
       IStringLocalizer<SmartParkingResource> localizer,
       IRepository<UnLinkedTransaction> unLinkedTransactionRepository,
       IPaymentAppService paymentAppService,
       ICalculationsEndPointsAppService calculationsEndPointAppService,
       IUnLinkedTransactionAppService unlinkedGarageTransactionsAppService,
       IGarageTransactionUtilityAppService garageTransactionUtilityAppService, 
       IRepository<Garage, int> garageRepository)
    {
        _garageTransactionsAppService = garageTransactionsAppService;
        _logger = logger;
        _customerRepoistory = customerRepoistory;
        _customerPackagesAppService = customerPackagesAppService;
        _organizationRepoistory = organizationRepoistory;
        _invoiceAppService = invoiceAppService;
        _dataFilter = dataFilter;
        _currentUser = currentUser;
        _localizer = localizer;
        _unLinkedTransactionRepository = unLinkedTransactionRepository;
        _paymentAppService = paymentAppService;
        _calculationsEndPointAppService = calculationsEndPointAppService;
        _unlinkedGarageTransactionsAppService = unlinkedGarageTransactionsAppService;
        _garageTransactionUtilityAppService = garageTransactionUtilityAppService;
        _garageRepository = garageRepository;
    }
    #region Web 
    public async Task<bool> CreateAsync(CreateUpdateGarageTransactionEndPointDto input)
    {

        _logger.LogError($"{DateTime.Now} ====>  {JsonSerializer.Serialize(new { input.PlateNumber, input.AccurateTime, input.DeviceID, input.VehiclePictureName })}");
        if (string.IsNullOrEmpty(input.PlateNumber)) {
            return await NotFoundEntityAndExceptionHandling(input, Message: input.PlateNumber);
        }
        var camera = await _garageTransactionUtilityAppService.GetCameraByDeviceIdAsync(input.DeviceID);
        
        if (camera == null || camera.Status==Status.Inactive)
        {
            return await NotFoundEntityAndExceptionHandling(input, Message:input.DeviceID);
        }
        var organizationId = await GetOrganizationId(camera.GarageId);
        if (organizationId == -1 )
        {
            return await NotFoundEntityAndExceptionHandling(input, Message:"Organization");
        }
        var cameraId = camera.Id;
        var garageId = camera.GarageId;

        input.PlateNumber = input.PlateNumber.Replace(" ", "");
        var result = await CheckIfOrganizationHasFreeSots(organizationId);
        CreateUpdateGarageTransactionDto GarageTransaction = GenerateCreateGarageTransactionDto(input, camera);
        try
        {
            switch (GarageTransaction.CameraType)
            {
                case CameraType.Exit:
                    
                    if (result.HasFreeSlots)
                    {
                        return false;
                    }
                  
                    return await HandleCamersExit(GarageTransaction);

                case CameraType.Entry:
                    GarageTransaction.CustomerCategory = await GetTransactionCategory(GarageTransaction);
                    var customerPackageDto = await CheckActivePackageForPlateNumber(GarageTransaction.PlateNumber, GarageTransaction.OrganizationID, GarageTransaction.AccurateTime.Value);
                    bool _hasActivePackage = customerPackageDto is not null;
                    bool canEnter = await CheckIfCanEnter(garageId, GarageTransaction.CustomerCategory);
                    bool finalResult =  (result.HasPackageOnly, result.HasFreeSlots, canEnter, _hasActivePackage) switch
                    {
                        (true , _  , _ , false) => false , 
                        (true , true , _ , true ) => true  , 
                        (true , false , true , true ) => true ,
                        (true, false, false, true) => false,
                        (false, true , _, _) => true,
                        (false, false , true , _) => true,
                        (false, false , false , _) => false,
                    };
                    if (finalResult)
                    {

                        await _garageTransactionsAppService.CreateAsync(GarageTransaction);
                        await CurrentUnitOfWork.SaveChangesAsync();


                    }
                    else
                    {
                        await CreateUnLinkedTransaction(input);
                    }

                    _logger.LogInformation($"{DateTime.Now} ====> Get Response {finalResult}");

                    return finalResult;
            };
        }
        catch (Exception ex)
        {
            return await NotFoundEntityAndExceptionHandling(input, ex.Message);
        }
        await CreateUnLinkedTransaction(input);
        return await Task.FromResult(false);

    }
    private async Task<( bool HasFreeSlots , bool HasPackageOnly)> CheckIfOrganizationHasFreeSots(int OrganizationId)
    {
        var organization = await GetOrganization(OrganizationId);
        return (organization.HasFreeSlots, organization.HasPackageOnly) ;

    }
    private async Task<CustomerCategoryStatus> GetTransactionCategory(CreateUpdateGarageTransactionDto GarageTransaction)
    {

        var packg = await CheckActivePackageForPlateNumber(GarageTransaction.PlateNumber, GarageTransaction.OrganizationID, GarageTransaction.AccurateTime.Value);
        
        return packg is not null ? packg.CustomerCategory : CustomerCategoryStatus.Customer;
    }
    private async Task<bool> NotFoundEntityAndExceptionHandling(CreateUpdateGarageTransactionEndPointDto input,string Message)
    {
        await CreateUnLinkedTransaction(input);
        _logger.LogError($"{DateTime.Now} ====>  {Message} is not found , Saved in Unlinked Transaction");
        _logger.LogError($"{DateTime.Now} ====> Get Response false");

        return await Task.FromResult(false);
    }
    private CreateUpdateGarageTransactionDto GenerateCreateGarageTransactionDto(CreateUpdateGarageTransactionEndPointDto input, CameraDto camera)
    {
        return new CreateUpdateGarageTransactionDto
        {
            AccurateTime = input.AccurateTime,
            SnapTime = input.SnapTime,
            VehiclePicture = input.VehiclePicture,
            PlateNumber = input.PlateNumber,
            CameraId = camera.Id,
            GarageId = camera.GarageId,
            OrganizationID = camera.Garage.OrganizationId,
            TenantId = camera.TenantId,
            CameraType = camera.CameraType,
            IsIn = camera.CameraType == CameraType.Exit ? false : true,
            IsMapped = camera.CameraType == CameraType.Entry ? false : true,

        };
    }
    private async Task<Garage> GetGarage(int id)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            var garage = await _garageRepository.FirstOrDefaultAsync(x => x.Id == id && x.Status == Status.Active);
            return garage;
        }

    }
    private async Task<bool> CheckIfCanEnter(int garageId, CustomerCategoryStatus customer)
    {
       var result = await _garageTransactionUtilityAppService.ReserverSlotsOfGarageDependsOnCategoryAsync(garageId, customer);
       return result;
        
    }
    #endregion
    private async Task CreateUnLinkedTransaction(CreateUpdateGarageTransactionEndPointDto input)
    {
        await _unlinkedGarageTransactionsAppService.CreateAsync(input);
    }
    private async Task<CreateUpdateInvoiceDto> CalculateInvoiceForCustomerWithoutPackage(DateTime EntranceTime, int totalHours, Customer exsitCustomer, CreateUpdateGarageTransactionDto GarageTransaction,CustomerCategoryStatus customerCategoryStatus)
    {
        decimal price = await CalculateInvoiceCases(EntranceTime, totalHours, GarageTransaction);
        CreateUpdateInvoiceDto invoice = await GenrateInvoiceNumberAndInitiateInvoice(EntranceTime, exsitCustomer, GarageTransaction, totalHours, price, false);
        return invoice;

    }
    private async Task<CreateUpdateInvoiceDto> CalculateInvoiceForCustomerPackage(DateTime EntranceTime, Organization organization, Customer exsitCustomer, CustomerPackageDto packageDto, CreateUpdateGarageTransactionDto GarageTransaction,int TotalHours,CustomerCategoryStatus customerCategory)
    {

        decimal price;

        if (packageDto.IsCustomPackage) {
            var totaldays = _calculationsEndPointAppService.CalculateTimeForCustomPackage(GarageTransaction.AccurateTime, packageDto);
            price = totaldays * organization.FullDayPrice; ;
        }
        else
        {
            var THoursOutPackage = _calculationsEndPointAppService.CalculateTimeDependsOnPackage(EntranceTime, GarageTransaction.AccurateTime, packageDto);
            price  = THoursOutPackage * organization.AdditionalHourPrice;
        }



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
          Math.Round(price,2),
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
    private async Task<CreateUpdateInvoiceDto> CalculateStayingHoursForCustomer(DateTime EntranceTime, CreateUpdateGarageTransactionDto GarageTransaction, Customer customer, Organization organization, int totalHours, CustomerPackageDto packageDto)
    {
        return (packageDto, packageDto?.Package , packageDto?.IsCustomPackage) switch
        {
            (null, _ , _ ) or (not null, null , false ) => await CalculateInvoiceForCustomerWithoutPackage(EntranceTime, totalHours, customer, GarageTransaction, CustomerCategoryStatus.Customer),
            (not null, not null,_ ) or (not null, null, true) => await CalculateInvoiceForCustomerPackage(EntranceTime, organization, customer, packageDto, GarageTransaction, totalHours, packageDto.CustomerCategory),

        };
    }
    private async Task<bool> IfLoggedOutFromMobile(string plateNumber)
    {
        var transaction = await _garageTransactionUtilityAppService.CheckIfCheckoutFromMobile(plateNumber);
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
    private async Task<bool> CheckForCategoriesInTransactionsHasBeenChanged(CreateUpdateGarageTransactionDto ExitTransaction, GarageTransactionDto EntryTransaction)
    {
        if(EntryTransaction.CustomerCategory != ExitTransaction.CustomerCategory)
        {
            EntryTransaction.CustomerCategory = ExitTransaction.CustomerCategory;
            await ChangeCategoryForEntryTransaction(EntryTransaction);
            return true;
        }
        return false;

    }
    private async Task<bool> HandleCamersExit(CreateUpdateGarageTransactionDto garageTransaction)
    {
        bool flag = false;
        var EntryTransaction = await _garageTransactionUtilityAppService.FindEntranceTime(garageTransaction.PlateNumber);
        var EntranceTime = EntryTransaction?.AccurateTime;
        var customerId = await _garageTransactionUtilityAppService.CheckPlateNumberBelongsToCustomerToGetHisIdAsync(garageTransaction.PlateNumber);
        CustomerPackageDto? packageDto = await GetCustomerWithItsFirstActivePackage(garageTransaction, EntranceTime.Value, customerId);
        garageTransaction.CustomerCategory = packageDto is null ? CustomerCategoryStatus.Customer : packageDto.CustomerCategory;

        if (EntranceTime is null || EntranceTime > garageTransaction.AccurateTime)
        {
            await _garageTransactionUtilityAppService.FreeSlotsOfGarageDependsOnCategoryAsync(garageTransaction.GarageId, CustomerCategoryStatus.Customer);
            await InVailedCasesForTransacrion(garageTransaction, EntranceTime);
            await CurrentUnitOfWork.SaveChangesAsync();
            return await Task.FromResult(false);
        }
        var lstCategory = EntryTransaction.CustomerCategory;

        var _categoryHasChanged = await CheckForCategoriesInTransactionsHasBeenChanged(garageTransaction, EntryTransaction);

        var organization = await GetOrganization(garageTransaction.OrganizationID);
      
        Customer exsitCustomer = customerId is not null ? await GetCustomer(customerId) : null;

        var CheckIfLoggedOutFromMobile = await IfLoggedOutFromMobile(garageTransaction.PlateNumber);
        if (CheckIfLoggedOutFromMobile)
        {
            await CheckForCategoryChangement(EntryTransaction, garageTransaction.GarageId , _categoryHasChanged, lstCategory);
            return await CheckGracePeriodScenarios(garageTransaction, EntranceTime, organization.Graceperiod, customerId, exsitCustomer);
        }

        int totalHours = _calculationsEndPointAppService.CalculateTime(EntranceTime.Value, garageTransaction.AccurateTime.Value, organization.FreeMinutes);

        if (totalHours*60 <= organization.FreeMinutes && organization.FreeMinutes!=0)
        {
            return await ExitsInFreeMinutesPeriod(garageTransaction);
        }
        switch (customerId)
        {
            case not null:
                flag = await CalculationForRegisteredCustomer(garageTransaction, EntranceTime, organization, packageDto, exsitCustomer, totalHours);
                await CheckForCategoryChangement(EntryTransaction, garageTransaction.GarageId, _categoryHasChanged, lstCategory);
                break;
            case null:
                flag=await CalculationForUnRegisteredCustomer(garageTransaction, EntranceTime, garageTransaction.GarageId, exsitCustomer, totalHours);
                break;
        };
        await _garageTransactionUtilityAppService.CloseEntryTransaction(garageTransaction.PlateNumber);
        await CurrentUnitOfWork.SaveChangesAsync();
        _logger.LogInformation($"{DateTime.Now} ====> Get Response {flag}");

        return flag;
    }

    private async Task CheckForCategoryChangement(GarageTransactionDto? EntryTransacrion, int garageId, bool _categoryHasChanged,CustomerCategoryStatus lstCategory)
    {
        if (_categoryHasChanged )
        {
            await _garageTransactionUtilityAppService.FreeSlotsOfGarageDependsOnCategoryAsync(garageId, lstCategory);
        }
        else
        {
            await _garageTransactionUtilityAppService.FreeSlotsOfGarageDependsOnCategoryAsync(garageId, EntryTransacrion.CustomerCategory);
        }
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    private async Task<bool> CalculationForUnRegisteredCustomer(CreateUpdateGarageTransactionDto GarageTransaction, DateTime? EntranceTime, int garageId, Customer exsitCustomer, int totalHours)
    {
        var newInvoice = await CalculateInvoiceForCustomerWithoutPackage(EntranceTime.Value, totalHours, exsitCustomer, GarageTransaction, CustomerCategoryStatus.Customer);
        await SaveTransactionWithInvoice(newInvoice, GarageTransaction);
        await _garageTransactionUtilityAppService.FreeSlotsOfGarageDependsOnCategoryAsync(garageId, CustomerCategoryStatus.Customer);
        await CurrentUnitOfWork.SaveChangesAsync();
        return newInvoice.InvoicePrice > 0 ? false : true;
    }

    private async Task<bool> CalculationForRegisteredCustomer(CreateUpdateGarageTransactionDto GarageTransaction, DateTime? EntranceTime, Organization organization, CustomerPackageDto? packageDto, Customer exsitCustomer, int totalHours)
    {
        var invoiceCreateUpdateDto = await CalculateStayingHoursForCustomer(EntranceTime.Value, GarageTransaction, exsitCustomer, organization, totalHours, packageDto);
        var resultedInvoice = await SaveTransactionWithInvoice(invoiceCreateUpdateDto, GarageTransaction);
        return (resultedInvoice.InvoicePrice > 0) ? false : true;
    }

    private async Task<bool> ExitsInFreeMinutesPeriod(CreateUpdateGarageTransactionDto GarageTransaction)
    {
        var garageTransactionDto = await _garageTransactionsAppService.CreateAsync(GarageTransaction);
        _logger.LogInformation($"{DateTime.Now} ====> Close Entry : {GarageTransaction.PlateNumber}");
        await _garageTransactionUtilityAppService.CloseEntryTransaction(GarageTransaction.PlateNumber);
        _logger.LogInformation($"{DateTime.Now} ====> PlateNumber  : {GarageTransaction.PlateNumber}");
        _logger.LogInformation($"{DateTime.Now} ====> Get Response true | In Free minutes period");
        return await Task.FromResult(true);
    }

    private async Task<bool> CheckGracePeriodScenarios(CreateUpdateGarageTransactionDto garageTransaction, DateTime? EntranceTime,int graceMinutes, string customerId,Customer exsitCustomer)
    {
        var invoice = await _invoiceAppService.GetLatestInvoiceForCustomerInOrganizationGarage(garageTransaction.PlateNumber, garageTransaction.OrganizationID, garageTransaction.GarageId);
        await _garageTransactionUtilityAppService.CloseCheckoutMobileTransaction(garageTransaction.PlateNumber);
        _logger.LogInformation($"{DateTime.Now} ====> Customer ID : {customerId}");
        _logger.LogInformation($"{DateTime.Now} ====> PlateNumber  : {garageTransaction.PlateNumber}");
        var PassGraceTime = await CheckIfPassGraceTime(garageTransaction.AccurateTime.Value, invoice.Value.InvoiceTime, graceMinutes);
        return PassGraceTime switch
        {
            (false)=> await DoesnotPassGracePeriod(garageTransaction),
            (true)=> await PassedGracePeriod(garageTransaction, EntranceTime, exsitCustomer, invoice),
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
            _logger.LogError($"{DateTime.Now} ====>  Entrance Time is greater exit time  ");
            _logger.LogError($"{DateTime.Now} ====> Get Response false");
        }
    }

    private async Task<CustomerPackageDto> GetCustomerWithItsFirstActivePackage(CreateUpdateGarageTransactionDto GarageTransaction,DateTime entryTransaction ,string? customer)
    {
        CustomerPackageDto package = null;
        if (customer != null)
        {
            package = await CheckActivePackageForPlateNumber(GarageTransaction.PlateNumber, GarageTransaction.OrganizationID,entryTransaction);
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
        var entryTransactionImage = await _garageTransactionUtilityAppService.FindEntranceImage(PlateNumber);
        var invoice = new CreateUpdateInvoiceDto
        {
            GarageId = GarageId,
            InvoiceNumber = GeneratedInvoiceNumber,
            InvoicePrice = Price,
            InvoiceType = Price >0?InvoiceType.UnPaid:InvoiceType.Paid,
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
            EntryTransactionImage = entryTransactionImage,

        };

        return invoice;

    }
    
    private async Task<CameraDto> GetAnExitCameraByGarageId(int GarageId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            return await _garageTransactionUtilityAppService.GetCameraByTypeBelongsToGarageAsync(GarageId , CameraType.Exit);

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
        var _garage= await GetGarage(GarageId);
        return _garage is not null ? _garage.OrganizationId:-1;

        
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

    #region Customer Mobile Region
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
    public async Task<Result<CreateUpdateInvoiceDto>> CustomerPreCheckoutFromHisMobile(int garageTransactionId)
    {
        var customerID = await GetCurrentCustomerId();
        var Customer = await GetCustomer(customerID.ToString());
        var entryGarageTransaction = await GetTransactionById(garageTransactionId);
        var camera = await GetAnExitCameraByGarageId(entryGarageTransaction.GarageId);
        var createdExitGarageTransaction= await InitiateGarageTransaction(entryGarageTransaction,camera.Id);
        var invoice = await CheckoutMobileProcessWithoutSave(entryGarageTransaction.AccurateTime.Value, createdExitGarageTransaction, Customer);
        return invoice;

    }
    public async Task<Result<InvoiceDto>> CustomerCheckoutFromHisMobile(int garageTransactionId)
    {
        var entryGarageTransaction = await GetTransactionById(garageTransactionId);
        var camera = await GetAnExitCameraByGarageId(entryGarageTransaction.GarageId);
        var createdExitGarageTransaction = await InitiateGarageTransaction(entryGarageTransaction,camera.Id);
        var lstCheckout = entryGarageTransaction.CheckoutMobile.HasValue ? entryGarageTransaction.CheckoutMobile.Value : DateTime.MinValue;
        var _garageTransactionDto = ObjectMapper.Map<GarageTransaction, GarageTransactionDto>(entryGarageTransaction);
        if (!(_garageTransactionDto.IsIn && _garageTransactionDto.IsPending == false && _garageTransactionDto.CanCheckout == true))
        {
            throw new Exception("Transaction Already Checked-Out");
        }
        var invoice = await CustomerPreCheckoutFromHisMobile(garageTransactionId);
        await _garageTransactionUtilityAppService.UpdateEntryTransactionCheckouFromMobile(entryGarageTransaction);
        return await SaveTransactionWithInvoice(invoice, createdExitGarageTransaction);
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
    #endregion
    private async Task<CreateUpdateGarageTransactionDto> InitiateGarageTransaction(GarageTransaction entryGarageTransaction,int cameraId)
    {
        
        var  exitGarageTransaction = new CreateUpdateGarageTransactionDto
        {
            AccurateTime = DateTime.Now,
            PlateNumber = entryGarageTransaction.PlateNumber,
            GarageId = entryGarageTransaction.GarageId,
            OrganizationID = entryGarageTransaction.OrganizationID,
            CameraId = cameraId,
            SnapTime = DateTime.Now,
            TenantId = entryGarageTransaction.TenantId,
            IsIn = true,
            CheckoutMobile = DateTime.Now,
            VehiclePicture = entryGarageTransaction.VehiclePicture,
            CameraType = CameraType.Exit
       };
        return (exitGarageTransaction);
    }

    #region Keeper Mobile Region
    public async Task<Result<InvoiceDto>> CustomerCheckoutFromKeeperMobile(int garageTransactionId)
    {
        var entryGarageTransaction = await GetTransactionById(garageTransactionId);
        var camera = await GetAnExitCameraByGarageId(entryGarageTransaction.GarageId);
        var exitTransaction = await InitiateGarageTransaction(entryGarageTransaction,camera.Id);
        var lstCheckout = entryGarageTransaction.CheckoutMobile.HasValue ? entryGarageTransaction.CheckoutMobile.Value : DateTime.MinValue;
        var _garageTransactionDto = ObjectMapper.Map<GarageTransaction, GarageTransactionDto>(entryGarageTransaction);
        if (!(_garageTransactionDto.IsIn && _garageTransactionDto.IsPending == false && _garageTransactionDto.CanCheckout == true))
        {
            throw new Exception("Transaction Already Checked-Out");
        }
        var invoice = await CustomerPreCheckoutFromKeeperMobile(garageTransactionId);
        await _garageTransactionUtilityAppService.UpdateEntryTransactionCheckouFromMobile(entryGarageTransaction);
        return await SaveTransactionWithInvoice(invoice, exitTransaction);

    }
    public async Task<Result<CreateUpdateInvoiceDto>> CustomerPreCheckoutFromKeeperMobile(int garageTransactionId)
    {
        var entryGarageTransaction = await GetTransactionById(garageTransactionId);
        var camera = await GetAnExitCameraByGarageId(entryGarageTransaction.GarageId);
        var createdExitGarageTransaction = await InitiateGarageTransaction(entryGarageTransaction,camera.Id);
        var org = await GetOrganization(createdExitGarageTransaction.OrganizationID);
        int totalHours = _calculationsEndPointAppService.CalculateTime(entryGarageTransaction.AccurateTime.Value, (DateTime)createdExitGarageTransaction.AccurateTime, org.FreeMinutes);
        var customerId = await _garageTransactionUtilityAppService.CheckPlateNumberBelongsToCustomerToGetHisIdAsync(createdExitGarageTransaction.PlateNumber);
        if (customerId == null) {

            decimal price = await CalculateInvoiceCases(entryGarageTransaction.AccurateTime.Value, totalHours, createdExitGarageTransaction);
            return await GenrateInvoiceNumberAndInitiateInvoice(entryGarageTransaction.AccurateTime.Value, null, createdExitGarageTransaction, totalHours, price, false);

        }
        CustomerPackageDto? packageDto = await GetCustomerWithItsFirstActivePackage(createdExitGarageTransaction, entryGarageTransaction.AccurateTime.Value, customerId);
        Customer exsitCustomer = await GetCustomer(customerId);

        return await CalculateStayingHoursForCustomer(entryGarageTransaction.AccurateTime.Value, createdExitGarageTransaction, exsitCustomer,org,totalHours,packageDto);
    }
    #endregion
    private async Task<GarageTransaction> GetTransactionById(int garagetransactionId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var transaction = await _garageTransactionUtilityAppService.GetTransactionById(garagetransactionId);
            if (transaction != null)
            {
                if (transaction.CheckoutMobile.HasValue)
                {
                    transaction.RemainingTime = CalculateRemainingTimeWithGrace(transaction.CheckoutMobile, transaction.Organization.Graceperiod);
                }
                return transaction;
            }
            throw new EntityNotFoundException($"Transaction with id {transaction} is not found");
        }
    }
    private DateTime CalculateRemainingTimeWithGrace(DateTime? checkoutMobile, int gracePeriod)
    {
        if (IfPassGracePeriod(checkoutMobile, gracePeriod))
        {
            return DateTime.MinValue;
        }
        return CalculateRemainingTime(checkoutMobile, gracePeriod);
    }
    private bool IfPassGracePeriod(DateTime? CheckoutMobile, int Graceperiod)
    {
        return (CheckoutMobile.Value.AddMinutes(Graceperiod) - DateTime.Now.AddHours(1)).Minutes < 0 ? true : false;
    }
    private DateTime CalculateRemainingTime(DateTime? MobileCheckout, int Graceperiod)
    {
        return DateTime.Now.Add(MobileCheckout.Value.AddMinutes(Graceperiod * 1.0) - DateTime.Now);
    }
}


