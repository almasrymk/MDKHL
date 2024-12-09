using SmartParking.StringLocalizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;

namespace SmartParking.Permissions.CustomerPermissions;

public class CustomerPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var smartParkingGroup = context.AddGroup(CustomerPermissions.Default, CustomStringLocalizer.L("Permission:Customer"));

        var garagePermission = smartParkingGroup.AddPermission(CustomerPermissions.Default, CustomStringLocalizer.L("Permission:Customer"));
        garagePermission.AddChild(CustomerPermissions.Create, CustomStringLocalizer.L(CustomStringLocalizer.Add));
        garagePermission.AddChild(CustomerPermissions.Edit, CustomStringLocalizer.L(CustomStringLocalizer.Edit));
        garagePermission.AddChild(CustomerPermissions.Delete, CustomStringLocalizer.L(CustomStringLocalizer.Delete));
    }
}
