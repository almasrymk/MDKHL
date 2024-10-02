using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Lookups.Governorate;
using SmartParking.Lookups;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using SmartParking.LookupsAppServices;
using System;
using Volo.Abp.Validation;
using System.Linq;

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
		var errorMessage = L["Can'tCreateGovernorate"];
		var validationErrorMessage = L["PleaseFixTheValidationErrors"];



		// Call your service to create the governorate
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

			var governorate = await GovernorateAppService.CreateAsync(createUpdateGovernorate);

			if (governorate != null)
			{
				Snackbar.Add(successedMessage, Severity.Success);

				MudDialog.Close(DialogResult.Ok(true));
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
			Snackbar.Add($"Failed to add governorate: {ex.Code} + ", Severity.Error);

		}
		catch (Exception ex)
		{
			Snackbar.Add($"Failed to add governorate: {ex.Message} + ", Severity.Error);
		}
		
    }
    private void Cancel()
    {
        MudDialog.Cancel();
    }
}
