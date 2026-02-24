using SmartParking.Shifts.ShiftDevices;
using SmartParking.Shifts.ShiftStaffDetail;
using System.Collections.Generic;

namespace SmartParking.Shifts.ShiftDetail;

public class CreateUpdateShiftDetailsDto
{
    public List<CreateUpdateShiftStaffDetailsDto>? ShiftStaffDetails { get; set; }
    public List<CreateUpdateShiftDeviceDto>? ShiftDevices { get; set; }
}