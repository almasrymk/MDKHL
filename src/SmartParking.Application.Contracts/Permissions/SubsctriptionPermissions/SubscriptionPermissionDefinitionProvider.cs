using SmartParking.StringLocalizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;

namespace SmartParking.Permissions.SubsctriptionPermissions;

public class SubscriptionPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var smartParkingGroup = context.AddGroup(SubscriptionPermissions.Default, CustomStringLocalizer.L("Permission:Subscription"));

        var subscriptionPermission = smartParkingGroup.AddPermission(SubscriptionPermissions.Default, CustomStringLocalizer.L("Permission:Subscription"));
        subscriptionPermission.AddChild(SubscriptionPermissions.Create, CustomStringLocalizer.L(CustomStringLocalizer.Add));
        subscriptionPermission.AddChild(SubscriptionPermissions.Edit, CustomStringLocalizer.L(CustomStringLocalizer.Edit));
        subscriptionPermission.AddChild(SubscriptionPermissions.Delete, CustomStringLocalizer.L(CustomStringLocalizer.Delete));
    }
}
