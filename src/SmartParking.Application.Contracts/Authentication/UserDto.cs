using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Identity;

namespace SmartParking.Authentication;

public class UserDto:IdentityUserDto
{
    public string Image { get; set; }
    public Guid RoleId { get; set; }
    public IdentityRoleDto Role { get; set; }
    public IdentificationTypes IdentificationTypes { get; set; }
    public string NationalId { get; set; }
    public bool IsCustomer { get; set; }
    public UserTypeCategory UserTypeCategory { get; set; }
}
