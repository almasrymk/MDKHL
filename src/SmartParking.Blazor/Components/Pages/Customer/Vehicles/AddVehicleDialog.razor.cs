

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Cutomers;
using SmartParking.Entities.Customers;
using SmartParking.Entities.Vehicles;
using SmartParking.Garages;
using SmartParking.Lookups.LicenceType;
using SmartParking.Lookups.PlateColor;
using SmartParking.Lookups.VehicleBrand;
using SmartParking.Lookups.VehicleModel;
using SmartParking.OrganizationsGarages;
using SmartParking.Validations;
using SmartParking.Vehicles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.Customer.Vehicles;

/// <summary>
/// Dialog component to add a new vehicle for a customer.
/// </summary>
public partial class AddVehicleDialog
{
    private MudForm form;

    /// <summary>
    /// Holds the form input for vehicle creation or update.
    /// </summary>
    public CreateUpdateVehicleDto createUpdateVehicle = new CreateUpdateVehicleDto();

    IReadOnlyList<PlateColorDto> plateColors = new List<PlateColorDto>();
    IReadOnlyList<LicenceTypeDto> LicenceTypes = new List<LicenceTypeDto>();
    IReadOnlyList<VehicleBrandDto> VehicleBrands = new List<VehicleBrandDto>();
    IReadOnlyList<VehicleModelDto> VehicleModels = new List<VehicleModelDto>();

    bool _clearable;

    /// <summary>
    /// Reference to the parent MudDialog instance.
    /// </summary>
    [CascadingParameter]
    private IMudDialogInstance MudDialogAdd { get; set; }

    /// <summary>
    /// The ID of the customer the vehicle will be assigned to.
    /// </summary>
    [Parameter]
    public Guid CustomerId { get; set; }

    private string plateLetters = "";
    private string plateNumbers = "";
    private string LicenceTypeId { get; set; }
    private string VehicleModelId { get; set; }

    /// <summary>
    /// Initializes the form with dropdown data for vehicle attributes.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        createUpdateVehicle.BelongToCompany = false;

        var resultplate = await PlateColorsAppService.GetAllPlateColorsAsync();
        plateColors = resultplate;

        var resultLicence = await LicenceTypesAppService.GetAllLicencesAsync();
        LicenceTypes = resultLicence;

        var resultVehicleBrand = await VehicleBrandAppService.GetAllVehicleBrandsAsync();
        VehicleBrands = resultVehicleBrand;
    }

    /// <summary>
    /// Loads vehicle models when a brand is selected.
    /// </summary>
    /// <param name="brandId">The selected vehicle brand ID.</param>
    private async Task SelectBrand(string brandId)
    {
        createUpdateVehicle.VehicleBrandId = int.Parse(brandId);
        var resultVehicleModel = await VehicleModelAppService.GetModelsByBrandAsync(createUpdateVehicle.VehicleBrandId);
        VehicleModels = resultVehicleModel.Value;
    }

    /// <summary>
    /// Submits the form to create a vehicle.
    /// </summary>
    private async void SubmitCreate()
    {
        try
        {
            createUpdateVehicle.CustomerId = CustomerId;
            createUpdateVehicle.VehicleModelId = int.Parse(VehicleModelId);
            createUpdateVehicle.LicenceTypeId = int.Parse(LicenceTypeId);

            var vehichle = await VehicleAppService.CreateAsync(createUpdateVehicle);

            if (vehichle != null)
            {
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.ADD]} {L[AlertMessages.AlertMessages.SUCCESS]} {L["Vehicle"]} {vehichle.ArPlateNumber}", Severity.Success);
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
    /// Handles key input for Arabic plate letters.
    /// </summary>
    /// <param name="e">Focus event arguments.</param>
    private void HandleKeyUpLettersArabic(FocusEventArgs e)
    {
        HandleKeyUpLetters(createUpdateVehicle.ArPlateLetters);
    }

    /// <summary>
    /// Handles key input for English plate letters.
    /// </summary>
    /// <param name="e">Focus event arguments.</param>
    private void HandleKeyUpLettersEnglish(FocusEventArgs e)
    {
        HandleKeyUpLetters(createUpdateVehicle.EnPlateLetters);
    }

    /// <summary>
    /// Validates and syncs plate letters in both Arabic and English.
    /// </summary>
    /// <param name="e">The entered plate letters.</param>
    private void HandleKeyUpLetters(string e)
    {
        try
        {
            (createUpdateVehicle.EnPlateLetters, createUpdateVehicle.ArPlateLetters) = VehicleValidation.HandleKeyUpLetters(e);
        }
        catch (Exception ex)
        {
            createUpdateVehicle.EnPlateLetters = createUpdateVehicle.ArPlateLetters = "";
            SnackBar.ShowSnackbar(Snackbar, ex.Message, Severity.Error);
        }
    }

    /// <summary>
    /// Handles key input for Arabic plate numbers.
    /// </summary>
    /// <param name="e">Focus event arguments.</param>
    private void HandleKeyUpArabic(FocusEventArgs e)
    {
        HandleKeyUp(createUpdateVehicle.ArPlateNumber);
    }

    /// <summary>
    /// Handles key input for English plate numbers.
    /// </summary>
    /// <param name="e">Focus event arguments.</param>
    private void HandleKeyUpEnglish(FocusEventArgs e)
    {
        HandleKeyUp(createUpdateVehicle.EnPlateNumber);
    }

    /// <summary>
    /// Validates and syncs plate numbers in both Arabic and English.
    /// </summary>
    /// <param name="PlateNumber">The entered plate number.</param>
    private void HandleKeyUp(string PlateNumber)
    {
        try
        {
            (createUpdateVehicle.EnPlateNumber, createUpdateVehicle.ArPlateNumber) = VehicleValidation.HandleKeyUp(PlateNumber);
        }
        catch (Exception ex)
        {
            createUpdateVehicle.EnPlateNumber = createUpdateVehicle.ArPlateNumber = "";
            SnackBar.ShowSnackbar(Snackbar, ex.Message, Severity.Error);
        }
    }

    /// <summary>
    /// Cancels the vehicle creation and closes the dialog.
    /// </summary>
    private async Task CancelCreate()
    {
        MudDialogAdd.Cancel();
    }
}
