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
/// Dialog component used to add a new city.
/// </summary>
public partial class AddCityDialog
{
    private MudForm form;
    public CreateUpdateCityDto input = new CreateUpdateCityDto();
    public List<StateLookupDto> states = new List<StateLookupDto>();

    private string StateSelectValue { get; set; }

    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; }

    /// <summary>
    /// Sets the selected state value.
    /// </summary>
    /// <param name="state">The selected state value.</param>
    private void SelectState(string state)
    {
        StateSelectValue = state;
    }

    /// <summary>
    /// Initializes the component and loads the list of states.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        states = await StateAppService.GetStateLookupAsync();
        await base.OnInitializedAsync();
    }

    /// <summary>
    /// Handles the form submission, creates a new city, and displays appropriate notifications.
    /// </summary>
    private async Task Submit()
    {
        try
        {
            CreateUpdateCityDto createUpdateCityDto = new CreateUpdateCityDto()
            {
                ArabicName = input.ArabicName,
                EnglishName = input.EnglishName,
                StateId = input.StateId,
            };

            var city = await CitiesAppService.CreateAsync(createUpdateCityDto);
            if (city != null)
            {
                SnackBar.ShowSnackbar(Snackbar, $"{createUpdateCityDto.ArabicName} {L[AlertMessages.AlertMessages.ADD]} {L[AlertMessages.AlertMessages.SUCCESS]}", Severity.Success);
                MudDialog.Close(DialogResult.Ok(true));
                await Task.Delay(1000);
                Navigation.NavigateTo("/cities", forceLoad: true);
            }
            else
            {
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]}  {L[AlertMessages.AlertMessages.ADD]} {createUpdateCityDto.ArabicName}", Severity.Error);
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
    /// Cancels the dialog without performing any actions.
    /// </summary>
    private void Cancel()
    {
        MudDialog.Cancel();
    }
}



