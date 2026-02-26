using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Entities.Shifts;

public class ShiftZone : FullAuditedEntity<Guid>, IMultiTenant
{
    public int ShiftId { get; set; }
    public virtual Shift Shift { get; set; }
    public int ZoneId { get; set; }
    public string ZoneArName { get; set; }
    public string ZoneEnName { get; set; }
    public ShiftManager ShiftManager { get; set; }
    public virtual List<ShiftDetails>? ShiftDetails { get; set; }
    public Guid? TenantId { get; set; }
    public int OrganizationId { get; set; }


    public ShiftZone()
    {
        ShiftManager = new ShiftManager();
    }

}