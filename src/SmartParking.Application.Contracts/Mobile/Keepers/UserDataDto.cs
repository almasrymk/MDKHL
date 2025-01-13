using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Mobile.Keepers;

public class UserDataDto 
{
    public string FullName { get; set; }
    public string UserName { get; set; }
    public string Image { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public int OrganizationId { get; set; }
}
