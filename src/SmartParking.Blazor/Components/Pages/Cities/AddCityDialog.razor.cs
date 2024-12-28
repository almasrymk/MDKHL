using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Lookups;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Validation;
using System.Linq;
using System;
using SmartParking.Lookups.State;
using SmartParking.Lookups.CameraBrands;
using SmartParking.Blazor.Components.Shared;
using System.Security.Authentication;

namespace SmartParking.Blazor.Components.Pages.Cities;

public partial class AddCityDialog
{
    private MudForm form;
    public CreateUpdateCityDto input = new CreateUpdateCityDto();
    public List<StateLookupDto> states = new List<StateLookupDto>();

    private string StateSelectValue { get; set; }

    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; }

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
