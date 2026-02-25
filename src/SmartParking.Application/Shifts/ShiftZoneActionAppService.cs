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
        IRepository<ShiftZone, Guid> shiftZoneRepository
        )
    {
        _shiftZoneRepository = shiftZoneRepository;
        _watcherRepository = watcherRepository;
    }
 

    public async Task<ICollection<ShiftWatcherActionDto>> GetShiftZoneDevicesDataAsync()
    {
        if (!CurrentUser.Id.HasValue)
            return new List<ShiftWatcherActionDto>();

        var today = DateOnly.FromDateTime(DateTime.Now);
        var timeOfDay = TimeOnly.FromDateTime(DateTime.Now);

        var shiftZoneQuery = (await _shiftZoneRepository.GetQueryableAsync());

        var filteredQuery = shiftZoneQuery
            .Where(sz => sz.ShiftDetails.Any(sd => sd.ShiftStaffDetails.StaffId == CurrentUser.Id))
            .Where(sz => sz.Shift.StartDate <= today && sz.Shift.EndDate >= today)
            .Where(sz => sz.Shift.Status == Status.Active)
            .Where(sz => sz.Shift.IsFullDay ||
                ((sz.Shift.StartTime <= sz.Shift.EndTime && sz.Shift.StartTime <= timeOfDay && sz.Shift.EndTime >= timeOfDay) ||
                 (sz.Shift.StartTime > sz.Shift.EndTime && (sz.Shift.StartTime <= timeOfDay || sz.Shift.EndTime >= timeOfDay))));

        var shiftZones = await AsyncExecuter.ToListAsync(filteredQuery);

        var deviceIds = shiftZones
            .SelectMany(sz => sz.ShiftDetails) 
            .Where(sd => sd.ShiftDevices != null)
            .Select(sd => sd.ShiftDevices.DeviceCode) 
            .Distinct()
            .ToList();

        var watcherQuery = await _watcherRepository.GetQueryableAsync();
        var watchers = await AsyncExecuter.ToListAsync(watcherQuery.Where(w => deviceIds.Contains(w.DeviceId)));

        return ObjectMapper.Map<List<Watcher>, List<ShiftWatcherActionDto>>(watchers);
    }

   

    public async Task<PagedResultDto<ShiftZoneUserViewDto>> GetShiftZoneUsersAsync(Guid? tenantId, PagedAndSortedAndSearchResultRequestDto input)
    {
        var query = (await _shiftZoneRepository.GetQueryableAsync());

        var shiftZones = await AsyncExecuter.ToListAsync(query);
        var mapping = ObjectMapper.Map<List<ShiftZone>, List<ShiftZoneDto>>(shiftZones);    
        var resultList = new List<ShiftZoneUserViewDto>();

        foreach (var zone in mapping)
        {
            string garageName = CultureInfo.CurrentCulture.Name == "ar" ? zone.ZoneArName : zone.ZoneEnName;

            // 1. Handle Manager
            if (zone.ShiftMangerDto != null)
            {
                // Collect every non-null device from all ShiftDetails in this zone
                var managerDevices = zone.ShiftDetailsDto?
                    .Where(d => !string.IsNullOrWhiteSpace(d.ShiftDevices?.DeviceCode))
                    .Select(d => d.ShiftDevices.DeviceCode)
                    .Distinct()
                    .ToList() ?? new List<string>();

                resultList.Add(new ShiftZoneUserViewDto
                {
                    UserId = zone.ShiftMangerDto.Id,
                    UserName = zone.ShiftMangerDto.ManagerUserName,
                    Name = zone.ShiftMangerDto.ManagerEnName,
                    Role = "Manager",
                    Device = "Multiple",          // legacy fallback
                    Devices = managerDevices,     // ← real list
                    Garage = garageName,
                    OrganizationArabicName = zone.ShiftDto?.OrganizationArabicName,
                    OrganizationEnglishName = zone.ShiftDto?.OrganizationEnglishName
                });
            }

            // 2. Handle Staff  (unchanged — Device is the single assigned device)
            if (zone.ShiftDetailsDto != null && zone.ShiftDetailsDto.Any())
            {
                foreach (var detail in zone.ShiftDetailsDto)
                {
                    if (detail.ShiftStaffDetails == null) continue;

                    resultList.Add(new ShiftZoneUserViewDto
                    {
                        UserId = detail.ShiftStaffDetails.StaffId,
                        UserName = detail.ShiftStaffDetails.StaffArName,
                        Name = detail.ShiftStaffDetails.StaffEnName,
                        Role = "Staff",
                        Device = detail.ShiftDevices?.DeviceCode ?? "None",
                        Devices = new List<string>(),   // not used for staff
                        Garage = garageName,
                        OrganizationArabicName = zone.ShiftDto?.OrganizationArabicName,
                        OrganizationEnglishName = zone.ShiftDto?.OrganizationEnglishName
                    });
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
        {
            var term = input.SearchingTerm.ToLower();
            resultList = resultList.Where(x =>
                (x.UserName?.ToLower().Contains(term) ?? false) ||
                (x.Name?.ToLower().Contains(term) ?? false) ||
                (x.Garage?.ToLower().Contains(term) ?? false)).ToList();
        }

        return new PagedResultDto<ShiftZoneUserViewDto>(
            resultList.Count,
            resultList.Skip(input.SkipCount).Take(input.MaxResultCount).ToList()
        );
    }
}
