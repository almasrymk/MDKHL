using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Lookups.LicenceType;
using SmartParking.Lookups.PlateColor;
using SmartParking.Lookups.VehicleBrand;
using SmartParking.Lookups.VehicleModel;
using SmartParking.Validations;
using SmartParking.Vehicles;
using System.Globalization;
using System.Security.Authentication;
using Volo.Abp;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.Customer.Vehicles;

/// <summary>
/// Dialog component for editing an existing vehicle's information.
/// </summary>
public partial class EditVehicleDialog
{
    private MudForm form;

    /// <summary>
    /// The ID of the vehicle to be edited.
    /// </summary>
    [Parameter]
    public int VehicleId { get; set; }

    /// <summary>
    /// The vehicle DTO containing the editable data.
    /// </summary>
    [Parameter]
    public CreateUpdateVehicleDto Vehicle { get; set; } = new CreateUpdateVehicleDto();

    IReadOnlyList<PlateColorDto> plateColors = new List<PlateColorDto>();
    IReadOnlyList<LicenceTypeDto> LicenceTypes = new List<LicenceTypeDto>();
    IReadOnlyList<VehicleBrandDto> VehicleBrands = new List<VehicleBrandDto>();
    IReadOnlyList<VehicleModelDto> VehicleModels = new List<VehicleModelDto>();

    bool _clearable;

    /// <summary>
    /// Reference to the parent MudDialog instance for controlling the dialog behavior.
    /// </summary>
    [CascadingParameter]
    private IMudDialogInstance MudDialogAdd { get; set; }

    private string plateLetters = "";
    private string plateNumbers = "";

    /// <summary>
    /// Initializes the dialog and loads dropdown data based on the current culture.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        if (CultureInfo.CurrentCulture.Name == "ar")
        {
            plateLetters = Vehicle.ArPlateLetters;
            plateNumbers = Vehicle.ArPlateNumber;
        }
        else
        {
            plateLetters = Vehicle.EnPlateLetters;
            plateNumbers = Vehicle.EnPlateNumber;
        }

        var resultplate = await PlateColorsAppService.GetAllPlateColorsAsync();
        plateColors = resultplate;

        var resultLicence = await LicenceTypesAppService.GetAllLicencesAsync();
        LicenceTypes = resultLicence;

        var resultVehicleBrand = await VehicleBrandAppService.GetAllVehicleBrandsAsync();
        VehicleBrands = resultVehicleBrand;
    }

    /// <summary>
    /// Loads vehicle models when a vehicle brand is selected.
    /// </summary>
    /// <param name="brandId">The ID of the selected brand.</param>
    private async Task SelectBrand(int brandId)
    {
        Vehicle.VehicleBrandId = brandId;
        var resultVehicleModel = await VehicleModelAppService.GetModelsByBrandAsync(brandId);
        VehicleModels = resultVehicleModel.Value;
    }

    /// <summary>
    /// Submits the updated vehicle information to the server.
    /// </summary>
    private async void SubmitEdit()
    {
        try
        {
            Vehicle.CustomerId = Vehicle.CustomerId;

            var vehichle = await VehicleAppService.UpdateAsync(VehicleId, Vehicle);
            if (vehichle != null)
            {
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]} {L[AlertMessages.AlertMessages.UPDATE]} {L["Vehicle"]} {vehichle.ArPlateNumber}", Severity.Success);
                MudDialogAdd.Close(DialogResult.Ok(true));

                await Task.Delay(1000);
                Navigation.Refresh(true);
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
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.ADD]} {L[AlertMessages.AlertMessages.FAILED]}: {ex.Code}", Severity.Error);
        }
        catch (AuthenticationException ex)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.ADD]} {L[AlertMessages.AlertMessages.FAILED]} : {ex.Message} ", Severity.Error);
        }
        catch (Exception ex)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.ADD]} {L[AlertMessages.AlertMessages.FAILED]}: {ex.Message} ", Severity.Error);
        }
    }

    /// <summary>
    /// Handles key input for Arabic plate letters and validates them.
    /// </summary>
    /// <param name="e">Focus event args.</param>
    private void HandleKeyUpLettersArabic(FocusEventArgs e)
    {
        HandleKeyUpLetters(Vehicle.ArPlateLetters);
    }

    /// <summary>
    /// Handles key input for English plate letters and validates them.
    /// </summary>
    /// <param name="e">Focus event args.</param>
    private void HandleKeyUpLettersEnglish(FocusEventArgs e)
    {
        HandleKeyUpLetters(Vehicle.EnPlateLetters);
    }

    /// <summary>
    /// Validates plate letters and syncs Arabic and English versions.
    /// </summary>
    /// <param name="e">Entered plate letters.</param>
    private void HandleKeyUpLetters(string e)
    {
        try
        {
            (Vehicle.EnPlateLetters, Vehicle.ArPlateLetters) = VehicleValidation.HandleKeyUpLetters(e);
        }
        catch (Exception ex)
        {
            Vehicle.EnPlateLetters = Vehicle.ArPlateLetters = "";
            SnackBar.ShowSnackbar(Snackbar, ex.Message, Severity.Error);
        }
    }

    /// <summary>
    /// Handles key input for Arabic plate numbers and validates them.
    /// </summary>
    /// <param name="e">Focus event args.</param>
    private void HandleKeyUpArabic(FocusEventArgs e)
    {
        HandleKeyUp(Vehicle.ArPlateNumber);
    }

    /// <summary>
    /// Handles key input for English plate numbers and validates them.
    /// </summary>
    /// <param name="e">Focus event args.</param>
    private void HandleKeyUpEnglish(FocusEventArgs e)
    {
        HandleKeyUp(Vehicle.EnPlateNumber);
    }

    /// <summary>
    /// Validates plate numbers and syncs Arabic and English versions.
    /// </summary>
    /// <param name="PlateNumber">Entered plate number.</param>
    private void HandleKeyUp(string PlateNumber)
    {
        try
        {
            (Vehicle.EnPlateNumber, Vehicle.ArPlateNumber) = VehicleValidation.HandleKeyUp(PlateNumber);
        }
        catch (Exception ex)
        {
            Vehicle.EnPlateNumber = Vehicle.ArPlateNumber = "";
            SnackBar.ShowSnackbar(Snackbar, ex.Message, Severity.Error);
        }
    }

    /// <summary>
    /// Cancels the edit dialog without saving.
    /// </summary>
    private void CancelEdit()
    {
        MudDialogAdd.Cancel();
    }
}
