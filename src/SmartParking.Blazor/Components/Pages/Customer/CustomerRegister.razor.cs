using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using SmartParking.Cutomers;
using SmartParking.DefaultPackages;
using SmartParking.Enums;
using SmartParking.Lookups.LicenceType;
using SmartParking.Lookups.PlateColor;
using SmartParking.Lookups.VehicleBrand;
using SmartParking.Lookups.VehicleModel;
using SmartParking.Mobile.Vehicles;
using SmartParking.Organizations;
using SmartParking.Packages;
using SmartParking.Vehicles;
using SmartParking.Vehicles.LookUps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.Customer;

public partial class CustomerRegister
{
    private MudForm form;
    public CreateUpdateCustomerDto createUpdateCustomerDto = new CreateUpdateCustomerDto();
    bool showVehicle = false;
    Guid CustomerId;
    public CreateUpdateVehicleDto createUpdateVehicle = new CreateUpdateVehicleDto();
    IReadOnlyList<PlateColorDto> plateColors = new List<PlateColorDto>();
    IReadOnlyList<LicenceTypeDto> LicenceTypes = new List<LicenceTypeDto>();
    IReadOnlyList<VehicleBrandDto> VehicleBrands = new List<VehicleBrandDto>();
    IReadOnlyList<VehicleModelDto> VehicleModels = new List<VehicleModelDto>();
    public string Picture { get; set; }

    private List<OrganizationDto> Organizations = new List<OrganizationDto>();
    IList<PackageDto> packages = new List<PackageDto>();
    private string plateLetters = "";
    private string plateNumbers = "";
    protected async override Task OnInitializedAsync()
        {
        var result = await OrganizationAppService.GetListAsync(new PagedAndSortedResultRequestDto());
        Organizations = result.Items.ToList();
        if (CurrentTenant.Id is not null)
            {
            var organization = await OrganizationAppService.GetOrganizationForCurrentTenat();

               
            createUpdateCustomerDto.OrganizationId = organization.Id;
            Picture = await OrganizationAppService.GetOrganizationPictureByOrganizationTenant(CurrentTenant.Id);
        }
        Picture = "images/MDHKL_04.png";
        createUpdateVehicle.BelongToCompany = false;
        var resultplate = await PlateColorsAppService.GetAllPlateColorsAsync();
        plateColors = resultplate;


        var resultLicence = await LicenceTypesAppService.GetAllLicencesAsync();
        LicenceTypes = resultLicence;

        var resultVehicleBrand = await VehicleBrandAppService.GetAllVehicleBrandsAsync();
        VehicleBrands = resultVehicleBrand;
    }
    private async Task SelectBrand(int brandId)
    {
        createUpdateVehicle.VehicleBrandId = brandId;
        var resultVehicleModel = await VehicleModelAppService.GetModelsByBrandAsync(brandId);
        VehicleModels = resultVehicleModel.Value;
    }
    private async Task getPackage(string customerCategory)
    {
        var CustomerCategory = (CustomerCategoryStatus)Enum.Parse(typeof(CustomerCategoryStatus), customerCategory);
        if (CurrentTenant.Id is null )
            packages = await PackagesAppService.GetAllMdkhlAsync(createUpdateCustomerDto.OrganizationId, CustomerCategory);
        else
            packages = await PackagesAppService.GetAllOrganizationAsync(CustomerCategory);

        if (packages.Count == 0)
        {
            ShowSnackbar("There Is No Package for that Customer Category !", Severity.Warning);
            await Task.Delay(1000);
            ShowSnackbar("Add Package !", Severity.Warning);
            await Task.Delay(1000);
            Navigation.NavigateTo("/package");




        }

    }
    public async  Task HandleSubmit()
    {
		var successedMessage = L["CustomerCreatedSuccessfullyWith"];
        var faildMessage = L["FailedCreateCustomer"];
		try
        {
            CustomerDto customer;

            createUpdateCustomerDto.Email = createUpdateCustomerDto.MobileNumber + "@gmail.com";
            createUpdateCustomerDto.Password = "P@ssw0rd";
            createUpdateCustomerDto.RepeatPassword = "P@ssw0rd";
            createUpdateCustomerDto.UserName = createUpdateCustomerDto.MobileNumber;

				customer = await CustomerAppService.CreateMdkhlAsync(createUpdateCustomerDto);
            if (customer != null)
            {
                CustomerId = customer.Id;
                ShowSnackbar($" {successedMessage} {createUpdateCustomerDto.FullName}" ,Severity.Success);
                showVehicle = true;
            }


        }
        catch (AbpValidationException abpValidationException)
        {
            string errors = string.Empty;
            foreach (var error in abpValidationException.ValidationErrors)
            {
                errors += error;
            }
            ShowSnackbar(faildMessage+ errors, Severity.Error);

        }
        catch (BusinessException businessException)
        {
            string errors = string.Empty;
            foreach (var error in businessException.Message)
            {
                errors += error;
            }
            ShowSnackbar(faildMessage+ errors, Severity.Error);

        }
        catch (Exception ex)
        {

            ShowSnackbar(faildMessage +ex.Message, Severity.Error);

        }

    }
   
    /// <summary>
    /// Displays a snackbar notification with the specified message and severity.
    /// </summary>
    /// <param name="message">The message to display in the snackbar.</param>
    /// <param name="severity">The severity level of the snackbar (e.g., Success, Error).</param>
    void ShowSnackbar(string message, Severity severity)
    {
        Snackbar.Clear();
        Snackbar.Add(message, severity, c => c.SnackbarVariant = Variant.Filled);
    }

    public void HandleCancel()
    {
        Navigation.NavigateTo("/customers", true);

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
                await Task.Delay(1000);
                Navigation.NavigateTo("/customers", true);

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

            ShowSnackbar($"{validationErrorMessage} {ex.Code} + ", Severity.Error);
        }
        catch (AuthenticationException ex)
        {
            ShowSnackbar($"{validationErrorMessage} {ex.Message} + ", Severity.Error);
        }
        catch (Exception ex)
        {

            ShowSnackbar($"{validationErrorMessage} {ex.Message} + ", Severity.Error);
        }

    }

    private void CancelCreate()
    {

        Navigation.NavigateTo("/customers", true);

    }

    private void HandleKeyUpLetters(FocusEventArgs e)
    {
        if (plateLetters == "")
            return;
        if (plateLetters.All(c => c >= '\u0600' && c <= '\u06FF'))
        {
            createUpdateVehicle.ArPlateLetters = plateLetters;
            createUpdateVehicle.EnPlateLetters = MapArabicToEnglish(plateLetters);
            if (createUpdateVehicle.EnPlateLetters == "")
            {
                plateLetters = "";
                createUpdateVehicle.ArPlateLetters = "";
            }
        }
        else if (plateLetters.All(c => (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')))
        {
            createUpdateVehicle.EnPlateLetters = plateLetters.ToUpper();
            createUpdateVehicle.ArPlateLetters = MapEnglishToArabic(plateLetters);
            if (createUpdateVehicle.ArPlateLetters == "")
            {
                plateLetters = "";
                createUpdateVehicle.EnPlateLetters = "";
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
        createUpdateVehicle.EnPlateNumber = plateNumbers;
        createUpdateVehicle.ArPlateNumber = MapToArabicNumerals(plateNumbers);

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
                createUpdateVehicle.ArPlateNumber = "";
                return "";
            }
            arabicNUmbers += englishToArabicMap[character];
        }
        return arabicNUmbers;
    }

}
