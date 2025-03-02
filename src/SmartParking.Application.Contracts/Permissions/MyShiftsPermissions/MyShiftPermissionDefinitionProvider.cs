//using SmartParking.Permissions.ShiftsPermissions;
//using SmartParking.StringLocalizer;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Volo.Abp.Authorization.Permissions;

//namespace SmartParking.Permissions.MyShiftsPermissions;

//public class MyShiftPermissionDefinitionProvider : PermissionDefinitionProvider
//{
//    public override void Define(IPermissionDefinitionContext context)
//    {
//        var smartParkingGroup = context.AddGroup(MyShiftPermissions.Default, CustomStringLocalizer.L("Permission:MyShift"));

//        var MyShiftsPermission = smartParkingGroup.AddPermission(MyShiftPermissions.Default, CustomStringLocalizer.L("Permission:MyShift"));
//    }
//}