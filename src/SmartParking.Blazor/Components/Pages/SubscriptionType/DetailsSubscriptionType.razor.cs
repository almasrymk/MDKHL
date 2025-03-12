using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.SubscriptionTypes;

namespace SmartParking.Blazor.Components.Pages.SubscriptionType;

public partial class DetailsSubscriptionType
{
    [CascadingParameter] IMudDialogInstance MudDialog { get; set; }
    [Parameter] public SubscriptionTypeDto subscriptionType { get; set; } = new SubscriptionTypeDto();


    private void Cancel()
    {
        MudDialog.Cancel();
    }
}
