using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Authentication;

public class LoginResultDto
{
    public bool Success { get; set; }
    public string AccessToken { get; set; }
    public string ErrorMessage { get; set; }
    public string Role { get; set; }
}
