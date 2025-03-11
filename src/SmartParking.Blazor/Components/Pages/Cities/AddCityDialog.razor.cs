using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Lookups;
using SmartParking.Lookups.State;
using System.Security.Authentication;
using Volo.Abp;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.Cities;

public partial class AddCityDialog
{
    private MudForm form;
    public CreateUpdateCityDto input = new CreateUpdateCityDto();
    public List<StateLookupDto> states = new List<StateLookupDto>();

    private string StateSelectValue { get; set; }

    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; }

    private void SelectState(string state)
    {
        StateSelectValue = state;
    }

    protected override async Task OnInitializedAsync()
    {
        states = await StateAppService.GetStateLookupAsync();
        await base.OnInitializedAsync();
    }
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



    private void Cancel()
    {
        MudDialog.Cancel();
    }

}
