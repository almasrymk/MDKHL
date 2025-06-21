using Volo.Abp.Domain.Repositories;
using SmartParking.BarrierAppService;
using SmartParking.Entities.Cameras;
using SmartParking.Entities.Cameras.Watchers;
using SmartParking.Enums;
using SmartParking.GarageTransactionEndPoints;
using SmartParking.GarageTransactions;
using SmartParking.WebSockets;
using System.Threading.Tasks;
using System.Globalization;
using System;
using Microsoft.Extensions.Logging;
using SmartParking.Entities.Organizations;
using Volo.Abp.MultiTenancy;
using static QRCoder.PayloadGenerator;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace SmartParking.DispenserEndPoint;

public class DispenserEndPointsAppService : SmartParkingAppService, IDispenserEndPointsAppService
{
    private readonly IGarageTransactionEndPointsAppService _garageTransactionEndPointsAppService;
    private readonly IBarrierAppService _barrierAppService;
    private readonly IRepository<Dispenser, int> _dispenserRepository;
    private readonly IRepository<Organization, int> _organizationRepository;
    private readonly ILogger<DispenserEndPointsAppService> _logger;

    public DispenserEndPointsAppService(IGarageTransactionEndPointsAppService garageTransactionEndPointsAppService, IBarrierAppService barrierAppService, IRepository<Dispenser, int> dispenserRepository, ILogger<DispenserEndPointsAppService> logger, IRepository<Organization, int> organizationRepository)
    {
        _garageTransactionEndPointsAppService = garageTransactionEndPointsAppService;
        _barrierAppService = barrierAppService;
        _dispenserRepository = dispenserRepository;
        _logger = logger;
        _organizationRepository = organizationRepository;
    }
    #region public methods
    public async Task<bool> Entry(EntryDto request)
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

            var result = await _garageTransactionEndPointsAppService.HandleEntrance(_unlinkedDto, _organization.HasFreeSlots, _organization.HasPackageOnly, _garageTransactionDto);
            await _barrierAppService.CallingBarrier(_organization.Id, new BarrierRequest { carNum = request.CarNum, laneNO = request.LaneNO, operatorType = result == false ? 2 : 5 });
            return result;
        }
    }

    public Task<bool> Exit(ExitDto request)
    {
        return Task.FromResult(true);

    }
    public async Task<bool> PreExit(JsonElement request)
    {
        string ticketId = null;
        string laneNo = null;
        string image = null;

        if (request.TryGetProperty("ticketID", out var ticketProp))
            ticketId = ticketProp.GetString();

        if (request.TryGetProperty("laneNO", out var laneProp))
            laneNo = laneProp.GetString();

        if (request.TryGetProperty("carImgData", out var imageProp))
            image = imageProp.GetString();
        using (DataFilter.Disable<IMultiTenant>())
        {
            var _dispenser = await _dispenserRepository.FirstOrDefaultAsync(dis => dis.DeviceId == laneNo);
            if (_dispenser == null)
            {
                _logger.LogInformation($"No Dispenser for : {laneNo}");
                return false;
            }
            var _organization = await _organizationRepository.FirstOrDefaultAsync(org => org.Id == _dispenser.OrganizationId);
            if (_organization == null || _organization.DispenserTransaction == false)
            {
                _logger.LogInformation($"No Organization for : {laneNo}");
                return false;
            }
            var _garageTransactionDto = InitiateCreateGarageTransactionDtoForExit(request, _dispenser);
            if(_garageTransactionDto.PlateNumber == null &&  _garageTransactionDto.TicketId == null)
            {
                return false;
            }
            var result = await _garageTransactionEndPointsAppService.HandleExit(_garageTransactionDto, _organization.Id);
            await _barrierAppService.CallingBarrier(_organization.Id, new BarrierRequest { carNum = _garageTransactionDto.PlateNumber, laneNO = _garageTransactionDto.LaneNO, operatorType = result == false ? 2 : 5 });
            return result;
        }
    }
    public Task<bool> PreEntry(PreEntryDto request)
    {
       return Task.FromResult(true);


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
    /// <summary>
    /// Method used to initate a create garage transaction dto.
    /// </summary>
    /// <param name="input">CreateUpdateGarageTransaction EndPoint Dto</param>
    /// <param name="dispenser">Dispenser which sends transaction</param>
    /// <returns>create update garage transaction dto</returns>
    private CreateUpdateGarageTransactionDto InitiateCreateGarageTransactionDtoForExit(JsonElement input, Dispenser dispenser)
    {
        string GetString(JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out var prop) ? prop.GetString() : null;
        }

        DateTime ParseDateTime(string dateStr)
        {
            if (DateTime.TryParseExact(dateStr, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                return result;

            return DateTime.Now;
        }

        var actTimeStr = GetString(input, "actTime");

        return new CreateUpdateGarageTransactionDto
        {
            AccurateTime = ParseDateTime(actTimeStr),
            SnapTime = ParseDateTime(actTimeStr),
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
            TicketId = GetString(input, "ticketID")
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

   
    #endregion


}
