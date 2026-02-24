using System;

namespace SmartParking.Shifts.ShiftsMangers;

public class CreateUpdateShiftMangerDto
{
    public Guid? Id { get; set; }           
    public Guid? ManagerUserId { get; set; } 
    public string ManagerEnName { get; set; }
    public string ManagerUserName { get; set; }
    public string ManagerPhoneNumber { get; set; }
}
