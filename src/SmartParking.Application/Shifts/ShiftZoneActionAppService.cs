using SmartParking.Entities.Cameras.Watchers;
using SmartParking.Entities.Shifts;
using SmartParking.Enums;
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

    public ShiftZoneActionAppService(
        IRepository<Watcher, int> watcherRepository,
        IRepository<ShiftZone, Guid> shiftZoneRepository)
    {
        _shiftZoneRepository = shiftZoneRepository;
        _watcherRepository = watcherRepository;
    }
    /// <inheritdoc/>
    public async Task<ICollection<ShiftWatcherActionDto>> GetShiftZoneDevicesDataAsync()
    {
        if (!CurrentUser.Id.HasValue)
            return new List<ShiftWatcherActionDto>();

        var today = DateOnly.FromDateTime(DateTime.Now);
        var timeOfDay = TimeOnly.FromDateTime(DateTime.Now);

        var query = await _shiftZoneRepository.WithDetailsAsync(
            x => x.ShiftDetails,
            x => x.Shift);

        var activeShiftsQuery = query
            .Where(sz => sz.Shift.StartDate <= today && sz.Shift.EndDate >= today)
            .Where(sz => sz.Shift.Status == Status.Active)
            .Where(sz => sz.Shift.IsFullDay ||
                ((sz.Shift.StartTime <= sz.Shift.EndTime && sz.Shift.StartTime <= timeOfDay && sz.Shift.EndTime >= timeOfDay) ||
                 (sz.Shift.StartTime > sz.Shift.EndTime && (sz.Shift.StartTime <= timeOfDay || sz.Shift.EndTime >= timeOfDay))));

        var isManager = activeShiftsQuery
            .Any(sz => sz.ShiftManager.ManagerUserName == CurrentUser.UserName);

        List<ShiftZone> shiftZones;

        if (isManager)
        {
            shiftZones = await AsyncExecuter.ToListAsync(
                activeShiftsQuery.Where(sz => sz.ShiftManager.ManagerUserName == CurrentUser.UserName));
        }
        else
        {
            shiftZones = await AsyncExecuter.ToListAsync(
                activeShiftsQuery.Where(sz =>
                    sz.ShiftDetails.Any(sd => sd.ShiftStaffDetails.StaffId == CurrentUser.Id)));
        }

        var deviceIds = shiftZones
            .SelectMany(sz => sz.ShiftDetails)
            .Where(sd => sd.ShiftDevices?.DeviceCode != null)
            .Where(sd => isManager || sd.ShiftStaffDetails.StaffId == CurrentUser.Id)
            .Select(sd => sd.ShiftDevices.DeviceCode)
            .Distinct()
            .ToList();

        var watcherQuery = await _watcherRepository.GetQueryableAsync();
        var watchers = await AsyncExecuter.ToListAsync(
            watcherQuery.Where(w => deviceIds.Contains(w.DeviceId)));

        return ObjectMapper.Map<List<Watcher>, List<ShiftWatcherActionDto>>(watchers);
    }    
    /// <inheritdoc/>
    public async Task<PagedResultDto<ShiftZoneUserViewDto>> GetShiftZoneUsersAsync(
        Guid? tenantId,
        PagedAndSortedAndSearchResultRequestDto input)
    {
        var query = await _shiftZoneRepository.WithDetailsAsync(
            x => x.ShiftDetails,
            x => x.Shift);

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
                OrganizationArabicName = zone.ShiftDto?.OrganizationArabicName,
                OrganizationEnglishName = zone.ShiftDto?.OrganizationEnglishName
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
                    OrganizationArabicName = zone.ShiftDto?.OrganizationArabicName,
                    OrganizationEnglishName = zone.ShiftDto?.OrganizationEnglishName
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
