using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Organizations;

public class CreateUpdateOrganiztionDto 
{
    public string ArabicName { get; set; }
    public string EnglishName { get; set; }

    public string UnifiedLicenseNum { get; set; }
   // public Guid TenantId { get; set; }

    public int NumOfCameras { get; set; }

    public int MaxCapacity { get; set; }
    public DateOnly CRExpiryDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public DateOnly LicenseExpiryDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public int CountryId { get; set; }

    public int EmaraaId { get; set; }

    public int StateId { get; set; }

    public int CityId { get; set; }
    public string PhoneNum { get; set; }
    public string WhatsAppNum { get; set; }
    public string ContactMail { get; set; }

    public Status? Status { get; set; }

}
