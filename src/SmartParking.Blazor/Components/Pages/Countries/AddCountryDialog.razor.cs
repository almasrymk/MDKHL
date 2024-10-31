using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Lookups;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.Countries;

public partial class AddCountryDialog
{
    private MudForm form;
    public CreateUpdateCountryDto input = new CreateUpdateCountryDto();

    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; }
    bool _clearable;


    private async Task Submit()
    {
		var successedMessage = L["CountryCreatedSuccessfully"];
		var errorMessage = L["Can'tCreateCountry"];
		var validationErrorMessage = L["PleaseFixTheValidationErrors"];


		// Call your service to create the country
		try
		{
			CreateUpdateCountryDto createUpdateCountryDto = new CreateUpdateCountryDto()
			{
				ArabicName = input.ArabicName,
				EnglishName = input.EnglishName,
				ArabicCapital = input.ArabicCapital,
				EnglishCapital = input.EnglishCapital,	
			};

			var coutnry = await CountriesAppService.CreateAsync(createUpdateCountryDto);
			if (coutnry != null)
			{
				Snackbar.Add(successedMessage, Severity.Success);
				MudDialog.Close(DialogResult.Ok(true));
				await Task.Delay(1000);
                Navigation.NavigateTo("/countries", forceLoad: true);

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
