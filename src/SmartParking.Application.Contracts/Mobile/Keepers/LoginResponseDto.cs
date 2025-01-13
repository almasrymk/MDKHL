using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Mobile.Keepers;

public class LoginResponseDto 
{
    public string AccessToken { get; set; }
    public string Role { get; set; }
    public string UserId { get; set; }
    public string Baseurl { get; set; }
    public string TenantId { get; set; }
    public int OrganizationId { get; set; }
    public int GracePeriod { get; set; }
}
