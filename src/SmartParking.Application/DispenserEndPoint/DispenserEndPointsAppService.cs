using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SmartParking.BarrierAppService;
using SmartParking.CustomerPackages;
using SmartParking.Entities.Cameras;
using SmartParking.Entities.Cameras.Watchers;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using SmartParking.GarageTransactionEndPoints;
using SmartParking.GarageTransactions;
using SmartParking.Packages;
using SmartParking.WebSockets;
using SmartParking.Utilities;
using System;
using System.Globalization;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using static QRCoder.PayloadGenerator;
using System.Globalization;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SmartParking.DispenserEndPoint;

public class DispenserEndPointsAppService : SmartParkingAppService, IDispenserEndPointsAppService
{
    private readonly IGarageTransactionEndPointsAppService _garageTransactionEndPointsAppService;
    private readonly IGarageTransactionUtilityAppService _garageTransactionUtilityAppService;
    private readonly IBarrierAppService _barrierAppService;
    private readonly ICustomerPackagesAppService _customerPackagesAppService;
    private readonly IRepository<Dispenser, int> _dispenserRepository;
    private readonly IRepository<Organization, int> _organizationRepository;
    private readonly ILogger<DispenserEndPointsAppService> _logger;
    private readonly IUnLinkedTransactionAppService _unlinkedGarageTransactionsAppService;

