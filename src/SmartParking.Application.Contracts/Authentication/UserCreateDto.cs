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
    [Required]
    public int UserType { get; set; }
    public int OrganizationId { get; set; }
    public Guid? TenantId { get; set; }
    [Required]
    [RegularExpression(@"^(([^<>()\[\]\\.,;:\s@\""]+(\.[^<>()\[\]\\.,;:\s@\""]+)*)|(\"".+\""))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$",
       ErrorMessage = "Invalid email format")]
    public string Email { get; set; }
}
