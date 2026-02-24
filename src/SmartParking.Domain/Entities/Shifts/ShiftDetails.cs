using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Entities.Shifts;

public class ShiftDetails : FullAuditedEntity<Guid>, IMultiTenant
{
    public virtual List<ShiftStaffDetails> ShiftStaffDetails { get; set; } = new List<ShiftStaffDetails>();
    public virtual List<ShiftDevice> ShiftDevices { get; set; } = new List<ShiftDevice>();

    public Guid? TenantId { get; set; }
}