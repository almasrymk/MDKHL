using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SmartParking.Enums;
using SmartParking.KeeperShifts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;

namespace SmartParking.Events;

public class ShiftEventHandler : IDistributedEventHandler<ShiftEvent>, ITransientDependency
{
    private readonly KeeperShiftSingletonService _keeperShiftSingleton;

    public ShiftEventHandler(KeeperShiftSingletonService keeperShiftSingleton)
    {
        _keeperShiftSingleton = keeperShiftSingleton;
    }

    public async Task HandleEventAsync(ShiftEvent eventData)
    {
        switch(eventData.StatusShift)
        {
            case KeeperShiftRequestType.StartShift:
               await _keeperShiftSingleton.SetAsync(eventData.KeeperShiftId);
                break;
            case KeeperShiftRequestType.EndShift:
                _keeperShiftSingleton.Dispose();
                break;
        }
    }
}
