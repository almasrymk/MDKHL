using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.CustomerUnitPackages;
using SmartParking.Cutomers;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using SmartParking.Organizations;
using SmartParking.UnitPackages;
using SmartParking.Units;
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
    private IReadOnlyList<UnitDto> Units { get; set; } = new List<UnitDto>();
    private string unitCode;
    private CustomerUnitPackageType customerUnitPackageType;

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
    }
    private void GetType(string value)
    {
        customerUnitPackageType = (CustomerUnitPackageType)Enum.Parse(typeof(CustomerUnitPackageType), value);
    }

    /// <summary>
    /// Initializes the component and loads the list of organizations.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        var result = await OrganizationAppService.GetListAsync(new PagedAndSortedResultRequestDto());
        Organizations = result.Items;
        var unitResult = await UnitAppService.GetListAsync(new PagedAndSortedResultRequestDto());
        Units = unitResult.Items;
        await base.OnInitializedAsync();
    }

    /// <summary>
    /// Submits the package assignment to the customer.
    /// </summary>
    private async Task SubmitEdit()
    {
        var result = await CustomerUnitPackageAppService.IfCustomerHasMultiplePackageAsync(Customer.Id);
        //if (result)
        //{
        //    SnackBar.ShowSnackbar(Snackbar, L["CustomerHasActivePackage"],Severity.Error);
        //    return;
        //}
        var package = selectedPackageDto.FirstOrDefault();

        if (package is null) 
        {
            Snackbar.Add(@L["PackageSelectValidation"], Severity.Error);
            return;
        }

        if (unitCode is null)
        {
            Snackbar.Add(@L["CodeSelectValidation"], Severity.Error);
            return;
        }

        preAssignState.UnitPackage=package;
        preAssignState.Customer = Customer;
        preAssignState.UnitCode = unitCode;
        preAssignState.CustomerUnitPackageType = customerUnitPackageType;
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
