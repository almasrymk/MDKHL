using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.DailyShifts;
public class CreateUpdateDailyShiftDto
{
    public Guid KeeperId { get; set; }
    public int OrganizationId { get; set; }
    public int GarageId { get; set; }
    public Guid? TenantId { get; set; }
    public DateTime RequestTime { get; set; }
    public KeeperShiftRequestType KeeperShiftRequestType { get; set; }
}
