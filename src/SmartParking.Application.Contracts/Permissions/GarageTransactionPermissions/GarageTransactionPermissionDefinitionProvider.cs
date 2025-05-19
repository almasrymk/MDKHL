using SmartParking.StringLocalizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;

namespace SmartParking.Permissions.GarageTransactionPermissions;

public class GarageTransactionPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var smartParkingGroup = context.AddGroup(GarageTransactionPermissions.Default, CustomStringLocalizer.L("Permission:GarageTransaction"));

        var transactionPermission = smartParkingGroup.AddPermission(GarageTransactionPermissions.Default, CustomStringLocalizer.L("Permission:GarageTransaction"));
        transactionPermission.AddChild(GarageTransactionPermissions.View, CustomStringLocalizer.L("Permission:View"));
       

    }
}