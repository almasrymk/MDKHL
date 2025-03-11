using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Lookups.Country;
using SmartParking.Lookups.Region;
using System.Security.Authentication;
using Volo.Abp;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.Regions;

public partial class EditRegionDialog
{
    [CascadingParameter] IMudDialogInstance MudDialog { get; set; }
    [Parameter] public RegionDto region { get; set; } = new RegionDto();
    public List<CountryLookupDto> countries { get; set; } = new List<CountryLookupDto>();
    [Parameter] public int UpdatedId { get; set; }
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
