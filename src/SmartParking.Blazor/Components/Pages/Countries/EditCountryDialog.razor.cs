using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Lookups;
using System.Security.Authentication;
using Volo.Abp;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.Countries;
/// <summary>
/// Represents the dialog for editing a country's details.
/// This dialog allows the user to modify the details of a selected country.
/// </summary>
public partial class EditCountryDialog
{
    [CascadingParameter]
    IMudDialogInstance MudDialog { get; set; }
    [Parameter]
    public CountryDto country { get; set; } = new CountryDto();
    private MudForm editForm;
    [Parameter]
    public int Id { get; set; }

    /// <summary>
    /// Saves the updated country details.
    /// This method sends the updated country data to the service for saving, 
    /// handles any validation or business exceptions, and shows appropriate messages.
    /// </summary>
    private async Task Save()
    {
        try
        {
            // Create the input object for update
            var createUpdateCountryDto = Mapper.Map<CreateUpdateCountryDto>(country);
            var countries = await CountriesAppService.UpdateAsync(country.Id, createUpdateCountryDto);
            if (countries != null)
            {
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]} {L[AlertMessages.AlertMessages.UPDATE]} {createUpdateCountryDto.ArabicName}", Severity.Success);
                MudDialog.Close(DialogResult.Ok(true));
                await Task.Delay(1000);
                Navigation.NavigateTo("/countries");
            }
            else
            {
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.UPDATE]} {createUpdateCountryDto.ArabicName}", Severity.Success);
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

    /// <summary>
    /// Cancels the editing and closes the dialog without saving changes.
    /// This method is typically used to cancel the action and close the dialog.
    /// </summary>
    private void Cancel()
    {
        MudDialog.Cancel();
    }
}
