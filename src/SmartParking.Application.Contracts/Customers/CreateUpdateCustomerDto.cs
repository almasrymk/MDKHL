using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Identity;

namespace SmartParking.Cutomers;

public class CreateUpdateCustomerDto
{
    public string FullName { get; set; }
    public string UserName { get; set; }
    public string ArabicName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string RepeatPassword { get; set; }
    public string MobileNumber { get; set; }
    public IdentificationTypes IdentificationType { get; set; }
    public string NationalId { get; set; }
    public bool isActive { get; set; } = true;
    public CustomerCategoryStatus CustomerCategory { get; set; }
    public int OrganizationId { get; set; }
    public int PackageId { get; set; }
}
