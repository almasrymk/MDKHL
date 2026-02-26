using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.CustomerUnitPackages;
using SmartParking.Cutomers;
using SmartParking.Enums;
using SmartParking.Organizations;
using SmartParking.UnitPackages;
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
    public List<UnitPackageDto> Packages { get; set; } = new List<UnitPackageDto>();

    bool _clearable;
    private MudForm form;
    public List<UnitPackageDto> selectedPackageDto { get; set; } = new List<UnitPackageDto>();
    private bool IsCustom { get; set; } = false;
    private OrganizationDto OrganizationDto { get; set; }

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

    /// <summary>
    /// Initializes the component and loads the list of organizations.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        var result = await OrganizationAppService.GetListAsync(new PagedAndSortedResultRequestDto());
        Organizations = result.Items;
        OrganizationDto = await OrganizationAppService.GetOrganizationForCurrentTenatAsync(); 
        await base.OnInitializedAsync();
    }

    /// <summary>
    /// Submits the package assignment to the customer.
    /// </summary>
    private async Task SubmitEdit()
    {
        var result = await CustomerUnitPackageAppService.IfCustomerHasMultiplePackageAsync(Customer.Id);
        if (result)
        {
            SnackBar.ShowSnackbar(Snackbar, L["CustomerHasActivePackage"],Severity.Error);
            return;
        }

        if (IsCustom)
        {
            _createUpdateUnitPackageDto.Package.EnglishName = "Custom Package";
            _createUpdateUnitPackageDto.Package.ArabicName = "باقة مخصصة";
            _createUpdateUnitPackageDto.Package.Description = "Custom Package";
            _createUpdateUnitPackageDto.Package.PackageStatus = SubscriptionStatus.Active;
            _createUpdateUnitPackageDto.Package.CustomerCategory = CustomerCategoryStatus.None;
            _createUpdateUnitPackageDto.Package.Status = Status.Active;
            _createUpdateUnitPackageDto.Package.PackageCategoryType = PackageCategoryTypes.CustomPackage;
            _createUpdateUnitPackageDto.Package.OrganizationId = OrganizationDto.Id;
            var packageDto = await PackageAppService.CreateAsync(_createUpdateUnitPackageDto);
            preAssignState.UnitPackage = packageDto;
        }
        else
        {
            var package = selectedPackageDto.FirstOrDefault();
            preAssignState.UnitPackage = package;
        }
        preAssignState.Customer = Customer;
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




}
