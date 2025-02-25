using SmartParking.Permissions.ShiftsPermissions;
using SmartParking.StringLocalizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;

namespace SmartParking.Permissions.UserShiftsPermissions;

public class UserShiftPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var smartParkingGroup = context.AddGroup(UserShiftPermission.Default, CustomStringLocalizer.L("Permission:UserShift"));

        var UserShiftsPermission = smartParkingGroup.AddPermission(UserShiftPermission.Default, CustomStringLocalizer.L("Permission:UserShift"));
        UserShiftsPermission.AddChild(UserShiftPermission.Assign, CustomStringLocalizer.L("Assign"));
        UserShiftsPermission.AddChild(UserShiftPermission.ViewUserShift, CustomStringLocalizer.L("ViewUserShift"));
        UserShiftsPermission.AddChild(UserShiftPermission.ViewUserActive, CustomStringLocalizer.L("ViewUserActive"));
    }
}