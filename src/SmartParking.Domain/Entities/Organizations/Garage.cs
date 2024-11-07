using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Entities.Organizations;

public class Garage : FullAuditedEntity<int> , IMultiTenant,IBseEntity
{
    public string Code { get; set; }
    public string NameAr { get; set; }
    public string NameEn { get; set; }
    public int MaxCapacity { get; set; }
    public TimeOnly ClosingHours { get; set; }
    public int OrganizationId { get; set; }
    public virtual Organization Organization { get; set; }
    public Status Status { get; set; }
    public int CurrnetCapicity { get; set; }
    public Guid? TenantId { get; set; }
    public virtual ICollection<GarageCategories> GarageCategories { get; set; }

}
