using Microsoft.AspNetCore.Authorization;
using SmartParking.Permissions.SubsctriptionPermissions;

namespace SmartParking.Blazor.Components.Pages.OrganizationSubscriptions;

public partial class OrganizationSubscription
{
    public string SubscriptionArName { get; set; }
    public string SubscriptionEnName { get; set; }
    public int NumbersOfGarages { get; set; }
    public int NumberOfCameras { get; set; }
    public int NumberOfUsers { get; set; }
    public int NumberOfMobileCustomers { get; set; }
    public int NumberOfOrganizations { get; set; }
    public int ParkingSlots { get; set; }
    protected override async Task OnInitializedAsync()
    {
        if (!await AuthorizationService.IsGrantedAsync(SubscriptionPermissions.Default))
        {
            Navigation.NavigateTo("/Account/AccessDenied", true);
        }
        var orgId = await OrganizationAppService.GetOrganizationIdByTenant(currentTenant.Id);
        var Organization = await OrganizationAppService.GetAsync(orgId);
        SubscriptionArName = Organization.SubscriptionArabicName;
        SubscriptionEnName = Organization.SubscriptionEnglishName;
        NumbersOfGarages = Organization.NumbersOfGarages;
        NumberOfCameras = Organization.NumberOfCameras;
        NumberOfUsers = Organization.NumberOfUsers;
        NumberOfMobileCustomers = Organization.NumberOfCustomers;
        ParkingSlots = Organization.ParkingSlots;
    }

}
