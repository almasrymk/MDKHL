using SmartParking.Cutomers;
using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.IntegrationUnit.Dtos;

public class CreateUpdateCustomerIntegrationUnitDto
{
    public string ArabicName { get; set; }
    public string EnglishName { get; set; }
    public string PhoneNumber { get; set; }
    public IdentificationTypes IdentificationType { get; set; }
    public string NationalId { get; set; }

    public CreateUpdateCustomerDto MapToCustomerDto( CreateUpdateCustomerIntegrationUnitDto input)
    {
        return new CreateUpdateCustomerDto
        {
            // Combine names (you can adjust logic as needed)
            FullName = input.EnglishName,

            // You can decide what UserName should be (example below)
            UserName = input.PhoneNumber,

            ArabicName = input.ArabicName,

            // No email in source → set default or generate
            Email = string.Empty,

            // No password in source → set default or generate
            Password = "P@ssw0rd",
            RepeatPassword = "P@ssw0rd",

            PhoneNumber = input.PhoneNumber,
            IdentificationType = input.IdentificationType,
            NationalId = input.NationalId,

            // Defaults / business rules
            isActive = true,
            CustomerCategory = CustomerCategoryStatus.Customer,
            OrganizationId = 0, 
            IsUser = false,

            RoleId = Guid.Empty, // لازم تجيبه من DB أو config
            TenantId = null
        };
    }
}
