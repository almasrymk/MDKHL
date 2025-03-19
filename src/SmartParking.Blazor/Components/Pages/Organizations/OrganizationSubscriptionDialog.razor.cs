using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Subscriptions;

namespace SmartParking.Blazor.Components.Pages.Organizations;

public partial class OrganizationSubscriptionDialog
{

    [CascadingParameter]
    IMudDialogInstance MudDialog { get; set; }
    [Parameter] public SubscriptionDto subscription { get; set; }


    private void Cancel()
    {
        MudDialog.Cancel();
    }


}
