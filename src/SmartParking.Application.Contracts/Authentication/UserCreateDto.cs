using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Identity;
using Volo.Abp.Validation;

namespace SmartParking.Authentication;

public class UserCreateDto: IdentityUserCreateDto
{
    public int UserType { get; set; }
    public int OrganizationId { get; set; }
}
