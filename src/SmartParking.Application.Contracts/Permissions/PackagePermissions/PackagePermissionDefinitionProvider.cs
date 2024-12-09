using SmartParking.StringLocalizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;

namespace SmartParking.Permissions.PackagePermissions;

public class PackagePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var smartParkingGroup = context.AddGroup(PackagePermissions.GroupName, CustomStringLocalizer.L("Permission:Camera"));

        var packagesPermission = smartParkingGroup.AddPermission(PackagePermissions.Default);
        packagesPermission.AddChild(PackagePermissions.Create, CustomStringLocalizer.L(CustomStringLocalizer.Add));
        packagesPermission.AddChild(PackagePermissions.Edit, CustomStringLocalizer.L(CustomStringLocalizer.Edit));
        packagesPermission.AddChild(PackagePermissions.Delete, CustomStringLocalizer.L(CustomStringLocalizer.Delete));
    }
}
