using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Utilities;

public class OpenGateRequestDto
{
    public string ip { get; set; }
    public int port { get; set; }
    public string username { get; set; }
    public string password { get; set; }
}
