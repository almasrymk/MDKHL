using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MudBlazor;
using SmartParking.Lookups;
using SmartParking.LookupsAppServices;
using System.Threading.Tasks;
using System;
using Volo.Abp;
using Abp.Runtime.Validation;
using System.Linq;
using SmartParking.SubscriptionTypes;

namespace SmartParking.Blazor.Components.Pages.SubscriptionType;

public partial class EditSubscriptionTypeDialog
{

	[CascadingParameter] MudDialogInstance MudDialog { get; set; }
	[Parameter] public SubscriptionTypeDto subscriptionType { get; set; } = new SubscriptionTypeDto();
	private MudForm editForm;
	bool _clearable;
	[Parameter] public int UpdatedId { get; set; }



	private async Task Save()
	{

		var successedMessage = L["SubscriptionTypeUpdatedSuccessfully"];
		var errorMessage = L["Can'tUpdateSubscriptionType"];
		var validationErrorMessage = L["PleaseFixTheValidationErrors"];
		try
		{
			// Create the input object for update
			var input = new CreateUpdateSubscriptionTypeDto
			{
				TypeEn = subscriptionType.TypeEn,
				TypeAr = subscriptionType.TypeAr,
			};

			// Call the update method

			var subscriptionTypes = await SubscriptionTypeAppService.UpdateAsync(UpdatedId, input);
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
			Snackbar.Add($"Failed to update subscription type: {ex.Code} + ", Severity.Error);

		}
		catch (Exception ex)
		{
			Snackbar.Add($"Failed to subscription type: {ex.Message} + ", Severity.Error);
		}

	}


	private void Cancel()
	{
		MudDialog.Cancel();
	}
}
