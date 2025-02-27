using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.StateManagements;

public class CreateUpdateStateManagementDto
{
    public Guid? TenantId { get; set; }
    public Guid? UserId { get; set; }
    public bool IsStateActive { get; set; }
    public DateTime? RemainingTime { get; set; }
    public int ShiftId { get; set; }
}
