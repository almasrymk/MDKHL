using SmartParking.CustomerPackages.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.PDF.Dtos;

public class PackagePdfData : PdfData<CustomerPackageDto>
{
    public new PackageLocalizationData Localization { get; set; }
}
