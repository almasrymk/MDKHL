using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Entities.Organizations;

public class Organization : BaseEntity
{
    public string Code { get; set; }

    public string UnifiedLicenseNum { get; set; }
    public DateOnly CRExpiryDate { get; set; }
    public DateOnly LicenseExpiryDate { get; set; }
    public int CountryId { get; set; }

    public int EmaraaId { get; set; }

    public int StatId { get; set; }

    public int CityId { get; set; }
    public string PhoneNum { get; set; }
    public string WhatsAppNum { get; set; }
    public string  ContactMail { get; set; }


}
