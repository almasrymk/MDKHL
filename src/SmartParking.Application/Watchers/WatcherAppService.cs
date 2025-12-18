

using Volo.Abp.Domain.Repositories;
using SmartParking.Entities.Cameras.Watchers;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Services;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Watchers;

public class WatcherAppService : ApplicationService, IWatcherAppService
{
    private readonly IRepository<Camera, int> _cameraRepository;
    private readonly IRepository<Dispenser, int> _dispenserRepository;
    public WatcherAppService(
        IRepository<Camera, int> cameraRepository,
        IRepository<Dispenser, int> dispenserRepository)
    {
        _cameraRepository = cameraRepository;
        _dispenserRepository = dispenserRepository;
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
}
