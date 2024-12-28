using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Lookups;
using System;
using System.Linq;
using System.Security.Authentication;
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
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]} {L[AlertMessages.AlertMessages.UPDATE]} {input.ArabicName}", Severity.Success);
                MudDialog.Close(DialogResult.Ok(true));
                await Task.Delay(1000);
                Navigation.NavigateTo("/countries");
            }

			else
			{
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.UPDATE]} {input.ArabicName}", Severity.Success);

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
