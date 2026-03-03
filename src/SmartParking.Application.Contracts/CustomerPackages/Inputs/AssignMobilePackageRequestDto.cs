using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.CustomerPackages.Inputs;

public class AssignMobilePackageRequestDto
{
    public int PackageId { get; set; }
    public string ArabicNumber { get; set; } = string.Empty;
    public string EnglishPlateNumber { get; set; } = string.Empty;
}
