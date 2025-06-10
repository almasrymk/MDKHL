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
    [MaxLength(277)]
    public string UserName { get; set; }


    public string Name { get; set; }

    
    public string Surname { get; set; }


    
    public string PhoneNumber { get; set; }

    public bool IsActive { get; set; } = true;
    
    public string Password { get; set; }
    public int UserType { get; set; }
    public int OrganizationId { get; set; }
    public Guid? TenantId { get; set; }
   
    public string Email { get; set; } = string.Empty;
    public IdentificationTypes IdentificationTypes { get; set; }
    public string NationalId { get; set; }

    public bool IsCustomer { get; set; }
    public UserTypeCategory? UserTypeCategory { get; set; }
}
