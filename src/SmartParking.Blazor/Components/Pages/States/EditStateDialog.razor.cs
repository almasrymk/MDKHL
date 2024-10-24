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
    public IReadOnlyList<RegionLookupDto> regions { get; set; } = new List<RegionLookupDto>();

    private MudForm editForm;


    protected override async Task OnInitializedAsync()
    {
        var result = await StateAppService.GetRegionLookupAsync();
        regions = result;
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
			Snackbar.Add($"Failed to update state: {ex.Code} + ", Severity.Error);

		}
		catch (Exception ex)
		{
			Snackbar.Add($"Failed to state: {ex.Message} + ", Severity.Error);
		}
    }


    private void Cancel()
    {
        MudDialog.Cancel();
    }
}
