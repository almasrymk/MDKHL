using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Lookups.Country;
using SmartParking.Lookups.Region;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.Regions;

public partial class AddRegionDialog
{
    public CreateUpdateRegionDto input = new CreateUpdateRegionDto();
    public List<CountryLookupDto> countries = new List<CountryLookupDto>();

    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; }
    bool _clearable;
    private string CountrySelectValue { get; set; }

    protected override async Task OnInitializedAsync()
    {
		countries = await CountriesAppService.GetCountryLookupAsync();
        await base.OnInitializedAsync();
    }

    private void SelectCountry(string country)
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
				CountryId = input.CountryId
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
