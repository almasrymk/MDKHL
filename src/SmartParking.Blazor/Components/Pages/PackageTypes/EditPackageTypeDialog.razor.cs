using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Lookups;
using SmartParking.LookupsAppServices;
using System;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.PackageTypes;

public partial class EditPackageTypeDialog
{


    [CascadingParameter] MudDialogInstance MudDialog { get; set; }
    [Parameter] public PackageTypeDto packageType { get; set; } = new PackageTypeDto();
    private MudForm editForm;
    bool _clearable;
    [Parameter] public int UpdatedId { get; set; }



    private async Task Save()
    {

		var successedMessage = L["PackageTypeUpdatedSuccessfully"];
		var errorMessage = L["Can'tUpdatePackageType"];
		var validationErrorMessage = L["PleaseFixTheValidationErrors"];
		try
		{
			// Create the input object for update
			var input = new CreateUpdatePackageTypeDto
			{
				ArabicName = packageType.ArabicName,
				EnglishName = packageType.EnglishName,
			};

			// Call the update method

			var packageTypes = await PackageTypesAppService.UpdateAsync(UpdatedId, input);
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

            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.UPDATE]} : {ex.Code}", Severity.Error);
        }
        catch (AuthenticationException ex)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.UPDATE]} : {ex.Message} ", Severity.Error);
        }
        catch (Exception ex)
        {

            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.UPDATE]} : {ex.Message} ", Severity.Error);
        }

    }


    private void Cancel()
    {
        MudDialog.Cancel();
    }
}
