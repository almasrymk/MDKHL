using SmartParking.Permissions.IdentityPermissions;
using SmartParking.StringLocalizer;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Identity.Localization;
using Volo.Abp.Localization;

namespace Volo.Abp.Identity;

public class CustomIdentityPermissionDefinitionProvider : IdentityPermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var identityGroup = context.GetGroup(IdentityPermissions.GroupName);
        identityGroup.DisplayName = CustomStringLocalizer.L("Administration");
        var rolesPermission = identityGroup.GetPermissionOrNull(IdentityPermissions.Roles.Default);
        rolesPermission.DisplayName = CustomStringLocalizer.L("Roles");
        //rolesPermission.AddChild(IdentityPermissions.Roles.Create, CustomStringLocalizer.L("Permission:Create"));
        //rolesPermission.AddChild(IdentityPermissions.Roles.Update, CustomStringLocalizer.L("Permission:Edit"));
        //rolesPermission.AddChild(IdentityPermissions.Roles.Delete, CustomStringLocalizer.L("Permission:Delete"));
        //rolesPermission.AddChild(IdentityPermissions.Roles.ManagePermissions, CustomStringLocalizer.L("Permission:ChangePermissions"));

        var usersPermission = identityGroup.GetPermissionOrNull(IdentityPermissions.Users.Default);
        usersPermission.DisplayName = CustomStringLocalizer.L("Users");
        //usersPermission.AddChild(IdentityPermissions.Users.Create, CustomStringLocalizer.L("Permission:Create"));
        //var editPermission = usersPermission.AddChild(IdentityPermissions.Users.Update, CustomStringLocalizer.L("Permission:Edit"));
        //editPermission.AddChild(IdentityPermissions.Users.ManageRoles, CustomStringLocalizer.L("Permission:ManageRoles"));
        //usersPermission.AddChild(IdentityPermissions.Users.Delete, CustomStringLocalizer.L("Permission:Delete"));
        //usersPermission.AddChild(IdentityPermissions.Users.ManagePermissions, CustomStringLocalizer.L("Permission:ChangePermissions"));
        usersPermission.AddChild(CustomIdentityPermissions.Users.ViewPermissions, CustomStringLocalizer.L("Permission:ViewPermissions"));

        //identityGroup
        //    .AddPermission(IdentityPermissions.UserLookup.Default, CustomStringLocalizer.L("Permission:UserLookup"))
        //    .WithProviders(ClientPermissionValueProvider.ProviderName);
    }
}