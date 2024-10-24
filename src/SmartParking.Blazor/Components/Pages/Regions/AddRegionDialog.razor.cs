using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Lookups.Region;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.Regions;

public partial class AddRegionDialog
{
    private MudForm form;
    public CreateUpdateRegionDto input = new CreateUpdateRegionDto();

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
		var errorMessage = L["Can'tCreateRegion"];
		var validationErrorMessage = L["PleaseFixTheValidationErrors"];


		// Call your service to create the region
		try
		{
			CreateUpdateRegionDto createUpdateRegionDto = new CreateUpdateRegionDto()
			{
				ArabicName = input.ArabicName,
				EnglishName = input.EnglishName,
				Status = input.Status,
				CountryId = CountrySelectValue
			};

			var regions = await RegionAppService.CreateAsync(createUpdateRegionDto);
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
			Snackbar.Add($"Failed to add region: {ex.Code} + ", Severity.Error);

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
