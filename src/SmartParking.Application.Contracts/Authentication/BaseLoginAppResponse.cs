using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Authentication;

public class BaseLoginAppResponse  : BaseLoginResponse
{
   
    public string Role { get; set; }
    public string Baseurl { get; set; }
    public string TenantId { get; set; }
}
public class BaseLoginResponse
{
    public string AccessToken { get; set; }


}
