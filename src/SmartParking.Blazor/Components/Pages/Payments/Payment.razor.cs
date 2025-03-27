using Microsoft.Extensions.Localization;
using MudBlazor;
using Microsoft.AspNetCore.Authorization;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.SPComponents;
using SmartParking.Localization;
using SmartParking.Organizations;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Authentication;
using System.Threading.Tasks;
using System;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using SmartParking.Payments;
using Blazorise;

using SmartParking.Permissions.PaymentsPermissions;
using Microsoft.AspNetCore.Components;
using SmartParking.Cameras;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Blazor.Components.Pages.Payments;

public partial class Payment
{
    private List<MudMenuData> MenuDatas = new List<MudMenuData>();
    private int TotalItem;
    public int CurrentPage { get; set; } = 1; // Default current page value is 10
    public int RowPerPage { get; set; } = 10; // Default value is 10
    public Guid? TenantId { get; private set; }
    private string sortColumn;
    private string ValueOfMenue = "All";
    private TableData itemData = new TableData();

    private string termSearch;
    private readonly IStringLocalizer<SmartParkingResource> _localizer;

    public Payment(IStringLocalizer<SmartParkingResource> localizer)
    {
        _localizer = localizer;
    }

    protected async override Task OnInitializedAsync()
    {
        if (!await AuthorizationService.IsGrantedAsync(PaymentsPermission.Default))
        {

            Navigation.NavigateTo("/Account/AccessDenied", true);
        }
        await CreateMenue();
        await base.OnInitializedAsync();

    }
    private async Task FetchTableData(PageChangeEventArgs pageInfo)
    {

        string sort;
        if (pageInfo.SortColumn == "Id")
            sort = "CreationTime DESC";
        else
            sort = pageInfo.SortColumn;

        termSearch = pageInfo.SearchTerm;
        CurrentPage = pageInfo.PageNumber;
        RowPerPage = pageInfo.RowsPerPage;
        sortColumn =sort;

        var input = new PagedAndSortedAndSearchResultRequestDto
        {
            MaxResultCount = RowPerPage,
            SkipCount = (CurrentPage - 1) * pageInfo.RowsPerPage,
            Sorting = sortColumn,
            SearchingTerm = termSearch
        };

        // Retrieve the list of payments from the server
        PagedResultDto<PaymentDto> paymentsDto;
        if (CurrentTenant.Id == null)
            paymentsDto = await PaymentWebAppService.GetListMdkhlAsync(input, TenantId);
        else
            paymentsDto = await PaymentWebAppService.GetListOrganizationAsync(input);
        TotalItem = (int)paymentsDto.TotalCount;
        await CreateTable(paymentsDto);

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

        // Create a new search input object with pagination, sorting, and search term parameters
        var input = new PagedAndSortedAndSearchResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn,
            SearchingTerm = termSearch // The term to use for filtering results
        };

        // Call the service to perform the search based on the input parameters
        PagedResultDto<PaymentDto> pagedResultDto;
        if (CurrentTenant.Id == null)
        {
            pagedResultDto = await PaymentWebAppService.GetListMdkhlAsync(input , null);
        }
        else
        {
            pagedResultDto = await PaymentWebAppService.GetListOrganizationAsync(input);
        }

