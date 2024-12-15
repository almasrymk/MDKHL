using SmartParking.StringLocalizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;

namespace SmartParking.Permissions.UnLinkedTransactionPermissions;

public class UnLinkedTransactionPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var smartParkingGroup = context.AddGroup(UnLinkedTransactionPermission.Default, CustomStringLocalizer.L("Permission:UnLinkedTransaction"));

        var organizationPermission = smartParkingGroup.AddPermission(UnLinkedTransactionPermission.Default, CustomStringLocalizer.L("Permission:UnLinkedTransaction"));


    }
}