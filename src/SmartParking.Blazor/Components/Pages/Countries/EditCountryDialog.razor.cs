using Microsoft.AspNetCore.Components;
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
    [Parameter] public int Id { get; set; }

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
			
			};

			// Call the update method

			var countries = await CountriesAppService.UpdateAsync(country.Id, input);

			if (countries != null)
			{
				Snackbar.Add(successedMessage, Severity.Success);
				MudDialog.Close(DialogResult.Ok(true));
                await Task.Delay(1000);
                Navigation.NavigateTo("/countries");
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
				Snackbar.Add($"{errorMessage} {validationError.MemberNames.First()} - {validationError.ErrorMessage}", Severity.Error);
			}
		}
		catch (BusinessException ex)
		{
			Snackbar.Add($"{errorMessage} {ex.Code}", Severity.Error);

		}
		catch (Exception ex)
		{
			Snackbar.Add($"{errorMessage} {ex.Message}", Severity.Error);
		}
		
    }


    private void Cancel()
    {
        MudDialog.Cancel();
    }
}
