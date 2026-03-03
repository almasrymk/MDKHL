using SmartParking.Enums;
using SmartParking.Shifts.ShiftDetail;
using SmartParking.Shifts.ShiftsMangers;
using System;
using System.Collections.Generic;

namespace SmartParking.Shifts.ShiftZones;

public class CreateUpdateShiftZoneDto
{
    public int ShiftId { get; set; }
    public int ZoneId { get; set; }
    public string ShiftArabicName { get; set; }
    public string ShiftEnglishName { get; set; }

    public string ZoneArName { get; set; }
    public string ZoneEnName { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationArName { get; set; }
    public string OrganizationEnName { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsFullDay { get; set; }
    public Status Status { get; set; }


    public List<CreateUpdateShiftDetailsDto>? ShiftDetails { get; set; }
    public CreateUpdateShiftMangerDto Manager { get; set; }
}