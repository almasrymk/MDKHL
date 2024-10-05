using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Authentication;

public class UserDataDto
{
    public string FullName { get; set; }
    public string Image { get; set; }
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
}
