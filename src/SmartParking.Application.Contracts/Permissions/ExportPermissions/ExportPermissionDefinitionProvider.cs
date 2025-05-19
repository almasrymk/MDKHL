//using SmartParking.StringLocalizer;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Volo.Abp.Authorization.Permissions;

//namespace SmartParking.Permissions.ExportPermissions;

//public class ExportPermissionDefinitionProvider : PermissionDefinitionProvider
//{
//    public override void Define(IPermissionDefinitionContext context)
//    {
//        var smartParkingGroup = context.AddGroup(ExportPermissions.Default, CustomStringLocalizer.L("Permission:Export"));
//        var exportPermission = smartParkingGroup.AddPermission(ExportPermissions.Default, CustomStringLocalizer.L("Permission:Export"));
//        exportPermission.AddChild(ExportPermissions.PDF, CustomStringLocalizer.L("Permission:PDF Export"));
//        exportPermission.AddChild(ExportPermissions.CSV, CustomStringLocalizer.L("Permission:CSV Export"));
//        exportPermission.AddChild(ExportPermissions.Excel, CustomStringLocalizer.L("Permission:Excel Export"));
//    }
//}
