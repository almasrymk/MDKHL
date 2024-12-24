

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using SmartParking.Cutomers;
using SmartParking.Entities.Customers;
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

public partial class AddVehicleDialog
{
    private MudForm form;
    public CreateUpdateVehicleDto createUpdateVehicle = new CreateUpdateVehicleDto();
    IReadOnlyList<PlateColorDto> plateColors = new List<PlateColorDto>();
    IReadOnlyList<LicenceTypeDto> LicenceTypes = new List<LicenceTypeDto>();
    IReadOnlyList<VehicleBrandDto> VehicleBrands = new List<VehicleBrandDto>();
    IReadOnlyList<VehicleModelDto> VehicleModels = new List<VehicleModelDto>();
    bool _clearable;
    [CascadingParameter]
    private MudDialogInstance MudDialogAdd { get; set; }

	[Parameter]
	public Guid CustomerId { get; set; }
    private string plateLetters = "";
    private string plateNumbers = "";


    protected override async Task OnInitializedAsync()
    {
        createUpdateVehicle.BelongToCompany = false;
        var resultplate = await PlateColorsAppService.GetAllPlateColorsAsync();
        plateColors= resultplate;


        var resultLicence = await LicenceTypesAppService.GetAllLicencesAsync();
        LicenceTypes= resultLicence;

        var resultVehicleBrand = await VehicleBrandAppService.GetAllVehicleBrandsAsync();
        VehicleBrands = resultVehicleBrand;

        

    }
    private async Task SelectBrand(int brandId)
    {
        createUpdateVehicle.VehicleBrandId = brandId;
        var resultVehicleModel = await VehicleModelAppService.GetModelsByBrandAsync(brandId);
        VehicleModels = resultVehicleModel.Value;
    }
    private async void SubmitCreate()
    {
        var successedMessage = L["VehichleWasCreatedSuccessfullyAndAddedToCustomer"];
        var validationErrorMessage = L["PleaseFixTheValidationErrors"];
        try
        {
            createUpdateVehicle.CustomerId = CustomerId;

            var vehichle = await VehicleAppService.CreateAsync(createUpdateVehicle);
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

			ShowSnackbar($"Failed to add vehicle: {ex.Code} + ", Severity.Error);
		}
		catch (AuthenticationException ex)
		{
			ShowSnackbar($"Failed to add vehicle: {ex.Message} + ", Severity.Error);
		}
		catch (Exception ex)
		{

			ShowSnackbar($"Failed to add vehicle: {ex.Message} + ", Severity.Error);
		}

	}


    private void HandleKeyUpLettersArabic(FocusEventArgs e)
    {
        HandleKeyUpLetters(createUpdateVehicle.ArPlateLetters);

    }
    private void HandleKeyUpLettersEnglish(FocusEventArgs e)
    {
        HandleKeyUpLetters(createUpdateVehicle.EnPlateLetters);


    }
    private void HandleKeyUpLetters(string e)
    {
        try
        {
            (createUpdateVehicle.EnPlateLetters, createUpdateVehicle.ArPlateLetters) = VehicleValidation.HandleKeyUpLetters(e);
        }
        catch (Exception ex)
        {
            createUpdateVehicle.EnPlateLetters = createUpdateVehicle.ArPlateLetters = "";
            ShowSnackbar(ex.Message, Severity.Error);
        }

    }
    private void HandleKeyUpArabic(FocusEventArgs e)
    {
        HandleKeyUp(createUpdateVehicle.ArPlateNumber);

    }
    private void HandleKeyUpEnglish(FocusEventArgs e)
    {
        HandleKeyUp(createUpdateVehicle.EnPlateNumber);


    }

    private void HandleKeyUp(string PlateNumber)
    {
        try
        {
            (createUpdateVehicle.EnPlateNumber, createUpdateVehicle.ArPlateNumber) = VehicleValidation.HandleKeyUp(PlateNumber);
        }
        catch (Exception ex)
        {
            createUpdateVehicle.EnPlateNumber = createUpdateVehicle.ArPlateNumber = "";
            ShowSnackbar(ex.Message, Severity.Error);
        }


    }

    private async Task CancelCreate()
    {

        MudDialogAdd.Cancel();

    }
	/// <summary>
	/// Displays a snackbar notification with the specified message and severity.
	/// </summary>
	/// <param name="message">The message to display in the snackbar.</param>
	/// <param name="severity">The severity level of the snackbar (e.g., Success, Error).</param>
	void ShowSnackbar(string message, Severity severity)
	{
		Snackbar.Clear();
		Snackbar.Configuration.PositionClass = Defaults.Classes.Position.TopRight;
		Snackbar.Add(message, severity, c => c.SnackbarVariant = Variant.Filled);
	}


}
