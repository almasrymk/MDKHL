using SmartParking.Shifts.ShiftDetail;
using SmartParking.Shifts.ShiftsMangers;
using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Shifts.ShiftZones;

public class ShiftZoneDto : FullAuditedEntityDto<Guid>
{
    public ShiftDto ShiftDto { get; set; }
    public int ZoneId { get; set; }
    public string ZoneArName { get; set; }
    public string ZoneEnName { get; set; }
    public int OrganizationId { get; set; }

    public List<ShiftDetailsDto> ShiftDetailsDto { get; set; }
    public ShiftMangerDto ShiftMangerDto { get; set; }
}