using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MudBlazor;
using SmartParking.Lookups.LicenceType;
using SmartParking.Lookups.PlateColor;
using SmartParking.Lookups.VehicleBrand;
using SmartParking.Lookups.VehicleModel;
using SmartParking.Mobile.Vehicles;
using SmartParking.Vehicles.LookUps;
using SmartParking.Vehicles;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Threading.Tasks;
using System;
using Volo.Abp;
using Volo.Abp.Validation;
using System.Linq;
using Microsoft.AspNetCore.Components.Web;

namespace SmartParking.Blazor.Components.Pages.Customer.Vehicles;

public partial class EditVehicleDialog
{
    private MudForm form;
    [Parameter]
    public int VehicleId { get; set; }
    [Parameter]
    public CreateUpdateVehicleDto Vehicle { get; set; } = new CreateUpdateVehicleDto();
    IReadOnlyList<PlateColorDto> plateColors = new List<PlateColorDto>();
    IReadOnlyList<LicenceTypeDto> LicenceTypes = new List<LicenceTypeDto>();
    IReadOnlyList<VehicleBrandDto> VehicleBrands = new List<VehicleBrandDto>();
    IReadOnlyList<VehicleModelDto> VehicleModels = new List<VehicleModelDto>();
    bool _clearable;
    [CascadingParameter]
    private MudDialogInstance MudDialogAdd { get; set; }


    protected override async Task OnInitializedAsync()
    {
        var resultplate = await PlateColorsAppService.GetAllPlateColorsAsync();
        plateColors = resultplate;


        var resultLicence = await LicenceTypesAppService.GetAllLicencesAsync();
        LicenceTypes = resultLicence;

        var resultVehicleBrand = await VehicleBrandAppService.GetAllVehicleBrandsAsync();
        VehicleBrands = resultVehicleBrand;
    }
    private async Task SelectBrand(int brandId)
    {
        Vehicle.VehicleBrandId = brandId;
        var resultVehicleModel = await VehicleModelAppService.GetModelsByBrandAsync(brandId);
        VehicleModels = resultVehicleModel.Value;
    }
    private async void SubmitEdit()
    {
        var successedMessage = L["VehichleWasUpdatedSuccessfully"];
        var validationErrorMessage = L["PleaseFixTheValidationErrors"];
        try
        {
            Vehicle.CustomerId = Vehicle.CustomerId;

            var vehichle = await VehicleAppService.UpdateAsync(VehicleId, Vehicle);
            if (vehichle != null)
            {

                ShowSnackbar(successedMessage, Severity.Success);
                MudDialogAdd.Close(DialogResult.Ok(true));

                await Task.Delay(1000);
                Navigation.Refresh(true);

            }


        }
        catch (AbpValidationException validationException)
        {
            foreach (var validationError in validationException.ValidationErrors)
            {
                ShowSnackbar($"{validationErrorMessage} {validationError.MemberNames.First()} - {validationError.ErrorMessage}", Severity.Error);
            }
        }
        catch (BusinessException ex)
        {

            ShowSnackbar($"Failed to edit vehicle: {ex.Code} + ", Severity.Error);
        }
        catch (AuthenticationException ex)
        {
            ShowSnackbar($"Failed to edit vehicle: {ex.Message} + ", Severity.Error);
        }
        catch (Exception ex)
        {

            ShowSnackbar($"Failed to edit vehicle: {ex.Message} + ", Severity.Error);
        }

    }

    private void HandleKeyUp(FocusEventArgs e)
    {

        Vehicle.ArPlateNumber = MapToArabicNumerals(Vehicle.ArPlateNumber);

    }

    private string MapToArabicNumerals(string englishNumber)
    {
        if (englishNumber == "")
        {
            return "";
        }
        var englishToArabicMap = new Dictionary<char, string>
         {
            { '0', "٠" },
            { '1', "١" },
            { '2', "٢" },
            { '3', "٣" },
            { '4', "٤" },
            { '5', "٥" },
            { '6', "٦" },
            { '7', "٧" },
            { '8', "٨" },
            { '9', "٩" }
        };
        string arabicNUmbers = "";
        foreach (var character in englishNumber)
        {

            if (character < '0' || character > '9')
            {
                ShowSnackbar("Only Numbers are accepted", Severity.Error);
                Vehicle.ArPlateNumber = "";
                return "";
            }
            arabicNUmbers += englishToArabicMap[character];
        }
        return arabicNUmbers;
    }

    private void CancelEdit()
    {

        MudDialogAdd.Cancel();

    }
    void ShowSnackbar(string message, Severity severity)
    {
        Snackbar.Clear();
        Snackbar.Configuration.PositionClass = Defaults.Classes.Position.TopRight;
        Snackbar.Add(message, severity, c => c.SnackbarVariant = Variant.Filled);
    }
}
