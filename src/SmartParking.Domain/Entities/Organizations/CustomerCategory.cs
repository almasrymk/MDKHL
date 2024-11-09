using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace SmartParking.Entities.Organizations;

public class CustomerCategory : FullAuditedEntity<int> , IBseEntity
{
    public string Name { get; set; }
    public string  Description { get; set; }
    public bool Default { get; set; }
    public int OrganizationId { get; set; }
    public Status Status { get; set; }
    public virtual Organization Organization { get; set; }
}
