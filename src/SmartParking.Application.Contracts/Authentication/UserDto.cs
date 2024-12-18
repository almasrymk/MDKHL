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
}
