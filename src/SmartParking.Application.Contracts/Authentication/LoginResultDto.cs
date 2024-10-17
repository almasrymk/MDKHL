using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Authentication;

public class LoginResultDto 
{
   
    public string AccessToken { get; set; }
   
    public string Role { get; set; }
    public string Baseurl { get; set; }
    public string TenantId { get; set; }
}
