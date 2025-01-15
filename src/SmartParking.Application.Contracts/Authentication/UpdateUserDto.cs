using JetBrains.Annotations;
using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Identity;
using Volo.Abp.Validation;

namespace SmartParking.Authentication;

public class UpdateUserDto 
{
    [Required]
    [DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.MaxUserNameLength))]
    public string UserName { get; set; }
    [Required]
    [DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.MaxNameLength))]
    public string Name { get; set; }

    [DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.MaxSurnameLength))]
    public string Surname { get; set; }

    [DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.MaxPhoneNumberLength))]
    public string PhoneNumber { get; set; }

    public bool IsActive { get; set; } = true;
    public Guid? TenantId { get; set; }
    
    [RegularExpression(@"^(([^<>()\[\]\\.,;:\s@\""]+(\.[^<>()\[\]\\.,;:\s@\""]+)*)|(\"".+\""))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$",
       ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;
    public Guid RoleId { get; set; }
    public string ConcurrencyStamp { get; set; }
    public bool ISNoRole { get; set; }
    [Required]
    public IdentificationTypes IdentificationTypes { get; set; }
    [Required]
    public string NationalId { get; set; }
    public bool IsCustomer { get; set; }
}
