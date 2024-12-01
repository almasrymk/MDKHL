using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Packages;

public class PayementPackageDto
{
    public string ArabicName { get; set; }
    public string EnglishName { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; }
    public DateTime PayementTime { get; set; }
    public string OrganizationArabicName { get; set; }
    public string OrganizationEnglishName { get; set; }
    public string CustomerEnglishName { get; set; }
    public string CustomerArabicName { get; set; }
    public string OrganizationAddress { get; set; }
    public string ZadacaCode { get; set; }

}
