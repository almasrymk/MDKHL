using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Pages.Garages;
using SmartParking.Garages;
using SmartParking.Localization;
using SmartParking.OrganizationsGarages;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Volo.Abp.Application.Dtos;
using SmartParking.Cutomers;
using Volo.Abp.Users;
using SmartParking.Enums;
using System.Globalization; 
using SmartParking.Blazor.Components.Pages.Customer.Vehicles;
using SmartParking.Blazor.Components.Pages.Customer.CustomerPackage;
using SmartParking.Permissions.PackagePermissions;
using SmartParking.Permissions.CustomerPermissions;
using SmartParking.Blazor.Components.Pages.User;
using SmartParking.Blazor.Components.SPComponents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Volo.Abp.Identity;
using iText.Commons.Utils;
using Microsoft.AspNetCore.Components.Web;
using SmartParking.Utilities;
using SmartParking.Blazor.Components.Shared;
using Microsoft.JSInterop;
using System.Data;
using SmartParking.Permissions.Reports;
using Org.BouncyCastle.Crypto.Digests;
using SmartParking.Helpers;
using SmartParking.Results;

namespace SmartParking.Blazor.Components.Pages.Customer;

/// <summary>
/// Represents the customer management page, where customers can be filtered, added, updated, and viewed.
/// This class handles the display and filtering of customer data in a paginated grid and manages user permissions for different actions.
/// </summary>
public partial class Customer
{
    private SPDataGrid DataGridComponent { get; set; }
    private List<MudMenuData> MenuDatas = new List<MudMenuData>();
    private int TotalItem;
    private bool assign = true;
    private string sortColumn;
    private Guid? TenantId = null;
    private string ValueOfMenue = "All";
    public int CurrentPage { get; set; } = 1;
    public int RowPerPage { get; set; } = 10;
    private TableData itemData = new TableData();
    public bool ViewAdd { get; set; } = true;
    private string termSearch;
    private readonly IStringLocalizer<SmartParkingResource> _localizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="Customer"/> class.
    /// </summary>
    /// <param name="localizer">The localized string resource provider.</param>
    /// <param name="currentUser">The current user context for checking permissions.</param>
    public Customer(IStringLocalizer<SmartParkingResource> localizer, ICurrentUser currentUser)
    {
        _localizer = localizer;
        CreatePolicyName = CustomerPermissions.Create;
        UpdatePolicyName = CustomerPermissions.Edit;
        DeletePolicyName = CustomerPermissions.Delete;
    }

    /// <summary>
    /// Initializes the component and checks user permissions and tenant assignment.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    protected async override Task OnInitializedAsync()
    {
        if (AuthorizationService.IsGrantedAnyAsync(ReportsPermissions.IT).Result || currrentTenant.Id is null)
        {
            await CreateMenue();
        }
        if (!await AuthorizationService.IsGrantedAsync(CustomerPermissions.Default))
        {
            Navigation.NavigateTo("/Account/AccessDenied", true);
        }
        if (currrentTenant.Id is null)
        {
            assign = false;
            ViewAdd = false;
        }
        await base.OnInitializedAsync();
    }

    /// <summary>
    /// Creates the menu options based on tenant assignment and user permissions.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task CreateMenue()
    {
        var MenuDatasMdkhl = new List<MudMenuData>
        {
            CreatePackageMenue(),
            await CreateOrganizationMenue(),
        };

        var MenuDatasOrganization = new List<MudMenuData>
        {
            CreatePackageMenue(),
        };

        if (currrentTenant.Id is null)
        {
            MenuDatas = MenuDatasMdkhl;
        }
        else
        {
            MenuDatas = MenuDatasOrganization;
        }
    }

