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
                    }
                    else
                    {
                        await _unlinkedGarageTransactionsAppService.CreateAsync(input);
                        await _garageTransactionUtilityAppService.FreeSlotsOfGarageDependsOnCategoryAsync(GarageTransaction.GarageId, GarageTransaction.CustomerCategory);

                    }
                    await CurrentUnitOfWork.SaveChangesAsync();
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
    #region private method to handle category changements
    /// <summary>
    /// Method updates entry transaction category
    /// </summary>
    /// <param name="entryTransaction">Entry Transaction</param>
    private async Task ChangeCategoryForEntryTransaction(GarageTransactionDto entryTransaction)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            await _garageTransactionsAppService.UpdateAsync(entryTransaction.Id, ObjectMapper.Map<GarageTransactionDto, CreateUpdateGarageTransactionDto>(entryTransaction));
        }
    }
    #endregion
    #region private method to handle exit
    /// <summary>
    /// Method handles request of exit transaction
    /// </summary>
    /// <param name="exitTransaction">Exit transaction dto</param>
    /// <param name="organization">Organization</param>
    /// <returns>true to open gate otherwise false</returns>
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
            var input = ObjectMapper.Map<CreateUpdateGarageTransactionDto, CreateUpdateGarageTransactionEndPointDto>(exitTransaction);
            await _unlinkedGarageTransactionsAppService.CreateAsync(input);
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
            await ExitBeforeEntry(exitTransaction, _entryTransaction.AccurateTime);
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
           return await CheckoutFromMobileSequence(exitTransaction, organization, _entryTransaction, _categoryHasChanged, _customer);
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
            _customerInvoicePrice=  _calculationsEndPointAppService.CalculationForRegisteredCustomer(exitTransaction.AccurateTime.Value, _entryTransaction.AccurateTime, organization, _packageDto, _totalHours);
            //update slots depends on category
            await _garageTransactionUtilityAppService.FreeSlotsOfGarageDependsOnCategoryAsync(_entryTransaction.GarageId, _entryTransaction.CustomerCategory);
        }
        else
        {
            _customerInvoicePrice= _calculationsEndPointAppService.CalculateInvoiceCases(_entryTransaction.AccurateTime, _totalHours, exitTransaction.AccurateTime.Value, organization);
            //update slots depends on category
            await _garageTransactionUtilityAppService.FreeSlotsOfGarageDependsOnCategoryAsync(_garage.Id, CustomerCategoryStatus.Customer);
        }
        //creating invoice
        CreateUpdateInvoiceDto _initialInvoice = await _garageTransactionUtilityAppService.InitiateInvoice(_entryTransaction.AccurateTime, exitTransaction, _totalHours, _customerInvoicePrice, true,_customer);
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
    /// <summary>
    /// Method handles exit in free minutes scenario
    /// Save exit transaction
    /// Map entry transaction
    /// </summary>
    /// <param name="exitTransaction">exit transaction dto</param>
    /// <returns>true</returns>
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
    /// <summary>
    /// Method used to detect if customer has checked out from mobile
    /// </summary>
    /// <param name="plateNumber">plate number of vehicle</param>
    /// <returns>true if passed false if no</returns>
    private async Task<bool> IfLoggedOutFromMobile(string plateNumber)
    {
        var _transaction = await _garageTransactionUtilityAppService.CheckIfCheckoutFromMobile(plateNumber);
        if (_transaction is null)
            return false;
        return _transaction.CheckoutMobile != null ? true : false;

    }
    /// <summary>
    /// Method used to handle flow of mobile checked out
    /// Method used check of grace defined in organization configuration 
    /// </summary>
    /// <param name="exitTransaction">Exit Transaction</param>
    /// <param name="organization">Organization</param>
    /// <param name="entryTransaction">Exntry Transaction</param>
    /// <param name="categoryHasChanged">flag to detect if category has been changed</param>
    /// <param name="customer">Customer</param>
    /// <returns>false if passed grace period as new invoice must paid otherwise true</returns>
    private async Task<bool> CheckoutFromMobileSequence(CreateUpdateGarageTransactionDto exitTransaction, Organization organization, GarageTransactionDto? entryTransaction, bool categoryHasChanged, Customer customer)
    {
        if (entryTransaction.CustomerCategory != exitTransaction.CustomerCategory)
        {
            entryTransaction.CustomerCategory = exitTransaction.CustomerCategory;
            await ChangeCategoryForEntryTransaction(entryTransaction);
        }
        var _lastInvoiceForPlateNumber = await _invoiceAppService.GetLatestInvoiceForCustomerInOrganizationGarage(exitTransaction.PlateNumber, exitTransaction.OrganizationID, exitTransaction.GarageId);
        await _garageTransactionUtilityAppService.CloseCheckoutMobileTransaction(exitTransaction.PlateNumber);
        _logger.LogInformation($"{DateTime.Now} ====> Customer ID : {customer?.Id}");
        _logger.LogInformation($"{DateTime.Now} ====> PlateNumber  : {exitTransaction.PlateNumber}");
        var PassGraceTime = await CheckIfPassGracePeriod(exitTransaction.AccurateTime.Value, _lastInvoiceForPlateNumber.Value.InvoiceTime, organization.Graceperiod);
        return PassGraceTime switch
        {
            (false) => await DoesnotPassGracePeriod(exitTransaction),
            (true) => await PassedGracePeriod(exitTransaction, entryTransaction.AccurateTime, customer, _lastInvoiceForPlateNumber, organization),
        };
    }
    /// <summary>
    /// Method used to detect if customer passed grace period or not
    /// </summary>
    /// <param name="currentTransactionTime">Time of cuuren exit transaction</param>
    /// <param name="lastTransactionTime">Time of last exit transaction</param>
    /// <param name="graceMinutes">Organization grace minutes</param>
    /// <returns>true if passed false if not</returns>
    private async Task<bool> CheckIfPassGracePeriod(DateTime currentTransactionTime, DateTime? lastTransactionTime, int graceMinutes)
    {
        if ((int)(currentTransactionTime - lastTransactionTime).Value.TotalMinutes > graceMinutes)
        {
            return true;
        }
        return false;
    }
    /// <summary>
    /// Method used to create new invoice as customer passed grace period.
    /// Time Calculated difference between in last invoice time and exit transaction time.
    /// Price Calculated dependes Organization Configuration
    /// </summary>
    /// <param name="exitTransaction">Exit transaction</param>
    /// <param name="entranceTime">Entrance time</param>
    /// <param name="customer">Customer</param>
    /// <param name="lastCreatedInvoice">Last Invoice created for Customer</param>
    /// <param name="organization">Organization</param>
    /// <returns>false</returns>
    private async Task<bool> PassedGracePeriod(CreateUpdateGarageTransactionDto exitTransaction, DateTime? entranceTime, Customer customer, Result<InvoiceDto> lastCreatedInvoice,Organization organization)
    {
        int _hours = (int)Math.Ceiling((decimal)(exitTransaction.AccurateTime.Value - lastCreatedInvoice.Value.InvoiceTime.Value).TotalHours);
        _logger.LogInformation($"{DateTime.Now} ====> Get Response false | Exceeded Grace Period must pay invoice");
        var _price = _calculationsEndPointAppService.CalculateInvoiceCases(entranceTime.Value, _hours, exitTransaction.AccurateTime.Value, organization);
        CreateUpdateInvoiceDto _initialInvoice = await _garageTransactionUtilityAppService.InitiateInvoice(entranceTime.Value, exitTransaction, _hours, _price, true, customer);
        _initialInvoice.EntranceTime = lastCreatedInvoice.Value.InvoiceTime.Value;
        _initialInvoice.EntryTransactionImage = lastCreatedInvoice.Value.EntryTransactionImage;
        var _createdExitgarageTransaction = await _garageTransactionsAppService.CreateAsync(exitTransaction);
        _initialInvoice.GarageTransactionExitId = _createdExitgarageTransaction.Id;
        await _invoiceAppService.CreateAsync(_initialInvoice); ;
        return await Task.FromResult(false);

    }
    /// <summary>
    /// Method saved exit transaction
    /// </summary>
    /// <param name="exitTransaction">Exit transaction</param>
    /// <returns>true</returns>
    private async Task<bool> DoesnotPassGracePeriod(CreateUpdateGarageTransactionDto exitTransaction)
    {
        _logger.LogInformation($"{DateTime.Now} ====> Get Response true | In Grace period ");
         await _garageTransactionsAppService.CreateAsync(exitTransaction);
        return true;
    }
    #endregion
    #region private methods for invalid case to set in unlinked
    /// <summary>
    /// Method used to save transaction in unlinked when invalid scenario happens
    /// </summary>
    /// <param name="input">Garage Transaction End Point Dto</param>
    /// <param name="Message">Message to be logged</param>
    /// <returns>false</returns>
    private async Task<bool> NotFoundEntityAndExceptionHandling(CreateUpdateGarageTransactionEndPointDto input, string Message)
    {
        await _unlinkedGarageTransactionsAppService.CreateAsync(input);
        _logger.LogError($"{DateTime.Now} ====>  {Message} is not found , Saved in Unlinked Transaction");
        _logger.LogError($"{DateTime.Now} ====> Get Response false");

        return await Task.FromResult(false);
    }
    /// <summary>
    /// Method used to handle exit before entry
    /// </summary>
    /// <param name="exitTransaction">eGarage Transaction Dto</param>
    /// <param name="entranceTime">Entrance Time</param>
    /// <returns>False</returns>
    private async Task ExitBeforeEntry(CreateUpdateGarageTransactionDto exitTransaction, DateTime? entranceTime)
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
    /// <summary>
    /// Method used to initate a create garage transaction dto.
    /// </summary>
    /// <param name="input">CreateUpdateGarageTransaction EndPoint Dto</param>
    /// <param name="camera">Camera which sends transaction</param>
    /// <returns>create update garage transaction dto</returns>
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
    #endregion
    
}


