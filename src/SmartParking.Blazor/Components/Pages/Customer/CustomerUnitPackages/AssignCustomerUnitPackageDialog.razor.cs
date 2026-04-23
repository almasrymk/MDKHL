using AutoMapper.Internal.Mappers;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.CustomerUnitPackages;
using SmartParking.Cutomers;
using SmartParking.Enums;
using SmartParking.Organizations;
using SmartParking.Packages;
using SmartParking.UnitPackages;
using SmartParking.Units;
using SmartParking.Utilities.StaticValues;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Blazor.Components.Pages.Customer.CustomerUnitPackages;

public partial class AssignCustomerUnitPackageDialog
{
    [CascadingParameter]
    private IMudDialogInstance MudDialogAdd { get; set; }
    public UnitPackageDto CustomerPackage = new UnitPackageDto();
    public CustomerUnitPackageDto _customerPackageDto = new CustomerUnitPackageDto();
    public CreateUpdateUnitPackageDto _createUpdateUnitPackageDto = new CreateUpdateUnitPackageDto();
    [Parameter]
    public CustomerDto Customer { get; set; } = new();

    public CustomerCategoryStatus Customercategory { get; set; }

    private int organizationId;
    private IReadOnlyList<OrganizationDto> Organizations = new List<OrganizationDto>();
    private OrganizationDto OrganizationDto { get; set; } = new OrganizationDto();
    public List<UnitPackageDto> Packages { get; set; } = new List<UnitPackageDto>();

    bool _clearable;
    private MudForm form;
    public List<UnitPackageDto> selectedPackageDto { get; set; } = new List<UnitPackageDto>();
    private bool IsCustom { get; set; } = false;
    private bool IsCustomRenter { get; set; } = false;
    private IReadOnlyList<UnitDto> Units { get; set; } = new List<UnitDto>();
    private string unitCode;
    private UnitDto selectedUnit { get; set; } = new();
    private CustomerUnitPackageType? SelectedPackageType = CustomerUnitPackageType.Owner;

    private UnitPackageDto ownerPackage = new UnitPackageDto
    {
        Package = new PackageDto()

    };

    private void MapOwnerPackageData()
    {
        ownerPackage.Package.ArabicName = PackageStaticValues.ArabicName;
        ownerPackage.Package.EnglishName = PackageStaticValues.EnglishName;
        ownerPackage.Package.DectivationDate = PackageStaticValues.DeactivationDate;
        ownerPackage.Package.ActivationDate = PackageStaticValues.ActivationDate;
        ownerPackage.Package.PublishFrom = PackageStaticValues.PublishedFrom;
        ownerPackage.Package.PublishTo = PackageStaticValues.PublishedTo;
        ownerPackage.Package.CustomerCategory = PackageStaticValues.CustomerCategory;
        ownerPackage.Package.Description = PackageStaticValues.Description;
        ownerPackage.Package.DurationOfPackage = PackageStaticValues.DurationOfPackage;
        ownerPackage.Package.PackageCategoryType = PackageStaticValues.PackageCategoryType;
        ownerPackage.Package.Status = PackageStaticValues.status;
        ownerPackage.Package.PackageStatus = PackageStaticValues.PackageStatus;
        ownerPackage.Package.Price = PackageStaticValues.Price;

        // These come from the loaded OrganizationDto
        ownerPackage.Package.OrganizationId = OrganizationDto.Id;
        ownerPackage.Package.OrganizationArabicName = OrganizationDto.ArabicName;
        ownerPackage.Package.OrganizationEnglishName = OrganizationDto.EnglishName;
        ownerPackage.Package.TenantId = OrganizationDto.TenantId;
        ownerPackage.NumberOfVehicles = OrganizationDto.OrganizationSettings.OwnerVehicles;
    }


    private async Task GetOrganization()
    {
        OrganizationDto = await OrganizationAppService.GetOrganizationForCurrentTenatAsync();

    }

    /// <summary>
    /// Retrieves the available packages based on the customer's category.
    /// </summary>
    /// <param name="customerCategory">The category of the customer.</param>
    private async Task getPackage(string customerCategory)
    {
        Customercategory = (CustomerCategoryStatus)Enum.Parse(typeof(CustomerCategoryStatus), customerCategory);
        if (CurrentTenant.Id is null)
            Packages = await PackageAppService.GetUnitPackagesByCategory(organizationId, Customercategory);
        else
            Packages = await PackageAppService.GetUnitPackagesByCategory(Customercategory);
        selectedPackageDto.Clear();
    }


    private void UnitCodeChange(string code)
    {
        unitCode = code;
        selectedUnit = Units.Where(x => x.Code == unitCode).FirstOrDefault();
        if (SelectedPackageType == CustomerUnitPackageType.Owner)
        {
            IsCustom = false;
            ShowOwnerPackageInTable();
        }
        StateHasChanged();
    }
    

