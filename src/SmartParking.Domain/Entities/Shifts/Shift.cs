using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Entities.Shifts;

public class Shift : FullAuditedEntity<int>, IMultiTenant, IBaseEntity
{

    public Guid? TenantId { get; set; }
    public Status Status { get; set; }
}
