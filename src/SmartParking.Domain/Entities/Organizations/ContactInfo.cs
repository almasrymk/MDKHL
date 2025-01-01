using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Entities.Organizations;

public class ContactInfo : FullAuditedEntity<int>, IMultiTenant , IBaseEntity
{
    public string Name { get; set; }
    public int OrganizationId { get; set; }
    public string PhoneNum { get; set; }
    public string WhatsAppNum { get; set; }
    public string ContactMail { get; set; }
    public Guid? TenantId { get; set; }
    public Status Status { get; set ; }
}
