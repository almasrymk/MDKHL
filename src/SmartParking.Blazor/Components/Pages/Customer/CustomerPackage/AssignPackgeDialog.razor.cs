using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Blazor.Components.Shared.Utilities;
using SmartParking.Blazor.Components.State;
using SmartParking.CustomerPackages;
using SmartParking.Cutomers;
using SmartParking.Enums;
using SmartParking.Localization;
using SmartParking.Organizations;
using SmartParking.Packages;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Blazor.Components.Pages.Customer.CustomerPackage;

/// <summary>
/// Dialog component for assigning a package to a customer.
/// </summary>
partial class AssignPackgeDialog
{
    [CascadingParameter]
    private IMudDialogInstance MudDialogAdd { get; set; }
    public PackageDto CustomerPackage = new PackageDto();
    public CustomerPackageDto _customerPackageDto = new CustomerPackageDto();
    [Parameter]
    public CustomerDto Customer { get; set; }
    [Parameter]
    public int VehicleId { get; set; }
    public CustomerCategoryStatus Customercategory { get; set; }

    private int organizationId;
    private IReadOnlyList<OrganizationDto> Organizations = new List<OrganizationDto>();
    public List<PackageDto> Packages { get; set; } = new List<PackageDto>();

    bool _clearable;
    private MudForm form;
    public List<PackageDto> selectedPackageDto { get; set; } = new List<PackageDto>();
    private bool IsCustom { get; set; } = false;
    private CustomPackageUiDto CustomPackageUiDto { get; set; } 


    /// <summary>
    /// Retrieves the available packages based on the customer's category.
    /// </summary>
    /// <param name="customerCategory">The category of the customer.</param>
    private async Task getPackage(string customerCategory)
    {
        Customercategory = (CustomerCategoryStatus)Enum.Parse(typeof(CustomerCategoryStatus), customerCategory);
        if (CurrentTenant.Id is null)
            Packages = await PackagesAppService.GetAllMdkhlAsync(organizationId, Customercategory);
        else
            Packages = await PackagesAppService.GetAllOrganizationAsync(Customercategory);
        selectedPackageDto.Clear();
    }

