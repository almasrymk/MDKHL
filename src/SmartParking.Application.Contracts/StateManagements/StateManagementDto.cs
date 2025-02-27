using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace SmartParking.StateManagements;

public class StateManagementDto:FullAuditedEntityDto<int>
{
    public Guid? TenantId { get; set; }
    public Guid UserId { get; set; }
    public bool IsStateActive { get; set; }
    public DateTime? RemainingTime { get; set; }
    public int ShiftId { get; set; }
}
