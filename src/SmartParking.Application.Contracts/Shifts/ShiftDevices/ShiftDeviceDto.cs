using System;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Shifts.ShiftDevices;

public class ShiftDeviceDto : FullAuditedEntityDto<Guid>
{
    public int DeviceId { get; set; }
    public string DeviceCode { get; set; }
    public string DeviceUserName { get; set; }
    public string DevicePassword { get; set; }
}
