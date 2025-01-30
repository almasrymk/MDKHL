using SmartParking.Entities.Organizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Entities.Analysis;

public class PackagePaymentAnalysis
{
    public Package Package { get; set; }
    public int CountUsePackage { get; set; }
    public decimal ValueOfPackage { get; set; }
    public decimal ValueTotalPackage { get; set; }
}
