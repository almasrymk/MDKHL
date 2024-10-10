using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Lookups;
using SmartParking.Subscriptions;
using SmartParking.SubscriptionTypes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartParking.Blazor.Components.Pages.Subscriptionss;

public partial class DetailsSubscriptionDialog
{
	[CascadingParameter] MudDialogInstance MudDialog { get; set; }
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
