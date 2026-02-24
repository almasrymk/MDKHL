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
    //public async Task<ICollection<ShiftWatcherActionDto>> GetShiftZoneDevicesDataAsync()
    //{
    //    if (!CurrentUser.Id.HasValue) 
    //        return new List<ShiftWatcherActionDto>();

    //    var today = DateOnly.FromDateTime(DateTime.Now);
    //    var timeOfDay = TimeOnly.FromDateTime(DateTime.Now);

    //    var shiftZoneQuery = (await _shiftZoneRepository.GetQueryableAsync())
    //        .Include(sz => sz.Shift)
    //        .Include(sz => sz.ShiftDetails)
    //            .ThenInclude(sd => sd.ShiftDevices)
    //        .Include(sz => sz.ShiftDetails)
    //            .ThenInclude(sd => sd.ShiftStaffDetails);

    //    var filteredQuery = shiftZoneQuery
    //    .Where(sz => sz.ShiftDetails.ShiftStaffDetails.Any(ssd => ssd.StaffId == CurrentUser.Id))

    //        .Where(sz =>
    //            sz.Shift.StartDate <= today &&
    //            sz.Shift.EndDate >= today
    //        )
    //        .Where(sz => sz.Shift.Status == Status.Active)
    //        .Where(sz =>
    //            sz.Shift.IsFullDay
    //            ||
    //            (
    //                (sz.Shift.StartTime <= sz.Shift.EndTime &&
    //                 sz.Shift.StartTime <= timeOfDay &&
    //                 sz.Shift.EndTime >= timeOfDay)
    //                ||
    //                (sz.Shift.StartTime > sz.Shift.EndTime &&
    //                 (sz.Shift.StartTime <= timeOfDay ||
    //                  sz.Shift.EndTime >= timeOfDay))
    //            )
    //        );

    //    var shiftZones = await AsyncExecuter.ToListAsync(filteredQuery);

    //    var deviceIds = shiftZones
    //        .Where(sz => sz.ShiftDetails?.ShiftDevices != null)
    //        .SelectMany(sz => sz.ShiftDetails.ShiftDevices)
    //        .Select(sd => sd.DeviceCode)
    //        .Distinct()
    //        .ToList();

    //    var watcherQuery = await _watcherRepository.GetQueryableAsync();

    //    var watchers = await AsyncExecuter.ToListAsync(
    //        watcherQuery.Where(w => deviceIds.Contains(w.DeviceId))
    //    );

    //    return ObjectMapper.Map<List<Watcher>, List<ShiftWatcherActionDto>>(watchers);
    //}

    public async Task<ICollection<ShiftWatcherActionDto>> GetShiftZoneDevicesDataAsync()
    {
        if (!CurrentUser.Id.HasValue)
            return new List<ShiftWatcherActionDto>();

        var today = DateOnly.FromDateTime(DateTime.Now);
        var timeOfDay = TimeOnly.FromDateTime(DateTime.Now);

        var shiftZoneQuery = (await _shiftZoneRepository.GetQueryableAsync());
            //.Include(sz => sz.Shift)
            //.Include(sz => sz.ShiftDetails) // This is now a List
            //    .ThenInclude(sd => sd.ShiftDevices) // This is now a Single Object
            //.Include(sz => sz.ShiftDetails)
            //    .ThenInclude(sd => sd.ShiftStaffDetails); // This is now a Single Object

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

    //public async Task<PagedResultDto<ShiftZoneUserViewDto>> GetShiftZoneUsersAsync(Guid? tenantId, PagedAndSortedAndSearchResultRequestDto input)
    //{
    //    var query = (await _shiftZoneRepository.GetQueryableAsync());

    //    var shiftZones = await AsyncExecuter.ToListAsync(query);

    //    var mapping = ObjectMapper.Map<List<ShiftZone>, List<ShiftZoneDto>>(shiftZones);

    //    var resultList = new List<ShiftZoneUserViewDto>();

    //    foreach (var zone in mapping)
    //    {
    //        string garageName = CultureInfo.CurrentCulture.Name == "ar" ? zone.ZoneArName : zone.ZoneEnName;

    //        if (zone.ShiftMangerDto != null)
    //        {
    //            resultList.Add(new ShiftZoneUserViewDto
    //            {
    //                UserName = zone.ShiftMangerDto.ManagerUserName,
    //                Name = zone.ShiftMangerDto.ManagerEnName,
    //                Role = "Manager",
    //                Device = "Multiple",
    //                Garage = garageName,
    //                OrganizationArabicName = zone.ShiftDto?.OrganizationArabicName,
    //                OrganizationEnglishName = zone.ShiftDto?.OrganizationEnglishName
    //            });
    //        }

    //        if (zone.ShiftDetailsDto?.ShiftStaffDetails != null)
    //        {
    //            var staffList = zone.ShiftDetailsDto.ShiftStaffDetails.ToList();
    //            var deviceList = zone.ShiftDetailsDto.ShiftDevices?.ToList() ?? new List<ShiftDeviceDto>();

    //            for (int i = 0; i < staffList.Count; i++)
    //            {
    //                var staff = staffList[i];

    //                var specificDevice = i < deviceList.Count ? deviceList[i].DeviceCode : "None";

    //                resultList.Add(new ShiftZoneUserViewDto
    //                {
    //                    UserName = staff.StaffArName,
    //                    Name = staff.StaffEnName,
    //                    Role = "Staff",
    //                    Device = specificDevice, // Assigning only the corresponding device
    //                    Garage = garageName,
    //                    OrganizationArabicName = zone.ShiftDto?.OrganizationArabicName,
    //                    OrganizationEnglishName = zone.ShiftDto?.OrganizationEnglishName
    //                });
    //            }
    //        }
    //    }

    //    if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
    //    {
    //        var term = input.SearchingTerm.ToLower();
    //        resultList = resultList.Where(x =>
    //            (x.UserName?.ToLower().Contains(term) ?? false) ||
    //            (x.Name?.ToLower().Contains(term) ?? false) ||
    //            (x.Garage?.ToLower().Contains(term) ?? false)).ToList();
    //    }

    //    var totalCount = resultList.Count;
    //    var pagedItems = resultList
    //        .Skip(input.SkipCount)
    //        .Take(input.MaxResultCount)
    //        .ToList();

    //    return new PagedResultDto<ShiftZoneUserViewDto>(totalCount, pagedItems);
    //}

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
                resultList.Add(new ShiftZoneUserViewDto
                {
                    UserId = zone.ShiftMangerDto.Id,
                    UserName = zone.ShiftMangerDto.ManagerUserName,
                    Name = zone.ShiftMangerDto.ManagerEnName,
                    Role = "Manager",
                    Device = "Multiple",
                    Garage = garageName,
                    OrganizationArabicName = zone.ShiftDto?.OrganizationArabicName,
                    OrganizationEnglishName = zone.ShiftDto?.OrganizationEnglishName
                });
            }

            // 2. Handle Staff - FIX: Loop through ShiftDetails List
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
