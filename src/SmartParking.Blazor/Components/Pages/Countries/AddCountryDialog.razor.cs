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

/// <summary>
/// Dialog for adding a new country.
/// This component allows the user to input country details and submit them to be created.
/// </summary>
public partial class AddCountryDialog
{
    private MudForm form;
    public CreateUpdateCountryDto createUpdateCountryDto = new CreateUpdateCountryDto();
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; }

    bool _clearable;

    /// <summary>
    /// Submits the form to create a new country.
    /// It gathers the data from the input fields, creates a `CreateUpdateCountryDto`, 
    /// and sends it to the `CountriesAppService` to be added to the system.
    /// </summary>
    private async Task Submit()
    {
        try
        {
         
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
        catch (AbpValidationException validationException)
        {
            foreach (var validationError in validationException.ValidationErrors)
            {
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.VAlIDATION_ERROR]}: {L[validationError.MemberNames.First()]} - {validationError.ErrorMessage}", Severity.Error);
            }
        }
        catch (BusinessException ex)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.ADD]} : {ex.Code}", Severity.Error);
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

    /// <summary>
    /// Cancels the dialog and closes it without making any changes.
    /// </summary>
    private void Cancel()
    {
        MudDialog.Cancel();
    }
}
