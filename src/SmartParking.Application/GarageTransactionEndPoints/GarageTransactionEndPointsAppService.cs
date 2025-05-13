using Castle.Core.Resource;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartParking.CalculationsEndpoint;
using SmartParking.CustomerPackages;
using SmartParking.Entities.Cameras;
using SmartParking.Entities.Customers;
using SmartParking.Entities.GarageTransactions;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using SmartParking.GarageTransactions;
using SmartParking.Invoices;
using SmartParking.Results;
using SmartParking.Utilities;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp.MultiTenancy;

namespace SmartParking.GarageTransactionEndPoints;

//[Authorize]
[IgnoreAntiforgeryToken]
public class GarageTransactionEndPointsAppService : SmartParkingAppService, IGarageTransactionEndPointsAppService 
{
    private readonly IGarageTransactionsAppService _garageTransactionsAppService;
    private readonly IUnLinkedTransactionAppService _unlinkedGarageTransactionsAppService;
    private readonly IGarageTransactionUtilityAppService _garageTransactionUtilityAppService;
    private readonly ILogger<GarageTransactionEndPointsAppService> _logger;
    private readonly ICustomerPackagesAppService _customerPackagesAppService;
    private readonly IInvoiceAppService _invoiceAppService;
    private readonly ICalculationsEndPointsAppService _calculationsEndPointAppService;
    private string customerEnName { get; set; } = "unknown";
    private string customerArName { get; set; } = "غير معرف";
    public GarageTransactionEndPointsAppService(
        IGarageTransactionsAppService garageTransactionsAppService,
        ILogger<GarageTransactionEndPointsAppService> logger,
        ICustomerPackagesAppService customerPackagesAppService,
        IInvoiceAppService invoiceAppService,
       ICalculationsEndPointsAppService calculationsEndPointAppService,
       IUnLinkedTransactionAppService unlinkedGarageTransactionsAppService,
       IGarageTransactionUtilityAppService garageTransactionUtilityAppService)
    {
        _garageTransactionsAppService = garageTransactionsAppService;
        _logger = logger;
        _customerPackagesAppService = customerPackagesAppService;
        _invoiceAppService = invoiceAppService;
        _calculationsEndPointAppService = calculationsEndPointAppService;
        _unlinkedGarageTransactionsAppService = unlinkedGarageTransactionsAppService;
        _garageTransactionUtilityAppService = garageTransactionUtilityAppService;
    }

