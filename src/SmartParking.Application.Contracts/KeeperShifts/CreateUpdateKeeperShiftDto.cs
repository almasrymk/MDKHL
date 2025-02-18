using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.KeeperShifts
{
    public class CreateUpdateKeeperShiftDto
    {
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public int GarageId { get; set; }
        public int OrganizationId { get; set; }
        public Guid KeeperId { get; set; }
        public int ShiftId { get; set; }
        public Guid TenantId { get; set; }

    }
}
