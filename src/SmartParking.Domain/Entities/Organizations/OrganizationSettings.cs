using System.ComponentModel.DataAnnotations.Schema;

namespace SmartParking.Entities.Organizations;
[ComplexType]
public class OrganizationSettings
{
    public bool CustomerPackage { get; set; }
    public bool VehcilePackage { get; set; }
    public bool UseShifts { get; set; }

}