    public DispenserEndPointsAppService(IGarageTransactionEndPointsAppService garageTransactionEndPointsAppService, IBarrierAppService barrierAppService, IRepository<Dispenser, int> dispenserRepository, ILogger<DispenserEndPointsAppService> logger, IRepository<Organization, int> organizationRepository, ICustomerPackagesAppService customerPackagesAppService , IUnLinkedTransactionAppService unlinkedGarageTransactionsAppService, IGarageTransactionUtilityAppService garageTransactionUtilityAppService)
    {
        _garageTransactionEndPointsAppService = garageTransactionEndPointsAppService;
        _barrierAppService = barrierAppService;
        _dispenserRepository = dispenserRepository;
        _logger = logger;
        _organizationRepository = organizationRepository;
        _unlinkedGarageTransactionsAppService = unlinkedGarageTransactionsAppService;
        _garageTransactionUtilityAppService = garageTransactionUtilityAppService;
        _customerPackagesAppService = customerPackagesAppService;
    }
    #region public methods
    public async Task<bool> Entry(EntryDto request)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            var _unlinkedDto = InitiateUnlinkedTransactionDto(request);
            var _dispenser = await _dispenserRepository.FirstOrDefaultAsync(dis => dis.DeviceId == request.LaneNO);
            if (_dispenser == null)
            {
                _logger.LogInformation($"No Dispenser for : {request.LaneNO}");
                return false;
            }
            if(string.IsNullOrEmpty(request.TicketID))
            {
                await _unlinkedGarageTransactionsAppService.CreateAsync(_unlinkedDto);
                _logger.LogInformation($"Should have ticket id ");
                return false;
            }
            var _organization = await _organizationRepository.FirstOrDefaultAsync(org => org.Id == _dispenser.OrganizationId);
            if (_organization == null || _organization.DispenserTransaction == false)
            {
                _logger.LogInformation($"No Organization for : {request.LaneNO}");
                return false;
            }
            var _garageTransactionDto = InitiateCreateGarageTransactionDtoForPreEntry(request, _dispenser);
            var packageDto = await _customerPackagesAppService.GetOneActiveCustomerPackageByOrganizationIdAndPlateNumberForGarageTransaction(_organization.Id, _garageTransactionDto.PlateNumber, _garageTransactionDto.AccurateTime.Value);
            var result = await _garageTransactionEndPointsAppService.HandleEntrance(_unlinkedDto, _organization.HasFreeSlots, _organization.HasPackageOnly, _organization.DispenserTransaction , _organization.CameraTransaction ,  _garageTransactionDto  , packageDto);
            await _barrierAppService.CallingBarrier(_organization.Id, new BarrierRequest { carNum = request.CarNum, laneNO = request.LaneNO, operatorType = result == false ? 2 : 5 });
            return result;
        }
    }
    public async Task<bool> PreEntry(PreEntryDto request)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            var _dispenser = await _dispenserRepository.FirstOrDefaultAsync(dis => dis.DeviceId == request.LaneNO);
            if (_dispenser == null)
            {
                _logger.LogInformation($"No Dispenser for : {request.LaneNO}");
                return false;
            }
            var _organization = await _organizationRepository.FirstOrDefaultAsync(org => org.Id == _dispenser.OrganizationId);
            if (_organization == null || _organization.DispenserTransaction == false)
            {
                _logger.LogInformation($"No Organization for : {request.LaneNO}");
                return false;
            }
            var _unlinkedDto = InitiateUnlinkedTransactionDto(request);
            var _garageTransactionDto = InitiateCreateGarageTransactionDtoForPreEntry(request, _dispenser);
            var packageDto = await _customerPackagesAppService.GetOneActiveCustomerPackageByOrganizationIdAndPlateNumberForGarageTransaction(_organization.Id, _garageTransactionDto.PlateNumber, _garageTransactionDto.AccurateTime.Value);
            var result = await _garageTransactionEndPointsAppService.HandleEntrance(_unlinkedDto, _organization.HasFreeSlots,true , _organization.DispenserTransaction , _organization.CameraTransaction, _garageTransactionDto , packageDto);
            if(result)
            {
                   _logger.LogError($"Open Barrier From Pre Entry : {request.CarNum} : {request.LaneNO}");
                   await _barrierAppService.CallingBarrier(_organization.Id, new BarrierRequest { carNum = request.CarNum, laneNO = request.LaneNO, operatorType = result == false ? 2 : 5 });
            }
            return result;
        }
    }
    public async Task<bool> Exit(ExitDto request)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            _logger.LogError($"{DateTime.Now} => start pre exit ");
            var laneNo = request.LaneNO;
            _logger.LogInformation($"Lane Number is found : {laneNo}");
            var _dispenser = await _dispenserRepository.FirstOrDefaultAsync(dis => dis.DeviceId == laneNo);
            if (_dispenser == null)
            {
                _logger.LogInformation($"No Dispenser for : {laneNo}");
                return false;
            }
            _logger.LogInformation($"Dispenser is found");
            var _organization = await _garageTransactionUtilityAppService.GetOrganization(_dispenser.OrganizationId);
            if (_organization == null || _organization.DispenserTransaction == false)
            {
                _logger.LogInformation($"No Organization for : {laneNo}");
                return false;
            }
            _logger.LogInformation($"Organization is found");

            var _garageTransactionDto = InitiateCreateGarageTransactionDtoForExit(request, _dispenser);

            var input = ObjectMapper.Map<CreateUpdateGarageTransactionDto, CreateUpdateGarageTransactionEndPointDto>(_garageTransactionDto);
            if (_garageTransactionDto.PlateNumber == null && _garageTransactionDto.TicketId == null)
            {
                return false;
            }
            var packageDto = await _customerPackagesAppService.GetOneActiveCustomerPackageByOrganizationIdAndPlateNumberForGarageTransaction(_organization.Id, _garageTransactionDto.PlateNumber, _garageTransactionDto.AccurateTime.Value);
            var result = await _garageTransactionEndPointsAppService.HandleExit(_garageTransactionDto, _organization, input  , packageDto);
            await _barrierAppService.CallingBarrier(_organization.Id, new BarrierRequest { carNum = _garageTransactionDto.PlateNumber, laneNO = _garageTransactionDto.LaneNO, operatorType = result == false ? 2 : 5 });
            return result;
        }
    }
    public async Task<bool> PreExit(JsonElement request)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            _logger.LogError($"{DateTime.Now} => start pre exit ");
            var laneNo = GetString(request , "laneNO");
            _logger.LogInformation($"Lane Number is found : {laneNo}");
            var _dispenser = await _dispenserRepository.FirstOrDefaultAsync(dis => dis.DeviceId == laneNo);
            if (_dispenser == null)
            {
                _logger.LogInformation($"No Dispenser for : {laneNo}");
                return false;
            }
            _logger.LogInformation($"Dispenser is found");

            var _organization = await _garageTransactionUtilityAppService.GetOrganization(_dispenser.OrganizationId);
            if (_organization == null || _organization.DispenserTransaction == false)
            {
                _logger.LogInformation($"No Organization for : {laneNo}");
                return false;
            }
            _logger.LogInformation($"Organization is found");

            var unlinkedDto = InitiateUnlinkedTransactionDto(request); 

            var _garageTransactionDto = InitiateCreateGarageTransactionDtoForExit(request, _dispenser);

            var input = ObjectMapper.Map<CreateUpdateGarageTransactionDto, CreateUpdateGarageTransactionEndPointDto>(_garageTransactionDto);
            if  (string.IsNullOrEmpty(_garageTransactionDto.TicketId) )
            {
                // Unlinked Transaction 
                await _unlinkedGarageTransactionsAppService.CreateAsync(unlinkedDto);
                return false;
            }
            var packageDto = await _customerPackagesAppService.GetOneActiveCustomerPackageByOrganizationIdAndPlateNumberForGarageTransaction(_organization.Id, _garageTransactionDto.PackageCode, _garageTransactionDto.AccurateTime.Value);
            var result = await _garageTransactionEndPointsAppService.HandleExit(_garageTransactionDto,_organization,  input  , packageDto);
            await _barrierAppService.CallingBarrier(_organization.Id, new BarrierRequest { carNum = _garageTransactionDto.PlateNumber, laneNO = _garageTransactionDto.LaneNO, operatorType = result == false ? 2 : 5 });
            return result;
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

        };
    }

    private CreateUpdateGarageTransactionDto InitiateCreateGarageTransactionDtoForPreEntry(PreEntryDto input, Dispenser dispenser)
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
            IsMapped = false
        };
    }
    /// <summary>
    /// Method used to initate a create garage transaction dto.
    /// </summary>
    /// <param name="input">CreateUpdateGarageTransaction EndPoint Dto</param>
    /// <param name="dispenser">Dispenser which sends transaction</param>
    /// <returns>create update garage transaction dto</returns>

    private CreateUpdateGarageTransactionDto InitiateCreateGarageTransactionDtoForExit(JsonElement input, Dispenser dispenser)
    {
        return new CreateUpdateGarageTransactionDto
        {
            AccurateTime = ParseDateTime(input, "actTime"),
            SnapTime = ParseDateTime(input, "actTime"),
            VehiclePicture = GetString(input, "carImgData"),
            PlateNumber = GetString(input, "carNum"),
            DispenserId = dispenser.Id,
            LaneNO = GetString(input, "laneNO"),
            GarageId = dispenser.GarageId,
            OrganizationID = dispenser.OrganizationId,
            TenantId = dispenser.TenantId,
            CameraType = WatcherType.Exit,
            IsIn = false,
            IsMapped = true,
            TicketId = GetString(input, "ticketID"),
            PackageCode=GetString(input,"PackageCode")  

        };
    }
    private CreateUpdateGarageTransactionEndPointDto InitiateUnlinkedTransactionDto (JsonElement request)
    {
        return new CreateUpdateGarageTransactionEndPointDto
        {
            AccurateTime = ParseDateTime(request, "actTime"),
            VehiclePicture = GetString(request, "carImgData"),
            PlateNumber = GetString(request, "carNum"),
            DeviceID = GetString(request, "laneNO"),

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
            CameraType = WatcherType.Exit,
            IsIn = false,
            IsMapped = true
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
