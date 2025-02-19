using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Entities.Shifts;
public class DailyShift : BaseEntity 
{
    public Guid KeeperId { get; set; }
    public int OrganizationId { get; set; }
    public int GarageId { get; set; }
    public int ShiftId { get; set; }
    public DateTime RequestTime { get; set; }
    public KeeperShiftRequestType KeeperShiftRequestType { get; set; }
}
