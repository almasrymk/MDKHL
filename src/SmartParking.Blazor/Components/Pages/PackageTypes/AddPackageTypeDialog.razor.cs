using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Lookups;
using SmartParking.LookupsAppServices;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Validation;

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
		    var errorMessage = L["Can'tCreatePackageType"];
            var validationErrorMessage = L["PleaseFixTheValidationErrors"];

        
            // Call your service to create the packageType
            try
            {
			

			    var packageTypes =  await PackageTypesAppService.CreateAsync(input);
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
                    Snackbar.Add($"{validationErrorMessage} {validationError.MemberNames.First()} - {validationError.ErrorMessage}", Severity.Error);
                }
            }
            catch (BusinessException ex)
            {
                Snackbar.Add($"Failed to add package type: {ex.Code} + ", Severity.Error);

            }
            catch (Exception ex)
            {
                Snackbar.Add($"Failed to package type {ex.Message} + ", Severity.Error);
            }


    }
    private void Cancel()
    {
        MudDialog.Cancel();
    }
}
