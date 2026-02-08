using System.ComponentModel.DataAnnotations.Schema;

namespace SmartParking.Entities.Organizations;
[ComplexType]
public class OrganizationSettings
{
    public bool UserAccess { get; set; }
    public bool VehcileAccess { get; set; }
}
