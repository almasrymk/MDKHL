using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Entities.Organizations;

public class OrganizationCustomers
{
    public int OrganizationCustomersId { get; set; }
    public int OrganizationId { get; set; }
    public int CustomerId { get; set; }
    public Status Status { get; set; }
}
