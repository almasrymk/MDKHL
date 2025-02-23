using SmartParking.Enums;
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
    public Task HandleEventAsync(ShiftEvent eventData)
    {
        switch(eventData.StatusShift)
        {
            case KeeperShiftRequestType.StartShift:
                break;
            case KeeperShiftRequestType.EndShift:
                break;
        }
        return Task.CompletedTask;
    }
}
