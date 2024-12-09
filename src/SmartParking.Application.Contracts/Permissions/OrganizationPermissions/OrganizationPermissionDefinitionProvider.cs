using SmartParking.StringLocalizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;

namespace SmartParking.Permissions.OrganizationPermissions;

public class OrganizationPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var smartParkingGroup = context.AddGroup(OrganizationPermissions.GroupName, CustomStringLocalizer.L("Permission:Organization"));

        var organizationPermission = smartParkingGroup.AddPermission(OrganizationPermissions.Default);
        organizationPermission.AddChild(OrganizationPermissions.Create, CustomStringLocalizer.L(CustomStringLocalizer.Add));
        organizationPermission.AddChild(OrganizationPermissions.Edit, CustomStringLocalizer.L(CustomStringLocalizer.Edit));
        organizationPermission.AddChild(OrganizationPermissions.Delete, CustomStringLocalizer.L(CustomStringLocalizer.Delete));

    }
}
