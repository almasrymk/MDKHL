using SmartParking.Enums;

namespace SmartParking.Entities.Organizations;

public class UnitPackage : BaseEntity
{
    public int PackageId { get; set; }
    public virtual Package Package { get; set; }
    public int NumberOfVehicles { get; set; }
    public int NumberOfSlots { get; set; }
}
