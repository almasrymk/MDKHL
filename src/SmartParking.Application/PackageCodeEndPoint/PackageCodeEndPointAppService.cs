using Microsoft.Extensions.Logging;
using SmartParking.BarrierAppService;
using SmartParking.CustomerPackages;
using SmartParking.CustomerPackages.CustomerPackageTransactions;
using SmartParking.DispenserEndPoint;
using SmartParking.Entities.Cameras.Watchers;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using SmartParking.GarageTransactionEndPoints;
using SmartParking.GarageTransactions;
using SmartParking.Utilities;
using System;
using System.Globalization;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SmartParking.PackageCodeEndPoint;
/// <summary>
/// Service For  handle entry and exit from garage with Package Code 
/// </summary>
public class PackageCodeEndPointAppService : SmartParkingAppService, IPackageCodeEndpointAppService
{
    private readonly IGarageTransactionEndPointsAppService _garageTransactionEndPointsAppService;
    private readonly IGarageTransactionUtilityAppService _garageTransactionUtilityAppService;
    private readonly IBarrierAppService _barrierAppService;
    private readonly ICustomerPackageHandlerAppService _customerPackagesHandlerAppService;
    private readonly IRepository<Dispenser, int> _dispenserRepository;
    private readonly IRepository<Organization, int> _organizationRepository;
    private readonly ILogger<DispenserEndPointsAppService> _logger;
    private readonly IUnLinkedTransactionAppService _unlinkedGarageTransactionsAppService;

