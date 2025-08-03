using Blazorise;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.SPComponents;
using SmartParking.Cameras;
using SmartParking.Currency;
using SmartParking.Enums;
using SmartParking.Localization;
using SmartParking.Organizations;
using SmartParking.Payments;
using SmartParking.Permissions.PaymentsPermissions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Authentication;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.MultiTenancy;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

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
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["InvoiceId"], Name = "InvoiceId", IsFilterable = false, PropertyName = "Invoice.Id", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["TicketId"], Name = "TicketId", IsFilterable = false, PropertyName = "Invoice.TicketId", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["CustomerName"], Name = "CustomerName", IsFilterable = false, PropertyName = "Customers.ArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["PaymentMethod"], Name = "PaymentMethod", IsFilterable = true, PropertyName = "PaymentMethod", IsSortable = true, TypeFilter = TypeFilter.Enum });
        tableData.Columns.Add(new ColumnDef() { DisplayName = $"{_localizer["Amount"]}", Name = "Amount", IsFilterable = true, PropertyName = "Amount", IsSortable = true, TypeFilter = TypeFilter.Number });
        tableData.Columns.Add(new ColumnDef() { DisplayName = $"{_localizer["Discount"]}", Name = "Discount", IsFilterable = true, PropertyName = "Invoice.Discount", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["PaymentCategory"], Name = "PaymentCategory", IsFilterable = true, PropertyName = "PaymentCategory", IsSortable = true, TypeFilter = TypeFilter.Enum });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["customer category"], Name = "customer category", IsFilterable = true, PropertyName = "Customers.CustomerCategory", IsSortable = true, TypeFilter = TypeFilter.Enum });
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
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["CreationTime"], Name = "CreationTime", IsFilterable = false, PropertyName = "CreationTime", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["PaymentStatus"], Name = "PaymentStatus", IsFilterable = true, PropertyName = "PaymentStatus", IsSortable = true, TypeFilter = TypeFilter.Enum });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["ZatcaStatus"], Name = _localizer["ZatcaStatus"], IsFilterable = true, PropertyName = "IsSentZatca", IsSortable = true, TypeFilter = TypeFilter.Text });

        foreach (var item in organizations.Items)
        {

            var rowDef = new RowDef();

            rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Code"], item.PaymentNumber?.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["InvoiceId"], item.InvoiceId.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["TicketId"], item.Invoice?.TicketId));

            if (CultureInfo.CurrentCulture.Name == "ar")
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["CustomerName"], item.Customers?.ArabicName ?? _localizer["NotCustomer"]));
            }
            else
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["CustomerName"], item.Customers?.IdentityUser?.Name ?? _localizer["NotCustomer"]));
            }

            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["PaymentMethod"], _localizer[$"{item.PaymentMethod}"]));

            var IconHtml = CurrencyIcon.ToHtml("icon-Saudi_Riyal_Symbolsvg", "15px");
            var amount = item.Amount == 0 ? "0.0" : item.Amount.ToString();
            rowDef.Cells.Add(new KeyValuePair<string, string>($"{_localizer["Amount"]}", $"{amount} {IconHtml}"));

            var discountValue = item.Invoice?.Discount == 0 || item.Invoice?.Discount is null ? "0.0" : item.Invoice?.Discount.ToString();
            rowDef.Cells.Add(new KeyValuePair<string, string>($"{_localizer["Discount"]}", $"{discountValue} {IconHtml}"));

            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["PaymentCategory"], _localizer[$"{item.PaymentCategory.ToString()}"]));
            rowDef.Cells.Add(new KeyValuePair<string, string>(
                _localizer["customer category"],
                item.PaymentCategory == PaymentCategory.Package
                    ? (item.Customers != null
                        ? _localizer[$"{item.Customers.CustomerCategory.ToString()}"]
                        : _localizer["NotCustomer"])
                    : (string.IsNullOrWhiteSpace(item.Customers?.IdentityUser?.Name)
                        ? _localizer["NotCustomer"]
                        : _localizer["Customer"])
            ));
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
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["CreationTime"], item.CreationTime.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["PaymentStatus"], _localizer[$"{item.PaymentStatus.ToString()}"]));
            if(item.IsSentZatca)
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["ZatcaStatus"], "<span class='text-success'>"+ _localizer["Sent"] + "</span>"));
            }
            else
            {
                //var buttonHtml = $"<div class=\"mud-grid-item d-flex justify-content-start align-items-center\"><!--!--><div class=\"mud-tooltip-root mud-tooltip-inline\"><!--!--><!--!--><button type=\"button\" class=\"mud-button-root mud-icon-button mud-button mud-button-filled mud-button-filled-primary mud-button-filled-size-small mud-ripple mud-icon-button-size-small mx-1\" _bl_d9afeaff-824e-4182-86d2-e435ae722d15=\"\"><span class=\"mud-icon-button-label\"><!--!--><svg class=\"mud-icon-root mud-svg-icon mud-icon-size-small\" focusable=\"false\" viewBox=\"0 0 24 24\" aria-hidden=\"true\" role=\"img\"><!--!--><path d=\"M0 0h24v24H0z\" fill=\"none\"></path><path d=\"M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 15h-2v-6h2v6zm0-8h-2V7h2v2z\"></path></svg></span></button><!--!--><div id=\"popover-1af6513d-4c02-411c-9877-b3a27d8ec5da\" class=\"mud-popover-cascading-value\"></div></div></div>";
                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["ZatcaStatus"], "<span class='text-danger'>" + _localizer["NotSent"] + "</span>"));
            }
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
