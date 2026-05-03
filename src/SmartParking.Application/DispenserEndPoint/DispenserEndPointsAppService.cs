using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartParking.BarrierAppService;
using SmartParking.CustomerPackages;
using SmartParking.CustomerPackages.CustomerPackageTransactions;
using SmartParking.CustomerPackages.Dtos;
using SmartParking.Entities.Cameras.Watchers;
using SmartParking.Entities.GarageTransactions;
using SmartParking.Enums;
using SmartParking.GarageTransactionEndPoints;
using SmartParking.GarageTransactions;
using SmartParking.Organizations;
using SmartParking.PackageCodeEndPoint;
using SmartParking.Utilities;
using System;
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
    private readonly ICustomerPackageHandlerAppService _customerPackagesHandlerAppService;
    private readonly IRepository<Dispenser, int> _dispenserRepository;
    private readonly ILogger<DispenserEndPointsAppService> _logger;
    private readonly IUnLinkedTransactionAppService _unlinkedGarageTransactionsAppService;

    // ── Response messages ────────────────────────────────────────────────────
    private const string PackageCodeNotFound = "Please confirm if the payment has been completed";
    private const string PlateOrTicketNotFound = "Ticket Id not found";
    private const string PackageNotFound = "Not have active package";
    private const string DispenserNotFound = "Dispenser code not valid";
    private const string OrganizationNotFound = "Not found organization for this dispenser";
    private const string OtherReasons = "Other";
    private const string MultipleRequests = "Multiple Request from Pre-Entry";

    public DispenserEndPointsAppService(
        IGarageTransactionEndPointsAppService garageTransactionEndPointsAppService,
        IBarrierAppService barrierAppService,
        IRepository<Dispenser, int> dispenserRepository,
        ILogger<DispenserEndPointsAppService> logger,
        ICustomerPackageHandlerAppService customerPackagesAppService,
        IUnLinkedTransactionAppService unlinkedGarageTransactionsAppService,
        IGarageTransactionUtilityAppService garageTransactionUtilityAppService)
    {
        _garageTransactionEndPointsAppService = garageTransactionEndPointsAppService;
        _barrierAppService = barrierAppService;
        _dispenserRepository = dispenserRepository;
        _logger = logger;
        _unlinkedGarageTransactionsAppService = unlinkedGarageTransactionsAppService;
        _garageTransactionUtilityAppService = garageTransactionUtilityAppService;
        _customerPackagesHandlerAppService = customerPackagesAppService;
    }

    // ── Public endpoints ─────────────────────────────────────────────────────

    [HttpPost("api/app/dispenser-end-points/entry/{OrganizationCode}")]
    public async Task<IBaseGateResponseDto> Entry(string OrganizationCode, EntryDto request)
    {
        _logger.LogError($"{DateTime.Now} => start entry");

        using (DataFilter.Disable<IMultiTenant>())
        {
            var (organization, dispenser, earlyExit) =
                await ValidateOrganizationAndDispenser(OrganizationCode, request.LaneNO, request.CarNum);

            if (earlyExit is not null) return earlyExit;

            if (string.IsNullOrEmpty(request.TicketID))
            {
                await _unlinkedGarageTransactionsAppService.CreateAsync(BuildUnlinkedDto(request));
                return FailResponse(request.CarNum, PlateOrTicketNotFound);
            }

            var transactionDto = BuildEntryTransactionDto(request, dispenser);

            var packageDto = await ResolveCustomerPackage(transactionDto);
            transactionDto.CustomerCategory = ResolveCustomerCategory(packageDto);

            var result = await _garageTransactionEndPointsAppService.HandleEntrance(
                BuildUnlinkedDto(request),
                BuildOrganizationSettings(organization),
                transactionDto,
                packageDto);

            await _barrierAppService.CallingBarrier(
                organization.Id,
                new BarrierRequest { carNum = request.CarNum, laneNO = request.LaneNO, operatorType = result ? 5 : 2 });

            return BuildGateResponse(request.CarNum, result, OtherReasons);
        }
    }

    [HttpPost("api/app/dispenser-end-points/pre-entry/{OrganizationCode}")]
    public async Task<IBaseGateResponseDto> PreEntry(string OrganizationCode, PackageCodeEntryDto request)
    {
        _logger.LogError($"{DateTime.Now} => start pre entry");

        using (DataFilter.Disable<IMultiTenant>())
        {
            if (string.IsNullOrEmpty(request.PackageCode))
            {
                await _unlinkedGarageTransactionsAppService.CreateAsync(BuildUnlinkedDto(request));
                return FailResponse(request.CarNum, PackageCodeNotFound);
            }

            var (organization, dispenser, earlyExit) =
                await ValidateOrganizationAndDispenser(OrganizationCode, request.LaneNO, request.CarNum);

            if (earlyExit is not null) return earlyExit;

            var transactionDto = BuildPreEntryTransactionDto(request, dispenser);
            var packageDto = await _customerPackagesHandlerAppService
                                       .GetOneActiveCustomerPackage(ToGarageTransactionDto(transactionDto));

            if (packageDto is null)
                return FailResponse(request.CarNum, PackageNotFound);

            var result = await _garageTransactionEndPointsAppService.HandleEntrance(
                BuildUnlinkedDto(request),
                BuildOrganizationSettings(organization),
                transactionDto,
                packageDto);

            return BuildGateResponse(request.CarNum, result, OtherReasons);
        }
    }

    [HttpPost("api/app/dispenser-end-points/exit/{OrganizationCode}")]
    public async Task<IBaseGateResponseDto> Exit(string OrganizationCode, ExitDto request)
    {
        _logger.LogError($"{DateTime.Now} => start exit");

        using (DataFilter.Disable<IMultiTenant>())
        {
            var (organization, dispenser, earlyExit) =
                await ValidateOrganizationAndDispenser(OrganizationCode, request.LaneNO, request.CarNum);

            if (earlyExit is not null) return earlyExit;

            if (string.IsNullOrEmpty(request.TicketId))
                return FailResponse(request.CarNum, PlateOrTicketNotFound);

            var transactionDto = BuildExitTransactionDto(request, dispenser);
            var endPointDto = ObjectMapper.Map<CreateUpdateGarageTransactionDto, CreateUpdateGarageTransactionEndPointDto>(transactionDto);

            if (await HasActivePackage(transactionDto))
                return FailResponse(request.CarNum, MultipleRequests);

            var packageDto = await ResolveCustomerPackage(transactionDto);
            transactionDto.CustomerCategory = ResolveCustomerCategory(packageDto);

            var result = await _garageTransactionEndPointsAppService
                               .HandleExit(transactionDto, organization, endPointDto, packageDto);

            return BuildGateResponse(request.CarNum, result, OtherReasons);
        }
    }

    [HttpPost("api/app/dispenser-end-points/pre-exit/{OrganizationCode}")]
    public async Task<IBaseGateResponseDto> PreExit(string OrganizationCode, PackageCodeExitDto request)
    {
        _logger.LogError($"{DateTime.Now} => start pre exit");

        using (DataFilter.Disable<IMultiTenant>())
        {
            // Delegate to normal Exit when no package code is present
            if (string.IsNullOrEmpty(request.PackageCode))
                return await Exit(OrganizationCode, ObjectMapper.Map<PackageCodeExitDto, ExitDto>(request));

            var (organization, dispenser, earlyExit) =
                await ValidateOrganizationAndDispenser(OrganizationCode, request.LaneNO, request.CarNum);

            if (earlyExit is not null) return earlyExit;

            var transactionDto = BuildPreExitTransactionDto(request, dispenser);
            var endPointDto = ObjectMapper.Map<CreateUpdateGarageTransactionDto, CreateUpdateGarageTransactionEndPointDto>(transactionDto);
            var packageDto = await _customerPackagesHandlerAppService
                                       .GetOneActiveCustomerPackage(ToGarageTransactionDto(transactionDto));

            if (packageDto is null)
                return FailResponse(request.CarNum, PackageNotFound);
            transactionDto.CustomerCategory = packageDto.CustomerCategory;

            var result = await _garageTransactionEndPointsAppService
                               .HandleExit(transactionDto, organization, endPointDto, packageDto);

            return BuildGateResponse(request.CarNum, result, OtherReasons);
        }
    }

    // ── Validation helpers ───────────────────────────────────────────────────

    /// <summary>
    /// Validates organization and dispenser in one shot.
    /// Returns a non-null <c>earlyExit</c> response when validation fails.
    /// </summary>
    private async Task<(TransactionOrganizationDto organization, Dispenser dispenser, IBaseGateResponseDto earlyExit)>
        ValidateOrganizationAndDispenser(string organizationCode, string laneNo, string carNum)
    {
        var organization = await _garageTransactionUtilityAppService.GetOrganization(organizationCode);
        if (organization is null || !organization.DispenserTransaction)
            return (null, null, FailResponse(carNum, OrganizationNotFound));

        var dispenser = await _dispenserRepository
            .FirstOrDefaultAsync(d => d.DeviceId == laneNo && d.OrganizationId == organization.Id);

        if (dispenser is null)
            return (organization, null, FailResponse(carNum, DispenserNotFound));

        return (organization, dispenser, null);
    }

    // ── Package helpers ──────────────────────────────────────────────────────

    private async Task<bool> HasActivePackage(CreateUpdateGarageTransactionDto dto)
    {
        var existing = await _customerPackagesHandlerAppService
            .GetOneActiveCustomerPackage(ToGarageTransactionDto(dto));
        return existing is not null;
    }

    private async Task<CustomerPackageTransactionDto> ResolveCustomerPackage(CreateUpdateGarageTransactionDto dto)
    {
        var customerId = await _garageTransactionUtilityAppService
            .CheckPlateNumberBelongsToCustomerToGetHisIdAsync(dto.PlateNumber);

        return new CustomerPackageTransactionDto
        {
            CustomerId = customerId ?? Guid.Empty
        };
    }

    private static CustomerCategoryStatus ResolveCustomerCategory(CustomerPackageTransactionDto packageDto) =>
        packageDto.CustomerId != Guid.Empty
            ? packageDto.CustomerCategory
            : CustomerCategoryStatus.Customer;

    // ── DTO builders ─────────────────────────────────────────────────────────

    private static OrganizationTransactionSettingsDto BuildOrganizationSettings(TransactionOrganizationDto org) =>
        new()
        {
            HasFreeSlots = org.HasFreeSlots,
            HasPackageOnly = org.HasPackageOnly,
            HasUserAccess = org.UserAccess,
            HasVehicleAccess = org.VehicleAccess
        };

    private static GarageTransactionDto ToGarageTransactionDto(CreateUpdateGarageTransactionDto dto) =>
        new()
        {
            OrganizationID = dto.OrganizationID,
            PlateNumber = dto.PlateNumber,
            TicketId = dto.TicketId,
            AccurateTime = dto.AccurateTime
        };

    private static CreateUpdateGarageTransactionDto BuildEntryTransactionDto(EntryDto input, Dispenser dispenser) =>
        new()
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
            CameraType = WatcherType.Entry,
            IsIn = true,
            IsMapped = false,
            TicketId = input.TicketID,
            CanCheckout = true
        };

    private static CreateUpdateGarageTransactionDto BuildPreEntryTransactionDto(PackageCodeEntryDto input, Dispenser dispenser) =>
        new()
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
            CameraType = WatcherType.Entry,
            IsIn = true,
            IsMapped = false,
            PackageCode = input.PackageCode,
            TicketId = input.TicketID,
            CanCheckout = true
        };

    private static CreateUpdateGarageTransactionDto BuildExitTransactionDto(ExitDto input, Dispenser dispenser) =>
        new()
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
            TicketId = input.TicketId,
            CameraType = WatcherType.Exit,
            IsIn = false,
            IsMapped = true,
            CanCheckout = false
        };

    private static CreateUpdateGarageTransactionDto BuildPreExitTransactionDto(PackageCodeExitDto input, Dispenser dispenser) =>
        new()
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
            IsMapped = true,
            TicketId = input.TicketID,
            PackageCode = input.PackageCode,
            CanCheckout = false
        };

    private static CreateUpdateGarageTransactionEndPointDto BuildUnlinkedDto(EntryDto input) =>
        new()
        {
            AccurateTime = ParseDateTime(input.ActTime),
            SnapTime = ParseDateTime(input.ActTime),
            VehiclePicture = input.CarImgData,
            PlateNumber = input.CarNum,
            DeviceID = input.LaneNO
        };

    private static CreateUpdateGarageTransactionEndPointDto BuildUnlinkedDto(PackageCodeEntryDto input) =>
        new()
        {
            AccurateTime = ParseDateTime(input.ActTime),
            SnapTime = ParseDateTime(input.ActTime),
            VehiclePicture = input.CarImgData,
            PlateNumber = input.CarNum,
            DeviceID = input.LaneNO
        };

    private static CreateUpdateGarageTransactionEndPointDto BuildUnlinkedDto(PackageCodeExitDto input) =>
        new()
        {
            AccurateTime = ParseDateTime(input.ActTime),
            SnapTime = ParseDateTime(input.ActTime),
            VehiclePicture = input.CarImgData,
            PlateNumber = input.CarNum,
            DeviceID = input.LaneNO
        };

    // ── Response builders ────────────────────────────────────────────────────

    private IBaseGateResponseDto BuildGateResponse(string carNum, bool open, string reason = null)
    {
        IBaseGateResponseDto response = open
            ? new OpenGateResponseDto { CarNum = carNum }
            : new NotToOpenGateResponseDto
            {
                CarNum = carNum,
                ResMsg = string.IsNullOrWhiteSpace(reason) ? "Vehicle is not allowed to enter" : reason
            };

        _logger.LogError($"{DateTime.Now} ====>  {JsonSerializer.Serialize(response)}");
        return response;
    }

    private IBaseGateResponseDto FailResponse(string carNum, string reason)
    {
        _logger.LogInformation(reason);
        return BuildGateResponse(carNum, false, reason);
    }

    // ── Utilities ─────────────────────────────────────────────────────────────

    public static DateTime ParseDateTime(string input, string format = "yyyyMMddHHmmss") =>
        DateTime.TryParseExact(input, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt)
            ? dt
            : DateTime.Now;

    public static string GetString(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var prop) && prop.ValueKind != JsonValueKind.Null
            ? prop.GetString()
            : null;

    public static DateTime ParseDateTime(JsonElement element, string propertyName, string format = "yyyyMMddHHmmss") =>
        element.TryGetProperty(propertyName, out var prop)
            && prop.ValueKind == JsonValueKind.String
            && DateTime.TryParseExact(prop.GetString(), format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt)
                ? dt
                : DateTime.Now;
}