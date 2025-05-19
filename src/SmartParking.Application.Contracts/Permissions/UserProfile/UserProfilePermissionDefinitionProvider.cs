using SmartParking.Permissions.MyAccount;
using SmartParking.StringLocalizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;

namespace SmartParking.Permissions.UserProfile;

public class UserProfilePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var smartParkingGroup = context.AddGroup(MyAccountPermissions.Default, CustomStringLocalizer.L("Permission:Profile"));

        var userProfilePermission = smartParkingGroup.AddPermission(MyAccountPermissions.Default, CustomStringLocalizer.L("Permission:Profile"));
        userProfilePermission.AddChild(UserProfilePermissions.Edit, CustomStringLocalizer.L(CustomStringLocalizer.Edit));
        userProfilePermission.AddChild(UserProfilePermissions.ChangePassword, CustomStringLocalizer.L(CustomStringLocalizer.ChangePassword));
    }
}
