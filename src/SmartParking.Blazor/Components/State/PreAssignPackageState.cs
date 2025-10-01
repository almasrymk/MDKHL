using SmartParking.CustomerPackages;
using SmartParking.Cutomers;

namespace SmartParking.Blazor.Components.State;

public class PreAssignPackageState
{
    public CustomerPackageDto? CustomerPackage { get; set; } = new();
    public CustomerDto? Customer { get; set; } = new();
}
