using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1.X509;
using SmartParking.BarrierAppService;
using SmartParking.CustomerPackages;
using SmartParking.CustomerPackages.CustomerPackageTransactions;
using SmartParking.Entities.Cameras.Watchers;
using SmartParking.Entities.GarageTransactions;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using SmartParking.GarageTransactionEndPoints;
using SmartParking.GarageTransactions;
using SmartParking.PackageCodeEndPoint;
using SmartParking.Utilities;
using System;
using System.Globalization;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SmartParking.DispenserEndPoint;
[Route("/api/app/dispenser-end-points")]

public class DispenserEndPointsAppService : SmartParkingAppService, IDispenserEndPointsAppService
{
    private readonly IGarageTransactionEndPointsAppService _garageTransactionEndPointsAppService;
    private readonly IGarageTransactionUtilityAppService _garageTransactionUtilityAppService;
    private readonly IBarrierAppService _barrierAppService;
    private readonly ICustomerPackageHandlerAppService _customerPackagesHandlerAppService;
    private readonly IRepository<Dispenser, int> _dispenserRepository;
    private readonly IRepository<Organization, int> _organizationRepository;
    private readonly ILogger<DispenserEndPointsAppService> _logger;
    private readonly IUnLinkedTransactionAppService _unlinkedGarageTransactionsAppService;
    private readonly string _packageCodeNotFound = "Please confirm if the payment has been completed";
    private readonly string _plateOrTicketeNotFound = "Ticket Id not found";
    private readonly string _packageNotFound = "Not have active package";
    private readonly string _laneNotFound = "LaneNotValid";
    private readonly string _dispenserNotFound = "Dispenser code not valid";
    private readonly string _organizationNotFound = "Not found organization for this dispenser";
    private readonly string _otherReasons = "Other";
    private readonly string _multipleRequests = "Multiple Request from Pre-Entry";


    public DispenserEndPointsAppService(IGarageTransactionEndPointsAppService garageTransactionEndPointsAppService, IBarrierAppService barrierAppService, IRepository<Dispenser, int> dispenserRepository, ILogger<DispenserEndPointsAppService> logger, IRepository<Organization, int> organizationRepository, ICustomerPackageHandlerAppService customerPackagesAppService , IUnLinkedTransactionAppService unlinkedGarageTransactionsAppService, IGarageTransactionUtilityAppService garageTransactionUtilityAppService)
    {
        _garageTransactionEndPointsAppService = garageTransactionEndPointsAppService;
        _barrierAppService = barrierAppService;
        _dispenserRepository = dispenserRepository;
        _logger = logger;
        _organizationRepository = organizationRepository;
        _unlinkedGarageTransactionsAppService = unlinkedGarageTransactionsAppService;
        _garageTransactionUtilityAppService = garageTransactionUtilityAppService;
        _customerPackagesHandlerAppService = customerPackagesAppService;
    }
    #region public methods

    [HttpPost("/entry/{OrganizationCode}")]
    public async Task<IBaseGateResponseDto> Entry(string OrganizationCode, EntryDto request)
    {
        _logger.LogError($"{DateTime.Now} => start entry ");

        using (DataFilter.Disable<IMultiTenant>())
        {
            var _unlinkedDto = InitiateUnlinkedTransactionDto(request);
            var _organization = await _garageTransactionUtilityAppService.GetOrganization(OrganizationCode);
            if (_organization == null || _organization.DispenserTransaction == false)
            {
                return NotFoundOrganizationHandler(request.CarNum);
            }
            if (_organization == null || _organization.DispenserTransaction == false)
            {
                return NotFoundOrganizationHandler(request.CarNum);
            }
            var _dispenser = await _dispenserRepository.FirstOrDefaultAsync(dis => dis.DeviceId == request.LaneNO && dis.OrganizationId == _organization.Id);
            if (_dispenser == null)
            {
                return NotFoundDispenserHandler(request.CarNum);
            }
            if(string.IsNullOrEmpty(request.TicketID))
            {
                await _unlinkedGarageTransactionsAppService.CreateAsync(_unlinkedDto);
                return NOtFoundPlateOrTicketHandler(request.CarNum);
            }
            
            var _garageTransactionDto = InitiateCreateGarageTransactionDtoForPreEntry(request, _dispenser);
            
            var packageDto = await _customerPackagesHandlerAppService.GetOneActiveCustomerPackage(new GarageTransactionDto { OrganizationID = _garageTransactionDto.OrganizationID , PlateNumber=_garageTransactionDto.PlateNumber,TicketId=_garageTransactionDto.TicketId,AccurateTime=_garageTransactionDto.AccurateTime });
            if (packageDto != null)
            {
                return GetGateResponse(request.CarNum, false, _multipleRequests);

            }

            packageDto = new();
            var customerId = await _garageTransactionUtilityAppService.CheckPlateNumberBelongsToCustomerToGetHisIdAsync(_garageTransactionDto.PlateNumber);
            packageDto.CustomerId = customerId.HasValue ? customerId.Value : Guid.Empty;
            _garageTransactionDto.CustomerCategory= packageDto.CustomerId != Guid.Empty ? packageDto.CustomerCategory : CustomerCategoryStatus.Customer;
            var result = await _garageTransactionEndPointsAppService.HandleEntrance(_unlinkedDto, new OrganizationTransactionSettingsDto { HasFreeSlots = _organization.HasFreeSlots, HasPackageOnly = _organization.HasPackageOnly, HasUserAccess = _organization.UserAccess, HasVehicleAccess = _organization.VehicleAccess },  _garageTransactionDto  , packageDto);
            await _barrierAppService.CallingBarrier(_organization.Id, new BarrierRequest { carNum = request.CarNum, laneNO = request.LaneNO, operatorType = result == false ? 2 : 5 });
            return GetGateResponse(request.CarNum, result, _otherReasons);
        }
    }

