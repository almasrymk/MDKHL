using SmartParking.Shifts.ShiftsMangers;
using System;
using System.Collections.Generic;

namespace SmartParking.Shifts.ShiftZones;

public class CreateUpdateShiftZoneDto
{
    public int ShiftId { get; set; }
    public int ZoneId { get; set; }
    public string ZoneArName { get; set; }
    public string ZoneEnName { get; set; }
    public Guid? ShiftDetailsId { get; set; }
    public CreateUpdateShiftMangerDto Manager { get; set; }
}