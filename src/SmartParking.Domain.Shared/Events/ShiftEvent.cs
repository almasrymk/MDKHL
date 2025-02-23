using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.EventBus;

namespace SmartParking.Events;
[EventName("ShiftEvent")]
public class ShiftEvent
{
    public KeeperShiftRequestType StatusShift { get; set; }
    public ShiftEvent(KeeperShiftRequestType status)
    {
        StatusShift = status;
    }
}
