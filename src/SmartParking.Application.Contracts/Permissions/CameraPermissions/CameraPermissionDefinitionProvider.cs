using SmartParking.Localization;
using SmartParking.StringLocalizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;
namespace SmartParking.Permissions.CameraPermissions;

public class CameraPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var smartParkingGroup = context.AddGroup(CameraPermissions.Default, CustomStringLocalizer.L("Permission:Camera"));

        var cameraPermission = smartParkingGroup.AddPermission(CameraPermissions.Default);
        cameraPermission.AddChild(CameraPermissions.Create, CustomStringLocalizer.L(CustomStringLocalizer.Add));
        cameraPermission.AddChild(CameraPermissions.Edit, CustomStringLocalizer.L(CustomStringLocalizer.Edit));
        cameraPermission.AddChild(CameraPermissions.Delete, CustomStringLocalizer.L(CustomStringLocalizer.Delete));
    }

}
