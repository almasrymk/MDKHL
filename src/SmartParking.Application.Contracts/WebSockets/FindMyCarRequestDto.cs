using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.WebSockets;

public class FindMyCarRequestDto
{
    public int OrganizationId { get; set; }
    public string LicensePlate { get; set; }

}
