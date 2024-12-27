using JetBrains.Annotations;
using SmartParking.Enums;
using SmartParking.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Identity;
using Volo.Abp.Validation;

namespace SmartParking.Authentication;

public class UserCreateDto
{
    public string UserName { get; set; }

    
    public string Name { get; set; }

    
    public string Surname { get; set; }


    
    public string PhoneNumber { get; set; }

    public bool IsActive { get; set; }

    [Required]
    
    public string Password { get; set; }
    [Required]
    public int UserType { get; set; }
    public int OrganizationId { get; set; }
    public Guid? TenantId { get; set; }
    [RegularExpression(@"^(([^<>()\[\]\\.,;:\s@\""]+(\.[^<>()\[\]\\.,;:\s@\""]+)*)|(\"".+\""))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$",
       ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;
    [Required]
    public IdentificationTypes IdentificationTypes { get; set; }
    [Required]
    public string NationalId { get; set; }

    public bool IsCustomer { get; set; }
}
