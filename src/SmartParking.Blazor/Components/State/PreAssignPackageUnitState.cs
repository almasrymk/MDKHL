using SmartParking.CustomerPackages;
using SmartParking.Cutomers;
using SmartParking.Enums;
using SmartParking.UnitPackages;

namespace SmartParking.Blazor.Components.State;

public class PreAssignPackageUnitState
{
    public UnitPackageDto? UnitPackage { get; set; } = new();
    public CustomerDto? Customer { get; set; } = new();
    public string UnitCode { get; set; }
    public CustomerUnitPackageType CustomerUnitPackageType { get; set; }
}
