using SmartParking.StringLocalizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;

namespace SmartParking.Permissions.DispenserPermissions;

public class DispenserPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var smartParkingGroup = context.AddGroup(DispenserPermissions.Default, CustomStringLocalizer.L("Permission:Dispenser"));

        var cameraPermission = smartParkingGroup.AddPermission(DispenserPermissions.Default, CustomStringLocalizer.L("Permission:Dispenser"));
        cameraPermission.AddChild(DispenserPermissions.Create, CustomStringLocalizer.L(CustomStringLocalizer.Add));
        cameraPermission.AddChild(DispenserPermissions.Edit, CustomStringLocalizer.L(CustomStringLocalizer.Edit));
        cameraPermission.AddChild(DispenserPermissions.Delete, CustomStringLocalizer.L(CustomStringLocalizer.Delete));
    }

}
