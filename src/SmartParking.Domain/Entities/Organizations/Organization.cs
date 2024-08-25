using SmartParking.Entities.Cameras;
using SmartParking.Entities.Lookups;
using SmartParking.Enums;
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
    public virtual Country Country { get; set; }

    public int EmaraaId { get; set; }

    public virtual Emaraa Emaraa { get; set; }

    public int StatsId { get; set; }

    public virtual Stats Stats { get; set; }
    public int CityId { get; set; }
    public virtual City City { get; set; }
    public string PhoneNum { get; set; }
    public string WhatsAppNum { get; set; }
    public string  ContactMail { get; set; }
    public Status Status { get; set; }
    public virtual ICollection<OrganizationGarage> OrganizationGarages { get; set; }
    public virtual ICollection<Camera> Cameras { get; set; }
    public virtual ICollection<OrganizationPackage> organizationPackages { get; set; }
    public virtual ICollection<OrganizationContactInfo> organizationContactInfos { get; set; }
    public virtual ICollection<CustomerCategory> CustomerCategories { get; set; }



}
