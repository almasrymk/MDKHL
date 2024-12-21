using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Identity;

namespace SmartParking.Authentication;

public class customIdentityRoleDto : IdentityRoleDto
{
    public string OrganizationEnglishName { get; set; }
    public string OrganizationArabicName { get; set; }
}
