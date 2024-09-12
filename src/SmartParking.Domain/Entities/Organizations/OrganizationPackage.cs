using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Entities.Organizations;

public class OrganizationPackage : BaseEntity
{
    public int PackageTypeId { get; set; }
    public decimal Price { get; set; }
    public string Details { get; set; }
    public int OrganizationId { get; set; }
    public Status Status { get; set; }
}