        // Calculate the total number of pages required to display all results
        TotalItem = (int)pagedResultDto.TotalCount;
        await CreateTable(pagedResultDto);
    }
    private async Task CreateMenue()
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

        MenuDatas = new List<MudMenuData>
        {
            new MudMenuData
            {
                Name = @L["Organizations"],
                Label = @L[ValueOfMenue],
                Items =itemsOrganizations
            },
        };
    }
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

        PagedResultDto<PaymentDto> paymentDto = await PaymentWebAppService.GetListMdkhlAsync(request, TenantId);

        TotalItem = (int)paymentDto.TotalCount;
        await CreateTable(paymentDto);
    }
    /// <summary>
    /// Fetches the table data for the specified page and updates the table with the retrieved data.
    /// </summary>
    /// <param name="page">The page number for which to fetch the data.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
   
    private async Task CreateTable(PagedResultDto<PaymentDto> organizations)
    {
        // Initialize table data to be displayed, setting the table's name and whether to show the header
        var tableData = new TableData()
        {
            Name = L["Payments"],
            ShowHeader = true
        };


        tableData.Columns.Add(new ColumnDef() { DisplayName = "Id", Name = "Id", IsFilterable = false, PropertyName = "Id", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Code"], Name = "PaymentNumber", IsFilterable = true, PropertyName = "PaymentNumber", IsSortable = true, TypeFilter = TypeFilter.Enum });
        if (CurrentTenant.Id == null)
        {
            if (CultureInfo.CurrentCulture.Name == "en")
            {

                tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("OrganizationName"), Name = "OrganizationName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "OrganizationEnglishName", IsFilterable = true });
            }
            else
            {
                tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("OrganizationName"), Name = "OrganizationName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "OrganizationArabicName", IsFilterable = true });

            }
        }
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["PlateNumber"], Name = "PlateNumber", IsFilterable = false, PropertyName = "Invoice.PlateNumber", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["PaymentCategory"], Name = "PaymentCategory", IsFilterable = true, PropertyName = "PaymentCategory", IsSortable = true, TypeFilter = TypeFilter.Enum });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["InvoiceId"], Name = "InvoiceId", IsFilterable = false, PropertyName = "Invoice.Id", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["PaymentMethod"], Name = "PaymentMethod", IsFilterable = true, PropertyName = "PaymentMethod", IsSortable = true, TypeFilter = TypeFilter.Enum });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["customer category"], Name = "customer category", IsFilterable = true, PropertyName = "Customers.CustomerCategory", IsSortable = true, TypeFilter = TypeFilter.Enum });
        tableData.Columns.Add(new ColumnDef() { DisplayName = $"{_localizer["Amount"]} ({_localizer["SAR"]})", Name = "Amount", IsFilterable = true, PropertyName = "Amount", IsSortable = true, TypeFilter = TypeFilter.Number });
        tableData.Columns.Add(new ColumnDef() { DisplayName = $"{_localizer["Discount"]} ({_localizer["SAR"]})", Name = "Discount", IsFilterable = true, PropertyName = "Invoice.Discount", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["CustomerName"], Name = "CustomerName", IsFilterable = false, PropertyName = "Customers.ArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["CreationTime"], Name = "CreationTime", IsFilterable = false, PropertyName = "CreationTime", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["PaymentStatus"], Name = "PaymentStatus", IsFilterable = true, PropertyName = "PaymentStatus", IsSortable = true, TypeFilter = TypeFilter.Enum });

       
        foreach (var item in organizations.Items)
        {
            
            var rowDef = new RowDef();
            rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Code"], item.PaymentNumber?.ToString()));
            if (CurrentTenant.Id == null)
            {
                if (CultureInfo.CurrentCulture.Name == "ar")
                {
                    rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("OrganizationName"), item.OrganizationArabicName));
                }
                else
                {
                    rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("OrganizationName"), item.OrganizationEnglishName));
                }
            }
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["PlateNumber"], item.InvoiceId.HasValue ? item.Invoice?.PlateNumber : @L["Pacakge"]));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["PaymentCategory"], _localizer[$"{item.PaymentCategory.ToString()}"]));
            rowDef.Cells.Add(new KeyValuePair<string, string>(
                _localizer["InvoiceId"],
                  item.InvoiceId.ToString()
            ));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["PaymentMethod"], _localizer[$"{item.PaymentMethod}"]));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["customer category"], item.Customers?.CustomerCategory.ToString() is null ? _localizer["NotCustomer"]: _localizer[$"{item.Customers?.CustomerCategory.ToString()}"]));
            rowDef.Cells.Add(new KeyValuePair<string, string>($"{_localizer["Amount"]} ({_localizer["SAR"]})", item.Amount.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>($"{_localizer["Discount"]} ({_localizer["SAR"]})", item.Invoice?.Discount.ToString()));
            if (CultureInfo.CurrentCulture.Name == "ar")
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["CustomerName"], item.Customers?.ArabicName ?? _localizer["NotCustomer"]));
            }
            else
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["CustomerName"], item.Customers?.IdentityUser?.Name ?? _localizer["NotCustomer"]));
            }
        
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["CreationTime"], item.CreationTime.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["PaymentStatus"], _localizer[$"{item.PaymentStatus.ToString()}"]));



            tableData.Rows.Add(rowDef);
        }
        itemData = tableData;
        await Task.Yield();
    }
    private async Task HandleView(RowDef rowDef)
    {
        var id = rowDef.Cells[0].Value;
        Navigation.NavigateTo($"/payments/view/{id}" , true);
    }
}
