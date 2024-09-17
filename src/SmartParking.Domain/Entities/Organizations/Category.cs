using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Entities.Organizations;

public class Category : FullAuditedEntity<int> , IMultiTenant
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool Default { get; set; }
    public int OrganizationId { get; set; }
    public Status Status { get; set; }

    public Guid? TenantId { get; set; }
}
