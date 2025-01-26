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

namespace SmartParking.Blazor.Components.Pages.Customer;

public partial class Customer
{
    private List<MudMenuData> MenuDatas = new List<MudMenuData>();
    private int TotalItem;
    private SPDataGrid sPDataGrid;
    private bool assign = true;
    private string sortColumn;
    private Guid? TenantId;
    private string ValueOfMenue = "All";
    public int CurrentPage { get; set; } = 1; // Default current page value is 10
    public int RowPerPage { get; set; } = 10; // Default value is 10

    private TableData itemData = new TableData();

    public bool ViewAdd { get; set; } = true;

    private string termSearch;
    private readonly IStringLocalizer<SmartParkingResource> _localizer;

    public Customer(IStringLocalizer<SmartParkingResource> localizer, ICurrentUser currentUser)
    {
        sPDataGrid = new SPDataGrid(localizer);
        _localizer = localizer;
        CreatePolicyName = CustomerPermissions.Create;
        UpdatePolicyName = CustomerPermissions.Edit;
        DeletePolicyName = CustomerPermissions.Delete;
    }

    protected async override Task OnInitializedAsync()
    {
        await CreateMenue();
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

    private async Task CreateMenue()
    {
        var itemsOrganizations = new List<MenuItem>();
        var organizations = await OrganizationAppService.GetListAsync(new PagedAndSortedResultRequestDto());
        itemsOrganizations.Add(new MenuItem { MenueName = "Organization", Label = @L["All"], OnClick = EventCallback.Factory.Create<string>(this, async (string selectedValue) => await HandleSelectTenantAsync(ValueOfMenue,null)) });
        if (CultureInfo.CurrentCulture.Name == "ar")
        {
            foreach (var org in organizations.Items)
            {
                itemsOrganizations.Add(new MenuItem { MenueName = "Organization", Label = org.ArabicName, OnClick = EventCallback.Factory.Create<string>(this, async (string selectedValue) => await HandleSelectTenantAsync(ValueOfMenue,org.TenantId)) });
            }
        }
        else
        {
            foreach (var org in organizations.Items)
            {
                itemsOrganizations.Add(new MenuItem { MenueName = "Organization", Label = org.EnglishName, OnClick = EventCallback.Factory.Create<string>(this, async (string selectedValue) => await HandleSelectTenantAsync(ValueOfMenue,org.TenantId)) });
            }
        }

        MenuDatas = new List<MudMenuData>
        {
            new MudMenuData
            {
                Name = "Customer",
            Label = @L[ValueOfMenue],
                Items = new List<MenuItem>
                {
                    new MenuItem { MenueName = "Customer", Label = @L["All"], OnClick = EventCallback.Factory.Create<string>(this, async (string selectedValue) => await HandleSelectAsync("All"))   },
                    new MenuItem {MenueName = "Customer", Label = @L["Beneficiaries"], OnClick =EventCallback.Factory.Create<string>(this, async (string selectedValue) => await HandleSelectAsync("Beneficiaries"))   },
                    new MenuItem { MenueName = "Customer",Label = @L["Employees"], OnClick = EventCallback.Factory.Create<string>(this, async (string selectedValue) => await  HandleSelectAsync("Employees"))   },
                    new MenuItem { MenueName = "Customer",Label = @L["Visitors"], OnClick =  EventCallback.Factory.Create<string>(this, async (string selectedValue) => await HandleSelectAsync("Visitors")) },
                    new MenuItem {MenueName = "Customer", Label = @L["NoPackages"], OnClick = EventCallback.Factory.Create<string>(this, async (string selectedValue) => await HandleSelectAsync("NoPackages")) },
                }
            },
            new MudMenuData
            {
                Name = "Organization",
            Label = @L[ValueOfMenue],
                Items =itemsOrganizations
            },
        };
    }

    /// <summary>
    /// filter for hotels on the server side using the hotel columns.
    /// </summary>
    /// <param name="columnFilter">The name of column to filter for among hotel.</param>
    /// <returns>Returns a task that represents the asynchronous operation.</returns>
    private async Task HandleFilter(PageChangeEventArgs pageInfo)
    {
        // Log the search term to the console for debugging purposes

        // Create a new search input object with pagination, sorting, and search term parameters
        var input = new PagedAndSortedAndFilteredResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn,
            SearchingTerm = termSearch, // The term to use for filtering results
            filters = pageInfo.Filters
        };
        if (currrentTenant.Id == null)
        {
            var customersDto = await CustomerAppService.FilterMDKHL(input);

            TotalItem = (int)customersDto.TotalCount;
            CreateTable(customersDto);
        }
        else
        {

            var customersDto = await CustomerAppService.Filter(input);

            TotalItem = (int)customersDto.TotalCount;
            CreateTable(customersDto);
        }

    }
    /// <summary>
    /// Searches for hotels on the server side using the hotel code search term.
    /// </summary>
    /// <param name="searchTerm">The term to search for among hotel.</param>
    /// <returns>Returns a task that represents the asynchronous operation.</returns>
    private async Task HandleSearch(PageChangeEventArgs pageInfo)
    {

        // Update the term used for searching with the provided search term
        termSearch = pageInfo.SearchTerm;
        CurrentPage = pageInfo.PageNumber;
        RowPerPage = pageInfo.RowsPerPage;
        sortColumn = pageInfo.SortColumn;
        // Create a new search input object with pagination, sorting, and search term parameters
        var input = new PagedAndSortedAndSearchResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn,
            SearchingTerm = termSearch // The term to use for filtering results
        };

