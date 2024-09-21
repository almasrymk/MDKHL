using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Entities.Organizations
{
    public class PackageCategory : FullAuditedEntity<int>, IMultiTenant
    {
        public int PackageId { get; set; }
        public int CategoryId { get; set; }
        public Guid? TenantId { get; set; }
        public virtual Category Category { get; set; }
        public virtual Package Package { get; set; }

    }
}
