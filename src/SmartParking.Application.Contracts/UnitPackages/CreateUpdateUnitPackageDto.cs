using SmartParking.Packages;

namespace SmartParking.UnitPackages;

public class CreateUpdateUnitPackageDto
{
    public CreateUpdatePackageDto Package {  get; set; }
    public int NumberOfVehicles { get; set; }
    public int NumberOfSlots { get; set; }
}
