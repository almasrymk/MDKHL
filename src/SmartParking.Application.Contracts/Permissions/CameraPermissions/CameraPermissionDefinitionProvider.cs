using SmartParking.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace SmartParking.Permissions.CameraPermissions;

public class CameraPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var smartParkingGroup = context.AddGroup(CameraPermissions.GroupName,L("Permission:Camera"));

        var cameraPermission = smartParkingGroup.AddPermission(CameraPermissions.Default);
        cameraPermission.AddChild(CameraPermissions.Create, L("Permission:Create"));
        cameraPermission.AddChild(CameraPermissions.Edit, L("Permission:Edit"));
        cameraPermission.AddChild(CameraPermissions.Delete, L("Permission:Delete"));
    }
    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<SmartParkingResource>(name);
    }
}
