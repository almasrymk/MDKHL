using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Lookups;
using SmartParking.Lookups.City;
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
    public IReadOnlyList<GovernorateLookupDto> governorates { get; set; } = new List<GovernorateLookupDto>();
    private MudForm editForm;
    bool _clearable;
    [Parameter] public int UpdatedId { get; set; }


    protected override async Task OnInitializedAsync()
    {
        var result = await CitiesAppService.GetGovernorateLookupAsync();
        governorates = result;
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
                Notes = city.Notes,
                GovernorateId = city.GovernorateId,
                Id = UpdatedId
            };

            // Call the update method

            var cities = await CitiesAppService.UpdateAsync(UpdatedId, input);
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
            Snackbar.Add($"Failed to update city: {ex.Code} + ", Severity.Error);

        }
        catch (Exception ex)
        {
            Snackbar.Add($"Failed to update city: {ex.Message} + ", Severity.Error);
        }
        
    }


    private void Cancel()
    {
        MudDialog.Cancel();
    }
}
