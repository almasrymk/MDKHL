using SmartParking.Entities.Cameras.Watchers;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Shifts;
using SmartParking.Enums;
using SmartParking.Organizations;
using SmartParking.Results;
using SmartParking.Shifts.ShiftZones;
using SmartParking.UserShifts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.Shifts;

[RemoteService(false)]
public class ShiftZoneActionAppService : SmartParkingAppService, IShiftZoneActionAppService
{
    private readonly IRepository<ShiftZone, Guid> _shiftZoneRepository;
    private readonly IRepository<Watcher, int> _watcherRepository;
    private readonly IOrganizationsAppService organizationsAppService;

    public ShiftZoneActionAppService(
        IRepository<Watcher, int> watcherRepository,
        IRepository<ShiftZone, Guid> shiftZoneRepository,
        IOrganizationsAppService organizationsAppService)
    {
        _shiftZoneRepository = shiftZoneRepository;
        _watcherRepository = watcherRepository;
        this.organizationsAppService = organizationsAppService;
    }
    /// <inheritdoc/>
    public async Task<Result<bool>> GetCurrentUserActiveShiftZoneStatusAsync()
    {
        if (!CurrentUser.Id.HasValue)
            return Result<bool>.Invalid(new ValidationError
            {
                ErrorCode = "400",
                ErrorMessage = "User not authenticated."
            });
        var today = DateOnly.FromDateTime(DateTime.Now);
        var timeOfDay = TimeOnly.FromDateTime(DateTime.Now);
        var query = await _shiftZoneRepository.WithDetailsAsync(
                        x => x.ShiftDetails);
        var activeShiftsQuery = query
            .Where(sz => sz.StartDate <= today && sz.EndDate >= today)
            .Where(sz => sz.Status == Status.Active)
            .Where(sz => sz.IsFullDay ||
                ((sz.StartTime <= sz.EndTime && sz.StartTime <= timeOfDay && sz.EndTime >= timeOfDay) ||
                 (sz.StartTime > sz.EndTime && (sz.StartTime <= timeOfDay || sz.EndTime >= timeOfDay))));
        var isActiveInShiftZone = await AsyncExecuter.AnyAsync(activeShiftsQuery.Where(sz =>
            sz.ShiftManager.ManagerUserName == CurrentUser.UserName ||
            sz.ShiftDetails.Any(sd => sd.ShiftStaffDetails.StaffId == CurrentUser.Id)));
        return Result<bool>.Success(isActiveInShiftZone);
    }


