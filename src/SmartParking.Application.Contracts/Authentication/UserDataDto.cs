using SmartParking.Enums;
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
    public string Phone { get; set; }
    public string Email { get; set; }
    public string IdentificationTypes { get; set; }
    public string NationalId { get; set; }
}
