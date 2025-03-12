using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Subscriptions;
using SmartParking.SubscriptionTypes;

namespace SmartParking.Blazor.Components.Pages.Subscriptions;

public partial class DetailsSubscriptionDialog
{
    [CascadingParameter] IMudDialogInstance MudDialog { get; set; }
    [Parameter] public SubscriptionDto subscription { get; set; } = new SubscriptionDto();
    public List<SubscriptionTypeLookupDto> subscriptions { get; set; } = new List<SubscriptionTypeLookupDto>();

    bool _clearable;

    protected override async Task OnInitializedAsync()
    {
        var result = await SubscriptionTypeAppService.GetSubscriptionTypeLookupAsync();
        subscriptions = result;
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }
}
