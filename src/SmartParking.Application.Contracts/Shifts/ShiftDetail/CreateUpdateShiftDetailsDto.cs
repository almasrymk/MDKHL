using SmartParking.Shifts.ShiftDevices;
using SmartParking.Shifts.ShiftStaffDetail;
using System;

namespace SmartParking.Shifts.ShiftDetail;

public class CreateUpdateShiftDetailsDto
{
    public CreateUpdateShiftStaffDetailsDto? ShiftStaffDetails { get; set; }
    public CreateUpdateShiftDeviceDto? ShiftDevices { get; set; }
    public int OrganizationId { get; set; }

    public Guid ShiftZoneId { get; set; }
}