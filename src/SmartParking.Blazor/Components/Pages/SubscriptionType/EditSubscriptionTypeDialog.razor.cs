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
using SmartParking.Blazor.Components.Shared;
using System.Security.Authentication;

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
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]} {L[AlertMessages.AlertMessages.UPDATE]} {input.TypeAr}", Severity.Success);
                MudDialog.Close(DialogResult.Ok(true));
				await Task.Delay(1000);
				Navigation.NavigateTo("/subscriptiontypes", true);
			}

			else
			{
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]} {L[AlertMessages.AlertMessages.UPDATE]} {input.TypeAr}", Severity.Error);

            }
		}
        catch (BusinessException ex)
        {

            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.UPDATE]} : {ex.Message}", Severity.Error);
        }
        catch (AuthenticationException ex)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.UPDATE]} : {ex.Message} ", Severity.Error);
        }
        catch (Exception ex)
        {

            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.UPDATE]} : {ex.Message} ", Severity.Error);
        }

    }


	private void Cancel()
	{
		MudDialog.Cancel();
	}
}
