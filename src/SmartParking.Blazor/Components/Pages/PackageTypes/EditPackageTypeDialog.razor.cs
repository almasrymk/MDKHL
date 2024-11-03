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
				Snackbar.Add($"{validationErrorMessage} {validationError.MemberNames.First()} - {validationError.ErrorMessage}", Severity.Error);
			}
		}
		catch (BusinessException ex)
		{
			Snackbar.Add($"Failed to update package type: {ex.Code} + ", Severity.Error);

		}
		catch (Exception ex)
		{
			Snackbar.Add($"Failed to package type: {ex.Message} + ", Severity.Error);
		}
		
    }


    private void Cancel()
    {
        MudDialog.Cancel();
    }
}