    [HttpPost("/pre-entry/{OrganizationCode}")]
    public async Task<IBaseGateResponseDto> PreEntry(string OrganizationCode ,PackageCodeEntryDto packageCodeEntry)
    {
        _logger.LogError($"{DateTime.Now} => start pre entry ");

        using (DataFilter.Disable<IMultiTenant>())
        {
            var unlinkedDto = InitiateUnlinkedTransactionDto(packageCodeEntry);
            if (string.IsNullOrEmpty(packageCodeEntry.PackageCode))
            {
                await _unlinkedGarageTransactionsAppService.CreateAsync(unlinkedDto);
                return NotFoundPackageCodeInRequestHandler(packageCodeEntry.CarNum);
            }
            var _organization = await _garageTransactionUtilityAppService.GetOrganization(OrganizationCode);
            if (_organization == null || _organization.DispenserTransaction == false)
            {
                return NotFoundOrganizationHandler(packageCodeEntry.CarNum);
            }
            var _dispenser = await _dispenserRepository.FirstOrDefaultAsync(dis => dis.DeviceId == packageCodeEntry.LaneNO && dis.OrganizationId == _organization.Id);
            if (_dispenser == null)
            {
                return NotFoundDispenserHandler(packageCodeEntry.CarNum);
            }
         
            var _garageTransactionDto = InitalizePreEntryCreateUpdateGarageTransactionDto(packageCodeEntry, _dispenser);
            var packageDto = await _customerPackagesHandlerAppService.GetOneActiveCustomerPackage(new GarageTransactionDto { OrganizationID = _garageTransactionDto.OrganizationID, PlateNumber = _garageTransactionDto.PlateNumber, TicketId = _garageTransactionDto.TicketId, AccurateTime = _garageTransactionDto.AccurateTime });
            if (packageDto == null)
            {
                return NotFoundPackageHandler(packageCodeEntry.CarNum);
            }
            var result = await _garageTransactionEndPointsAppService.HandleEntrance(unlinkedDto, new OrganizationTransactionSettingsDto { HasFreeSlots = _organization.HasFreeSlots, HasPackageOnly = _organization.HasPackageOnly, HasUserAccess = _organization.UserAccess, HasVehicleAccess = _organization.VehicleAccess }, _garageTransactionDto, packageDto);
            return GetGateResponse(packageCodeEntry.CarNum ,result, _otherReasons);
        }
    }
    [HttpPost("/exit/{OrganizationCode}")]
    public async Task<IBaseGateResponseDto> Exit(string OrganizationCode,ExitDto request)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            _logger.LogError($"{DateTime.Now} => start exit ");
            var _organization = await _garageTransactionUtilityAppService.GetOrganization(OrganizationCode);
            if (_organization == null || _organization.DispenserTransaction == false)
            {
                return NotFoundOrganizationHandler(request.CarNum);
            }
            var _dispenser = await _dispenserRepository.FirstOrDefaultAsync(dis => dis.DeviceId == request.LaneNO && dis.OrganizationId == _organization.Id);
            if (_dispenser == null)
            {
                return NotFoundDispenserHandler(request.CarNum);

            }
            _logger.LogInformation($"Dispenser is found");
            
