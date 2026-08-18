using SmartParking.StringLocalizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;

namespace SmartParking.Permissions.UnlinkedEntryPermissions;

public class UnlinkedEntryPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var smartParkingGroup = context.AddGroup(UnlinkedEntryPermissions.Default, CustomStringLocalizer.L("Permission:UnlinkedEntry"));

        var unlinkedEntryPermission = smartParkingGroup.AddPermission(UnlinkedEntryPermissions.Default, CustomStringLocalizer.L("Permission:UnlinkedEntry"));
        unlinkedEntryPermission.AddChild(UnlinkedEntryPermissions.View, CustomStringLocalizer.L(CustomStringLocalizer.View));
        unlinkedEntryPermission.AddChild(UnlinkedEntryPermissions.SendAsEntry, CustomStringLocalizer.L(CustomStringLocalizer.SendAsEntry));
    }
}
