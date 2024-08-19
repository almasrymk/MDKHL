using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Entities.Organizations;

public class OrganizationSubscriptions 
{
    public int OrganizationSubscriptionsId { get; set; }
    public int OrganizationId { get; set; }
    public int SubscriptionsId { get; set; }

    public Status Status { get; set; }
}
