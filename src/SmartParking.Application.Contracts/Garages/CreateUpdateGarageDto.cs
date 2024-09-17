using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.OrganizationsGarages;

public class CreateUpdateGarageDto
{
    public string Code { get; set; }
    public string Name { get; set; }
    public int MaxCapacity { get; set; }
    public TimeOnly ClosingHours { get; set; }
    public int OrganizationId { get; set; }
    public Status Status { get; set; }
}
