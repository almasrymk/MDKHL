using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Pages.Customer;
using SmartParking.Customers;
using SmartParking.Cutomers;
using SmartParking.Enums;
using SmartParking.Localization;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System;
using Volo.Abp.Users;
using Volo.Abp.Application.Dtos;
using SmartParking.Blazor.Components.Pages.Customer.Vehicles;
using SmartParking.Blazor.Components.Pages.Customer.CustomerPackage;

namespace SmartParking.Blazor.Components.Pages.Employees;
/// <summary>
/// Represents the employee page.
/// </summary>
public partial class Employee
{
    private int TotalItem;
    public int CurrentPage { get; set; } = 1;
    public int RowPerPage { get; set; } = 10; 

    private TableData itemData = new TableData();

    public bool ViewAdd { get; set; } = true;

    private string termSearch;
    private DialogOptions options = new DialogOptions { CloseOnEscapeKey = true };
    private readonly IStringLocalizer<SmartParkingResource> _localizer;
    private readonly ICurrentUser _currentUser;
    /// <summary>
    /// Initializes a new instance of the <see cref="Employee"/> class.
    /// </summary>
    /// <param name="localizer"></param>
    /// <param name="currentUser"></param>
    public Employee(IStringLocalizer<SmartParkingResource> localizer, ICurrentUser currentUser)
    {
        _localizer = localizer;
        _currentUser = currentUser;
    }
    /// <summary>
    /// Initializes the component.
    /// </summary>
    /// <returns></returns>
    protected override Task OnInitializedAsync()
    {
        if (_currentUser.IsInRole(RolesType.Mdkhl.ToString()) || _currentUser.IsInRole("admin"))
        {
            ViewAdd = false;

        }
        return base.OnInitializedAsync();
    }
    /// <summary>
    /// filter for employees on the server side using the employee columns.
    /// </summary>
    /// <param name="columnFilter">The name of column to filter for among employee.</param>
    /// <returns>Returns a task that represents the asynchronous operation.</returns>
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
            var customersDto = await EmployeeAppService.FilterMDKHL(input);
            TotalItem = (int)customersDto.TotalCount;
            CreateTable(customersDto);
        }
        else
        {
            var customersDto = await EmployeeAppService.Filter(input);
            TotalItem = (int)customersDto.TotalCount;
            CreateTable(customersDto);
        }

    }
    /// <summary>
    /// Searches for employees on the server side using the employee code search term.
    /// </summary>
    /// <param name="searchTerm">The term to search for among employee.</param>
    /// <returns>Returns a task that represents the asynchronous operation.</returns>
    private async Task HandleSearch(PageChangeEventArgs pageInfo)
    {
        termSearch = pageInfo.SearchTerm;
        var input = new PagedAndSortedAndSearchResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn,
            SearchingTerm = termSearch 
        };

        if (currrentTenant.Id == null)
        {
            var customersDto = await EmployeeAppService.SearchMDKHL(input);

            TotalItem = (int)customersDto.TotalCount;
            CreateTable(customersDto);
        }
        else
        {
            var customersDto = await EmployeeAppService.Search(input);
            TotalItem = (int)customersDto.TotalCount;
            CreateTable(customersDto);
        }
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
        var input = new PagedAndSortedResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn
        };


        PagedResultDto<CustomerDto> customersDto;
        if (currrentTenant.Id == null)
            customersDto = await EmployeeAppService.GetListMdkhlAsync(input);
        else
            customersDto = await EmployeeAppService.GetListAsync(input);
        TotalItem = (int)customersDto.TotalCount;
        CreateTable(customersDto);

    }
    /// <summary>
    /// Navigates to the employee registration page.
    /// </summary>
    private void HandleAdd()
    {
        Navigation.NavigateTo("/employee/register", true);
    }
    /// <summary>
    /// Creates a table data object and populates it with the provided customers.
    /// </summary>
    /// <param name="customers"></param>
    private void CreateTable(PagedResultDto<CustomerDto> customers)
    {
        var tableData = new TableData()
        {
            Name = L["Employees"],
            ShowHeader = true
        };
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Id"], Name = "Id", IsFilterable = false, PropertyName = "Id", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["UserName"], Name = "UserName", IsFilterable = true, PropertyName = "IdentityUser.UserName", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["EnglishName"], Name = "FullName", IsFilterable = true, PropertyName = "IdentityUser.Name", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["ArabicName"], Name = "ArabicName", IsFilterable = true, PropertyName = "ArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Email"], Name = "Email", IsFilterable = true, PropertyName = "IdentityUser.Email", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["NationalId"], Name = "NationalId", IsFilterable = true, PropertyName = "NationalId", IsSortable = true, TypeFilter = TypeFilter.Enum });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["MobileNumber"], Name = "MobileNumber", IsFilterable = false, PropertyName = "MobileNumber", IsSortable = true, TypeFilter = TypeFilter.Number });
        if (currrentTenant.Id == null)
        {
            if (CultureInfo.CurrentCulture.DisplayName == "ar")

                tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["OrganizationName"], Name = "OrganizationName", IsFilterable = false, PropertyName = "CustomerPackages.Organization.ArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });
            else
                tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["OrganizationName"], Name = "OrganizationName", IsFilterable = false, PropertyName = "CustomerPackages.Organization.EnglishName", IsSortable = true, TypeFilter = TypeFilter.Text });
        }

        foreach (var item in customers.Items)
        {
            var rowDef = new RowDef();
            rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["UserName"], item.UserName));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["EnglishName"], item.FullName));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["ArabicName"], item.ArabicName));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Email"], item.Email));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["NationalId"], item.NationalId));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["MobileNumber"], item.MobileNumber));
            if (currrentTenant.Id == null)
            {
                if (CultureInfo.CurrentCulture.Name == "ar")
                    rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["OrganizationName"], item.OrganizationArabicName));
                else
                    rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["OrganizationName"], item.OrganizationEnglishName));
            }


            tableData.Rows.Add(rowDef);
        }
        itemData = tableData;
    }
    /// <summary>
    /// create vehicle for employee
    /// </summary>
    /// <param name="row"></param>
    /// <returns></returns>
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
    /// view employee's vehicles
    /// </summary>
    /// <param name="row"></param>
    /// <returns></returns>
    public async Task<IDialogReference> ShowVehiclesForCustomer(RowDef row)
    {
        var id = Guid.Parse(row.Cells[0].Value);

        var parameters = new DialogParameters
        {
            ["CustomerId"] = id,
        };


        return await DialogService.ShowAsync<EmployeeVehicles>("Simple Dialog", parameters);


    }
    /// <summary>
    /// assign package to customer 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<IDialogReference> AssignPackage(string id)
    {
        var Gid = Guid.Parse(id);

        var parameters = new DialogParameters
        {
            ["CustomerId"] = Gid,
        };

        return await DialogService.ShowAsync<AssignPackgeDialog>("Assign Package", parameters);

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
}
