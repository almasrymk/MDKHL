using System;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Shifts.ShiftsMangers;

public class ShiftMangerDto : FullAuditedEntityDto<Guid>
{
    public string ManagerEnName { get; set; }
    public string ManagerUserName { get; set; }
    public string ManagerPhoneNumber { get; set; }
}
