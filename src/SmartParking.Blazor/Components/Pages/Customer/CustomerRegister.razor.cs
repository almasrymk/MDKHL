using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Identity;
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
using SmartParking.Validations;
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
using Volo.Abp.Identity;
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
    IList<IdentityRoleDto> Roles = new List<IdentityRoleDto>();
    private string plateLetters = "";
    private string plateNumbers = "";
    private string RoleId = "";

    private async void CheckIsUser(bool isUser)
    {

        if (isUser && createUpdateCustomerDto.OrganizationId > 0)
        {
            createUpdateCustomerDto.IsUser = isUser;
            var organization = await OrganizationAppService.GetAsync(createUpdateCustomerDto.OrganizationId);
            Roles = await RoleAppService.GetListByTenant(organization.TenantId);
        }
            
    }
    protected async override Task OnInitializedAsync()
        {
        var result = await OrganizationAppService.GetListAsync(new PagedAndSortedResultRequestDto());
        Organizations = result.Items.ToList();
        if (CurrentTenant.Id is not null)
            {
            var organization = await OrganizationAppService.GetOrganizationForCurrentTenat();

            Roles = await RoleAppService.GetListByTenant(CurrentTenant.Id);   
            createUpdateCustomerDto.OrganizationId = organization.Id;
            createUpdateCustomerDto.TenantId = CurrentTenant.Id;
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




        }

    }
    public async  Task HandleSubmit()
    {
		var successedMessage = L["CustomerCreatedSuccessfullyWith"];
        var faildMessage = L["FailedCreateCustomer"];
		try
        {
            await CreateUser();
            CustomerDto customer;

            createUpdateCustomerDto.Email = createUpdateCustomerDto.MobileNumber + "@gmail.com";
            createUpdateCustomerDto.Password = "P@ssw0rd";
            createUpdateCustomerDto.RepeatPassword = "P@ssw0rd";
            createUpdateCustomerDto.UserName = createUpdateCustomerDto.MobileNumber;

            customer = await CustomerAppService.CreateMdkhlAsync(createUpdateCustomerDto);
            if (customer != null)
            {
                CustomerId = customer.Id;
                ShowSnackbar($" {successedMessage} {createUpdateCustomerDto.FullName}", Severity.Success);
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

    private async Task CreateUser()
    {
        if (createUpdateCustomerDto.IsUser)
        {
            var organization = await OrganizationAppService.GetAsync(createUpdateCustomerDto.OrganizationId);
            createUpdateCustomerDto.TenantId = organization.TenantId;
            createUpdateCustomerDto.RoleId = Guid.Parse(RoleId);
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

}
