using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Entities.Shifts;

public class ShiftDevice : FullAuditedEntity<Guid>, IMultiTenant
{
    public int DeviceId { get; set; }
    public string DeviceCode { get; set; }
    public string DeviceUserName { get; set; }
    public string DevicePassword { get; set; }
    public Guid? ShiftDetailsId { get; set; }
    public virtual ShiftDetails ShiftDetails { get; set; }
    public int OrganizationId { get; set; }


    public Guid? TenantId {  get; set; }
}