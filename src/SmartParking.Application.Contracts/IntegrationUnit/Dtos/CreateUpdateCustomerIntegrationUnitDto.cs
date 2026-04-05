using SmartParking.Cutomers;
using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SmartParking.IntegrationUnit.Dtos;

public class CreateUpdateCustomerIntegrationUnitDto
{
    public string Name { get; set; }
    public string Mobile { get; set; }
    public string Email { get; set; }
    public string NId { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public IdentificationTypes IdType { get; set; }
    public List<CreateUpdateVehicleIntegrationUnitDto> Cars { get; set; }


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
            IdentificationType = input.IdType,
            NationalId = input.NId,

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