    /// <summary>
    /// Creates the menu for organizations based on the available organizations and their names.
    /// </summary>
    /// <returns>A task representing the asynchronous operation that returns the organization menu.</returns>
    private async Task<MudMenuData> CreateOrganizationMenue()
    {
        var itemsOrganizations = new List<MenuItem>();
        var organizations = await OrganizationAppService.GetListAsync(new PagedAndSortedResultRequestDto());
        itemsOrganizations.Add(new MenuItem { MenueName = "Organization", Label = @L["All"], OnClick = EventCallback.Factory.Create<string>(this, async (string selectedValue) => await HandleSelectTenantAsync(ValueOfMenue, null)) });
        if (CultureInfo.CurrentCulture.Name == "ar")
        {
            foreach (var org in organizations.Items)
            {
                itemsOrganizations.Add(new MenuItem { MenueName = "Organization", Label = org.ArabicName, OnClick = EventCallback.Factory.Create<string>(this, async (string selectedValue) => await HandleSelectTenantAsync(ValueOfMenue, org.TenantId)) });
            }
        }
        else
        {
            foreach (var org in organizations.Items)
            {
                itemsOrganizations.Add(new MenuItem { MenueName = "Organization", Label = org.EnglishName, OnClick = EventCallback.Factory.Create<string>(this, async (string selectedValue) => await HandleSelectTenantAsync(ValueOfMenue, org.TenantId)) });
            }
        }

        return new MudMenuData
        {
            Name = L["Organizations"],
            Label = L[ValueOfMenue],
            Items = itemsOrganizations
        };
    }

    /// <summary>
    /// Creates the package menu with various package types.
    /// </summary>
    /// <returns>The package menu data containing various package options.</returns>
    private MudMenuData CreatePackageMenue()
    {
        return new MudMenuData
        {
            Name = "Package",
            Label = @L[ValueOfMenue],
            Items = new List<MenuItem>
            {
                new MenuItem { MenueName = "Package", Label = @L["All"], OnClick = EventCallback.Factory.Create<string>(this, async (string selectedValue) => await HandleSelectAsync("All"))   },
                new MenuItem { MenueName = "Package", Label = @L["Beneficiaries"], OnClick =EventCallback.Factory.Create<string>(this, async (string selectedValue) => await HandleSelectAsync("Beneficiaries"))   },
                new MenuItem { MenueName = "Package", Label = @L["Employees"], OnClick = EventCallback.Factory.Create<string>(this, async (string selectedValue) => await  HandleSelectAsync("Employees"))   },
                new MenuItem { MenueName = "Package", Label = @L["Visitors"], OnClick =  EventCallback.Factory.Create<string>(this, async (string selectedValue) => await HandleSelectAsync("Visitors")) },
                new MenuItem { MenueName = "Package", Label = @L["NoPackages"], OnClick = EventCallback.Factory.Create<string>(this, async (string selectedValue) => await HandleSelectAsync("NoPackages")) },
            }
        };
    }

    /// <summary>
    /// Filters customer data based on specified parameters, including page information, sorting, and search term.
    /// </summary>
    /// <param name="pageInfo">The page information that contains sorting, filtering, and paging details.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task HandleFilter(PageChangeEventArgs pageInfo)
    {
        var input = new PagedAndSortedAndFilteredResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn,
            SearchingTerm = termSearch,
            filters = pageInfo.Filters
        };