        if (currrentTenant.Id == null)
        {
            var customersDto = await CustomerAppService.SearchMDKHL(input);

            TotalItem = (int)customersDto.TotalCount;
            CreateTable(customersDto);
        }
        else
        {

            var customersDto = await CustomerAppService.Search(input);

            TotalItem = (int)customersDto.TotalCount;
            CreateTable(customersDto);
        }
        //await HandleSelectAsync("All");
    }
    /// <summary>
    /// Fetches the table data for the specified page and updates the table with the retrieved data.
    /// </summary>
    /// <param name="page">The page number for which to fetch the data.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task FetchTableData(PageChangeEventArgs pageInfo)
    {

        CurrentPage = pageInfo.PageNumber;
        RowPerPage = pageInfo.RowsPerPage;
        sortColumn = pageInfo.SortColumn;
        // Create a request object for paged and sorted data
        var input = new PagedAndSortedAndSearchResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn
        };

        // Retrieve the list of organizations from the server
        PagedResultDto<CustomerDto> customersDto;
        if (currrentTenant.Id == null)
            customersDto = await CustomerAppService.GetListMdkhlAsync(input,null);
        else
            customersDto = await CustomerAppService.GetListAsync(input);
        TotalItem = (int)customersDto.TotalCount;
        CreateTable(customersDto);

    }

    private void HandleAdd()
    {
        Navigation.NavigateTo("/customers/register", true);
    }

    private void CreateTable(PagedResultDto<CustomerDto> customers)
    {
        // Initialize table data to be displayed, setting the table's name and whether to show the header
        var tableData = new TableData()
        {
            Name = "Customers",
            ShowHeader = true
        };


        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Id"], Name = "Id", IsFilterable = false, PropertyName = "Id", IsSortable = true, TypeFilter = TypeFilter.Text });
        //tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["UserName"], Name = "UserName", IsFilterable = true, PropertyName = "IdentityUser.UserName", IsSortable = true, TypeFilter = TypeFilter.Text });
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
            //rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["UserName"], item.UserName));
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
    }

    public async Task<IDialogReference> CreateVehicle(RowDef row)
    {
        var id = Guid.Parse(row.Cells[0].Value);

        var parameters = new DialogParameters
        {
            ["CustomerId"] = id,
        };


        return await DialogService.ShowAsync<AddVehicleDialog>("Simple Dialog", parameters);


    }
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

    public void ViewCustomer(RowDef row)
    {
        var customerId = Guid.Parse(row.Cells[0].Value);

        Navigation.NavigateTo($"/customers/view/{customerId}", true);


    }

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

    public void HandleEdit(RowDef row)
    {
        var customerId = Guid.Parse(row.Cells[0].Value);

        Navigation.NavigateTo($"/customers/edit/{customerId}", true);
    }

    public async Task HandleSelectAsync(string selectedValue)
    {
        ValueOfMenue = selectedValue;
        var request = new PagedAndSortedAndSearchResultRequestDto
        {
            MaxResultCount = RowPerPage,
            SkipCount = (CurrentPage - 1) * RowPerPage,
            Sorting = sortColumn,
            SearchingTerm = sPDataGrid._searchString
        };

        PagedResultDto<CustomerDto> customers = await GetCustomersAsync(request, selectedValue);

        TotalItem = (int)customers.TotalCount;
        CreateTable(customers);
    }
    
    public async Task HandleSelectTenantAsync(string selectedValue,Guid? tenantId)
    {
        TenantId = tenantId;
        ValueOfMenue = selectedValue;
        var request = new PagedAndSortedAndSearchResultRequestDto
        {
            MaxResultCount = RowPerPage,
            SkipCount = (CurrentPage - 1) * RowPerPage,
            Sorting = sortColumn,
            SearchingTerm = sPDataGrid._searchString
        };

        PagedResultDto<CustomerDto> customers = await GetCustomersAsync(request, selectedValue, TenantId);

        TotalItem = (int)customers.TotalCount;
        CreateTable(customers);
    }

    private async Task<PagedResultDto<CustomerDto>> GetCustomersAsync(
        PagedAndSortedAndSearchResultRequestDto request,
        string selectedValue,
        Guid? tenantId = null)
    {
        if (currrentTenant.Id == null)
        {
            return await GetCustomersForMdkhlAsync(request, selectedValue,tenantId);
        }
        else
        {
            var organizationId = (await OrganizationAppService.GetOrganizationForCurrentTenatAsync()).Id;
            return await GetCustomersForOrganizationAsync(request, selectedValue, organizationId);
        }
    }

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

    private async Task<PagedResultDto<CustomerDto>> GetCustomersForOrganizationAsync(
        PagedAndSortedAndSearchResultRequestDto request,
        string selectedValue,
        int organizationId)
    {
        return selectedValue switch
        {
            "All" => await CustomerAppService.GetListAsync(request),
            "Beneficiaries" => await CustomerAppService.GetListForCustomerCategoryAsync(request, CustomerCategoryStatus.Customer, organizationId,null),
            "Employees" => await CustomerAppService.GetListForCustomerCategoryAsync(request, CustomerCategoryStatus.Employee, organizationId, null),
            "Visitors" => await CustomerAppService.GetListForCustomerCategoryAsync(request, CustomerCategoryStatus.Visitor, organizationId, null),
            "NoPackages" => await CustomerAppService.GetListNoPackageAsync(request, organizationId, null),
            _ => throw new ArgumentException("Invalid selected value", nameof(selectedValue))
        };
    }

}
