using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MudBlazor;
using SmartParking.Lookups;
using SmartParking.LookupsAppServices;
using System.Threading.Tasks;
using System;
using Volo.Abp;
using SmartParking.SubscriptionTypes;
using System.Linq;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.SubscriptionType;

public partial class AddSubscriptionTypeDialog
{
	private MudForm form;
	public CreateUpdateSubscriptionTypeDto input = new CreateUpdateSubscriptionTypeDto();

	[CascadingParameter]
	private MudDialogInstance MudDialog { get; set; }
	bool _clearable;


	private async Task Submit()
	{

		var successedMessage = L["SubscriptionTypeCreatedSuccessfully"];
		var errorMessage = L["Can'tCreateSubscriptionType"];
		var validationErrorMessage = L["PleaseFixTheValidationErrors"];


		// Call your service to create the packageType
		try
		{


			var subscriptionTypes = await SubscriptionTypeAppService.CreateAsync(input);
			if (subscriptionTypes != null)
			{
				Snackbar.Add(successedMessage, Severity.Success);
				MudDialog.Close(DialogResult.Ok(true));
				await Task.Delay(1000);
				Navigation.NavigateTo("/subscriptiontypes", true);
			}

			else
			{
				Snackbar.Add(errorMessage, Severity.Error);

			}
		}
		catch (AbpValidationException validationException)
		{
			foreach (var validationError in validationException.ValidationErrors)
			{
				Snackbar.Add($"{validationErrorMessage} {validationError.MemberNames.First()} - {validationError.ErrorMessage}", Severity.Error);
			}
		}
		catch (BusinessException ex)
		{
			Snackbar.Add($"Failed to add package type: {ex.Code} + ", Severity.Error);

		}
		catch (Exception ex)
		{
			Snackbar.Add($"Failed to package type {ex.Message} + ", Severity.Error);
		}


	}
	private void Cancel()
	{
		MudDialog.Cancel();
	}
}
