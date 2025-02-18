using SmartParking.StringLocalizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;

namespace SmartParking.Permissions.MyAccount;

public class MyAccountPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {

        var smartParkingGroup = context.AddGroup(MyAccountPermissions.Default, CustomStringLocalizer.L("Permission:MyAccount"));

        var cameraPermission = smartParkingGroup.AddPermission(MyAccountPermissions.Default, CustomStringLocalizer.L("Permission:MyAccount"));
        cameraPermission.AddChild(MyAccountPermissions.Edit, CustomStringLocalizer.L(CustomStringLocalizer.Edit));
        cameraPermission.AddChild(MyAccountPermissions.ChanageStatus, CustomStringLocalizer.L(CustomStringLocalizer.ChanageStatus));
    }
}
