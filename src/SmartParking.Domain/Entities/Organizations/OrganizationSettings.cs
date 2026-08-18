using System.ComponentModel.DataAnnotations.Schema;

namespace SmartParking.Entities.Organizations;
[ComplexType]
public class OrganizationSettings
{
    public bool CustomerPackage { get; set; }
    public bool VehcilePackage { get; set; }
    public bool UseShifts { get; set; }
    public int OwnerVehicles { get; set; }
    public int OwnerSlots { get; set; }
    public int RentalSlots { get; set; }
    public bool IsSlotsEditable { get; set; }
    public bool UnlinkedEntry { get; set; }
}
