using SmartParking.Permissions.PackagePermissions;
using SmartParking.StringLocalizer;
using Volo.Abp.Authorization.Permissions;

namespace SmartParking.Permissions.UnitsPermissions;

public class UnitPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var smartParkingGroup = context.AddGroup(UnitPermissions.Default, CustomStringLocalizer.L("Permission:Unit"));

        var unitPermission = smartParkingGroup.AddPermission(UnitPermissions.Default, CustomStringLocalizer.L("Permission:Unit"));
        unitPermission.AddChild(UnitPermissions.Create, CustomStringLocalizer.L(CustomStringLocalizer.Add));
        unitPermission.AddChild(UnitPermissions.Edit, CustomStringLocalizer.L(CustomStringLocalizer.Edit));
        unitPermission.AddChild(UnitPermissions.Delete, CustomStringLocalizer.L(CustomStringLocalizer.Delete));
    }
}
