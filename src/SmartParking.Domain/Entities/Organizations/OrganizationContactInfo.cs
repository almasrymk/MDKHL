using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Entities.Organizations;

public class OrganizationContactInfo
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string Name { get; set; }
    public int NationalityId { get; set; }
    public string PhoneNum { get; set; }
    public string WhatsAppNum { get; set; }
    public string ContactMail { get; set; }
    public Status Status { get; set; }

}
