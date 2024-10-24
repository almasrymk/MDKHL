using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MudBlazor;
using SmartParking.Lookups;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.Countries;

public partial class EditCountryDialog
{

    [CascadingParameter] MudDialogInstance MudDialog { get; set; }
    [Parameter] public CountryDto country { get; set; } = new CountryDto();
    private MudForm editForm;
    [Parameter] public int UpdatedId { get; set; }

    private async Task Save()
    {
		var successedMessage = L["CountryUpdatedSuccessfully"];
		var errorMessage = L["Can'tUpdateCountry"];
		var validationErrorMessage = L["PleaseFixTheValidationErrors"];
		try
		{
			// Create the input object for update
			var input = new CreateUpdateCountryDto
			{
				ArabicName = country.ArabicName,
				EnglishName = country.EnglishName,
				ArabicCapital = country.ArabicCapital,
				EnglishCapital = country.EnglishCapital,
				Notes = country.Notes,
				Id = UpdatedId
			};

			// Call the update method

			var countries = await CountriesAppService.UpdateAsync(UpdatedId, input);

			if (countries != null)
			{
				Snackbar.Add(successedMessage, Severity.Success);
				MudDialog.Close(DialogResult.Ok(true));
                Navigation.Refresh(forceReload: true);
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
			Snackbar.Add($"Failed to update country: {ex.Code} + ", Severity.Error);

		}
		catch (Exception ex)
		{
			Snackbar.Add($"Failed to update country: {ex.Message} + ", Severity.Error);
		}
		
    }


    private void Cancel()
    {
        MudDialog.Cancel();
    }
}
