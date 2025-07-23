using SmartParking.Cameras;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using SmartParking.Localization;
using System;
using Volo.Abp.Domain.Repositories;
using System.Threading.Tasks;
using System.Diagnostics.Metrics;
using SmartParking.Entities.Vehicles;
using Microsoft.Extensions.Logging;
using SmartParking.GarageTransactionEndPoints;
using SmartParking.Entities.GarageTransactions;
using SmartParking.GarageTransactions;
using Volo.Abp.MultiTenancy;
using System.Linq;
using Volo.Abp.Data;
using Microsoft.EntityFrameworkCore;
using SmartParking.Entities.Customers;
using Volo.Abp;
using SmartParking.Invoices;
using SmartParking.Entities.Cameras.Watchers;
using SmartParking.Cutomers;
using SmartParking.Services.GarageTransactions;
using SmartParking.Services.Garages;
using SmartParking.Services.Cameras;
using SmartParking.Services.Vehicles;
using SmartParking.Organizations;


namespace SmartParking.Utilities;

public class GarageTransactionUtilityAppService : SmartParkingAppService, IGarageTransactionUtilityAppService
{
    private readonly ILogger<GarageTransactionUtilityAppService> _logger;
    private readonly IRepository<Vehicle, int> _vehicleRepository;
    private readonly IRepository<Customer, Guid> _customerRepoistory;
    private readonly IRepository<Organization, int> _organizationRepoistory;
    private readonly IDataFilter _dataFilter;
    private readonly GarageTransactionManager _garageTransactionManager;
    private readonly GarageManager _garageManager;
    private readonly CameraManager _cameraManager;
    private readonly VehicleManager _vehicleManager;
    private string customerEnName { get; set; } = "unknown";
    private string customerArName { get; set; } = "غير معرف";

    public GarageTransactionUtilityAppService(
        ILogger<GarageTransactionUtilityAppService> logger,
        IRepository<Vehicle, int> vehicleRepository,
        IDataFilter dataFilter,
        IRepository<Customer, Guid> customerRepoistory,
        IRepository<Organization, int> organizationRepoistory,
        GarageTransactionManager garageTransactionManager,
        GarageManager garageManager,
        CameraManager cameraManager,
        VehicleManager vehicleManager)
    {

        _vehicleRepository = vehicleRepository;
        _logger = logger;
        _dataFilter = dataFilter;
        _customerRepoistory = customerRepoistory;
        _organizationRepoistory = organizationRepoistory;
        _garageTransactionManager = garageTransactionManager;
        _garageManager = garageManager;
        _cameraManager = cameraManager;
        _vehicleManager = vehicleManager;
    }

    public async Task<GarageTransaction> CheckIfCheckoutFromMobile(string plateNumber)
    {
         return await _garageTransactionManager.CheckIfCheckoutFromMobile(plateNumber);
    }

    public async Task<string> CheckPlateNumberBelongsToCustomerToGetHisIdAsync(string plateNumber)
    {
        return await _vehicleManager.CheckPlateNumberBelongsToCustomerToGetHisIdAsync(plateNumber);
    }

    public async Task CloseCheckoutMobileTransaction(string? plateNumber, int entryTransactionId)
    {
        await _garageTransactionManager.CloseCheckoutMobileTransaction(plateNumber, entryTransactionId);
    }

    public async Task CloseEntryTransaction(string? plateNumber,string TicketId , int entryTransactionId)
    {
         await _garageTransactionManager.CloseEntryTransaction(plateNumber, TicketId, entryTransactionId);  
    }

    public async Task<string> FindEntranceImage(string? plateNumber , string ticketId,int OrganiztionId)
    {
         return await _garageTransactionManager.FindEntranceImage(plateNumber, ticketId, OrganiztionId);
    }

    public async Task<GarageTransactionDto?> FindEntranceTransaction(string? PlateNumber , string? TicketId, bool DispenserTransaction, bool CameraTransaction , int organizationId , bool hasActivePackage)
    {
            GarageTransactionDto? result = null!;
           if (DispenserTransaction)
           {
                if (TicketId != null)
                {
                    result = await GetTransactionByDispenser(TicketId, organizationId);
                }
                else if ( hasActivePackage && PlateNumber != null)
                {
                    result = await GetTransactionByCamera(PlateNumber, organizationId);
                }
                else
                     return result;
            }
            if (CameraTransaction && result==null)
                result = await GetTransactionByCamera(PlateNumber , organizationId);
            return result;   
    }

