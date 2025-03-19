using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Lookups;
using System.Security.Authentication;
using Volo.Abp;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.PackageTypes;

public partial class AddPackageTypeDialog
{
    private MudForm form;
    public CreateUpdatePackageTypeDto input = new CreateUpdatePackageTypeDto();

    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; }
    bool _clearable;


    private async Task Submit()
    {

        var successedMessage = L["PackageTypeCreatedSuccessfully"];
        var errorMessage = L["Can'tCreatePackageType"];
        var validationErrorMessage = L["PleaseFixTheValidationErrors"];


        // Call your service to create the packageType
        try
        {


            var packageTypes = await PackageTypesAppService.CreateAsync(input);
            if (packageTypes != null)
            {
                Snackbar.Add(successedMessage, Severity.Success);
                MudDialog.Close(DialogResult.Ok(true));
                await Task.Delay(1000);
                Navigation.NavigateTo("/packagetypes", true);
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
