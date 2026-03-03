using SmartParking.Enums;
using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Entities.Shifts;

public class ShiftZone : FullAuditedEntity<Guid>, IMultiTenant
{
    public int ShiftId { get; set; }
    public string ShiftArabicName { get; set; }
    public string ShiftEnglishName { get; set; }
    public int ZoneId { get; set; }
    public string ZoneArName { get; set; }
    public string ZoneEnName { get; set; }
    public ShiftManager ShiftManager { get; set; }
    public virtual List<ShiftDetails>? ShiftDetails { get; set; }
    public Guid? TenantId { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationArName { get; set; }
    public string OrganizationEnName { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsFullDay { get; set; }
    public Status Status { get; set; } = Status.Active;



    public ShiftZone()
    {
        ShiftManager = new ShiftManager();
    }

}