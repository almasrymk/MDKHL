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
        var successedMessage = L["CityCreatedSuccessfully"];
        var errorMessage = L["Can'tCreateCity"];
        var validationErrorMessage = L["PleaseFixTheValidationErrors"];

        
            // Call your service to create the city
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
                    Snackbar.Add(successedMessage, Severity.Success);
                    MudDialog.Close(DialogResult.Ok(true));
				    await Task.Delay(1000);
				    Navigation.NavigateTo("/cities", forceLoad: true);
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
                Snackbar.Add($"{errorMessage} {ex.Code}", Severity.Error);

            }
            catch (Exception ex)
            {
                Snackbar.Add($"{errorMessage} {ex.Message} ", Severity.Error);
            }


    }
        
        
    
    private void Cancel()
    {
        MudDialog.Cancel();
    }
    
}
