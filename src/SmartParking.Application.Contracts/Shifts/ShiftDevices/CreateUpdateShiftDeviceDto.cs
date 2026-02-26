using System;

namespace SmartParking.Shifts.ShiftDevices;

public class CreateUpdateShiftDeviceDto
{
    public Guid? Id { get; set; }       
    public int DeviceId { get; set; }
    public string DeviceCode { get; set; }
    public string DeviceUserName { get; set; }
    public string DevicePassword { get; set; }
    public int OrganizationId { get; set; }


    public Guid? ShiftDetailsId { get; set; }
}