    /// <summary>
    /// Initializes the component and loads the list of organizations.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        var result = await OrganizationAppService.GetListAsync(new PagedAndSortedResultRequestDto());
        Organizations = result.Items;
        var unitResult = await UnitAppService.GetAllActiveUnitsAsync();
        Units = unitResult;
        await GetOrganization();
        MapOwnerPackageData();
        await base.OnInitializedAsync();
    }

    /// <summary>
    /// Submits the package assignment to the customer.
    /// </summary>
    private async Task SubmitEdit()
    {

        if (unitCode is null && SelectedPackageType.Value != CustomerUnitPackageType.Other)
        {
            Snackbar.Add(@L["CodeSelectValidation"], Severity.Error);
            return;
        }

        if (SelectedPackageType == CustomerUnitPackageType.Renter)
        {
            if (!IsCustom)
            {
                var package = Packages.FirstOrDefault();

                CustomerPackage.NumberOfSlots = package.NumberOfSlots;
                CustomerPackage.NumberOfVehicles = package.NumberOfVehicles;
                CustomerPackage.Package = package.Package;

                CustomerPackage.Package.TenantId = CurrentTenant.Id;
                var result = await HandleRenterPackage(CustomerPackage, unitCode);
                if (result)
                    return;
            }
            
        }

        else
        {
            var packageOverUnits = await CustomerUnitPackageAppService.CheckPackageOverUnits(unitCode);

            if (packageOverUnits)
            {
                SnackBar.ShowSnackbar(Snackbar, L["PackageOverUnitValidation"], Severity.Error);
                return;
            }

        }

        if (IsCustom)
        {
            _createUpdateUnitPackageDto.Package.EnglishName = "Custom Package";
            _createUpdateUnitPackageDto.Package.ArabicName = "باقه خاصه";
            _createUpdateUnitPackageDto.Package.Description = "Description";
            _createUpdateUnitPackageDto.Package.TenantId = CurrentTenant.Id;
            var pkg = new UnitPackageDto
            {
                NumberOfSlots = _createUpdateUnitPackageDto.NumberOfSlots,
                NumberOfVehicles = _createUpdateUnitPackageDto.NumberOfVehicles,
                Package = new PackageDto
                {
                    TenantId = _createUpdateUnitPackageDto.Package.TenantId,
                    DurationOfPackage = _createUpdateUnitPackageDto.Package.DurationOfPackage,
                    EnglishName = _createUpdateUnitPackageDto.Package.EnglishName,
                    ActivationDate = _createUpdateUnitPackageDto.Package.ActivationDate,
                    ArabicName = _createUpdateUnitPackageDto.Package.ArabicName,
                    DectivationDate = _createUpdateUnitPackageDto.Package.DeactivationDate,
                    Description = _createUpdateUnitPackageDto.Package.Description,
                    CustomerCategory = _createUpdateUnitPackageDto.Package.CustomerCategory.ToString(),
                    OrganizationArabicName = OrganizationDto.ArabicName,
                    OrganizationEnglishName = OrganizationDto.EnglishName,
                    OrganizationId = OrganizationDto.Id,
                    PackageCategoryType = _createUpdateUnitPackageDto.Package.PackageCategoryType,
                    PackageStatus = _createUpdateUnitPackageDto.Package.PackageStatus,
                    Price = _createUpdateUnitPackageDto.Package.Price,
                    Published = _createUpdateUnitPackageDto.Package.Published,
                    

                }
            };
            
            if (SelectedPackageType == CustomerUnitPackageType.Renter)
            {
                var result = await HandleRenterPackage(pkg, unitCode);
                if (result)
                    return;
            }
            preAssignState.UnitPackage = pkg;

        }
        else
        {
            preAssignState.UnitPackage = CustomerPackage;

        }

        preAssignState.Customer = Customer;
        preAssignState.UnitCode = unitCode;
        preAssignState.CustomerUnitPackageType = SelectedPackageType.Value;
        Navigation.NavigateTo("/prepaymentpackageunit");

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

    private void OnPackageTypeChanged(CustomerUnitPackageType? value)
    {
        SelectedPackageType = value;

        if (SelectedPackageType == CustomerUnitPackageType.Owner)
        {
            IsCustom = false;
            ShowOwnerPackageInTable();
        }

        StateHasChanged();
    }
    private void ShowOwnerPackageInTable()
    {
        ownerPackage.NumberOfSlots = selectedUnit.OwnerSlots;
        ownerPackage.NumberOfVehicles = OrganizationDto.OrganizationSettings.OwnerVehicles;
        selectedPackageDto.Clear();
        selectedPackageDto.Add(ownerPackage);
        CustomerPackage = ownerPackage;
    }
    private async Task<bool> HandleRenterPackage(UnitPackageDto dto, string unitCode)
    {
        if (CustomerPackage is null)
        {
            Snackbar.Add(@L["PackageSelectValidation"], Severity.Error);
            return true;
        }
        var result = await CustomerUnitPackageAppService.MultipuleCustomerRentalValidation(Customer.Id, dto, unitCode);
        if (result)
        {
            Snackbar.Add(@L["MultipleRentalError"], Severity.Error);
            return true;
        }
        var result1 = await CustomerUnitPackageAppService.IfCustomerHasMultiplePackageAsync(unitCode, dto);
        if (result1)
        {
            SnackBar.ShowSnackbar(Snackbar, L["CustomerHasActivePackage"], Severity.Error);
            return true;
        }
        var result2 = await CustomerUnitPackageAppService.CheckRenterSlotsOverUnit(dto, unitCode);
        if (result2)
        {
            SnackBar.ShowSnackbar(Snackbar, L["RenterSlotsOverUnitValidation"], Severity.Error);
            return true;
        }
        return false;
    }
}
