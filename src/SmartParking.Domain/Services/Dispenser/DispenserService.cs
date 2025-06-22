using SmartParking.Localization;
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace SmartParking.Services.Dispenser;

public class DispenserService : DomainService
{
    private readonly IRepository<Entities.Cameras.Watchers.Dispenser, int> _repository;

    public DispenserService(IRepository<Entities.Cameras.Watchers.Dispenser, int> repository)
    {
        _repository = repository;
    }


    public async Task DispenserDeviceIDExistsOnUpdate (string DeviceId , int DispenserID)
    {
        var currentDispenserCounts = await _repository
            .CountAsync(i => i.DeviceId == DeviceId &&i.Id==DispenserID);

        if (currentDispenserCounts > 0)
        {
            throw new Exception(DeviceId+" " +LocalizationHelper.GetLocalizedString("DuplicateValidation"));
        }

    }
    public async Task DispenserDeviceIDExistsOnCreate (string DeviceId)
    {
        var currentDispenserCounts = await _repository
            .CountAsync(i => i.DeviceId == DeviceId );

        if (currentDispenserCounts > 0)
        {
            throw new Exception(DeviceId + " " + LocalizationHelper.GetLocalizedString("DuplicateValidation"));
        }

    }
}
