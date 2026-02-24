using SmartParking.Shifts.ShiftDetail;
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
    public List<CreateUpdateShiftDetailsDto>? ShiftDetails { get; set; }
    public CreateUpdateShiftMangerDto Manager { get; set; }
}