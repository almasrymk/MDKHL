using Microsoft.EntityFrameworkCore;
using SmartParking.Entities.Cameras.Watchers;
using SmartParking.Entities.Shifts;
using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Watchers;

public class WatcherAppService : ApplicationService, IWatcherAppService
{
    private readonly IRepository<Camera, int> _cameraRepository;
    private readonly IRepository<Dispenser, int> _dispenserRepository;
    private readonly IRepository<Watcher, int> _watcherRepository;
    public WatcherAppService(
        IRepository<Camera, int> cameraRepository,
        IRepository<Dispenser, int> dispenserRepository,
        IRepository<Watcher, int> watcherRepository)
    {
        _cameraRepository = cameraRepository;
        _dispenserRepository = dispenserRepository;
        _watcherRepository = watcherRepository;
    }
    /// <inheritdoc/>
    public async Task<Camera> GetcameraByWatherCodeAsync(string watcherCode)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {

            var camera= await _cameraRepository.FirstOrDefaultAsync(x=>x.DeviceId==watcherCode);
            return camera;
        }
    }
    /// <inheritdoc/>
    public Task<Dispenser> GetDispenserByWatherCodeAsync(string watcherCode)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            var dispenser = _dispenserRepository.FirstOrDefaultAsync(d => d.DeviceId == watcherCode);
            return dispenser!;
        }
    }
    /// <inheritdoc/>
    public async Task<ICollection<Watcher>> GetAllWatchersAsync()
    {
        
            return await _watcherRepository.GetListAsync();
        
    }
    /// <inheritdoc/>
    public async Task<string> GetWatcherCodeByWatcherIdAsync(int watcherId)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            var watcher= await _watcherRepository.FirstOrDefaultAsync(x=>x.Id==watcherId);
            return watcher!.DeviceId;
        }
    }
    //public async Task<ICollection<Watcher>> GetAllWatchersByShiftZoneAsync()
    //{
    //    var shiftZones = await _shiftZoneRepository.GetListAsync();
    //    if(shiftZones == null)
    //    {
    //        return await GetAllWatchersAsync();
    //    }
    //    var currentDateOnly = DateOnly.FromDateTime(DateTime.Now);
    //    var currentTimeOnly = TimeOnly.FromDateTime(DateTime.Now);

    //    foreach (var shiftZone in shiftZones)
    //    {
    //        bool isDateValid = shiftZone.Shift.StartDate <= currentDateOnly &&
    //                           shiftZone.Shift.EndDate >= currentDateOnly;

    //        bool isTimeValid = shiftZone.Shift.StartTime <= currentTimeOnly &&
    //                           shiftZone.Shift.EndTime >= currentTimeOnly;

    //        bool isActive = shiftZone.Shift.Status == Status.Active;

    //        if (isActive && isDateValid && (shiftZone.Shift.IsFullDay || isTimeValid))
    //        {
    //            var watchers = await _watcherRepository.GetListAsync(x => x.GarageId == shiftZone.ZoneId);
    //            return watchers;
    //        }
    //    }
    //    return await GetAllWatchersAsync();
    //}
}