    #region public methods web methods
    public async Task<bool> CreateAsync(CreateUpdateGarageTransactionEndPointDto input)
    {

        _logger.LogError($"{DateTime.Now} ====>  {JsonSerializer.Serialize(new { input.PlateNumber, input.AccurateTime, input.DeviceID, input.VehiclePictureName })}");
        if (string.IsNullOrEmpty(input.PlateNumber)) {
            return await NotFoundEntityAndExceptionHandling(input, Message: input.PlateNumber);
        }
        //Pre-processing for transaction.
        var camera = await _garageTransactionUtilityAppService.GetCameraByDeviceIdAsync(input.DeviceID);
        if (camera == null || camera.Status==Status.Inactive)
        {
            return await NotFoundEntityAndExceptionHandling(input, Message:input.DeviceID);
        }
        if (camera.GarageId ==0 || camera.OrganizationId == 0)
        {
            return await NotFoundEntityAndExceptionHandling(input, Message:"Organization");
        }
        //Remove white spaces from plate number.
        input.PlateNumber = input.PlateNumber.Replace(" ", "");
        //Load Organization Configurations.
        var organization = await _garageTransactionUtilityAppService.GetOrganization(camera.OrganizationId);
        var (_hasFreeSlots, _hasPackageOnly)=(organization.HasFreeSlots, organization.HasPackageOnly) ;
        //Initiate a CreateUpdateGarageTransaction .
        CreateUpdateGarageTransactionDto GarageTransaction = InitiateCreateGarageTransactionDto(input, camera);
        //Detect Camera Type Sends Request.
        try
        {
            switch (GarageTransaction.CameraType)
            {
                case CameraType.Exit:
                    //Returns Flase if Organization Configuration Has No Exit Camera
                    if (_hasFreeSlots)
                        return false;
                    return await HandleCameraExit(GarageTransaction,organization);

                case CameraType.Entry:
                    var customerPackageDto = await _customerPackagesAppService.GetOneActiveCustomerPackageByOrganizationIdAndPlateNumber( GarageTransaction.OrganizationID, GarageTransaction.PlateNumber, GarageTransaction.AccurateTime.Value);
                    GarageTransaction.CustomerCategory = customerPackageDto is not null ? customerPackageDto.CustomerCategory : CustomerCategoryStatus.Customer;
                    bool _hasActivePackage = customerPackageDto is not null;
                    bool canEnter =  await _garageTransactionUtilityAppService.ReserverSlotsOfGarageDependsOnCategoryAsync(GarageTransaction.GarageId, GarageTransaction.CustomerCategory);
                    bool finalResult =  (_hasPackageOnly, _hasFreeSlots, canEnter, _hasActivePackage) switch
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
                        await _unlinkedGarageTransactionsAppService.CreateAsync(input);
                    }

                    _logger.LogInformation($"{DateTime.Now} ====> Get Response {finalResult}");
                    return finalResult;
            };
        }
        catch (Exception ex)
        {
            return await NotFoundEntityAndExceptionHandling(input, ex.Message);
        }
        await _unlinkedGarageTransactionsAppService.CreateAsync(input);
        return await Task.FromResult(false);
    }

    #endregion
    #region public methods customer Mobile Region
    public async Task<Result<CreateUpdateInvoiceDto>> CustomerPreCheckoutFromHisMobile(int garageTransactionId)
    {
        var customerId =await _garageTransactionUtilityAppService.GetCurrentCustomerId(); 
        var Customer = await _garageTransactionUtilityAppService.GetCustomer(customerId);
        var entryGarageTransaction = await _garageTransactionUtilityAppService.GetTransactionById(garageTransactionId);
        var camera = await _garageTransactionUtilityAppService.GetCameraByTypeBelongsToGarageAsync(entryGarageTransaction.GarageId,CameraType.Exit);
        var createdExitGarageTransaction= await InitiateGarageTransaction(entryGarageTransaction,camera.Id);
        var invoice = await PreCheckoutFlow(entryGarageTransaction.AccurateTime.Value, createdExitGarageTransaction, Customer);
        return invoice;

    }
    public async Task<Result<InvoiceDto>> CustomerCheckoutFromHisMobile(int garageTransactionId)
    {
        var entryGarageTransaction = await _garageTransactionUtilityAppService.GetTransactionById(garageTransactionId);
        var camera = await _garageTransactionUtilityAppService.GetCameraByTypeBelongsToGarageAsync(entryGarageTransaction.GarageId, CameraType.Exit);
        var createdExitGarageTransaction = await InitiateGarageTransaction(entryGarageTransaction,camera.Id);
        var lstCheckout = entryGarageTransaction.CheckoutMobile.HasValue ? entryGarageTransaction.CheckoutMobile.Value : DateTime.MinValue;
        var _garageTransactionDto = ObjectMapper.Map<GarageTransaction, GarageTransactionDto>(entryGarageTransaction);
        if (!(_garageTransactionDto.IsIn && _garageTransactionDto.IsPending == false && _garageTransactionDto.CanCheckout == true))
        {
            throw new Exception("Transaction Already Checked-Out");
        }
        var invoice = await CustomerPreCheckoutFromHisMobile(garageTransactionId);
        await _garageTransactionUtilityAppService.UpdateEntryTransactionCheckouFromMobile(entryGarageTransaction);
        var savedExitgarageTransaction = await _garageTransactionsAppService.CreateAsync(createdExitGarageTransaction);
        invoice.Value.GarageTransactionExitId = savedExitgarageTransaction.Id;
        return await _invoiceAppService.CreateAsync(invoice); ;
    }

    public async Task<Result<InvoiceDto>> CustomerCheckoutFromKeeperMobile(int garageTransactionId)
    {
        var entryGarageTransaction = await _garageTransactionUtilityAppService.GetTransactionById(garageTransactionId);
        var camera = await _garageTransactionUtilityAppService.GetCameraByTypeBelongsToGarageAsync(entryGarageTransaction.GarageId, CameraType.Exit);
        var createdExitTransaction = await InitiateGarageTransaction(entryGarageTransaction, camera.Id);
        var lstCheckout = entryGarageTransaction.CheckoutMobile.HasValue ? entryGarageTransaction.CheckoutMobile.Value : DateTime.MinValue;
        var _garageTransactionDto = ObjectMapper.Map<GarageTransaction, GarageTransactionDto>(entryGarageTransaction);
        if (!(_garageTransactionDto.IsIn && _garageTransactionDto.IsPending == false && _garageTransactionDto.CanCheckout == true))
        {
            throw new Exception("Transaction Already Checked-Out");
        }
        var invoice = await CustomerPreCheckoutFromKeeperMobile(garageTransactionId);
        await _garageTransactionUtilityAppService.UpdateEntryTransactionCheckouFromMobile(entryGarageTransaction);
        var garageTransaction = await _garageTransactionsAppService.CreateAsync(createdExitTransaction);
        invoice.Value.GarageTransactionExitId = garageTransaction.Id;
        return await _invoiceAppService.CreateAsync(invoice); ;

    }
    public async Task<Result<CreateUpdateInvoiceDto>> CustomerPreCheckoutFromKeeperMobile(int garageTransactionId)
    {
        var entryGarageTransaction = await _garageTransactionUtilityAppService.GetTransactionById(garageTransactionId);
        var camera = await _garageTransactionUtilityAppService.GetCameraByTypeBelongsToGarageAsync(entryGarageTransaction.GarageId, CameraType.Exit);
        var createdExitGarageTransaction = await InitiateGarageTransaction(entryGarageTransaction,camera.Id);
        var organization = await _garageTransactionUtilityAppService.GetOrganization(createdExitGarageTransaction.OrganizationID);
        int totalHours = _calculationsEndPointAppService.CalculateTime(entryGarageTransaction.AccurateTime.Value, (DateTime)createdExitGarageTransaction.AccurateTime, organization.FreeMinutes);
        var customerId = await _garageTransactionUtilityAppService.CheckPlateNumberBelongsToCustomerToGetHisIdAsync(createdExitGarageTransaction.PlateNumber);
        if (customerId == null) {

            decimal price = await CalculateInvoiceCases(entryGarageTransaction.AccurateTime.Value, totalHours, createdExitGarageTransaction.AccurateTime.Value,organization);
            return await InitiateInvoice(entryGarageTransaction.AccurateTime.Value, null, createdExitGarageTransaction, totalHours, price, false);

        }
        var customerPackageDto = await _customerPackagesAppService.GetOneActiveCustomerPackageByOrganizationIdAndPlateNumber(createdExitGarageTransaction.OrganizationID, createdExitGarageTransaction.PlateNumber, createdExitGarageTransaction.AccurateTime.Value);
        createdExitGarageTransaction.CustomerCategory = customerPackageDto.CustomerCategory;
        Customer exsitCustomer = await _garageTransactionUtilityAppService.GetCustomer(Guid.Parse(customerId));
        var _customerInvoicePrice = await CalculateStayingHoursForCustomer(entryGarageTransaction.AccurateTime.Value, createdExitGarageTransaction.AccurateTime.Value, organization, totalHours, customerPackageDto);
        CreateUpdateInvoiceDto invoice = await InitiateInvoice(entryGarageTransaction.AccurateTime.Value, exsitCustomer, createdExitGarageTransaction, totalHours, _customerInvoicePrice, true);
        return invoice;

    }
    #endregion
    #region private method to handle category changements
    private async Task ChangeCategoryForEntryTransaction(GarageTransactionDto entryTransaction)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            await _garageTransactionsAppService.UpdateAsync(entryTransaction.Id, ObjectMapper.Map<GarageTransactionDto, CreateUpdateGarageTransactionDto>(entryTransaction));
        }
    }
    private async Task UpdateSlotsCountIfCategoryChanged(GarageTransactionDto? entryTransaction, bool categoryHasChanged, CustomerCategoryStatus lstCategory)
    {
        if (categoryHasChanged)
        {
            await _garageTransactionUtilityAppService.FreeSlotsOfGarageDependsOnCategoryAsync(entryTransaction.GarageId, lstCategory);
        }
        else
        {
            await _garageTransactionUtilityAppService.FreeSlotsOfGarageDependsOnCategoryAsync(entryTransaction.GarageId, entryTransaction.CustomerCategory);
        }
    }
    #endregion
    #region private method to handle exit
    private async Task<bool> HandleCameraExit(CreateUpdateGarageTransactionDto exitTransaction,Organization organization)
    {
        bool _requestToGate;
        //getting entrance transaction
        var _entryTransaction = await _garageTransactionUtilityAppService.FindEntranceTime(exitTransaction.PlateNumber);
        //geeting garage
        var _garage = await _garageTransactionUtilityAppService.GetGarageByIdAsync(exitTransaction.GarageId);
        if (_entryTransaction == null)
        {
            _logger.LogError($"{DateTime.Now} ====>  Entrance Time is null  ");
            _logger.LogError($"{DateTime.Now} ====> Get Response false");
            await _garageTransactionUtilityAppService.FreeSlotsOfGarageDependsOnCategoryAsync(_garage.Id, CustomerCategoryStatus.Customer);
            return await Task.FromResult(false);
        }
        //getting customer id if found
        var _customerId = await _garageTransactionUtilityAppService.CheckPlateNumberBelongsToCustomerToGetHisIdAsync(exitTransaction.PlateNumber);
        //getting package on vehicle if found
        var _packageDto = await _customerPackagesAppService.GetOneActiveCustomerPackageByOrganizationIdAndPlateNumber(exitTransaction.OrganizationID, exitTransaction.PlateNumber, exitTransaction.AccurateTime.Value);
        exitTransaction.CustomerCategory = _packageDto is null ? CustomerCategoryStatus.Customer : _packageDto.CustomerCategory;
        //Check On Entrance Time Validation
        if ( _entryTransaction.AccurateTime > exitTransaction.AccurateTime )
        {
            await _garageTransactionUtilityAppService.FreeSlotsOfGarageDependsOnCategoryAsync(_garage.Id, CustomerCategoryStatus.Customer);
            await InVailedCasesForTransaction(exitTransaction, _entryTransaction.AccurateTime);
            return await Task.FromResult(false);
        }
        var _lstCategory = _entryTransaction.CustomerCategory;
        // Check if Entry Garage transaction has changed.
        bool _categoryHasChanged=false;
        if (_entryTransaction.CustomerCategory != exitTransaction.CustomerCategory)
        {
            _categoryHasChanged = true;
            _entryTransaction.CustomerCategory = exitTransaction.CustomerCategory;
            await ChangeCategoryForEntryTransaction(_entryTransaction);
        }

        Customer _customer = _customerId is not null ? await _garageTransactionUtilityAppService.GetCustomer(Guid.Parse(_customerId)) : null;
        //Check if that transaction has been checked out before.
        var CheckIfLoggedOutFromMobile = await IfLoggedOutFromMobile(exitTransaction.PlateNumber);
        if(CheckIfLoggedOutFromMobile)
           return await CheckoutFromMobileSequence(exitTransaction, organization, _entryTransaction, _entryTransaction.AccurateTime, _lstCategory, _categoryHasChanged, _customer);
        //Check if doesn't pass organization free minutes.
        int _totalHours = _calculationsEndPointAppService.CalculateTime(_entryTransaction.AccurateTime, exitTransaction.AccurateTime.Value, organization.FreeMinutes);
        if (_totalHours * 60 <= organization.FreeMinutes && organization.FreeMinutes != 0)
        {
            return await ExitsInFreeMinutesPeriod(exitTransaction);
        }
        //processing calculations depends on transaction belongs to customer or not.
        decimal _customerInvoicePrice;
        if (_customerId is not null)
        {
            _customerInvoicePrice= await CalculationForRegisteredCustomer(exitTransaction.AccurateTime.Value, _entryTransaction.AccurateTime, organization, _packageDto, _totalHours);
            //update slots depends on category
            await UpdateSlotsCountIfCategoryChanged(_entryTransaction, _categoryHasChanged, _lstCategory);
        }
        else
        {
            _customerInvoicePrice= await CalculateInvoiceCases(_entryTransaction.AccurateTime, _totalHours, exitTransaction.AccurateTime.Value, organization);
            //update slots depends on category
            await _garageTransactionUtilityAppService.FreeSlotsOfGarageDependsOnCategoryAsync(_garage.Id, CustomerCategoryStatus.Customer);
        }
        //creating invoice
        CreateUpdateInvoiceDto _initialInvoice = await InitiateInvoice(_entryTransaction.AccurateTime, _customer, exitTransaction, _totalHours, _customerInvoicePrice, true);
        _initialInvoice.EntranceTime = _entryTransaction.AccurateTime;
        //Save Exit Transaction
        var _createdExitGarageTransaction = await _garageTransactionsAppService.CreateAsync(exitTransaction);
        _initialInvoice.GarageTransactionExitId = _createdExitGarageTransaction.Id;
        //Save Invoice
        await _invoiceAppService.CreateAsync(_initialInvoice);
        //Opens Gate Depends On Invoice Price
        _requestToGate = _initialInvoice.InvoicePrice <= 0;
        //Close and Map Entry Transactions
        await _garageTransactionUtilityAppService.CloseEntryTransaction(exitTransaction.PlateNumber);
        await CurrentUnitOfWork.SaveChangesAsync();
        _logger.LogInformation($"{DateTime.Now} ====> Get Response {_requestToGate}");
        return _requestToGate;
    }
    private async Task<bool> ExitsInFreeMinutesPeriod(CreateUpdateGarageTransactionDto exitTransaction)
    {
        await _garageTransactionsAppService.CreateAsync(exitTransaction);
        _logger.LogInformation($"{DateTime.Now} ====> Close Entry : {exitTransaction.PlateNumber}");
        await _garageTransactionUtilityAppService.CloseEntryTransaction(exitTransaction.PlateNumber);
        _logger.LogInformation($"{DateTime.Now} ====> PlateNumber  : {exitTransaction.PlateNumber}");
        _logger.LogInformation($"{DateTime.Now} ====> Get Response true | In Free minutes period");
        return await Task.FromResult(true);
    }
    #endregion
    #region private methods to handle checkout from mobile
    private async Task<bool> IfLoggedOutFromMobile(string plateNumber)
    {
        var _transaction = await _garageTransactionUtilityAppService.CheckIfCheckoutFromMobile(plateNumber);
        if (_transaction is null)
            return false;
        return _transaction.CheckoutMobile != null ? true : false;

    }
    private async Task<bool> CheckIfPassGracePeriod(DateTime currentCheckoutTime, DateTime? lastCheckoutTime, int graceMinutes)
    {
        if ((int)(currentCheckoutTime - lastCheckoutTime).Value.TotalMinutes > graceMinutes)
        {
            return true;
        }
        return false;
    }
    private async Task<bool> CheckoutFromMobileSequence(CreateUpdateGarageTransactionDto garageTransaction, Organization organization, GarageTransactionDto? entryTransaction, DateTime? entranceTime, CustomerCategoryStatus lstCategory, bool categoryHasChanged, Customer customer)
    {
       await UpdateSlotsCountIfCategoryChanged(entryTransaction, categoryHasChanged, lstCategory);
       return await CheckGracePeriodScenarios(garageTransaction, entranceTime, organization.Graceperiod, customer, organization);   
    }
    private async Task<bool> CheckGracePeriodScenarios(CreateUpdateGarageTransactionDto exitTransaction, DateTime? entranceTime, int graceMinutes, Customer customer,Organization organization)
    {
        var _lastInvoiceForPlateNumber = await _invoiceAppService.GetLatestInvoiceForPlateInOrganizationGarage(exitTransaction.PlateNumber, exitTransaction.OrganizationID, exitTransaction.GarageId);
        await _garageTransactionUtilityAppService.CloseCheckoutMobileTransaction(exitTransaction.PlateNumber);
        _logger.LogInformation($"{DateTime.Now} ====> Customer ID : {customer.Id}");
        _logger.LogInformation($"{DateTime.Now} ====> PlateNumber  : {exitTransaction.PlateNumber}");
        var PassGraceTime = await CheckIfPassGracePeriod(exitTransaction.AccurateTime.Value, _lastInvoiceForPlateNumber.Value.InvoiceTime, graceMinutes);
        return PassGraceTime switch
        {
            (false) => await DoesnotPassGracePeriod(exitTransaction),
            (true) => await PassedGracePeriod(exitTransaction, entranceTime, customer, _lastInvoiceForPlateNumber, organization),
        };

    }
    private async Task<bool> PassedGracePeriod(CreateUpdateGarageTransactionDto exitTransaction, DateTime? entranceTime, Customer customer, Result<InvoiceDto> lastCreatedInvoice,Organization organization)
    {
        int _hours = (int)Math.Ceiling((decimal)(exitTransaction.AccurateTime.Value - lastCreatedInvoice.Value.InvoiceTime.Value).TotalHours);
        _logger.LogInformation($"{DateTime.Now} ====> Get Response false | Exceeded Grace Period must pay invoice");
        var _price = await CalculateInvoiceCases(entranceTime.Value, _hours, exitTransaction.AccurateTime.Value,organization);
        CreateUpdateInvoiceDto _initialInvoice = await InitiateInvoice(entranceTime.Value, customer, exitTransaction, _hours, _price, true);
        _initialInvoice.EntranceTime = lastCreatedInvoice.Value.InvoiceTime.Value;
        _initialInvoice.EntryTransactionImage = lastCreatedInvoice.Value.EntryTransactionImage;
        var _createdExitgarageTransaction = await _garageTransactionsAppService.CreateAsync(exitTransaction);
        _initialInvoice.GarageTransactionExitId = _createdExitgarageTransaction.Id;
        await _invoiceAppService.CreateAsync(_initialInvoice); ;
        return await Task.FromResult(false);

    }
    private async Task<bool> DoesnotPassGracePeriod(CreateUpdateGarageTransactionDto exitTransaction)
    {
        _logger.LogInformation($"{DateTime.Now} ====> Get Response true | In Grace period ");
         await _garageTransactionsAppService.CreateAsync(exitTransaction);
        return true;
    }
    #endregion
    #region private methods to get pre invoice
    private async Task<CreateUpdateInvoiceDto> PreCheckoutFlow(DateTime EntranceTime, CreateUpdateGarageTransactionDto exitTransaction, Customer customer)
    {
        var organization = await _garageTransactionUtilityAppService.GetOrganization(exitTransaction.OrganizationID);
        int totalHours = _calculationsEndPointAppService.CalculateTime(EntranceTime, exitTransaction.AccurateTime.Value, organization.FreeMinutes);
        if (totalHours == 0)
        {
            return await InitiateInvoice(EntranceTime, customer, exitTransaction, totalHours, 0, false);

        }
        var packageDto= await _customerPackagesAppService.GetOneActiveCustomerPackageByOrganizationIdAndPlateNumber(exitTransaction.OrganizationID, exitTransaction.PlateNumber, exitTransaction.AccurateTime.Value);
        var _price = await CalculateStayingHoursForCustomer(EntranceTime, exitTransaction.AccurateTime.Value, organization, totalHours, packageDto);
        CreateUpdateInvoiceDto newInvoice = await InitiateInvoice(EntranceTime, customer, exitTransaction, totalHours, _price, true);
        newInvoice.EntranceTime = EntranceTime;
        return newInvoice;
    }
    #endregion
    #region private methods for invalid case to set in unlinked
    private async Task<bool> NotFoundEntityAndExceptionHandling(CreateUpdateGarageTransactionEndPointDto input, string Message)
    {
        await _unlinkedGarageTransactionsAppService.CreateAsync(input);
        _logger.LogError($"{DateTime.Now} ====>  {Message} is not found , Saved in Unlinked Transaction");
        _logger.LogError($"{DateTime.Now} ====> Get Response false");

        return await Task.FromResult(false);
    }
    private async Task InVailedCasesForTransaction(CreateUpdateGarageTransactionDto exitTransaction, DateTime? entranceTime)
    {
        exitTransaction.IsIn = false;
        exitTransaction.IsMapped = false;
        await _unlinkedGarageTransactionsAppService.CreateAsync(ObjectMapper.Map<CreateUpdateGarageTransactionDto, CreateUpdateGarageTransactionEndPointDto>(exitTransaction));

        if (entranceTime > exitTransaction.AccurateTime)
        {
            _logger.LogError($"{DateTime.Now} ====>  Entrance Time is greater exit time  ");
            _logger.LogError($"{DateTime.Now} ====> Get Response false");
        }
    }
    #endregion
    #region private methods to initiate dtos
    private CreateUpdateGarageTransactionDto InitiateCreateGarageTransactionDto(CreateUpdateGarageTransactionEndPointDto input, Camera camera)
    {
        return new CreateUpdateGarageTransactionDto
        {
            AccurateTime = input.AccurateTime,
            SnapTime = input.SnapTime,
            VehiclePicture = input.VehiclePicture,
            PlateNumber = input.PlateNumber,
            CameraId = camera.Id,
            GarageId = camera.GarageId,
            OrganizationID = camera.OrganizationId,
            TenantId = camera.TenantId,
            CameraType = camera.CameraType,
            IsIn = camera.CameraType == CameraType.Exit ? false : true,
            IsMapped = camera.CameraType == CameraType.Entry ? false : true,

        };
    }
    private async Task<CreateUpdateInvoiceDto> InitiateInvoice(DateTime entranceTime, Customer customer, CreateUpdateGarageTransactionDto exitTransaction, int totalHours, decimal price, bool isPackage)
    {
        string image = exitTransaction.VehiclePicture;
        if (exitTransaction.VehiclePicture.Length > 120)
        {
            image = _garageTransactionsAppService.SaveBase64Image(exitTransaction);
        }
        var entryTransactionImage = await _garageTransactionUtilityAppService.FindEntranceImage(exitTransaction.PlateNumber);
        var invoice = new CreateUpdateInvoiceDto
        {
            GarageId = exitTransaction.GarageId,
            InvoiceNumber = "",
            InvoicePrice = price,
            InvoiceType = price > 0 ? InvoiceType.UnPaid : InvoiceType.Paid,
            OrganizationId = exitTransaction.OrganizationID,
            TotalHours = totalHours,
            PlateNumber = exitTransaction.PlateNumber,
            InvoiceTime = exitTransaction.AccurateTime,
            EntranceTime = entranceTime,
            CustomerEnName = customer.ArabicName,
            CustomerArName = customer.IdentityUser.Name,
            CustomerId = customer.Id,
            IsPackage = isPackage,
            ExitTransactionImage = image,
            EntryTransactionImage = entryTransactionImage,

        };

        return invoice;
    }
    private async Task<CreateUpdateGarageTransactionDto> InitiateGarageTransaction(GarageTransaction entryGarageTransaction, int cameraId)
    {
        var exitGarageTransaction = new CreateUpdateGarageTransactionDto
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
    #endregion
    #region private methods to call calculations service 
    private async Task<decimal> CalculateStayingHoursForCustomer(DateTime entranceTime, DateTime exitTime, Organization organization, int totalHours, CustomerPackageDto packageDto)
    {
        return (packageDto, packageDto?.Package, packageDto?.IsCustomPackage) switch
        {
            (null, _, _) or (not null, null, false) => await CalculateInvoiceCases(entranceTime, totalHours, exitTime,organization),
            (not null, not null, _) or (not null, null, true) => await CalculateInvoiceForCustomerPackage(entranceTime, organization, packageDto, exitTime),
        };
    }
    private async Task<decimal> CalculationForRegisteredCustomer(DateTime? exitTime, DateTime? entranceTime, Organization organization, CustomerPackageDto? packageDto, int totalHours)
    {
        return (packageDto, packageDto?.Package, packageDto?.IsCustomPackage) switch
        {
            (null, _, _) or (not null, null, false) => await CalculateInvoiceCases(entranceTime.Value, totalHours, exitTime.Value, organization),
            (not null, not null, _) or (not null, null, true) => await CalculateInvoiceForCustomerPackage(entranceTime.Value, organization, packageDto, exitTime.Value),
        };
    }
    private async Task<decimal> CalculateInvoiceForCustomerPackage(DateTime entranceTime, Organization organization, CustomerPackageDto packageDto, DateTime exitTime)
    {
        decimal price;
        if (packageDto.IsCustomPackage)
        {
            var totaldays = _calculationsEndPointAppService.CalculateTimeForCustomPackage(exitTime, packageDto);
            price = totaldays * organization.FullDayPrice;
        }
        else
        {
            var THoursOutPackage = _calculationsEndPointAppService.CalculateTimeDependsOnPackage(entranceTime, exitTime, packageDto);
            price = THoursOutPackage * organization.AdditionalHourPrice;
        }
        return price;
    }
    private async Task<Decimal> CalculateInvoiceCases(DateTime entranceTime, int totalHours, DateTime exitTime,Organization organization)
    {
        //4 Cases
        return (organization.IsNumberOfHoursExceeded, organization.IsClose) switch
        {
            (false, false) => _calculationsEndPointAppService.CalculatePriceHoursExceededDay(organization, totalHours),
            (true, false) => _calculationsEndPointAppService.CalculatePriceForExceededHours(totalHours, organization),
            (false, true) => _calculationsEndPointAppService.CalculatePriceForOrganizationIsClosed(totalHours, organization, exitTime, entranceTime),
            (true, true) => _calculationsEndPointAppService.CalculatePriceForOrganizationIsClosedAndExceededHours(organization, exitTime, entranceTime),
        };
    }
    #endregion
}


