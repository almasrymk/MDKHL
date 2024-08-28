using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Organizations;

public class OrganizationDto
{
    public int Id { get; set; }
    public string EnglishName { get; set; }
    public string ArabicName { get; set; }
    public string Code { get; set; }

    public string UnifiedLicenseNum { get; set; }
    public DateOnly CRExpiryDate { get; set; }
    public DateOnly LicenseExpiryDate { get; set; }
    public string CountryName { get; set; }

    public string EmaraaName { get; set; }

    public string StatsName { get; set; }

    public int NumOfCameras { get; set; }

    public int MaxCapacity { get; set; }

    public string CityName { get; set; }
    public string PhoneNum { get; set; }
    public string WhatsAppNum { get; set; }
    public string ContactMail { get; set; }
    public Guid TenantId { get; set; }
    public Status Status { get; set; }


}
