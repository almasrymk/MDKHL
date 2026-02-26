using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Entities.Shifts;

public class ShiftDetails : FullAuditedEntity<Guid>, IMultiTenant
{
    public virtual ShiftStaffDetails ShiftStaffDetails { get; set; }
    public virtual ShiftDevice ShiftDevices { get; set; }
    public Guid? ShiftZoneId { get; set; }
    public int OrganizationId { get; set; }

    public Guid? TenantId { get; set; }
}