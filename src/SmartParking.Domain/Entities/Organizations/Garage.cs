using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace SmartParking.Entities.Organizations;

public class Garage : FullAuditedEntity<int>
{
    public string Code { get; set; }
    public string Name { get; set; }
    public int MaxCapacity { get; set; }
    public TimeOnly ClosingHours { get; set; }
    public int OrganizationId { get; set; }
    public Status Status { get; set; }
}
