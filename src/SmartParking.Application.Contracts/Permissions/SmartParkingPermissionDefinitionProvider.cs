using SmartParking.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace SmartParking.Permissions;

public class SmartParkingPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(SmartParkingPermissions.GroupName);
        //Define your own permissions here. Example:
        //myGroup.AddPermission(SmartParkingPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<SmartParkingResource>(name);
    }
}
