using SmartParking.Localization;
using SmartParking.StringLocalizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;

namespace SmartParking.Permissions.GaragePermissions;

public class GaragePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var smartParkingGroup = context.AddGroup(GaragePermissions.GroupName, CustomStringLocalizer.L("Permission:Garage"));

        var garagePermission = smartParkingGroup.AddPermission(GaragePermissions.Default);
        garagePermission.AddChild(GaragePermissions.Create, CustomStringLocalizer.L(CustomStringLocalizer.Add));
        garagePermission.AddChild(GaragePermissions.Edit, CustomStringLocalizer.L(CustomStringLocalizer.Edit));
        garagePermission.AddChild(GaragePermissions.Delete, CustomStringLocalizer.L(CustomStringLocalizer.Delete));
    }
}
