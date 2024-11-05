using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Mobile.Keepers;

public class LoginRequestDto
{
    public string PhoneNumber { get; set; }
    public string Password { get; set; }
}
