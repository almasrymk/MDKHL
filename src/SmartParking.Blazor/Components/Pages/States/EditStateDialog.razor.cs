using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Lookups.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.States;

public partial class EditStateDialog
{
    [CascadingParameter] MudDialogInstance MudDialog { get; set; }
    [Parameter] public StateDto state { get; set; } = new StateDto();
    [Parameter] public int UpdatedId { get; set; }

    private string RegionSelectValue { get; set; }


    protected override async Task OnInitializedAsync()
    {
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
				RegionId = int.Parse(RegionSelectValue),


			};

			// Call the update method

			var regions = await StateAppService.UpdateAsync(UpdatedId, input);

			if (regions != null)
			{
				Snackbar.Add(successedMessage, Severity.Success);
				MudDialog.Close(DialogResult.Ok(true));
				await Task.Delay(1000);
				Navigation.NavigateTo("/states", true);
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
			Snackbar.Add($"{errorMessage} {ex.Code} + ", Severity.Error);

		}
		catch (Exception ex)
		{
			Snackbar.Add($"{errorMessage} {ex.Message} + ", Severity.Error);
		}
    }


    private void Cancel()
    {
        MudDialog.Cancel();
    }
}
