using System;

namespace SmartParking.Shifts.ShiftStaffDetail;

public class CreateUpdateShiftStaffDetailsDto
{
    public Guid? Id { get; set; }     
    public Guid StaffId { get; set; }
    public string StaffEnName { get; set; }
    public string StaffUserName { get; set; }
    public string StaffPhoneNumber { get; set; }

    public Guid? ShiftDetailsId { get; set; }
}