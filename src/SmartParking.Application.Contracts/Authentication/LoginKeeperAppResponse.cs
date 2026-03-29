using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Authentication;
public class LoginKeeperAppResponse : BaseLoginAppResponse
{
    public int OrganizationId { get; set; }
    public string ImageBase64 { get; set; }
}
