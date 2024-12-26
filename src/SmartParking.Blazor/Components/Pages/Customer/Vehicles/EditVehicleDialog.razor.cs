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
using System.Text;
using System.Globalization;
using SmartParking.Validations;
using SmartParking.Blazor.Components.Shared;

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
    private string plateLetters = "";
    private string plateNumbers = "";

    protected override async Task OnInitializedAsync()
    {
        if(CultureInfo.CurrentCulture.Name == "ar")
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
    private async Task SelectBrand(int brandId)
    {
        Vehicle.VehicleBrandId = brandId;
        var resultVehicleModel = await VehicleModelAppService.GetModelsByBrandAsync(brandId);
        VehicleModels = resultVehicleModel.Value;
    }
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
    private void HandleKeyUpLettersArabic(FocusEventArgs e)
    {
        HandleKeyUpLetters(Vehicle.ArPlateLetters);

    }
    private void HandleKeyUpLettersEnglish(FocusEventArgs e)
    {
        HandleKeyUpLetters(Vehicle.EnPlateLetters);


    }
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

    private void HandleKeyUpArabic(FocusEventArgs e)
    {
        HandleKeyUp(Vehicle.ArPlateNumber);

    }
    private void HandleKeyUpEnglish(FocusEventArgs e)
    {
        HandleKeyUp(Vehicle.EnPlateNumber);


    }

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

    private void CancelEdit()
    {

        MudDialogAdd.Cancel();

    }

}
