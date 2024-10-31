using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Lookups;
using SmartParking.Lookups.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.Cities;

public partial class EditCityDialog
{
    [CascadingParameter] MudDialogInstance MudDialog { get; set; }
    [Parameter] public CityDto city { get; set; } = new CityDto();
    [Parameter] public CreateUpdateCityDto input { get; set; } = new CreateUpdateCityDto();
	public IReadOnlyList<RegionLookupDto> regions { get; set; } = new List<RegionLookupDto>();
	private MudForm editForm;
    bool _clearable;
	private string StatetSelectValue { get; set; }

	[Parameter] public int Id { get; set; }

    private void SelectState(string state)
    {
        StatetSelectValue = state;
    }

    protected override async Task OnInitializedAsync()
    {
		await base.OnInitializedAsync();
	}

    private async Task Save()
    {
        var successedMessage = L["CityUpdatedSuccessfully"];
        var errorMessage = L["Can'tUpdateCity"];
        var validationErrorMessage = L["PleaseFixTheValidationErrors"];
        try
        {
            // Create the input object for update
            var input = new CreateUpdateCityDto
            {
                ArabicName = city.ArabicName,
                EnglishName = city.EnglishName,
                StateId = int.Parse(StatetSelectValue),
                
            };

            // Call the update method

            var cities = await CitiesAppService.UpdateAsync(city.Id, input);
            if (cities != null)
            {
                Snackbar.Add(successedMessage, Severity.Success);
                MudDialog.Close(DialogResult.Ok(true));
				await Task.Delay(1000);
				Navigation.NavigateTo("/cities", forceLoad: true);
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
            Snackbar.Add($"{errorMessage} {ex.Code} + ", Severity.Error);

        }
        catch (Exception ex)
        {
            Snackbar.Add($"{errorMessage} {ex.Message} + ", Severity.Error);
        }
        
    }

	private void Cancel()
	{
		MudDialog.Cancel();
	}


}
