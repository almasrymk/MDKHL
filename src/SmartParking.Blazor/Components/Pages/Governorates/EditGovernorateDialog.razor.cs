using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Lookups.Governorate;
using SmartParking.Lookups;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartParking.LookupsAppServices;
using System;
using Volo.Abp;
using Volo.Abp.Validation;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace SmartParking.Blazor.Components.Pages.Governorates;

public partial class EditGovernorateDialog
{
    [CascadingParameter] MudDialogInstance MudDialog { get; set; }
    [Parameter] public GovernorateDto governorate { get; set; } = new GovernorateDto();
    public IReadOnlyList<CountryLookupDto> countries { get; set; } = new List<CountryLookupDto>();
    private MudForm editForm;
    bool _clearable;
    [Parameter] public int UpdatedId { get; set; }



    protected override async Task OnInitializedAsync()
    {
        var result = await GovernorateAppService.GetCountryLookupAsync();
        countries = result;
    }

    private async Task Save()
    {

		var successedMessage = L["GovernorateUpdatedSuccessfully"];
		var errorMessage = L["Can'tUpdateGovernorate"];
		var validationErrorMessage = L["PleaseFixTheValidationErrors"];
		try
		{
			// Create the input object for update
			var input = new CreateUpdateGovernorateDto
			{
				ArabicName = governorate.ArabicName,
				EnglishName = governorate.EnglishName,
				ArabicCapital = governorate.ArabicCapital,
				EnglishCapital = governorate.EnglishCapital,
				Notes = governorate.Notes,
				CountryId = governorate.CountryId,
				Id = UpdatedId

			};

			// Call the update method

			var governorates = await GovernorateAppService.UpdateAsync(UpdatedId, input);

			if (governorates != null)
			{
				Snackbar.Add(successedMessage, Severity.Success);
				MudDialog.Close(DialogResult.Ok(true));
                await Task.Delay(1000);
                Navigation.NavigateTo("/governorates", forceLoad: true);
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
				Snackbar.Add($"Validation failed: {validationError.MemberNames.First()} - {validationError.ErrorMessage}", Severity.Error);
			}
		}
		catch (BusinessException ex)
		{
			Snackbar.Add($"Failed to update governorate: {ex.Code} + ", Severity.Error);

		}
		catch (Exception ex)
		{
			Snackbar.Add($"Failed to update governorate: {ex.Message} + ", Severity.Error);
		}
		

            
    }


    private void Cancel()
    {
        MudDialog.Cancel();
    }
}
