using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Lookups;
using System.Threading.Tasks;
using Volo.Abp;

namespace SmartParking.Blazor.Components.Pages.PackageTypes;

public partial class AddPackageTypeDialog
{
    private MudForm form;
    public CreateUpdatePackageTypeDto input = new CreateUpdatePackageTypeDto();

    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; }
    bool _clearable;


    private async Task Submit()
    {
        var successedMessage = L["PackageTypeCreatedSuccessfully"];
        var errorMessage = L["PleaseFixTheValidationErrors"];
        if (form.IsValid)
        {
            // Call your service to create the camera
            try
            {
                await PackageTypesAppService.CreateAsync(input);
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
