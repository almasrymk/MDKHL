using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Lookups;
using SmartParking.Lookups.Governorate;
using SmartParking.Lookups.Region;
using SmartParking.LookupsAppServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.Regions;

public partial class EditRegionDialog
{
    [CascadingParameter] MudDialogInstance MudDialog { get; set; }
    [Parameter] public RegionDto region { get; set; } = new RegionDto();
    public IReadOnlyList<CountryLookupDto> countries { get; set; } = new List<CountryLookupDto>();
    private MudForm editForm;
    [Parameter] public int UpdatedId { get; set; }


    protected override async Task OnInitializedAsync()
    {
        var result = await GovernorateAppService.GetCountryLookupAsync();
        countries = result;
    }


    private async Task Save()
    {

		var successedMessage = L["RegionUpdatedSuccessfully"];
		var errorMessage = L["Can'tUpdateRegion"];
		var validationErrorMessage = L["PleaseFixTheValidationErrors"];
		try
		{
			// Create the input object for update
			var input = new CreateUpdateRegionDto
			{
				ArabicName = region.ArabicName,
				EnglishName = region.EnglishName,
				Status = region.Status,
				CountryId = region.CountryId,


			};

			// Call the update method

			var regions = await RegionAppService.UpdateAsync(UpdatedId, input);
			if (regions != null)
			{
				Snackbar.Add(successedMessage, Severity.Success);
				MudDialog.Close(DialogResult.Ok(true));
				await Task.Delay(1000);
				Navigation.NavigateTo("/regions", true);
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
			Snackbar.Add($"Failed to update region: {ex.Code} + ", Severity.Error);

		}
		catch (Exception ex)
		{
			Snackbar.Add($"Failed to region: {ex.Message} + ", Severity.Error);
		}

    }


    private void Cancel()
    {
        MudDialog.Cancel();
    }
}
