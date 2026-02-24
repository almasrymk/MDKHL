using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Entities.Shifts;

public class ShiftDetails : FullAuditedEntity<Guid>, IMultiTenant
{
    public virtual ShiftStaffDetails ShiftStaffDetails { get; set; } = new ShiftStaffDetails();
    public virtual ShiftDevice ShiftDevices { get; set; } = new ShiftDevice();
    public Guid? ShiftZoneId { get; set; }

    public Guid? TenantId { get; set; }
}