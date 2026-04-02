using SmartParking.Cutomers;
using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SmartParking.IntegrationUnit.Dtos;

public class CreateUpdateCustomerIntegrationUnitDto
{
    public string Name { get; set; }
    public string Mobile { get; set; }
    public string Email { get; set; }
    public string NationalId { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public IdentificationTypes IdentificationType { get; set; }

    public CreateUpdateCustomerDto MapToCustomerDto( CreateUpdateCustomerIntegrationUnitDto input)
    {
        return new CreateUpdateCustomerDto
        {
            // Combine names (you can adjust logic as needed)
            FullName = input.Name,

            // You can decide what UserName should be (example below)
            UserName = input.Mobile,

            ArabicName = "مستاجر",

            Email = input.Email,

            // No password in source → set default or generate
            Password = "P@ssw0rd",
            RepeatPassword = "P@ssw0rd",

            PhoneNumber = input.Mobile,
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
