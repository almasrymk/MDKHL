using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Lookups.City;
using SmartParking.Lookups;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using SmartParking.Garages;
using Volo.Abp.Validation;
using System.Linq;
using Blazorise;
using System;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace SmartParking.Blazor.Components.Pages.Cities;

public partial class AddCityDialog
{
    private MudForm form;
    public CreateUpdateCityDto input = new CreateUpdateCityDto();
    public IReadOnlyList<GovernorateLookupDto> cities = new List<GovernorateLookupDto>();
    private int GovernmentSelectValue { get; set; }
    private int RegionSelectValue { get; set; }

    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; }

    private void SelectGovernment(int Government)
    {
        GovernmentSelectValue = Government;
    }
    private void SelectRegion(int region)
    {
        RegionSelectValue = region;
    }
    protected override async Task OnInitializedAsync()
    {
        var result = await CitiesAppService.GetGovernorateLookupAsync();
        cities = result;
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
                    Notes = input.Notes,
                    GovernorateId = GovernmentSelectValue,
                    RegionId = RegionSelectValue

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
                Snackbar.Add($"Failed to add city: {ex.Code}", Severity.Error);

            }
            catch (Exception ex)
            {
                Snackbar.Add($"Failed to add city: {ex.Message} ", Severity.Error);
            }


    }
        
        
    
    private void Cancel()
    {
        MudDialog.Cancel();
    }
    
}
