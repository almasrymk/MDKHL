using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.CustomerPackages.QRGenerator;
public class CreateUpdateQrCodeDto
{
    public string PhoneNumber { get; set; }
    public int OrganizationId { get; set; }
    public string PackageCode { get; set; }
}
