using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Lookups;
using System.Threading.Tasks;
using Volo.Abp;

namespace SmartParking.Blazor.Components.Pages.Countries;

public partial class AddCountryDialog
{
    private MudForm form;
    public CreateUpdateCountryDto input = new CreateUpdateCountryDto();

    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; }
    bool _clearable;


    private async Task Submit()
    {
        var successedMessage = L["CountryCreatedSuccessfully"];
        var errorMessage = L["PleaseFixTheValidationErrors"];
        if (form.IsValid)
        {
            // Call your service to create the camera
            try
            {
                await CountriesAppService.CreateAsync(input);
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
