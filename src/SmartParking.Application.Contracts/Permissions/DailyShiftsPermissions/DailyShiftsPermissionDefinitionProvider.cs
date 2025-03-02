//using SmartParking.Permissions.MyShiftsPermissions;
//using SmartParking.Permissions.UserShiftsPermissions;
//using SmartParking.StringLocalizer;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Volo.Abp.Authorization.Permissions;

//namespace SmartParking.Permissions.DailyShiftsPermissions;

//public class DailyShiftsPermissionDefinitionProvider : PermissionDefinitionProvider
//{
//    public override void Define(IPermissionDefinitionContext context)
//    {
//        var smartParkingGroup = context.AddGroup(DailyShiftsPermission.Default, CustomStringLocalizer.L("Permission:DailyShift"));

//        var UserShiftsPermission = smartParkingGroup.AddPermission(DailyShiftsPermission.Default, CustomStringLocalizer.L("Permission:DailyShift"));
//    }
//}