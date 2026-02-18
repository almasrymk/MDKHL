using SmartParking.CustomerPackages;
using SmartParking.Cutomers;
using SmartParking.UnitPackages;

namespace SmartParking.Blazor.Components.State;

public class PreAssignPackageUnitState
{
    public UnitPackageDto? UnitPackage { get; set; } = new();
    public CustomerDto? Customer { get; set; } = new();
}
