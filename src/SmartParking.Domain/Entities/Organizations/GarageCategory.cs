using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Entities.Organizations;

public class GarageCategory : FullAuditedEntity<int>,IMultiTenant, IBseEntity
{
    public int Id { get; set; }
    public int GarageId { get; set; }
    public int CategoryId { get; set; }
    public TimeOnly From { get; set; }
    public TimeOnly To { get; set; }

    public bool Default { get; set; }
    public Status Status { get; set; }

    public Guid? TenantId { get; set; }
}
