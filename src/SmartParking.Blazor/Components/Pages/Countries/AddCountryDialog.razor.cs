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

public partial class AddCountryDialog
{
    private MudForm form;
    public CreateUpdateCountryDto input = new CreateUpdateCountryDto();

    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; }
    bool _clearable;


    private async Task Submit()
    {
		
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
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]} {L[AlertMessages.AlertMessages.ADD]} {createUpdateCountryDto.ArabicName}", Severity.Success);
                MudDialog.Close(DialogResult.Ok(true));
				await Task.Delay(1000);
                Navigation.NavigateTo("/countries", forceLoad: true);

            }

			else
			{
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.ADD]} {createUpdateCountryDto.ArabicName}", Severity.Error);
				
            }
		}
        catch (BusinessException ex)
        {

            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.ADD]} : {ex.Message}", Severity.Error);
        }
        catch (AuthenticationException ex)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.ADD]} : {ex.Message} ", Severity.Error);
        }
        catch (Exception ex)
        {

            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.ADD]} : {ex.Message} ", Severity.Error);
        }
    }
    private void Cancel()
    {
        MudDialog.Cancel();
    }
}
