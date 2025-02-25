using SmartParking.StringLocalizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;

namespace SmartParking.Permissions.ShiftsPermissions;

public class ShiftPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var smartParkingGroup = context.AddGroup(ShiftPermission.Default, CustomStringLocalizer.L("Permission:Shift"));

        var shiftsPermission = smartParkingGroup.AddPermission(ShiftPermission.Default, CustomStringLocalizer.L("Permission:Shift"));
        shiftsPermission.AddChild(ShiftPermission.Create, CustomStringLocalizer.L(CustomStringLocalizer.Add));
        shiftsPermission.AddChild(ShiftPermission.Edit, CustomStringLocalizer.L(CustomStringLocalizer.Edit));
    }
}