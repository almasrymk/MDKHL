using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus;
using SmartParking.Entities.Shifts;

namespace SmartParking.Shifts
{
    public class ShiftCascadeSoftDeleteEventHandler :
        ILocalEventHandler<EntityDeletedEventData<ShiftDetails>>,
        ILocalEventHandler<EntityDeletedEventData<ShiftZone>>,
        ITransientDependency
    {
        private readonly IRepository<ShiftStaffDetails, Guid> _staffDetailsRepository;
        private readonly IRepository<ShiftDevice, Guid> _deviceRepository;
        private readonly IRepository<ShiftDetails, Guid> _shiftDetailsRepository;

        public ShiftCascadeSoftDeleteEventHandler(
            IRepository<ShiftStaffDetails, Guid> staffDetailsRepository,
            IRepository<ShiftDevice, Guid> deviceRepository,
            IRepository<ShiftDetails, Guid> shiftDetailsRepository)
        {
            _staffDetailsRepository = staffDetailsRepository;
            _deviceRepository = deviceRepository;
            _shiftDetailsRepository = shiftDetailsRepository;
        }

        public async Task HandleEventAsync(EntityDeletedEventData<ShiftDetails> eventData)
        {
            var shiftDetailsId = eventData.Entity.Id;

            var staffIds = await (await _staffDetailsRepository.GetQueryableAsync())
                .Where(x => x.ShiftDetailsId.HasValue && x.ShiftDetailsId.Value == shiftDetailsId)
                .Select(x => x.Id)
                .ToListAsync();

            if (staffIds.Any())
            {
                await _staffDetailsRepository.DeleteManyAsync(staffIds);
            }

            var deviceIds = await (await _deviceRepository.GetQueryableAsync())
                .Where(x => x.ShiftDetailsId.HasValue && x.ShiftDetailsId.Value == shiftDetailsId)
                .Select(x => x.Id)
                .ToListAsync();

            if (deviceIds.Any())
            {
                await _deviceRepository.DeleteManyAsync(deviceIds);
            }
        }

        public async Task HandleEventAsync(EntityDeletedEventData<ShiftZone> eventData)
        {
            var shiftZoneId = eventData.Entity.Id;

            var shiftDetailsIds = await (await _shiftDetailsRepository.GetQueryableAsync())
                .Where(x => x.ShiftZoneId.HasValue && x.ShiftZoneId.Value == shiftZoneId)
                .Select(x => x.Id)
                .ToListAsync();

            if (!shiftDetailsIds.Any())
                return;

            var staffIds = await (await _staffDetailsRepository.GetQueryableAsync())
                .Where(x => x.ShiftDetailsId.HasValue && shiftDetailsIds.Contains(x.ShiftDetailsId.Value))
                .Select(x => x.Id)
                .ToListAsync();

            if (staffIds.Any())
            {
                await _staffDetailsRepository.DeleteManyAsync(staffIds);
            }

            var deviceIds = await (await _deviceRepository.GetQueryableAsync())
                .Where(x => x.ShiftDetailsId.HasValue && shiftDetailsIds.Contains(x.ShiftDetailsId.Value))
                .Select(x => x.Id)
                .ToListAsync();

            if (deviceIds.Any())
            {
                await _deviceRepository.DeleteManyAsync(deviceIds);
            }

            await _shiftDetailsRepository.DeleteManyAsync(shiftDetailsIds);
        }
    }
}