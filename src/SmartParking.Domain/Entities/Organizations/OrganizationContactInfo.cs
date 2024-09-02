using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace SmartParking.Entities.Organizations;

public class OrganizationContactInfo : FullAuditedEntity<int>
{
    public int OrganizationId { get; set; }
    public string Name { get; set; }
    public int NationalityId { get; set; }
    public string PhoneNum { get; set; }
    public string WhatsAppNum { get; set; }
    public string ContactMail { get; set; }
    public Status Status { get; set; }

}
