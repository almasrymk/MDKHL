using SmartParking.Permissions.MyAccount;
using SmartParking.StringLocalizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;

namespace SmartParking.Permissions.Reports;

public class ReportsPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var smartParkingGroup = context.AddGroup(ReportsPermissions.Default, CustomStringLocalizer.L("Permission:Reports"));

        var rolesPermissions = smartParkingGroup.AddPermission(ReportsPermissions.Default, CustomStringLocalizer.L("Permission:Reports"));
        rolesPermissions.AddChild(ReportsPermissions.IT, CustomStringLocalizer.L(CustomStringLocalizer.IT));
        rolesPermissions.AddChild(ReportsPermissions.SecurityManager, CustomStringLocalizer.L(CustomStringLocalizer.SecurityManager));
        rolesPermissions.AddChild(ReportsPermissions.FinancialManagement, CustomStringLocalizer.L(CustomStringLocalizer.FinancialManagement));
    }
}
