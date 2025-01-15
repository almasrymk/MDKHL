using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Lookups;
using SmartParking.Lookups.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.Cities;

public partial class EditCityDialog
{
    [CascadingParameter] MudDialogInstance MudDialog { get; set; }
    [Parameter] public CityDto city { get; set; } = new CityDto();
    [Parameter] public CreateUpdateCityDto input { get; set; } = new CreateUpdateCityDto();
	public IReadOnlyList<StateLookupDto> states { get; set; } = new List<StateLookupDto>();
	private MudForm editForm;
    bool _clearable;
	private string StateSelectValue { get; set; }

	[Parameter] public int Id { get; set; }

    private void SelectState(string state)
    {
		StateSelectValue = state;
    }

    protected override async Task OnInitializedAsync()
    {
        states = await StateAppService.GetStateLookupAsync();
		await base.OnInitializedAsync();
	}

    private async Task Save()
    {
        try
        {
            // Create the input object for update
            var input = new CreateUpdateCityDto
            {
                ArabicName = city.ArabicName,
                EnglishName = city.EnglishName,
                StateId = city.StateId,
                
            };

            // Call the update method

            var cities = await CitiesAppService.UpdateAsync(city.Id, input);
            if (cities != null)
            {
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]} {L[AlertMessages.AlertMessages.UPDATE]} {input.ArabicName} ", Severity.Success);
                MudDialog.Close(DialogResult.Ok(true));
				await Task.Delay(1000);
				Navigation.NavigateTo("/cities", forceLoad: true);
			}

            else
            {
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.UPDATE]} {input.ArabicName} ", Severity.Error);

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
