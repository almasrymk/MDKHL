using SmartParking.Permissions.CameraPermissions;
using SmartParking.Permissions.MyAccount;
using SmartParking.Permissions.UnLinkedTransactionPermissions;
using SmartParking.StringLocalizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;

namespace SmartParking.Permissions.InvoicesPermissions;

public class InvoicePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var smartParkingGroup = context.AddGroup(InvoicePermission.Default, CustomStringLocalizer.L("Permission:Invoice"));

        var invoicePermission = smartParkingGroup.AddPermission(InvoicePermission.Default, CustomStringLocalizer.L("Permission:Invoice"));
        invoicePermission.AddChild(InvoicePermission.Confirm, CustomStringLocalizer.L("Permission:Confirm"));
        invoicePermission.AddChild(InvoicePermission.View, CustomStringLocalizer.L("Permission:View"));
    }
}