using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Entities.Shifts;

public class ShiftStaffDetails : FullAuditedEntity<Guid>, IMultiTenant
{
    public Guid StaffId { get; set; }
    public Guid? ShiftDetailsId { get; set; }
    public virtual ShiftDetails ShiftDetails { get; set; }
    public string StaffEnName { get; set; }
    public string StaffArName { get; set; }
    public string StaffPhoneNumber { get; set; }

    public Guid? TenantId { get; set; }

}