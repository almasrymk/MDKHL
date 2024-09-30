using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Lookups.Governorate;
using SmartParking.Lookups;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;

namespace SmartParking.Blazor.Components.Pages.Governorates;

public partial class AddGovernorateDialog
{

    private MudForm form;
    public CreateUpdateGovernorateDto input = new CreateUpdateGovernorateDto();
    public IReadOnlyList<CountryLookupDto> countries = new List<CountryLookupDto>();
    private int CountrySelectValue { get; set; }

    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; }
    bool _clearable;


    private void SelectCountry(int country)
    {
        CountrySelectValue = country;
    }
    protected override async Task OnInitializedAsync()
    {
        var result = await GovernorateAppService.GetCountryLookupAsync();
        countries = result;
    }
    private async Task Submit()
    {
        var successedMessage = L["GovernorateCreatedSuccessfully"];
        var errorMessage = L["PleaseFixTheValidationErrors"];
        if (form.IsValid)
        {
            // Call your service to create the camera
            try
            {
                CreateUpdateGovernorateDto createUpdateGovernorate = new CreateUpdateGovernorateDto()
                {
                    ArabicName = input.ArabicName,
                    EnglishName = input.EnglishName,
                    ArabicCapital = input.ArabicCapital,
                    EnglishCapital = input.EnglishCapital,
                    Notes = input.Notes,
                    CountryId = CountrySelectValue,
                };
                await GovernorateAppService.CreateAsync(createUpdateGovernorate);
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
