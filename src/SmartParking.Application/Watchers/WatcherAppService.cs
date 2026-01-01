

using Volo.Abp.Domain.Repositories;
using SmartParking.Entities.Cameras.Watchers;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Services;
using Volo.Abp.MultiTenancy;
using System.Collections.Generic;

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
    public async Task<Camera> GetcameraByWatherCodeAsync(string watcherCode)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {

            var camera= await _cameraRepository.FirstOrDefaultAsync(x=>x.DeviceId==watcherCode);
            return camera;
        }
    }

    public Task<Dispenser> GetDispenserByWatherCodeAsync(string watcherCode)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            var dispenser = _dispenserRepository.FirstOrDefaultAsync(d => d.DeviceId == watcherCode);
            return dispenser!;
        }
    }

    public async Task<ICollection<Watcher>> GetAllWatchersAsync()
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            return await _watcherRepository.GetListAsync();
        }
    }
}
