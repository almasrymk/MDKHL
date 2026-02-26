using Abp.Application.Services.Dto;
using SmartParking.Shifts.ShiftDevices;
using SmartParking.Shifts.ShiftStaffDetail;
using System;

namespace SmartParking.Shifts.ShiftDetail;

public class ShiftDetailsDto : AuditedEntityDto<Guid>
{
    public ShiftStaffDetailsDto ShiftStaffDetails { get; set; }
    public ShiftDeviceDto ShiftDevices { get; set; }
    public int OrganizationId { get; set; }

}