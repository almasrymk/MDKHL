using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Currency;
using SmartParking.Invoices;
using SmartParking.Localization;
using System.Globalization;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Blazor.Components.Pages.Customer;

public partial class ViewCustomerInvoiceDialog
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; }
    [Parameter] public string CustomerId { get; set; }
    private void Cancel() => MudDialog.Cancel();

    private int TotalItem;
    public int CurrentPage { get; set; } = 1; // Default current page value is 10
    public int RowPerPage { get; set; } = 10; // Default value is 10

    private TableData itemData = new TableData();

    private string termSearch;
    private readonly IStringLocalizer<SmartParkingResource> _localizer;

    public ViewCustomerInvoiceDialog(IStringLocalizer<SmartParkingResource> localizer)
    {
        _localizer = localizer;
    }
    /// <summary>
    /// filter for cities on the server side using the city columns.
    /// </summary>
    /// <param name="columnFilter">The name of column to filter for among city.</param>
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
        // Call the service to perform the search based on the input parameters
        var invoiceDtos = await InvoiceAppService.Filter(input);

        // Calculate the total number of pages required to display all results
        TotalItem = (int)invoiceDtos.TotalCount;
        CreateTable(invoiceDtos.Items.ToList());


    }
    /// <summary>
    /// Fetches the table data for the specified page and updates the table with the retrieved data.
    /// </summary>
    /// <param name="page">The page number for which to fetch the data.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task FetchTableData(PageChangeEventArgs pageInfo)
    {
        var input = new PagedResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage
        };


        var invoiceDto = await InvoiceAppService.GetInvoiceForCustomer(Guid.Parse(CustomerId), input);
        TotalItem = (int)invoiceDto.TotalCount;
        CreateTable(invoiceDto.Items.ToList());

    }

    private void CreateTable(List<InvoiceDto> invoice)
    {
        // Initialize table data to be displayed, setting the table's name and whether to show the header
        var tableData = new TableData()
        {
            Name = "customer invoice Data",
            ShowHeader = true
        };

        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Id"], Name = "Id", IsFilterable = false, PropertyName = "Id", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["InvoiceId"], Name = "InvoiceId", IsFilterable = false, PropertyName = "InvoiceId", IsSortable = true, TypeFilter = TypeFilter.Text });
        if (CultureInfo.CurrentCulture.Name == "ar")
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["CustomerArName"], Name = "CustomerArName", IsFilterable = false, PropertyName = "CustomerArName", IsSortable = true, TypeFilter = TypeFilter.Text });
        }
        else
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["CustomerEnName"], Name = "CustomerEnName", IsFilterable = false, PropertyName = "CustomerEnName", IsSortable = true, TypeFilter = TypeFilter.Text });
        }

        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["TotalHours"], Name = "TotalHours", IsFilterable = false, PropertyName = "TotalHours", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["InvoicePrice"], Name = "InvoicePrice", IsFilterable = false, PropertyName = "InvoicePrice", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["InvoiceType"], Name = "InvoiceType", IsFilterable = false, PropertyName = "InvoiceType", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["GarageName"], Name = "GarageName", IsFilterable = false, PropertyName = "GarageName", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["PlateNumber"], Name = "PlateNumber", IsFilterable = false, PropertyName = "PlateNumber", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["InvoiceTime"], Name = "InvoiceTime", IsFilterable = false, PropertyName = "InvoiceTime", IsSortable = true, TypeFilter = TypeFilter.Text });
        foreach (var item in invoice)
        {

            var rowDef = new RowDef();
            rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["InvoiceId"], item.Id.ToString()));
            if (CultureInfo.CurrentCulture.Name == "ar")
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["CustomerArName"], item.CustomerArName));
            }
            else
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["CustomerEnName"], item.CustomerEnName));
            }

            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["TotalHours"], item.TotalHours.ToString()));
            var iconHtml = CurrencyIcon.ToHtml("icon-Saudi_Riyal_Symbolsvg", "15px");

            rowDef.Cells.Add(new KeyValuePair<string, string>(
                _localizer["InvoicePrice"],
                item.InvoicePrice != null
                    ? $"{item.InvoicePrice.ToString()} {iconHtml}"
                    : $"0 {iconHtml}"
            ));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["InvoiceType"], item.InvoiceType));
            if (CultureInfo.CurrentCulture.Name == "ar")
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["GarageName"], item.GarageArabicName));
            }
            else
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["GarageName"], item.GarageEnglishName));


            }
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["PlateNumber"], item.PlateNumber));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["InvoiceTime"], item.InvoiceTime.ToString()));


            tableData.Rows.Add(rowDef);
        }
        itemData = tableData;
    }


}
