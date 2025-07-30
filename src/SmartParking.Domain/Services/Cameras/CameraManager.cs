using SmartParking.Entities.Cameras.Watchers;
using SmartParking.Enums;
using SmartParking.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Services.Cameras;
public class CameraManager : DomainService
{
    private readonly IRepository<Camera, int> _cameraRepository;
    private readonly IDataFilter _dataFilter;

    public CameraManager(IRepository<Camera, int> cameraRepository, IDataFilter dataFilter)
    {
        _cameraRepository = cameraRepository;
        _dataFilter = dataFilter;
    }
    public async Task<Camera> GetCameraByTypeBelongsToGarageAsync(int garageId, WatcherType cameraType)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var camera = await _cameraRepository.FirstOrDefaultAsync(cam => cam.GarageId == garageId && cam.CameraType == cameraType);
            if (camera == null)
            {
                throw new Exception(LocalizationHelper.GetLocalizedString("Camera") + " " + LocalizationHelper.GetLocalizedString("NotFoundValidation"));
            }
            return camera;
        }
    }

    public async Task<Camera> GetCameraByDeviceIdAsync(string deviceId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var camera = await _cameraRepository.FirstOrDefaultAsync(cam => cam.DeviceId == deviceId);
            if (camera == null)
            {
                throw new Exception(LocalizationHelper.GetLocalizedString("Camera") + " " + LocalizationHelper.GetLocalizedString("NotFoundValidation"));
            }
            return camera;
        }
    }
}
