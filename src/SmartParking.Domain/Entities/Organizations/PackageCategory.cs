using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Entities.Organizations
{
    public class PackageCategory : FullAuditedEntity<int>, IMultiTenant , IBaseEntity
    {
        public int PackageId { get; set; }
        public int CustomerCategoryId { get; set; }
        public Guid? TenantId { get; set; }
        public virtual CustomerCategory CustomerCategory { get; set; }
        public virtual Package Package { get; set; }
        public Status Status { get ; set ; }
    }
}
