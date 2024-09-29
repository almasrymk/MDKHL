using System.Threading.Tasks;
using SmartParking.Localization;
using Volo.Abp.Identity.Blazor;
using Volo.Abp.SettingManagement.Blazor.Menus;
using Volo.Abp.TenantManagement.Blazor.Navigation;
using Volo.Abp.UI.Navigation;
using Microsoft.AspNetCore.Components.Authorization; // For injecting AuthenticationStateProvider
using Microsoft.Extensions.DependencyInjection; // For IServiceProvider
using SmartParking.MultiTenancy; // Import the MultiTenancyConsts

namespace SmartParking.Blazor.Menus
{
    public class SmartParkingMenuContributor : IMenuContributor
    {
        public async Task ConfigureMenuAsync(MenuConfigurationContext context)
        {
            if (context.Menu.Name == StandardMenus.Main)
            {
                await ConfigureMainMenuAsync(context);
            }
        }

        private async Task ConfigureMainMenuAsync(MenuConfigurationContext context)
        {
            var administration = context.Menu.GetAdministration();
            var l = context.GetLocalizer<SmartParkingResource>();

            // Get the authentication state provider
            var authStateProvider = context.ServiceProvider.GetRequiredService<AuthenticationStateProvider>();
            var authState = await authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            // Add the "Home" menu item
            context.Menu.Items.Insert(
                0,
                new ApplicationMenuItem(
                    SmartParkingMenus.Home,
                    l["Menu:Home"],
                    "/",
                    icon: "fas fa-home",
                    order: 0
                )
            );

            

            // Conditionally add menu items based on authentication status
            if (!user.Identity.IsAuthenticated)
            {
                context.Menu.AddItem(
                    new ApplicationMenuItem(
                        "Login",
                        l["Login"],
                        "/login",
                        icon: "fas fa-sign-in-alt"
                    )
                );
            }
            else
            {
                context.Menu.AddItem(
                    new ApplicationMenuItem(
                        "Profile",
                        l["Profile"],
                        "/profile",
                        icon: "fas fa-user"
                    )
                );
            }

            // Handle tenant and administration menus based on multi-tenancy status
            if (MultiTenancyConsts.IsEnabled)
            {
                administration.SetSubItemOrder(TenantManagementMenuNames.GroupName, 1);
            }
            else
            {
                administration.TryRemoveMenuItem(TenantManagementMenuNames.GroupName);
            }

            administration.SetSubItemOrder(IdentityMenuNames.GroupName, 2);
            administration.SetSubItemOrder(SettingManagementMenus.GroupName, 3);
        }
    }
}
