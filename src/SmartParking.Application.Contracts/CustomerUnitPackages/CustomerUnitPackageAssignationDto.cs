using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.CustomerUnitPackages;

public class CustomerUnitPackageAssignationDto
{
    public decimal Discount { get; set; }
    public int DurationOfPackage { get; set; }
    public decimal Vat { get; set; }
}
