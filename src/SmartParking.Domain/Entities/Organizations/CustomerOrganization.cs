using SmartParking.Entities.Customers;
using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace SmartParking.Entities.Organizations;

public class CustomerOrganization:FullAuditedEntity<int> , IBseEntity
{
    public int OrganizationId { get; set; }
    public virtual Organization Organizations { get; set; }
    public Guid CustomerId { get; set; }
    public virtual Customer Customers { get; set; }
    public Status Status { get; set; }
}