        if (currrentTenant.Id == null)
        {
            var customersDto = await CustomerAppService.FilterMDKHL(input);
            TotalItem = (int)customersDto.TotalCount;
            await CreateTable(customersDto);
        }
        else
        {
            var customersDto = await CustomerAppService.Filter(input);
            TotalItem = (int)customersDto.TotalCount;
            await CreateTable(customersDto);
        }
    }


    /// <summary>
    /// Fetches the table data for the specified page and updates the table with the retrieved data.
    /// </summary>
    /// <param name="page">The page number for which to fetch the data.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task FetchTableData(PageChangeEventArgs pageInfo)
    {
        termSearch = pageInfo.SearchTerm;
        CurrentPage = pageInfo.PageNumber;
        RowPerPage = pageInfo.RowsPerPage;
        sortColumn = pageInfo.SortColumn;
        var input = new PagedAndSortedAndSearchResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn,
            SearchingTerm = termSearch
        };
        PagedResultDto<CustomerDto> customersDto;
        if (currrentTenant.Id == null)
            customersDto = await CustomerAppService.GetListMdkhlAsync(input, null);
        else
            customersDto = await CustomerAppService.GetListAsync(input);
        TotalItem = (int)customersDto.TotalCount;
        await CreateTable(customersDto);

    }
    /// <summary>
    /// Navigates to the customer registration page.
    /// </summary>
    private void HandleAdd()
    {
        Navigation.NavigateTo("/customers/register", true);
    }

    /// <summary>
    /// Creates and populates a dynamic table with customer data.
    /// </summary>
    /// <param name="customers">Paged list of customer DTOs.</param>
    private async Task CreateTable(PagedResultDto<CustomerDto> customers)
    {

        var tableData = new TableData()
        {
            Name = L[ "Customers"],
            ShowHeader = true
        };
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Id"], Name = "Id", IsFilterable = false, PropertyName = "Id", IsSortable = true, TypeFilter = TypeFilter.Text });

        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["EnglishName"], Name = "FullName", IsFilterable = true, PropertyName = "IdentityUser.Name", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["ArabicName"], Name = "ArabicName", IsFilterable = true, PropertyName = "ArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Email"], Name = "Email", IsFilterable = true, PropertyName = "IdentityUser.Email", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["NationalId"], Name = "NationalId", IsFilterable = false, PropertyName = "NationalId", IsSortable = true, TypeFilter = TypeFilter.Enum });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["MobileNumber"], Name = "MobileNumber", IsFilterable = false, PropertyName = "IdentityUser.PhoneNumber", IsSortable = true, TypeFilter = TypeFilter.Number });
        if (CultureInfo.CurrentCulture.Name == "ar")
        {

            tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Package"], Name = "Package", IsFilterable = false, PropertyName = "CustomerPackages.PackageArabicName", IsSortable = false, TypeFilter = TypeFilter.Text });
        }
        else
        {

            tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Package"], Name = "Package", IsFilterable = false, PropertyName = "CustomerPackages.PackageEnglishName", IsSortable = false, TypeFilter = TypeFilter.Text });
        }
        if (CurrentTenant.Id == null)
        {
            if (CultureInfo.CurrentCulture.Name == "ar")
            {

                tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["OrganizationName"], Name = "OrganizationName", IsFilterable = false, PropertyName = "CustomerPackages.Organization.ArabicName", IsSortable = false, TypeFilter = TypeFilter.Text });
            }

            else
            {

                tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["OrganizationName"], Name = "OrganizationName", IsFilterable = false, PropertyName = "CustomerPackages.Organization.EnglishName", IsSortable = false, TypeFilter = TypeFilter.Text });
            }

        }

        foreach (var item in customers.Items)
        {
            var rowDef = new RowDef();
            rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["EnglishName"], item.FullName));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["ArabicName"], item.ArabicName));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Email"], item.Email));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["NationalId"], item.NationalId));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["MobileNumber"], item.MobileNumber));
            if (CultureInfo.CurrentCulture.Name == "ar")
            {

                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Package"], item.PackageArabicNames.IsNullOrWhiteSpace() ? _localizer["NoPackages"] : item.PackageArabicNames));

            }
            else
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Package"], item.PackageEnglishNames.IsNullOrWhiteSpace() ? _localizer["NoPackages"] : item.PackageEnglishNames));

            }
            if (currrentTenant.Id == null)
            {
                if (CultureInfo.CurrentCulture.Name == "ar")
                {
                    rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["OrganizationName"], item.OrganizationArabicName.IsNullOrWhiteSpace() ? _localizer["NoOrganization"] : item.OrganizationArabicName));
                }
                else
                {
                    rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["OrganizationName"], item.OrganizationEnglishName.IsNullOrWhiteSpace() ? _localizer["NoOrganization"] : item.OrganizationEnglishName));
                }
            }


            tableData.Rows.Add(rowDef);
        }
        itemData = tableData;
        await Task.Yield();
    }


    /// <summary>
    /// Opens a dialog for creating a vehicle associated with a customer.
    /// </summary>
    /// <param name="row">The selected customer row.</param>
    /// <returns>The dialog reference.</returns>
    public async Task<IDialogReference> CreateVehicle(RowDef row)
    {
        var id = Guid.Parse(row.Cells[0].Value);

        var parameters = new DialogParameters
        {
            ["CustomerId"] = id,
        };

        return await DialogService.ShowAsync<AddVehicleDialog>("Simple Dialog", parameters);
    }

    /// <summary>
    /// Opens a dialog to assign a role to a user.
    /// </summary>
    /// <param name="row">The row string containing the customer ID.</param>
    /// <returns>The dialog reference.</returns>
    public async Task<IDialogReference> AssignRole(string row)
    {
        var id = Guid.Parse(row);
        var customer = await CustomerAppService.GetAsync(id);

        var parameters = new DialogParameters
        {
            ["Id"] = customer.IdentityId,
        };

        return await DialogService.ShowAsync<AssignationDialog>("Simple Dialog", parameters);
    }

    /// <summary>
    /// Navigates to the customer details view page.
    /// </summary>
    /// <param name="row">The selected customer row.</param>
    public void ViewCustomer(RowDef row)
    {
        var customerId = Guid.Parse(row.Cells[0].Value);
        Navigation.NavigateTo($"/customers/view/{customerId}", true);
    }

    /// <summary>
    /// Downloads an Excel report of a customer's garage transactions.
    /// </summary>
    /// <param name="row">The selected customer row.</param>
    public async Task ShowCustomerTransactions(RowDef row)
    {
        var customerId = Guid.Parse(row.Cells[0].Value);
        var garageTransactionAnalysis = await AnalysisAppService.ReportGarageTransactionCustomerIdAsync(customerId, CurrentTenant.Id);
        var data = await CreateTableHelper.CreateDataTableForGarageTransactionCustomer(garageTransactionAnalysis, CurrentTenant.Id);
        await DownloadExcel(data, row.Cells[1].Value);
    }
    /// <summary>
    /// Opens a dialog for assigning a package to a customer by ID.
    /// </summary>
    /// <param name="id">The customer ID as a string.</param>
    /// <returns>The dialog reference for the assign package dialog.</returns>
    public async Task<IDialogReference> AssignPackage(string id)
    {
        var Gid = Guid.Parse(id);
        var customer = await CustomerAppService.GetAsync(Gid);
        var parameters = new DialogParameters
        {
            ["Customer"] = customer,
        };

        return await DialogService.ShowAsync<AssignPackgeDialog>("Assign Package", parameters);
    }

    /// <summary>
    /// Navigates to the edit page for the selected customer.
    /// </summary>
    /// <param name="row">The row data containing the customer ID.</param>
    public void HandleEdit(RowDef row)
    {
        var customerId = Guid.Parse(row.Cells[0].Value);
        Navigation.NavigateTo($"/customers/edit/{customerId}", true);
    }

    /// <summary>
    /// Handles customer category selection and updates the displayed customer list.
    /// </summary>
    /// <param name="selectedValue">The selected category value.</param>
    public async Task HandleSelectAsync(string selectedValue)
    {
        ValueOfMenue = selectedValue;
        var request = new PagedAndSortedAndSearchResultRequestDto
        {
            MaxResultCount = RowPerPage,
            SkipCount = (CurrentPage - 1) * RowPerPage,
            Sorting = sortColumn,
            SearchingTerm = termSearch
        };

        PagedResultDto<CustomerDto> customers = await GetCustomersAsync(request, selectedValue);
        TotalItem = (int)customers.TotalCount;
        await CreateTable(customers);
    }

    /// <summary>
    /// Downloads an Excel report for customer transaction data.
    /// </summary>
    /// <param name="Data">The table data to export.</param>
    /// <param name="Name">The name of the customer for the exported file.</param>
    public async Task DownloadExcel(TableData Data, string Name)
    {
        var folderName = FileHelper.CreateImageDirectory(PathHelper.ReportsFolder);
        string file = System.IO.Path.Combine(folderName, $"{Name} Transactions.xlsx");
        await GenerateFilesAppService.ExportToExcel(Data, file);

        SnackBar.ShowSnackbar(Snackbar, LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.SUCCESS) + " " +
                    LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.EXPORT), Severity.Success);

        await JSRuntime.InvokeVoidAsync("window.open", $"{PathHelper.ReportsFolder}/{Name} Transactions.xlsx", "_blank");
    }

    /// <summary>
    /// Handles tenant selection and updates the customer list accordingly.
    /// </summary>
    /// <param name="selectedValue">The selected customer category value.</param>
    /// <param name="tenantId">The selected tenant ID.</param>
    public async Task HandleSelectTenantAsync(string selectedValue, Guid? tenantId)
    {
        TenantId = tenantId;
        ValueOfMenue = selectedValue;

        var request = new PagedAndSortedAndSearchResultRequestDto
        {
            MaxResultCount = RowPerPage,
            SkipCount = (CurrentPage - 1) * RowPerPage,
            Sorting = sortColumn,
            SearchingTerm = termSearch
        };

        PagedResultDto<CustomerDto> customers = await GetCustomersAsync(request, selectedValue, TenantId);
        TotalItem = (int)customers.TotalCount;
        await CreateTable(customers);
    }

    /// <summary>
    /// Retrieves customers based on the tenant and selected category.
    /// </summary>
    /// <param name="request">The paged, sorted, and searched request parameters.</param>
    /// <param name="selectedValue">The selected category.</param>
    /// <param name="tenantId">Optional tenant ID (used only for super admin).</param>
    /// <returns>A paged result of customer DTOs.</returns>
    private async Task<PagedResultDto<CustomerDto>> GetCustomersAsync(
        PagedAndSortedAndSearchResultRequestDto request,
        string selectedValue,
        Guid? tenantId = null)
    {
        if (currrentTenant.Id == null)
        {
            return await GetCustomersForMdkhlAsync(request, selectedValue, tenantId);
        }
        else
        {
            var organizationId = (await OrganizationAppService.GetOrganizationForCurrentTenatAsync()).Id;
            return await GetCustomersForOrganizationAsync(request, selectedValue, organizationId);
        }
    }

    /// <summary>
    /// Retrieves customers for the super admin (Mdkhl) based on category and tenant.
    /// </summary>
    /// <param name="request">The paged, sorted, and searched request parameters.</param>
    /// <param name="selectedValue">The selected customer category.</param>
    /// <param name="tenantId">Optional tenant ID.</param>
    /// <returns>A paged result of customer DTOs.</returns>
    private async Task<PagedResultDto<CustomerDto>> GetCustomersForMdkhlAsync(
        PagedAndSortedAndSearchResultRequestDto request,
        string selectedValue,
        Guid? tenantId = null)
    {
        return selectedValue switch
        {
            "All" => await CustomerAppService.GetListMdkhlAsync(request, tenantId),
            "Beneficiaries" => await CustomerAppService.GetListForCustomerCategoryAsync(request, CustomerCategoryStatus.Customer, null, tenantId),
            "Employees" => await CustomerAppService.GetListForCustomerCategoryAsync(request, CustomerCategoryStatus.Employee, null, tenantId),
            "Visitors" => await CustomerAppService.GetListForCustomerCategoryAsync(request, CustomerCategoryStatus.Visitor, null, tenantId),
            "NoPackages" => await CustomerAppService.GetListNoPackageAsync(request, null, tenantId),
            _ => throw new ArgumentException("Invalid selected value", nameof(selectedValue))
        };
    }

    /// <summary>
    /// Retrieves customers for a specific organization based on category.
    /// </summary>
    /// <param name="request">The paged, sorted, and searched request parameters.</param>
    /// <param name="selectedValue">The selected customer category.</param>
    /// <param name="organizationId">The organization ID.</param>
    /// <returns>A paged result of customer DTOs.</returns>
    private async Task<PagedResultDto<CustomerDto>> GetCustomersForOrganizationAsync(
        PagedAndSortedAndSearchResultRequestDto request,
        string selectedValue,
        int organizationId)
    {
        return selectedValue switch
        {
            "All" => await CustomerAppService.GetListAsync(request),
            "Beneficiaries" => await CustomerAppService.GetListForCustomerCategoryAsync(request, CustomerCategoryStatus.Customer, organizationId, null),
            "Employees" => await CustomerAppService.GetListForCustomerCategoryAsync(request, CustomerCategoryStatus.Employee, organizationId, null),
            "Visitors" => await CustomerAppService.GetListForCustomerCategoryAsync(request, CustomerCategoryStatus.Visitor, organizationId, null),
            "NoPackages" => await CustomerAppService.GetListNoPackageAsync(request, organizationId, null),
            _ => throw new ArgumentException("Invalid selected value", nameof(selectedValue))
        };
    }

}
