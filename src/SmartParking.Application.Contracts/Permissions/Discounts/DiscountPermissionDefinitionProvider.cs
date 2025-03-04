using SmartParking.StringLocalizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;

namespace SmartParking.Permissions.Discounts;
public class DiscountPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var smartParkingGroup = context.AddGroup(DiscountPermissions.Default, CustomStringLocalizer.L("Permission:Discount"));
        var cameraPermission = smartParkingGroup.AddPermission(DiscountPermissions.Default, CustomStringLocalizer.L("Permission:Discount"));
        cameraPermission.AddChild(DiscountPermissions.Create, CustomStringLocalizer.L(CustomStringLocalizer.Add));
        cameraPermission.AddChild(DiscountPermissions.Edit, CustomStringLocalizer.L(CustomStringLocalizer.Edit));
        cameraPermission.AddChild(DiscountPermissions.Delete, CustomStringLocalizer.L(CustomStringLocalizer.Delete));
    }
}