            _logger.LogInformation($"Organization is found");

            var _garageTransactionDto = InitiateCreateGarageTransactionDtoForExit(request, _dispenser);

            var input = ObjectMapper.Map<CreateUpdateGarageTransactionDto, CreateUpdateGarageTransactionEndPointDto>(_garageTransactionDto);
            if (string.IsNullOrEmpty(_garageTransactionDto.TicketId))
            {
                return NOtFoundPlateOrTicketHandler(request.CarNum);
            }
            var packageDto = await _customerPackagesHandlerAppService.GetOneActiveCustomerPackage(new GarageTransactionDto { OrganizationID = _garageTransactionDto.OrganizationID, PlateNumber = _garageTransactionDto.PlateNumber, TicketId = _garageTransactionDto.TicketId, AccurateTime = _garageTransactionDto.AccurateTime });
            if (packageDto != null)
            {
                return GetGateResponse(request.CarNum, false, _multipleRequests);

            }
            packageDto = new();
            var customerId = await _garageTransactionUtilityAppService.CheckPlateNumberBelongsToCustomerToGetHisIdAsync(_garageTransactionDto.PlateNumber);
            packageDto.CustomerId = customerId.HasValue ? customerId.Value : Guid.Empty;
            _garageTransactionDto.CustomerCategory = packageDto.CustomerId != Guid.Empty ? packageDto.CustomerCategory : CustomerCategoryStatus.Customer;
            var result = await _garageTransactionEndPointsAppService.HandleExit(_garageTransactionDto, _organization, input  , packageDto);
            return GetGateResponse(request.CarNum, result, _otherReasons);
        }
    }
    [HttpPost("/pre-exit/{OrganizationCode}")]
    public async Task<IBaseGateResponseDto> PreExit(string OrganizationCode,PackageCodeExitDto packageCodeExit)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            _logger.LogError($"{DateTime.Now} => start pre exit ");
            _logger.LogInformation($"Lane Number is found : {packageCodeExit.LaneNO}");
            if (string.IsNullOrEmpty(packageCodeExit.PackageCode))
            {
                var ExitDto = ObjectMapper.Map<PackageCodeExitDto,ExitDto>(packageCodeExit);
                return await Exit(OrganizationCode,ExitDto);
            }
            var unlinkedDto = InitiatePreExitUnlinkedTransactionDto(packageCodeExit);
            var _organization = await _garageTransactionUtilityAppService.GetOrganization(OrganizationCode);
            if (_organization == null || _organization.DispenserTransaction == false)
            {
                return NotFoundOrganizationHandler(packageCodeExit.CarNum);
            }
            var _dispenser = await _dispenserRepository.FirstOrDefaultAsync(dis => dis.DeviceId == packageCodeExit.LaneNO && dis.OrganizationId == _organization.Id);
            if (_dispenser == null)
            {
                return NotFoundDispenserHandler(packageCodeExit.CarNum);
            }
            _logger.LogInformation($"Dispenser is found");
           
            _logger.LogInformation($"Organization is found");
            var _garageTransactionDto = InitalizePreExitCreateUpdateGarageTransactionDto(packageCodeExit, _dispenser);
            var input = ObjectMapper.Map<CreateUpdateGarageTransactionDto, CreateUpdateGarageTransactionEndPointDto>(_garageTransactionDto);
            var packageDto = await _customerPackagesHandlerAppService.GetOneActiveCustomerPackage(new GarageTransactionDto { OrganizationID = _garageTransactionDto.OrganizationID, PlateNumber = _garageTransactionDto.PlateNumber, TicketId = _garageTransactionDto.TicketId, AccurateTime = _garageTransactionDto.AccurateTime });
            if (packageDto == null)
            {
                return NotFoundPackageHandler(packageCodeExit.CarNum);
            }
            var result = await _garageTransactionEndPointsAppService.HandleExit(_garageTransactionDto, _organization, input, packageDto);
            return GetGateResponse(packageCodeExit.CarNum, result,_otherReasons );
        }
    }

    

    #endregion

    #region private methods to initiate dtos
    /// <summary>
    /// Method used to initate a create garage transaction dto.
    /// </summary>
    /// <param name="input">CreateUpdateGarageTransaction EndPoint Dto</param>
    /// <param name="dispenser">Dispenser which sends transaction</param>
    /// <returns>create update garage transaction dto</returns>
    private CreateUpdateGarageTransactionDto InitiateCreateGarageTransactionDtoForPreEntry(EntryDto input,Dispenser dispenser )
    {
        return new CreateUpdateGarageTransactionDto
        {
            AccurateTime = DateTime.ParseExact(input.ActTime, "yyyyMMddHHmmss", CultureInfo.InvariantCulture),
            SnapTime = DateTime.ParseExact(input.ActTime, "yyyyMMddHHmmss", CultureInfo.InvariantCulture),
            VehiclePicture = input.CarImgData,
            PlateNumber = input.CarNum,
            DispenserId = dispenser.Id,
            LaneNO = input.LaneNO,
            GarageId = dispenser.GarageId,
            OrganizationID = dispenser.OrganizationId,
            TenantId = dispenser.TenantId,
            CameraType = WatcherType.Entry,
            IsIn = true,
            IsMapped = false,
            TicketId=input.TicketID,
            CanCheckout=true

        };
    }
    private IBaseGateResponseDto NotFoundPackageCodeInRequestHandler(string CarNumber)
    {
        Logger.LogInformation(_packageCodeNotFound);
        return GetGateResponse(CarNumber, false, _packageCodeNotFound);
    }
    private IBaseGateResponseDto NotFoundPackageHandler(string CarNumber)
    {
        Logger.LogInformation(_packageNotFound);
        return GetGateResponse(CarNumber, false, _packageNotFound);
    }

    private IBaseGateResponseDto NOtFoundPlateOrTicketHandler(string CarNumber)
    {
        _logger.LogInformation($"{_plateOrTicketeNotFound}");
        return GetGateResponse(CarNumber, false, _plateOrTicketeNotFound);
    }
    private IBaseGateResponseDto NotFoundOrganizationHandler(string CarNumber)
    {
        _logger.LogInformation($"{_organizationNotFound}");
        return GetGateResponse(CarNumber, false, _organizationNotFound);
    }

    private IBaseGateResponseDto NotFoundDispenserHandler(string CarNumber)
    {
        _logger.LogInformation($"{_dispenserNotFound}");
        return GetGateResponse(CarNumber, false, _dispenserNotFound);
    }
    private  IBaseGateResponseDto GetGateResponse(string CarNum,
    bool shouldOpenGate,
    string reason = null)
    {
        IBaseGateResponseDto response;
        if (shouldOpenGate)
        {
            response = new OpenGateResponseDto
            {
                CarNum = CarNum,
            };
        }
        else
        {
            response = new NotToOpenGateResponseDto
            {
                CarNum=CarNum,
                ResMsg = string.IsNullOrWhiteSpace(reason) ? "Vehicle is not allowed to enter" : reason
            };

        }
        _logger.LogError($"{DateTime.Now} ====>  {JsonSerializer.Serialize(response)}");

        return response; 
    }

    private CreateUpdateGarageTransactionDto InitalizePreEntryCreateUpdateGarageTransactionDto(PackageCodeEntryDto packageCodeEntryDto, Dispenser dispenser)
    {
        return new CreateUpdateGarageTransactionDto
        {
            AccurateTime = ParseDateTime(packageCodeEntryDto.ActTime),
            SnapTime = ParseDateTime(packageCodeEntryDto.ActTime),
            VehiclePicture = packageCodeEntryDto.CarImgData,
            PlateNumber = packageCodeEntryDto.CarNum,
            DispenserId = dispenser.Id,
            LaneNO = packageCodeEntryDto.LaneNO,
            GarageId = dispenser.GarageId,
            OrganizationID = dispenser.OrganizationId,
            TenantId = dispenser.TenantId,
            CameraType = WatcherType.Entry,
            IsIn = true,
            IsMapped = false,
            PackageCode = packageCodeEntryDto.PackageCode,
            TicketId = packageCodeEntryDto.TicketID,
            CanCheckout = true
        };
    }
    private CreateUpdateGarageTransactionDto InitalizePreExitCreateUpdateGarageTransactionDto(PackageCodeExitDto packageCodeExitDto, Dispenser dispenser)
    {
        return new CreateUpdateGarageTransactionDto
        {
            AccurateTime = ParseDateTime(packageCodeExitDto.ActTime),
            SnapTime = ParseDateTime(packageCodeExitDto.ActTime),
            VehiclePicture = packageCodeExitDto.CarImgData,
            PlateNumber = packageCodeExitDto.CarNum,
            DispenserId = dispenser.Id,
            LaneNO = packageCodeExitDto.LaneNO,
            GarageId = dispenser.GarageId,
            OrganizationID = dispenser.OrganizationId,
            TenantId = dispenser.TenantId,
            CameraType = WatcherType.Exit,
            IsIn = false,
            IsMapped = true,
            TicketId = packageCodeExitDto.TicketID,
            PackageCode = packageCodeExitDto.PackageCode,
            CanCheckout = false,

        };
    }
    private CreateUpdateGarageTransactionEndPointDto InitiatePreExitUnlinkedTransactionDto(PackageCodeExitDto input)
    {
        return new CreateUpdateGarageTransactionEndPointDto
        {
            AccurateTime = DateTime.ParseExact(input.ActTime, "yyyyMMddHHmmss", CultureInfo.InvariantCulture),
            SnapTime = DateTime.ParseExact(input.ActTime, "yyyyMMddHHmmss", CultureInfo.InvariantCulture),
            VehiclePicture = input.CarImgData,
            PlateNumber = input.CarNum,
            DeviceID = input.LaneNO
        };
    }

    private CreateUpdateGarageTransactionDto InitiateCreateGarageTransactionDtoForExit(ExitDto input, Dispenser dispenser)
    {
        return new CreateUpdateGarageTransactionDto
        {
            AccurateTime = ParseDateTime(input.ActTime),
            SnapTime = ParseDateTime(input.ActTime),
            VehiclePicture = input.CarImgData,
            PlateNumber = input.CarNum,
            DispenserId = dispenser.Id,
            LaneNO = input.LaneNO,
            GarageId = dispenser.GarageId,
            OrganizationID = dispenser.OrganizationId,
            TenantId = dispenser.TenantId,
            TicketId=input.TicketId,
            CameraType = WatcherType.Exit,
            IsIn = false,
            IsMapped = true,
            CanCheckout = false

        };
    }


    /// <summary>
    /// Method used to initate a create garage transaction dto.
    /// </summary>
    /// <param name="input">CreateUpdateGarageTransaction EndPoint Dto</param>
    /// <param name="dispenser">Dispenser which sends transaction</param>
    /// <returns>create update garage transaction dto</returns>
    private CreateUpdateGarageTransactionEndPointDto InitiateUnlinkedTransactionDto(EntryDto input )
    {
        return new CreateUpdateGarageTransactionEndPointDto
        {
            AccurateTime = DateTime.ParseExact(input.ActTime, "yyyyMMddHHmmss", CultureInfo.InvariantCulture),
            SnapTime = DateTime.ParseExact(input.ActTime, "yyyyMMddHHmmss", CultureInfo.InvariantCulture),
            VehiclePicture = input.CarImgData,
            PlateNumber = input.CarNum,
            DeviceID = input.LaneNO
        };
    }
    private CreateUpdateGarageTransactionEndPointDto InitiateUnlinkedTransactionDto(PreEntryDto input)
    {
        return new CreateUpdateGarageTransactionEndPointDto
        {
            AccurateTime = DateTime.ParseExact(input.ActTime, "yyyyMMddHHmmss", CultureInfo.InvariantCulture),
            SnapTime = DateTime.ParseExact(input.ActTime, "yyyyMMddHHmmss", CultureInfo.InvariantCulture),
            VehiclePicture = input.CarImgData,
            PlateNumber = input.CarNum,
            DeviceID = input.LaneNO
        };
    }
    public static string GetString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var prop) && prop.ValueKind != JsonValueKind.Null
            ? prop.GetString()
            : null;
    }

    public static DateTime ParseDateTime(JsonElement element, string propertyName, string format = "yyyyMMddHHmmss")
    {
        if (element.TryGetProperty(propertyName, out var prop) &&
            prop.ValueKind == JsonValueKind.String &&
            DateTime.TryParseExact(prop.GetString(), format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
        {
            return dt;
        }

        return DateTime.Now;
    }
    public static DateTime ParseDateTime( string input, string format = "yyyyMMddHHmmss")
    {
        if (DateTime.TryParseExact(input, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
        {
            return dt;
        }
        return DateTime.Now;
    }
    #endregion


}
