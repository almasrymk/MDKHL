using SmartParking.Permissions.Discounts;
using SmartParking.StringLocalizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;

namespace SmartParking.Permissions.CheckoutPermissions;

internal class CheckoutPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var smartParkingGroup = context.AddGroup(CheckoutPermissions.Default, CustomStringLocalizer.L("Permission:Checkout"));
        var checkoutPermission = smartParkingGroup.AddPermission(CheckoutPermissions.Default, CustomStringLocalizer.L("Permission:Checkout"));
        checkoutPermission.AddChild(CheckoutPermissions.Confirm, CustomStringLocalizer.L("Permission: Confirm"));
        checkoutPermission.AddChild(CheckoutPermissions.OpenGate, CustomStringLocalizer.L("Permission:OpenGate"));
        checkoutPermission.AddChild(CheckoutPermissions.Discount, CustomStringLocalizer.L("Permission:CheckoutDiscount"));

    }
}
