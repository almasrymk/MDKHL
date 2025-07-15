using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.CustomerPackages.QRGenerator;
public class QrCodeDto
{
    public Guid UserId { get; set; }
    public int OrganizationId { get; set; }
    public string Code { get; set; }
    public string QrCodeImage { get; set; }
}
