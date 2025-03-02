using SmartParking.Permissions.DailyShiftsPermissions;
using SmartParking.Permissions.ShiftsPermissions;
using SmartParking.Permissions.UserShiftsPermissions;
using SmartParking.StringLocalizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;

namespace SmartParking.Permissions.MyShiftsPermissions;

public class ManageShiftsPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var smartParkingGroup = context.AddGroup(ManageShiftsPermissions.Default, CustomStringLocalizer.L("Permission:ManageShifts"));

        var shiftsPermission = smartParkingGroup.AddPermission(ShiftPermission.Default, CustomStringLocalizer.L("Permission:Shift"));
        shiftsPermission.AddChild(ShiftPermission.Create, CustomStringLocalizer.L(CustomStringLocalizer.Add));
        shiftsPermission.AddChild(ShiftPermission.Edit, CustomStringLocalizer.L(CustomStringLocalizer.Edit));

        var MyShiftsPermission = smartParkingGroup.AddPermission(MyShiftPermissions.Default, CustomStringLocalizer.L("Permission:MyShift"));

        var UserShiftsPermission = smartParkingGroup.AddPermission(UserShiftPermission.Default, CustomStringLocalizer.L("Permission:UserShift"));
        UserShiftsPermission.AddChild(UserShiftPermission.Assign, CustomStringLocalizer.L("Assign"));
        UserShiftsPermission.AddChild(UserShiftPermission.ViewUserShift, CustomStringLocalizer.L("ViewUserShift"));
        UserShiftsPermission.AddChild(UserShiftPermission.ViewUserActive, CustomStringLocalizer.L("ViewUserActive"));

        var DailyUserShiftsPermission = smartParkingGroup.AddPermission(DailyShiftsPermission.Default, CustomStringLocalizer.L("Permission:DailyShift"));


    }
}