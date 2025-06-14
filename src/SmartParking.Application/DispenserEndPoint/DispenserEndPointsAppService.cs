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
    public Task<bool> Entry(EntryDto request)
    {
        return Task.FromResult(true);
    }

    public async Task<bool> Exit(ExitDto request)
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
            if (_dispenser == null)
            {
                _logger.LogInformation($"No Organization for : {request.LaneNO}");
                return false;
            }
            var _garageTransactionDto = InitiateCreateGarageTransactionDtoForExit(request, _dispenser);
            var result = await _garageTransactionEndPointsAppService.HandleExit(_garageTransactionDto, _organization.Id);
            await _barrierAppService.CallingBarrier(_organization.Id, new BarrierRequest { carNum = request.CarNum, laneNO = request.LaneNO, operatorType = result == false ? 2 : 5 });
            return result;
        }
    }

    public async Task<bool> PreEntry(PreEntryDto request)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {

            var _dispenser = await _dispenserRepository.FirstOrDefaultAsync(dis=>dis.DeviceId==request.LaneNO);
            if (_dispenser == null)
            {
                _logger.LogInformation($"No Dispenser for : {request.LaneNO}");
                return false;
            }
            var _organization = await _organizationRepository.FirstOrDefaultAsync(org=>org.Id==_dispenser.OrganizationId);
            if (_dispenser == null)
            {
                _logger.LogInformation($"No Organization for : {request.LaneNO}");
                return false;
            }
            var _unlinkedDto= InitiateUnlinkedTransactionDto(request);
            var _garageTransactionDto = InitiateCreateGarageTransactionDtoForPreEntry(request, _dispenser);

            var result =await _garageTransactionEndPointsAppService.HandleEntrance(_unlinkedDto, _organization.HasFreeSlots, _organization.HasPackageOnly, _garageTransactionDto);
            await _barrierAppService.CallingBarrier(_organization.Id , new BarrierRequest { carNum=request.CarNum,laneNO=request.LaneNO,operatorType = result ==false ? 2 :5});
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
    private CreateUpdateGarageTransactionDto InitiateCreateGarageTransactionDtoForPreEntry(PreEntryDto input,Dispenser dispenser )
    {
        return new CreateUpdateGarageTransactionDto
        {
            AccurateTime = DateTime.ParseExact(input.ActTime, "yyyyMMddHHmmss", CultureInfo.InvariantCulture),
            SnapTime = DateTime.ParseExact(input.ActTime, "yyyyMMddHHmmss", CultureInfo.InvariantCulture),
            VehiclePicture = input.CarImgData,
            PlateNumber = input.CarNum,
            CameraId = dispenser.Id,
            GarageId = dispenser.GarageId,
            OrganizationID = dispenser.OrganizationId,
            TenantId = dispenser.TenantId,
            CameraType = input.LaneType==0?WatcherType.Entry:WatcherType.Exit,
            IsIn = input.LaneType == 1 ? false : true,
            IsMapped = input.LaneType == 0 ? false : true,

        };
    }
    /// <summary>
    /// Method used to initate a create garage transaction dto.
    /// </summary>
    /// <param name="input">CreateUpdateGarageTransaction EndPoint Dto</param>
    /// <param name="dispenser">Dispenser which sends transaction</param>
    /// <returns>create update garage transaction dto</returns>
    private CreateUpdateGarageTransactionDto InitiateCreateGarageTransactionDtoForExit(ExitDto input,Dispenser dispenser )
    {
        return new CreateUpdateGarageTransactionDto
        {
            AccurateTime = DateTime.ParseExact(input.ActTime, "yyyyMMddHHmmss", CultureInfo.InvariantCulture),
            SnapTime = DateTime.ParseExact(input.ActTime, "yyyyMMddHHmmss", CultureInfo.InvariantCulture),
            VehiclePicture = input.CarImgData,
            PlateNumber = input.CarNum,
            CameraId = dispenser.Id,
            GarageId = dispenser.GarageId,
            OrganizationID = dispenser.OrganizationId,
            TenantId = dispenser.TenantId,
            CameraType = input.LaneType==0?WatcherType.Entry:WatcherType.Exit,
            IsIn = input.LaneType == 1 ? false : true,
            IsMapped = input.LaneType == 0 ? false : true,

        };
    }
    
    /// <summary>
    /// Method used to initate a create garage transaction dto.
    /// </summary>
    /// <param name="input">CreateUpdateGarageTransaction EndPoint Dto</param>
    /// <param name="dispenser">Dispenser which sends transaction</param>
    /// <returns>create update garage transaction dto</returns>
    private CreateUpdateGarageTransactionEndPointDto InitiateUnlinkedTransactionDto(PreEntryDto input )
    {
        return new CreateUpdateGarageTransactionEndPointDto
        {
            AccurateTime = DateTime.ParseExact(input.ActTime, "yyyyMMddHHmmss", CultureInfo.InvariantCulture),
            SnapTime = DateTime.ParseExact(input.ActTime, "yyyyMMddHHmmss", CultureInfo.InvariantCulture),
            VehiclePicture = input.CarImgData,
            PlateNumber = input.CarNum,
        };
    }
    #endregion


}
