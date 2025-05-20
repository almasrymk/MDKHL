using SmartParking.Permissions.UnLinkedTransactionPermissions;
using SmartParking.StringLocalizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;

namespace SmartParking.Permissions.PaymentsPermissions;

public  class PaymentsPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var smartParkingGroup = context.AddGroup(PaymentsPermission.Default, CustomStringLocalizer.L("Permission:Payment"));

        var paymentPermission = smartParkingGroup.AddPermission(PaymentsPermission.Default, CustomStringLocalizer.L("Permission:Payment"));

        paymentPermission.AddChild(PaymentsPermission.View, CustomStringLocalizer.L("Permission:View"));

    }
}