    public PackageCodeEndPointAppService(IGarageTransactionEndPointsAppService garageTransactionEndPointsAppService, IGarageTransactionUtilityAppService garageTransactionUtilityAppService, IBarrierAppService barrierAppService, ICustomerPackageHandlerAppService customerPackagesHandlerAppService, IRepository<Dispenser, int> dispenserRepository, IRepository<Organization, int> organizationRepository, ILogger<DispenserEndPointsAppService> logger, IUnLinkedTransactionAppService unlinkedGarageTransactionsAppService)
    {
        _garageTransactionEndPointsAppService = garageTransactionEndPointsAppService;
        _garageTransactionUtilityAppService = garageTransactionUtilityAppService;
        _barrierAppService = barrierAppService;
        _customerPackagesHandlerAppService = customerPackagesHandlerAppService;
        _dispenserRepository = dispenserRepository;
        _organizationRepository = organizationRepository;
        _logger = logger;
        _unlinkedGarageTransactionsAppService = unlinkedGarageTransactionsAppService;
        _customerPackagesHandlerAppService = customerPackagesHandlerAppService;
    }
    #region public methods
    public async Task<bool> PackageCodeEntry(PackageCodeEntryDto packageCodeEntry)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            var unlinkedDto = InitiateUnlinkedTransactionDto(packageCodeEntry);
            if(string.IsNullOrEmpty(packageCodeEntry.PackageCode))
            {
                await _unlinkedGarageTransactionsAppService.CreateAsync(unlinkedDto);
                Logger.LogInformation($"Should enter Package Code to handle entrance with package code ");
                return false;
            }
            var _dispenser = await _dispenserRepository.FirstOrDefaultAsync(dis => dis.DeviceId == packageCodeEntry.LaneNO);
            if (_dispenser == null)
            {
                _logger.LogInformation($"No Dispenser for : {packageCodeEntry.LaneNO}");
                return false;
            }
            var _organization = await _garageTransactionUtilityAppService.GetOrganization(_dispenser.OrganizationId);
            if (_organization == null || _organization.DispenserTransaction == false)
            {
                _logger.LogInformation($"No Organization for : {packageCodeEntry.LaneNO}");
                return false;
            }
            var _garageTransactionDto = InitalizeCreateUpdateGarageTransactionDto(packageCodeEntry, _dispenser);
            var packageDto = await _customerPackagesHandlerAppService.GetOneActiveCustomerPackage(new GarageTransactionDto { OrganizationID = _garageTransactionDto.OrganizationID, PlateNumber = _garageTransactionDto.PlateNumber, TicketId = _garageTransactionDto.TicketId, AccurateTime = _garageTransactionDto.AccurateTime });
            var result = await _garageTransactionEndPointsAppService.HandleEntrance(unlinkedDto, _organization.HasFreeSlots, _organization.HasPackageOnly, _garageTransactionDto, packageDto);
            if(result)
            {
              await _barrierAppService.CallingBarrier(_organization.Id, new BarrierRequest { carNum = packageCodeEntry.CarNum, laneNO = packageCodeEntry.LaneNO, operatorType = 5 });
            }
            return result;
        }
    }

    public async Task<bool> PackageCodeExit(PackageCodeExitDto packageCodeExit)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            _logger.LogError($"{DateTime.Now} => start pre exit ");
            _logger.LogInformation($"Lane Number is found : {packageCodeExit.LaneNO}");
            var unlinkedDto = InitiateUnlinkedTransactionDto(packageCodeExit);
            if (string.IsNullOrEmpty(packageCodeExit.PackageCode))
            {
                await _unlinkedGarageTransactionsAppService.CreateAsync(unlinkedDto);
                Logger.LogInformation($"Should enter Package Code to handle exit with package code ");
                return false;
            }
            var _dispenser = await _dispenserRepository.FirstOrDefaultAsync(dis => dis.DeviceId == packageCodeExit.LaneNO);
            if (_dispenser == null)
            {
                _logger.LogInformation($"No Dispenser for : {packageCodeExit.LaneNO}");
                return false;
            }
            _logger.LogInformation($"Dispenser is found");
            var _organization = await _garageTransactionUtilityAppService.GetOrganization(_dispenser.OrganizationId);
            if (_organization == null || _organization.DispenserTransaction == false)
            {
                _logger.LogInformation($"No Organization for : {packageCodeExit.LaneNO}");
                return false;
            }
            _logger.LogInformation($"Organization is found");
            var _garageTransactionDto = InitalizeCreateUpdateGarageTransactionDto(packageCodeExit, _dispenser);
            var input = ObjectMapper.Map<CreateUpdateGarageTransactionDto, CreateUpdateGarageTransactionEndPointDto>(_garageTransactionDto);
            var packageDto = await _customerPackagesHandlerAppService.GetOneActiveCustomerPackage(new GarageTransactionDto { OrganizationID = _garageTransactionDto.OrganizationID, PlateNumber = _garageTransactionDto.PlateNumber, TicketId = _garageTransactionDto.TicketId, AccurateTime = _garageTransactionDto.AccurateTime });
            var result = await _garageTransactionEndPointsAppService.HandleExit(_garageTransactionDto, _organization, input, packageDto);
            if(result)
            {
               await _barrierAppService.CallingBarrier(_organization.Id, new BarrierRequest { carNum = _garageTransactionDto.PlateNumber, laneNO = _garageTransactionDto.LaneNO, operatorType = 5 });
            }
            return result;
        }
    }
    #endregion
    #region Private methods
    private CreateUpdateGarageTransactionDto InitalizeCreateUpdateGarageTransactionDto(PackageCodeEntryDto packageCodeEntryDto , Dispenser dispenser )
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
            IsMapped = false , 
            PackageCode = packageCodeEntryDto.PackageCode , 
            TicketId = packageCodeEntryDto.TicketID
        };
    }
    private CreateUpdateGarageTransactionDto InitalizeCreateUpdateGarageTransactionDto(PackageCodeExitDto packageCodeExitDto , Dispenser dispenser)
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
            IsMapped = true , 
            TicketId = packageCodeExitDto.TicketID  , 
            PackageCode = packageCodeExitDto.PackageCode
        };
    }
    /// <summary>
    /// Method used to initate a create garage transaction dto.
    /// </summary>
    /// <param name="input">CreateUpdateGarageTransaction EndPoint Dto</param>
    /// <param name="dispenser">Dispenser which sends transaction</param>
    /// <returns>create update garage transaction dto</returns>
    private CreateUpdateGarageTransactionEndPointDto InitiateUnlinkedTransactionDto(PackageCodeEntryDto input)
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
    private CreateUpdateGarageTransactionEndPointDto InitiateUnlinkedTransactionDto(PackageCodeExitDto input)
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
    public static DateTime ParseDateTime(string input, string format = "yyyyMMddHHmmss")
    {
        if (DateTime.TryParseExact(input, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
        {
            return dt;
        }
        return DateTime.Now;
    }
    #endregion

}
