using SmartParking.StringLocalizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;

namespace SmartParking.Permissions.ContactPermissions;

public class ContactPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var smartParkingGroup = context.AddGroup(ContactPermissions.Default, CustomStringLocalizer.L("Permission:Contact"));

        var cameraPermission = smartParkingGroup.AddPermission(ContactPermissions.Default, CustomStringLocalizer.L("Permission:Contact"));
        cameraPermission.AddChild(ContactPermissions.Create, CustomStringLocalizer.L(CustomStringLocalizer.Add));
        cameraPermission.AddChild(ContactPermissions.Edit, CustomStringLocalizer.L(CustomStringLocalizer.Edit));
        cameraPermission.AddChild(ContactPermissions.Delete, CustomStringLocalizer.L(CustomStringLocalizer.Delete));
        cameraPermission.AddChild(ContactPermissions.View, CustomStringLocalizer.L(CustomStringLocalizer.View));
    }
}
