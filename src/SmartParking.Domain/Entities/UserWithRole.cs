
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Identity;

namespace SmartParking.Entities;

public class UserWithRole
{
    public IdentityUser User { get; set; }
    public IdentityRole Role { get; set; }
}
