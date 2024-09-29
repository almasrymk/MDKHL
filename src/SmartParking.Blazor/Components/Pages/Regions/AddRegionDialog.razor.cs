using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Lookups.Region;
using System.Threading.Tasks;
using Volo.Abp;

namespace SmartParking.Blazor.Components.Pages.Regions;

public partial class AddRegionDialog
{
    private MudForm form;
    public CretaeUpdateRegionDto input = new CretaeUpdateRegionDto();

    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; }
    bool _clearable;
    private int CountrySelectValue { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }

    private void SelectCountry(int country)
    {
        CountrySelectValue = country;
    }
    private async Task Submit()
    {
        var successedMessage = L["RegionCreatedSuccessfully"];
        var errorMessage = L["PleaseFixTheValidationErrors"];
        if (form.IsValid)
        {
            // Call your service to create the camera
            try
            {
                CretaeUpdateRegionDto createUpdateRegionDto = new CretaeUpdateRegionDto()
                {
                    ArabicName = input.ArabicName,
                    EnglishName = input.EnglishName,
                    Status = input.Status,
                    CountryId = CountrySelectValue
                };
                await RegionAppService.CreateAsync(createUpdateRegionDto);
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
