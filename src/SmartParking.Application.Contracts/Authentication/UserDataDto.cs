using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Authentication;

public class UserDataDto : BaseResultDto
{
    public string FullName { get; set; }
    public string Image { get; set; }
}
