using SmartParking.Entities.Shifts;
using System;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus;

namespace SmartParking.Shifts;

public class ShiftDetailsDeletedEventHandler :
    ILocalEventHandler<EntityDeletedEventData<ShiftDetails>>,
    ITransientDependency
{
    private readonly IRepository<ShiftStaffDetails, Guid> _staffDetailsRepository;
    private readonly IRepository<ShiftDevice, Guid> _deviceRepository;

    public ShiftDetailsDeletedEventHandler(
        IRepository<ShiftStaffDetails, Guid> staffDetailsRepository,
        IRepository<ShiftDevice, Guid> deviceRepository)
    {
        _staffDetailsRepository = staffDetailsRepository;
        _deviceRepository = deviceRepository;
    }

    public async Task HandleEventAsync(EntityDeletedEventData<ShiftDetails> eventData)
    {
        var shiftDetailsId = eventData.Entity.Id;

        var staffDetails = await _staffDetailsRepository
            .FirstOrDefaultAsync(x => x.ShiftDetailsId == shiftDetailsId);
        if (staffDetails != null)
            await _staffDetailsRepository.DeleteAsync(staffDetails);

        var device = await _deviceRepository
            .FirstOrDefaultAsync(x => x.ShiftDetailsId == shiftDetailsId);
        if (device != null)
            await _deviceRepository.DeleteAsync(device);
    }
}