    /// <summary>
    /// Initializes the component and loads the list of organizations.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        var result = await OrganizationAppService.GetListAsync(new PagedAndSortedResultRequestDto());
        Organizations = result.Items;
        await base.OnInitializedAsync();
    }
 
    /// <summary>
    /// Submits the package assignment to the customer.
    /// </summary>
    private async Task SubmitEdit()
    {

        var package = selectedPackageDto.FirstOrDefault();
        if (CustomerPackage.CustomerCategory != Customercategory.ToString() && !IsCustom)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{LocalizationHelper.GetLocalizedString("Package")} , " +
            $"{LocalizationHelper.GetLocalizedString("customer category")} " +
            $"{LocalizationHelper.GetLocalizedString("NotSameType")} ", Severity.Error);
            return;
        }
        if (IsCustom)
        {
            if (CustomPackageUiDto.startTime == null || CustomPackageUiDto.endTime == null)
            {
                SnackBar.ShowSnackbar(Snackbar, $"{LocalizationHelper.GetLocalizedString("Time")} , " +
                $"{LocalizationHelper.GetLocalizedString("TimeMustBeEntered")} ", Severity.Error);
                return;
            }
        }

        var vehicle = await VehiclesAppService.GetAsync(VehicleId);
        if (CustomerPackage.CustomerCategory == CustomerCategoryStatus.Customer.ToString())
        {
            await CustomerPackagesAppService.CheckSubscriptionCustomersLimitsAsync(CustomerPackage.OrganizationId);
        }

        _customerPackageDto.ArPlateNumber = vehicle.ArPlateNumber;
        _customerPackageDto.EnPlateNumber = vehicle.EnPlateNumber;
        _customerPackageDto.CustomerId = Customer.Id;
        _customerPackageDto.CustomerCategory = Customercategory;



        // Handle custom package assignment
        if (IsCustom)
        {
            //Method  to Check Is Custom Varaiable
            CustomPackageValidations.CheckIsCustomProperties(CustomPackageUiDto,Snackbar);
            _customerPackageDto.publishedOfPackage = true;
            _customerPackageDto.ActivationDateOfPackage = CustomPackageUiDto.ActivationDate.HasValue && CustomPackageUiDto.startTime.HasValue
                                                        ? CustomPackageUiDto.ActivationDate.Value.Date + CustomPackageUiDto.startTime.Value
                                                        : CustomPackageUiDto.ActivationDate.Value.Date;
            _customerPackageDto.ExpiryDate = CustomPackageUiDto.DectivationDate.HasValue && CustomPackageUiDto.endTime.HasValue
                                            ? CustomPackageUiDto.DectivationDate.Value.Date + CustomPackageUiDto.endTime.Value
                                            : CustomPackageUiDto.DectivationDate.Value.Date;
            _customerPackageDto.IsCustomPackage = true;
            _customerPackageDto.DurationOfPackage = CustomPackageUiDto.DectivationDate.Value.DayOfYear - CustomPackageUiDto.ActivationDate.Value.DayOfYear;
            CustomerPackage.Id = default;
            _customerPackageDto.PackageArabicName = AlertMessages.AlertMessages.PackageNameAr;
            _customerPackageDto.PackageEnglishName = AlertMessages.AlertMessages.PackageNameEn;
            _customerPackageDto.PublishFromOfPackage = CustomPackageUiDto.startTime.HasValue ? TimeOnly.FromTimeSpan(CustomPackageUiDto.startTime.Value) : null;
            _customerPackageDto.PublishToOfPackage = CustomPackageUiDto.endTime.HasValue ? TimeOnly.FromTimeSpan(CustomPackageUiDto.endTime.Value) : null;
            if (CustomPackageUiDto.ActivationDate == CustomPackageUiDto.DectivationDate &&  CustomPackageUiDto.endTime.Value <= CustomPackageUiDto.startTime.Value)
                throw new Exception(LocalizationHelper.GetLocalizedString("EndTimeShouldBeGreaterThanStartTime"));
        }
        else
        {
            _customerPackageDto.PackageId = package.Id;
            _customerPackageDto.PriceOfPackage = package.Price;
            _customerPackageDto.PackageArabicName = package.ArabicName;
            _customerPackageDto.PackageEnglishName = package.EnglishName;
            _customerPackageDto.ExpiryDate = package.PackageCategoryType!=PackageCategoryTypes.CustomPackage ? DateTime.Now.AddDays(package.DurationOfPackage).AddMinutes(60 - DateTime.Now.AddDays(package.DurationOfPackage).Minute): package.DectivationDate; 
            _customerPackageDto.PublishFromOfPackage = package.PublishFrom;
            _customerPackageDto.PublishToOfPackage = package.PublishTo;
            _customerPackageDto.ActivationDateOfPackage=  package.PackageCategoryType != PackageCategoryTypes.CustomPackage ? DateTime.Now : package.ActivationDate;
            _customerPackageDto.ActivationDate = package.PackageCategoryType != PackageCategoryTypes.CustomPackage ? DateOnly.FromDateTime(DateTime.Now) : DateOnly.FromDateTime(package.ActivationDate.Date);
            _customerPackageDto.DectivationDateOfPackage = package.PackageCategoryType != PackageCategoryTypes.CustomPackage ? DateTime.Now.AddDays(package.DurationOfPackage).AddMinutes(60 - DateTime.Now.AddDays(package.DurationOfPackage).Minute) : package.DectivationDate;
            _customerPackageDto.ExpiryDateOfPackage = package.PackageCategoryType != PackageCategoryTypes.CustomPackage ? DateOnly.FromDateTime(DateTime.Now.AddDays(package.DurationOfPackage).AddMinutes(60 - DateTime.Now.AddDays(package.DurationOfPackage).Minute) ): DateOnly.FromDateTime(package.DectivationDate);
            _customerPackageDto.DescriptionOfPackage = package.Description;
            _customerPackageDto.DurationOfPackage = package.DurationOfPackage;
            _customerPackageDto.publishedOfPackage = package.Published;
            _customerPackageDto.CountOfDays = package.DurationOfPackage;
            _customerPackageDto.PriceOfPackagePerDay = Math.Round((package.Price / package.DurationOfPackage),2);


        }
        if (CurrentTenant.Id is null)
        {
            _customerPackageDto.OrganizationId = organizationId;
            _customerPackageDto.TenantId = Organizations.FirstOrDefault(x => x.Id == organizationId).TenantId;
        }
        else
        {
            _customerPackageDto.OrganizationId = Organizations.FirstOrDefault(x => x.TenantId == CurrentTenant.Id).Id;
            _customerPackageDto.TenantId = CurrentTenant.Id;
        }
        _customerPackageDto.Organization = Organizations.FirstOrDefault(x => x.TenantId == CurrentTenant.Id);
        preAssignState.CustomerPackage = _customerPackageDto;
        preAssignState.Customer = Customer;

        Navigation.NavigateTo("/prepayment");

    }

    /// <summary>
    /// Cancels the edit operation and closes the dialog.
    /// </summary>
    private async Task CancelEdit()
    {
        MudDialogAdd.Cancel();
    }

    /// <summary>
    /// Handles the package selection change event and updates the selected package.
    /// </summary>
    /// <param name="value">The ID of the selected package.</param>
    private async Task PackageValueChanged(int value)
    {
        selectedPackageDto.Clear();
        selectedPackageDto.Add(Packages.Find(x => x.Id == value));
        CustomerPackage = Packages.Find(x => x.Id == value);

    }





}
