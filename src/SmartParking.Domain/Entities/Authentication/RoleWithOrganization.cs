
using SmartParking.Entities.Organizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Identity;

namespace SmartParking.Entities.Authentication;

public class RoleWithOrganization
{
    public IdentityRole Role { get; set; }
    public Organization Organization { get; set; }
}
