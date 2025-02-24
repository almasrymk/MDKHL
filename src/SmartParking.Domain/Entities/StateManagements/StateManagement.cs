using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Entities.StateManagements;

public class StateManagement : FullAuditedEntity<int>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public Guid UserId { get; set; }
    public bool IsStateActive { get; set; }
    public DateTime? RemainingTime { get; set; }
}
