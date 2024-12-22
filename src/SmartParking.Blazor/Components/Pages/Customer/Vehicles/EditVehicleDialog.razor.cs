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
    private void HandleKeyUpLetters(FocusEventArgs e)
    {
        if (plateLetters == "")
            return;
        if (plateLetters.All(c => c >= '\u0600' && c <= '\u06FF'))
        {
            Vehicle.ArPlateLetters = plateLetters;
            Vehicle.EnPlateLetters = MapArabicToEnglish(plateLetters);
            if (Vehicle.EnPlateLetters == "")
            {
                plateLetters = "";
                Vehicle.ArPlateLetters = "";
            }
        }
        else if (plateLetters.All(c => (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')))
        {
            Vehicle.EnPlateLetters = plateLetters.ToUpper();
            Vehicle.ArPlateLetters = MapEnglishToArabic(plateLetters);
            if (Vehicle.ArPlateLetters == "")
            {
                plateLetters = "";
                Vehicle.EnPlateLetters = "";
            }
        }
        else
        {
            ShowSnackbar("Letters can't be mixed", Severity.Error);
        }

    }

    private string MapArabicToEnglish(string arabicText)
    {
        var ArabicToEnglishMap = new Dictionary<char, char>
{
    {'أ', 'A'}, {'ب', 'B'}, {'د', 'D'}, {'ح', 'J'}, {'ر', 'R'}, {'س', 'S'}, {'ص', 'X'}, {'ط', 'T'},
    {'ع', 'E'}, {'ق', 'G'}, {'ك', 'K'}, {'ل', 'L'}, {'ن', 'N'}, {'م', 'Z'}, {'ه', 'H'}, {'و', 'U'}, {'ي', 'V'}
};


        StringBuilder result = new StringBuilder();
        foreach (var charInArabic in arabicText)
        {
            if (ArabicToEnglishMap.ContainsKey(charInArabic))
            {
                result.Append(ArabicToEnglishMap[charInArabic]);
            }
            else
            {
                ShowSnackbar("Invalid PlateLetter", Severity.Error);
                return "";
            }
        }
        return result.ToString();
    }

    private string MapEnglishToArabic(string englishText)
    {
        var EnglishToArabicMap = new Dictionary<char, char>
      {
          {'A', 'أ'}, {'B', 'ب'}, {'D', 'د'},{'J', 'ح'},{'R', 'ر'}, {'S', 'س'},{'X', 'ص'},{'T', 'ط'},
          {'E', 'ع'}, {'G', 'ق'},{'K', 'ك'}, {'L', 'ل'},{'N', 'ن'},{'Z', 'م'},{'H', 'ه'},{'U', 'و'},{'V', 'ي'}
      };
        englishText = englishText.ToUpper();
        StringBuilder result = new StringBuilder();
        foreach (var charInEnglish in englishText)
        {
            if (EnglishToArabicMap.ContainsKey(charInEnglish))
            {
                result.Append(EnglishToArabicMap[charInEnglish]);
            }
            else
            {
                ShowSnackbar("Invalid PlateLetter", Severity.Error);
                return "";
            }
        }
        return result.ToString();
    }


    private void HandleKeyUp(FocusEventArgs e)
    {
        Vehicle.EnPlateNumber = plateNumbers;
        Vehicle.ArPlateNumber = MapToArabicNumerals(plateNumbers);

    }

    //private void HandleKeyUp(FocusEventArgs e)
    //{

    //    Vehicle.ArPlateNumber = MapToArabicNumerals(Vehicle.ArPlateNumber);

    //}

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
