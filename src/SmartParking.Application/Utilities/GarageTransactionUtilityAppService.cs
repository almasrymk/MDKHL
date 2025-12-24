using iText.Svg.Renderers.Path.Impl;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartParking.Cameras;
using SmartParking.Cutomers;
using SmartParking.Entities.Cameras.Watchers;
using SmartParking.Entities.Customers;
using SmartParking.Entities.GarageTransactions;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Vehicles;
using SmartParking.Enums;
using SmartParking.GarageTransactions;
using SmartParking.Invoices;
using SmartParking.Localization;
using SmartParking.Organizations;
using SmartParking.Services.GarageTransactions;
using SmartParking.Services.Vehicles;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;


namespace SmartParking.Utilities;

public class GarageTransactionUtilityAppService : SmartParkingAppService, IGarageTransactionUtilityAppService
{
    private readonly ILogger<GarageTransactionUtilityAppService> _logger;
    private readonly IRepository<Vehicle, int> _vehicleRepository;
    private readonly IRepository<GarageTransaction, int> _repository;
    private readonly IRepository<Garage, int> _garageRepository;
    private readonly IRepository<Camera, int> _cameraRepository;
    private readonly IRepository<Customer, Guid> _customerRepoistory;
    private readonly IRepository<Organization, int> _organizationRepoistory;
    private readonly IDataFilter _dataFilter;
    private readonly IGarageUtilityAppService _garageUtilityAppService;
    private readonly GarageTransactionManager _garageTransactionManager;
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
        VehicleManager vehicleManager,
        IRepository<Camera, int> cameraRepository,
        IRepository<GarageTransaction, int> repository,
        IRepository<Garage, int> garageRepository,
        IGarageUtilityAppService garageUtilityAppService)
    {

        _vehicleRepository = vehicleRepository;
        _logger = logger;
        _dataFilter = dataFilter;
        _customerRepoistory = customerRepoistory;
        _organizationRepoistory = organizationRepoistory;
        _garageTransactionManager = garageTransactionManager;
       _vehicleManager = vehicleManager;
        _cameraRepository = cameraRepository;
        _repository = repository;
        _garageRepository = garageRepository;
        _garageUtilityAppService = garageUtilityAppService;
    }


    public async Task<Guid?> CheckPlateNumberBelongsToCustomerToGetHisIdAsync(string plateNumber)
    {
        return await _vehicleManager.CheckPlateNumberBelongsToCustomerToGetHisIdAsync(plateNumber);
    }

    public async Task CloseCheckoutMobileTransaction(string? plateNumber)
    {
        await _garageTransactionManager.CloseCheckoutMobileTransaction(plateNumber);
    }

    public async Task CloseEntryTransaction(string? plateNumber,string TicketId )
    {
         await _garageTransactionManager.CloseEntryTransaction(plateNumber, TicketId);  
    }

    public async Task<string> FindEntranceImage(string? plateNumber , string ticketId,int OrganiztionId)
    {
         return await _garageTransactionManager.FindEntranceImage(plateNumber, ticketId, OrganiztionId);
    }

    public async Task<GarageTransactionDto?> FindEntranceTransaction(string? PlateNumber , string? TicketId, bool DispenserTransaction, bool CameraTransaction , int organizationId , bool hasActivePackage)
    {
           if (DispenserTransaction)
               return await GetTransactionByDispenser(TicketId, organizationId);
           if (CameraTransaction)
              return await GetTransactionByCamera(PlateNumber , organizationId);
           return null;
    }

    private async Task<GarageTransactionDto?> GetTransactionByCamera(string plateNumber , int organizationId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _repository.GetQueryableAsync();
            var latestTransaction = await query
                    .Where(gt => gt.PlateNumber == plateNumber && gt.CameraType == WatcherType.Entry && gt.IsIn == true && gt.OrganizationID == organizationId)
                    .OrderByDescending(gt => gt.AccurateTime)
                    .Select(x => new GarageTransactionDto
                    {
                        Id  = x.Id,
                        CameraId = x.CameraId,
                        PlateNumber = x.PlateNumber,
                        SnapTime = (DateTime)x.SnapTime,
                        AccurateTime = (DateTime)x.AccurateTime,
                        OrganizationID = x.OrganizationID,
                        GarageId = x.GarageId,
                        VehiclePicture = x.VehiclePicture,
                        CameraType = x.CameraType,
                        TenantId = x.TenantId,
                        IsIn = x.IsIn,
                        CheckoutMobile = x.CheckoutMobile,
                        IsMapped = x.IsMapped,
                        RemainingTime = x.RemainingTime,
                        IsPending = x.IsPending,
                        CustomerCategory = x.CustomerCategory,
                        CanCheckout = x.CanCheckout,
                        DispenserId = x.DispenserId,
                        LaneNO = x.LaneNO,
                        TicketId = x.TicketId 
                    })
                    .FirstOrDefaultAsync();
            return latestTransaction;
        }
    }
    private async Task<GarageTransactionDto?> GetTransactionByDispenser(string? TicketId , int organizationId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _repository.GetQueryableAsync();
            var latestTransaction = query
                    .Where(gt => gt.TicketId == TicketId && gt.CameraType == WatcherType.Entry && gt.IsIn == true && gt.OrganizationID == organizationId)
                    .OrderByDescending(gt => gt.AccurateTime)
                    .Select(x => new GarageTransactionDto
                    { 
                        Id = x.Id,
                        CameraId = x.CameraId,
                        PlateNumber = x.PlateNumber,
                        SnapTime = (DateTime)x.SnapTime,
                        AccurateTime = (DateTime)x.AccurateTime,
                        OrganizationID = x.OrganizationID,
                        GarageId = x.GarageId,
                        VehiclePicture = x.VehiclePicture,
                        CameraType = x.CameraType,
                        TenantId = x.TenantId,
                        IsIn = x.IsIn,
                        CheckoutMobile = x.CheckoutMobile,
                        IsMapped = x.IsMapped,
                        RemainingTime = x.RemainingTime,
                        IsPending = x.IsPending,
                        CustomerCategory = x.CustomerCategory,
                        CanCheckout = x.CanCheckout,
                        DispenserId = x.DispenserId,
                        LaneNO = x.LaneNO,
                        TicketId = x.TicketId 
                    })
                    .FirstOrDefault();
            return latestTransaction;
        }


    }
    //
    public async Task<bool> FreeSlotsOfGarageDependsOnCategoryAsync(int garageId,  bool  hasFreeSlots , CustomerCategoryStatus category)
    {
       return await _garageUtilityAppService.FreeSlotsOfGarageDependsOnCategoryAsync(garageId, hasFreeSlots, category);
    }
    //
    public async Task<CameraForGarageTransactionDto> GetCameraByDeviceIdAsync(string deviceId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _cameraRepository.GetQueryableAsync();
            var camera = await (from c in query
                                where c.DeviceId == deviceId
                                select  new CameraForGarageTransactionDto
                                {
                                    Id = c.Id,
                                    DeviceId = c.DeviceId,
                                    CameraType = c.CameraType,
                                    GarageId = c.GarageId,
                                    Status = c.Status,
                                    OrganizationId = c.OrganizationId,
                                    TenantId = c.TenantId
                                }).FirstOrDefaultAsync();
            if (camera == null)
            {
                throw new Exception(LocalizationHelper.GetLocalizedString("Camera") + " " + LocalizationHelper.GetLocalizedString("NotFoundValidation"));
            }
            return camera;
        }
    }
    public async Task<Garage> GetGarageByIdAsync(int garageId)
    {
        return await _garageUtilityAppService.GetGarageByIdAsync(garageId);
    }
   
    public async Task<GarageTransactionDto> GetTransactionById(int garagetransactionId)
    {
         var garageTransactionDto= await _garageTransactionManager.GetTransactionById(garagetransactionId);
        return ObjectMapper.Map<GarageTransaction,GarageTransactionDto>(garageTransactionDto);

    }
    public async Task<bool> ReserverSlotsOfGarageDependsOnCategoryAsync(int garageId, bool hasFreeSlots ,  CustomerCategoryStatus customer)
    {
       return await _garageUtilityAppService.ReserverSlotsOfGarageDependsOnCategoryAsync(garageId, hasFreeSlots, customer);
    }

    //
    public async Task UpdateEntryTransactionCheckouFromMobile(int entryGarageTransactionId)
    {
        await _garageTransactionManager.UpdateEntryTransactionCheckouFromMobile(entryGarageTransactionId);
    }
    public async Task<CustomerDto> GetCurrentCustomer()
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var customerQuery = await _customerRepoistory.GetQueryableAsync();
            var customer = customerQuery.Where(s => s.IdentityUserId == CurrentUser.Id).FirstOrDefault();
            return ObjectMapper.Map<Customer,CustomerDto>(customer!);
        }
    }
    public async Task<CustomerDto> GetCustomer(Guid customerId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _customerRepoistory.GetQueryableAsync();
            query = query.Include(x => x.IdentityUser);
            var result = await (from x in query
                                where x.Id == customerId
                                select new CustomerDto
                                {
                                    Id = x.Id,
                                    ArabicName = x.ArabicName,
                                    UserName = x.IdentityUser.UserName,
                                    FullName = x.IdentityUser.Name
                                }).FirstOrDefaultAsync();
            return result;
        }
    }
    public async Task<TransactionOrganizationDto> GetOrganization(int OrganizationID)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            var query  = await _organizationRepoistory.GetQueryableAsync();
            var result = await (from x in query
                                where x.Id == OrganizationID
                                select
                                new TransactionOrganizationDto
                                {
                                    Id = x.Id,
                                    ArabicName = x.ArabicName,
                                    EnglishName = x.EnglishName,
                                    Status = x.Status,
                                    Code = x.Code,
                                    FirstHourPrice = x.FirstHourPrice,
                                    SecondHourPrice = x.SecondHourPrice,
                                    ThirdHourPrice = x.ThirdHourPrice,
                                    AdditionalHourPrice = x.AdditionalHourPrice,
                                    FullDayPrice = x.FullDayPrice,
                                    IsNumberOfHoursExceeded = x.IsNumberOfHoursExceeded,
                                    NumberOfHours = x.NumberOfHours,
                                    IsClose = x.IsClose,
                                    ClosingHoursStart = x.ClosingHoursStart,
                                    ClosingHoursEnd = x.ClosingHoursEnd,
                                    District = x.District,
                                    ActiveCameras = x.ActiveCameras,
                                    FreeMinutes = x.FreeMinutes,
                                    Graceperiod = x.Graceperiod,
                                    HasFreeSlots = x.HasFreeSlots,
                                    HasPackageOnly = x.HasPackageOnly,
                                    PaymentCode = x.PaymentCode,
                                    DispenserTransaction = x.DispenserTransaction,
                                    CameraTransaction = x.CameraTransaction  ,
                                    TenantId = x.TenantId!.Value
                                }).FirstOrDefaultAsync();

            return result; 
        }

    }
    [RemoteService(false)]
    public async Task<CreateUpdateGarageTransactionDto> InitiateExitGarageTransactionForMobileCheckout(GarageTransactionDto garageTransactionDto)
    {
        return new CreateUpdateGarageTransactionDto
        {
            AccurateTime = DateTime.Now,
            PlateNumber = garageTransactionDto.PlateNumber,
            GarageId = garageTransactionDto.GarageId,
            OrganizationID = garageTransactionDto.OrganizationID,
            CameraId = garageTransactionDto.CameraId,
            SnapTime = DateTime.Now,
            TenantId = garageTransactionDto.TenantId,
            IsIn = true,
            IsPending = true,
            IsMapped=false,
            CanCheckout = false,
            CheckoutMobile = DateTime.Now,
            VehiclePicture = garageTransactionDto.VehiclePicture,
            CameraType = WatcherType.Exit,
            LaneNO = garageTransactionDto.LaneNO,
            TicketId = garageTransactionDto.TicketId,

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
            Vat=Math.Round(price*0.15m,2)
        };

        return invoice;
    }

    public async Task<GarageTransactionDto> CheckIfCheckoutFromMobileTicket(string ticketNumber)
    {
        var query = await _repository.GetQueryableAsync();
        var latestTransaction = await query
                .Where(gt => gt.TicketId == ticketNumber && gt.CameraType == WatcherType.Exit && gt.IsIn == true)
                .OrderByDescending(gt => gt.AccurateTime)
                .Select(x => new GarageTransactionDto
                {
                    Id = x.Id,
                    CameraId = x.CameraId,
                    PlateNumber = x.PlateNumber,
                    SnapTime = (DateTime)x.SnapTime,
                    AccurateTime = (DateTime)x.AccurateTime,
                    OrganizationID = x.OrganizationID,
                    GarageId = x.GarageId,
                    VehiclePicture = x.VehiclePicture,
                    CameraType = x.CameraType,
                    TenantId = x.TenantId,
                    IsIn = x.IsIn,
                    CheckoutMobile = x.CheckoutMobile,
                    IsMapped = x.IsMapped,
                    RemainingTime = x.RemainingTime,
                    IsPending = x.IsPending,
                    CustomerCategory = x.CustomerCategory,
                    CanCheckout = x.CanCheckout,
                    DispenserId = x.DispenserId,
                    LaneNO = x.LaneNO,
                    TicketId = x.TicketId
                })
                .FirstOrDefaultAsync();
        return latestTransaction;
    }

}



