using SmartParking.Permissions.UnitsPermissions;
using SmartParking.StringLocalizer;
using Volo.Abp.Authorization.Permissions;

namespace SmartParking.Permissions.UnitVehiclePackagesPermissions;

public class UnitVehiclePackagePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var smartParkingGroup = context.AddGroup(UnitVehiclePackagePermissions.Default, CustomStringLocalizer.L("Permission:UnitVehiclePackage"));

        var unitVehiclePackagePermission = smartParkingGroup.AddPermission(UnitVehiclePackagePermissions.Default, CustomStringLocalizer.L("Permission:UnitVehiclePackage"));
        unitVehiclePackagePermission.AddChild(UnitVehiclePackagePermissions.View, CustomStringLocalizer.L(CustomStringLocalizer.View));
    }
}
