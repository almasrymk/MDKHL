using Microsoft.EntityFrameworkCore;
using SmartParking.Cameras.Watchers;
using SmartParking.Entities.Cameras.Watchers;
using SmartParking.Entities.Shifts;
using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

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

        var shiftZoneQuery = (await _shiftZoneRepository.GetQueryableAsync())
            .Include(sz => sz.Shift)
            .Include(sz => sz.ShiftDetails)
                .ThenInclude(sd => sd.ShiftDevices)
            .Include(sz => sz.ShiftDetails)
                .ThenInclude(sd => sd.ShiftStaffDetails);

        var filteredQuery = shiftZoneQuery
        .Where(sz => sz.ShiftDetails.ShiftStaffDetails.Any(ssd => ssd.StaffId == CurrentUser.Id))

            .Where(sz =>
                sz.Shift.StartDate <= today &&
                sz.Shift.EndDate >= today
            )
            .Where(sz => sz.Shift.Status == Status.Active)
            .Where(sz =>
                sz.Shift.IsFullDay
                ||
                (
                    (sz.Shift.StartTime <= sz.Shift.EndTime &&
                     sz.Shift.StartTime <= timeOfDay &&
                     sz.Shift.EndTime >= timeOfDay)
                    ||
                    (sz.Shift.StartTime > sz.Shift.EndTime &&
                     (sz.Shift.StartTime <= timeOfDay ||
                      sz.Shift.EndTime >= timeOfDay))
                )
            );

        var shiftZones = await AsyncExecuter.ToListAsync(filteredQuery);

        var deviceIds = shiftZones
            .Where(sz => sz.ShiftDetails?.ShiftDevices != null)
            .SelectMany(sz => sz.ShiftDetails.ShiftDevices)
            .Select(sd => sd.DeviceCode)
            .Distinct()
            .ToList();

        var watcherQuery = await _watcherRepository.GetQueryableAsync();

        var watchers = await AsyncExecuter.ToListAsync(
            watcherQuery.Where(w => deviceIds.Contains(w.DeviceId))
        );

        return ObjectMapper.Map<List<Watcher>, List<ShiftWatcherActionDto>>(watchers);
    }
}
