using System;

namespace SmartParking.Shifts;

public class CreateUpdateZoneAssignmentDto
{
    public Guid? ExistingShiftDetailsId { get; set; }

    public Guid? StaffId { get; set; }
    public string? StaffUserName { get; set; }
    public string? StaffEnName { get; set; }
    public string? StaffPhoneNumber { get; set; }

    public int? DeviceId { get; set; }
    public string? DeviceCode { get; set; }
    public string? DeviceUserName { get; set; }
    public string? DevicePassword { get; set; }
}
