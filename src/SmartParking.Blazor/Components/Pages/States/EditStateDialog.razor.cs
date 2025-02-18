using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Lookups.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.States;

public partial class EditStateDialog
{
    [CascadingParameter] MudDialogInstance MudDialog { get; set; }
    [Parameter] public StateDto state { get; set; } = new StateDto();
	public List<RegionLookupDto> regions { get; set; } = new List<RegionLookupDto>();
	[Parameter] public int UpdatedId { get; set; }

    private string RegionSelectValue { get; set; }


    protected override async Task OnInitializedAsync()
    {
		regions = await StateAppService.GetRegionLookupAsync();
       await base.OnInitializedAsync();
    }

    private void SelectRegion(string region)
    {
        RegionSelectValue = region;
    }
    private async Task Save()
    {
		var successedMessage = L["StateUpdatedSuccessfully"];
		var errorMessage = L["Can'tUpdateState"];
		var validationErrorMessage = L["PleaseFixTheValidationErrors"];
		try
		{
			// Create the input object for update
			var input = new CreateUpdateStateDto
			{
				ArabicName = state.ArabicName,
				EnglishName = state.EnglishName,
				Status = state.Status,
				RegionId = state.RegionId,


			};

			// Call the update method

			var regions = await StateAppService.UpdateAsync(UpdatedId, input);

			if (regions != null)
			{
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]} {L[AlertMessages.AlertMessages.UPDATE]} {regions.EnglishName}", Severity.Success);

                MudDialog.Close(DialogResult.Ok(true));
				await Task.Delay(1000);
				Navigation.NavigateTo("/states", true);
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

            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.UPDATE]} : {ex.Code}", Severity.Error);
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