    /// <inheritdoc/>
    public async Task<Result<ICollection<ShiftWatcherActionDto>>> GetShiftZoneDevicesDataAsync()
    {
        if (!CurrentUser.Id.HasValue)
            return Result<ICollection<ShiftWatcherActionDto>>.Invalid(new ValidationError
            {
                ErrorCode = "400",
                ErrorMessage = "User not authenticated."
            });

        var currentOrganization = await organizationsAppService.GetOrganizationForCurrentTenatAsync();
        if(currentOrganization.OrganizationSettings.UseShifts==false)
        {
            var allWatcherQuery = await _watcherRepository.GetQueryableAsync();
            var allWatchers = await AsyncExecuter.ToListAsync(allWatcherQuery.Where(w => w.Status == Status.Active));
            return Result<ICollection<ShiftWatcherActionDto>>.Success(ObjectMapper.Map<List<Watcher>, List<ShiftWatcherActionDto>>(allWatchers));
        }
        var today = DateOnly.FromDateTime(DateTime.Now);
        var timeOfDay = TimeOnly.FromDateTime(DateTime.Now);

        var query = await _shiftZoneRepository.WithDetailsAsync(
                        x => x.ShiftDetails);

        var activeShiftsQuery = query
            .Where(sz => sz.StartDate <= today && sz.EndDate >= today)
            .Where(sz => sz.Status == Status.Active)
            .Where(sz => sz.IsFullDay ||
                ((sz.StartTime <= sz.EndTime && sz.StartTime <= timeOfDay && sz.EndTime >= timeOfDay) ||
                 (sz.StartTime > sz.EndTime && (sz.StartTime <= timeOfDay || sz.EndTime >= timeOfDay))));

        var activeShift = activeShiftsQuery.FirstOrDefault();

        int? organizationId = activeShift?.OrganizationId ?? query.FirstOrDefault()?.OrganizationId;

        if (!organizationId.HasValue)
            return Result<ICollection<ShiftWatcherActionDto>>.Invalid(new ValidationError
            {
                ErrorCode = "400",
                ErrorMessage = "Organization not found."
            });

        IQueryable<ShiftZone> finalActiveShiftsQuery = currentOrganization.OrganizationSettings?.UseShifts == true
            ? activeShiftsQuery
            : query.Where(sz => sz.OrganizationId == organizationId.Value);

        var isManager = finalActiveShiftsQuery
            .Any(sz => sz.ShiftManager.ManagerUserName == CurrentUser.UserName);

        List<ShiftZone> shiftZones;

        if (isManager)
        {
            shiftZones = await AsyncExecuter.ToListAsync(
                finalActiveShiftsQuery.Where(sz => sz.ShiftManager.ManagerUserName == CurrentUser.UserName));
        }
        else
        {
            shiftZones = await AsyncExecuter.ToListAsync(
                finalActiveShiftsQuery.Where(sz =>
                    sz.ShiftDetails.Any(sd => sd.ShiftStaffDetails.StaffId == CurrentUser.Id)));
        }

        var deviceIds = shiftZones
            .SelectMany(sz => sz.ShiftDetails ?? new List<ShiftDetails>())
            .Where(sd => sd.ShiftDevices?.DeviceCode != null)
            .Where(sd => isManager || sd.ShiftStaffDetails.StaffId == CurrentUser.Id)
            .Select(sd => sd.ShiftDevices.DeviceCode)
            .Distinct()
            .ToList();

        var watcherQuery = await _watcherRepository.GetQueryableAsync();
        var watchers = await AsyncExecuter.ToListAsync(
            watcherQuery.Where(w => deviceIds.Contains(w.DeviceId) && w.Status == Status.Active));


        return Result<ICollection<ShiftWatcherActionDto>>.Success(ObjectMapper.Map<List<Watcher>, List<ShiftWatcherActionDto>>(watchers));
    }
    /// <inheritdoc/>
    public async Task<PagedResultDto<ShiftZoneUserViewDto>> GetShiftZoneUsersAsync(
        Guid? tenantId,
        PagedAndSortedAndSearchResultRequestDto input)
    {
        var query = await _shiftZoneRepository.WithDetailsAsync(
            x => x.ShiftDetails);

        if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
        {
            var term = input.SearchingTerm.ToLower();
            query = query.Where(sz =>
                sz.ZoneEnName.ToLower().Contains(term) ||
                sz.ZoneArName.ToLower().Contains(term));
        }

        var shiftZones = await AsyncExecuter.ToListAsync(query);
        var mapping = ObjectMapper.Map<List<ShiftZone>, List<ShiftZoneDto>>(shiftZones);

        var garageName = (ShiftZoneDto zone) =>
            CultureInfo.CurrentCulture.Name == "ar" ? zone.ZoneArName : zone.ZoneEnName;

        var managerRows = mapping
            .Where(zone => zone.ShiftMangerDto != null)
            .Select(zone => new ShiftZoneUserViewDto
            {
                UserId = zone.ShiftMangerDto.Id,
                UserName = zone.ShiftMangerDto.ManagerUserName,
                Name = zone.ShiftMangerDto.ManagerEnName,
                Role = "Manager",
                Device = "Multiple",
                Devices = zone.ShiftDetailsDto?
                                              .Where(d => !string.IsNullOrWhiteSpace(d.ShiftDevices?.DeviceCode))
                                              .Select(d => d.ShiftDevices.DeviceCode)
                                              .Distinct()
                                              .ToList() ?? new List<string>(),
                Garage = garageName(zone),
                OrganizationArabicName = zone?.OrganizationArName,
                OrganizationEnglishName = zone?.OrganizationEnName
            });

        var staffRows = mapping
            .Where(zone => zone.ShiftDetailsDto?.Any() == true)
            .SelectMany(zone => zone.ShiftDetailsDto
                .Where(detail => detail.ShiftStaffDetails != null)
                .Select(detail => new ShiftZoneUserViewDto
                {
                    UserId = detail.ShiftStaffDetails.StaffId,
                    UserName = detail.ShiftStaffDetails.StaffArName,
                    Name = detail.ShiftStaffDetails.StaffEnName,
                    Role = "Staff",
                    Device = detail.ShiftDevices?.DeviceCode ?? "None",
                    Devices = new List<string>(),
                    Garage = garageName(zone),
                    OrganizationArabicName = zone?.OrganizationArName,
                    OrganizationEnglishName = zone?.OrganizationEnName
                }));

        var resultList = managerRows
            .Concat(staffRows)
            .ToList();

        if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
        {
            var term = input.SearchingTerm.ToLower();
            resultList = resultList
                .Where(x =>
                    (x.UserName?.ToLower().Contains(term) ?? false) ||
                    (x.Name?.ToLower().Contains(term) ?? false) ||
                    (x.Garage?.ToLower().Contains(term) ?? false))
                .ToList();
        }

        return new PagedResultDto<ShiftZoneUserViewDto>(
            resultList.Count,
            resultList
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToList());
    }
}
