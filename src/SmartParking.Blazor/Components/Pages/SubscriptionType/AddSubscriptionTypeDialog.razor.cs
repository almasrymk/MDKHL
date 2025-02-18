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
using SmartParking.Blazor.Components.Shared;
using SmartParking.Localization;
using System.Security.Authentication;

namespace SmartParking.Blazor.Components.Pages.SubscriptionType;

public partial class AddSubscriptionTypeDialog
{
	public CreateUpdateSubscriptionTypeDto input = new CreateUpdateSubscriptionTypeDto();

	[CascadingParameter]
	private MudDialogInstance MudDialog { get; set; }
	bool _clearable;


	private async Task Submit()
	{

		try
		{


			var subscriptionTypes = await SubscriptionTypeAppService.CreateAsync(input);
			if (subscriptionTypes != null)
			{
                SnackBar.ShowSnackbar(Snackbar, LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.SUCCESS) + " " +
				LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.ADD) + " " +
				LocalizationHelper.GetLocalizedString("SubscriptionType"), Severity.Success);
				MudDialog.Close(DialogResult.Ok(true));
				await Task.Delay(1000);
				Navigation.NavigateTo("/subscriptiontypes", true);
			}
		}
        catch (AbpValidationException validationException)
        {
            foreach (var validationError in validationException.ValidationErrors)
            {
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.VAlIDATION_ERROR]}: {L[validationError.MemberNames.First()]} - {validationError.ErrorMessage}", Severity.Error);
            }
        }
        catch (BusinessException ex)
        {

            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.ADD]} {L[AlertMessages.AlertMessages.FAILED]}: {ex.Message}", Severity.Error);
        }
        catch (AuthenticationException ex)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.ADD]} {L[AlertMessages.AlertMessages.FAILED]} : {ex.Message} ", Severity.Error);
        }
        catch (Exception ex)
        {

            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.ADD]} {L[AlertMessages.AlertMessages.FAILED]}: {ex.Message} ", Severity.Error);
        }

    }
	private void Cancel()
	{
		MudDialog.Cancel();
	}
}
