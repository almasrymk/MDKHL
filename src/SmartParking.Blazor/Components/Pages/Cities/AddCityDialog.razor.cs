using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Lookups.City;
using SmartParking.Lookups;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;

namespace SmartParking.Blazor.Components.Pages.Cities;

public partial class AddCityDialog
{
    private MudForm form;
    public CreateUpdateCityDto input = new CreateUpdateCityDto();
    public IReadOnlyList<GovernorateLookupDto> cities = new List<GovernorateLookupDto>();
    private int GovernmentSelectValue { get; set; }

    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; }

    private void SelectGovernment(int Government)
    {
        GovernmentSelectValue = Government;
    }
    protected override async Task OnInitializedAsync()
    {
        var result = await CitiesAppService.GetGovernorateLookupAsync();
        cities = result;
    }
    private async Task Submit()
    {
        var successedMessage = L["CityCreatedSuccessfully"];
        var errorMessage = L["PleaseFixTheValidationErrors"];

        if (form.IsValid)
        {
            // Call your service to create the camera
            try
            {
                CreateUpdateCityDto createUpdateCityDto = new CreateUpdateCityDto()
                {
                    ArabicName = input.ArabicName,
                    EnglishName = input.EnglishName,
                    Notes = input.Notes,
                    GovernorateId = GovernmentSelectValue
                };
                await CitiesAppService.CreateAsync(createUpdateCityDto);
            }
            catch (UserFriendlyException ex)
            {
                Snackbar.Add(ex.Message, Severity.Error);
                return;
            }
            Snackbar.Add(successedMessage, Severity.Success);
            MudDialog.Close(DialogResult.Ok(true));

        }
        else
        {
            Snackbar.Add(errorMessage, Severity.Error);
        }
    }
    private void Cancel()
    {
        MudDialog.Cancel();
    }
}
