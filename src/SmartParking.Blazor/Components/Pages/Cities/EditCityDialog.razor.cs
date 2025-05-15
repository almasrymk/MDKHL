using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Lookups;
using SmartParking.Lookups.State;
using System.Security.Authentication;
using Volo.Abp;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.Cities;

/// <summary>
/// Represents the dialog component for editing an existing city's details.
/// </summary>
public partial class EditCityDialog
{

    [CascadingParameter]
    IMudDialogInstance MudDialog { get; set; }
    [Parameter]
    public CityDto city { get; set; } = new CityDto();
    [Parameter]
    public CreateUpdateCityDto input { get; set; } = new CreateUpdateCityDto();
    public IReadOnlyList<StateLookupDto> states { get; set; } = new List<StateLookupDto>();
    private MudForm editForm;
    bool _clearable;
    private string StateSelectValue { get; set; }
    [Parameter]
    public int Id { get; set; }

    /// <summary>
    /// Updates the selected state value when a state is selected.
    /// </summary>
    /// <param name="state">The state to select.</param>
    private void SelectState(string state)
    {
        StateSelectValue = state;
    }

    /// <summary>
    /// Called when the component is initialized. Loads the list of states.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    protected override async Task OnInitializedAsync()
    {
        states = await StateAppService.GetStateLookupAsync();
        await base.OnInitializedAsync();
    }

    /// <summary>
    /// Saves the edited city details.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task Save()
    {
        try
        {
            var input = new CreateUpdateCityDto
            {
                ArabicName = city.ArabicName,
                EnglishName = city.EnglishName,
                StateId = city.StateId,
            };

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

    /// <summary>
    /// Cancels the editing operation and closes the dialog without saving changes.
    /// </summary>
    private void Cancel()
    {
        MudDialog.Cancel();
    }
}