    private async Task<GarageTransactionDto?> GetTransactionByCamera(string plateNumber , int organizationId)
    {
        var latestTransaction = await _garageTransactionManager.GetTransactionByCamera(plateNumber, organizationId);
        return ObjectMapper.Map<GarageTransaction, GarageTransactionDto>(latestTransaction);
    }
    private async Task<GarageTransactionDto?> GetTransactionByDispenser(string? TicketId , int organizationId)
    {

        var latestTransaction = await _garageTransactionManager.GetTransactionByDispenser(TicketId, organizationId);
        return ObjectMapper.Map<GarageTransaction, GarageTransactionDto>(latestTransaction);
    }
    //
    public async Task<bool> FreeSlotsOfGarageDependsOnCategoryAsync(int garageId,  bool  hasFreeSlots , CustomerCategoryStatus category)
    {
       return await _garageManager.FreeSlotsOfGarageDependsOnCategoryAsync(garageId, hasFreeSlots, category);
    }
    //
    public async Task<Camera> GetCameraByDeviceIdAsync(string deviceId)
    {
        return await _cameraManager.GetCameraByDeviceIdAsync(deviceId);
    }
    public async Task<Camera> GetCameraByTypeBelongsToGarageAsync(int garageId, WatcherType cameraType)
    {
         return await _cameraManager.GetCameraByTypeBelongsToGarageAsync(garageId, cameraType);
      
    }
    public async Task<Garage> GetGarageByIdAsync(int garageId)
    {
        return await _garageManager.GetGarageByIdAsync(garageId);
    }
   
    public async Task<GarageTransaction> GetTransactionById(int garagetransactionId)
    {
         return await _garageTransactionManager.GetTransactionById(garagetransactionId);
    }
    public async Task<bool> ReserverSlotsOfGarageDependsOnCategoryAsync(int garageId, bool hasFreeSlots ,  CustomerCategoryStatus customer)
    {
       return await _garageManager.ReserverSlotsOfGarageDependsOnCategoryAsync(garageId, hasFreeSlots, customer);
    }

    //
    public async Task UpdateEntryTransactionCheckouFromMobile(int entryGarageTransactionId)
    {
        await _garageTransactionManager.UpdateEntryTransactionCheckouFromMobile(entryGarageTransactionId);
    }
    public async Task<Guid> GetCurrentCustomerId()
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var customer = await _customerRepoistory.GetQueryableAsync();
            var customerId = customer.Where(s => s.IdentityUserId == CurrentUser.Id).Select(s => s.Id).FirstOrDefault();
            return customerId;
        }
    }
    public async Task<CustomerDto> GetCustomer(Guid customerId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _customerRepoistory.GetQueryableAsync();
            query = query.Include(x => x.IdentityUser);
            var customer = await query.FirstOrDefaultAsync(x => x.Id == customerId);
            return ObjectMapper.Map<Customer,CustomerDto>(customer);
        }
    }
    public async Task<TransactionOrganizationDto> GetOrganization(int OrganizationID)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            var organization = await _organizationRepoistory.FirstOrDefaultAsync(x => x.Id == OrganizationID);
            return ObjectMapper.Map<Organization, TransactionOrganizationDto>(organization);
        }

    }
    [RemoteService(false)]
    public async Task<CreateUpdateGarageTransactionDto> InitiateExitGarageTransactionForMobileCheckout(string PlateNumber , string EntryPicture , int OrganizationId , int GarageId,int CameraId , Guid TenantId)
    {
        return new CreateUpdateGarageTransactionDto
        {
            AccurateTime = DateTime.Now,
            PlateNumber = PlateNumber,
            GarageId = GarageId,
            OrganizationID = OrganizationId,
            CameraId = CameraId,
            SnapTime = DateTime.Now,
            TenantId = TenantId,
            IsIn = true,
            CheckoutMobile = DateTime.Now,
            VehiclePicture = EntryPicture,
            CameraType = WatcherType.Exit
        };
    }
    [RemoteService(false)]
    public async Task<CreateUpdateInvoiceDto> InitiateInvoice(DateTime entranceTime, CreateUpdateGarageTransactionDto exitTransaction, int totalHours, decimal price, bool isPackage , CustomerDto customer = null)
    {
        var entryTransactionImage =await FindEntranceImage(exitTransaction.PlateNumber , exitTransaction.TicketId,exitTransaction.OrganizationID);
        var invoice =  new CreateUpdateInvoiceDto
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
            CustomerEnName = customer is null ? customerEnName : customer!.FullName,
            CustomerArName = customer is null ? customerArName : customer!.ArabicName,
            CustomerId = customer is null ? null : customer!.Id,
            IsPackage = isPackage,
            ExitTransactionImage = exitTransaction.VehiclePicture,
            EntryTransactionImage = entryTransactionImage,
            TicketId = exitTransaction.TicketId,
        };

        return invoice;
    }
}



