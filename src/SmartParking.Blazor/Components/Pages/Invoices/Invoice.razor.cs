using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Localization;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Volo.Abp.Application.Dtos;
using SmartParking.Invoices;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace SmartParking.Blazor.Components.Pages.Invoices;

public partial class Invoice
{
    private int TotalItem;
    public int CurrentPage { get; set; } = 1; // Default current page value is 10
    public int RowPerPage { get; set; } = 10; // Default value is 10

    private TableData itemData = new TableData();

    private string termSearch;

    private DialogOptions options = new DialogOptions { CloseOnEscapeKey = true };

    private readonly IStringLocalizer<SmartParkingResource> _localizer;
    public Invoice(IStringLocalizer<SmartParkingResource> localizer)
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
        var regionDto = await AppService.Filter(input);

        // Calculate the total number of pages required to display all results
        TotalItem = (int)regionDto.TotalCount;
        CreateTable(regionDto);


    }

    /// <summary>
    /// Searches for hotels on the server side using the hotel code search term.
    /// </summary>
    /// <param name="searchTerm">The term to search for among hotel.</param>
    /// <returns>Returns a task that represents the asynchronous operation.</returns>
    private async Task HandleSearch(PageChangeEventArgs pageInfo)
    {
        // Log the search term to the console for debugging purposes
        Console.WriteLine(pageInfo.SearchTerm);

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
        var invoiceDto = await AppService.Search(input);

        // Calculate the total number of pages required to display all results
        TotalItem = (int)invoiceDto.TotalCount;
        CreateTable(invoiceDto);
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
        Console.WriteLine(RowPerPage);
        // Create a request object for paged and sorted data
        var input = new PagedAndSortedResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn
        };

        // Retrieve the list of cities from the server
        var invoiceDto = await AppService.GetListAsync(input);
        TotalItem = (int)invoiceDto.TotalCount;
        CreateTable(invoiceDto);

    }

    private void CreateTable(PagedResultDto<InvoiceDto> invoices)
    {
        // Initialize table data to be displayed, setting the table's name and whether to show the header
        var tableData = new TableData()
        {
            Name = "Invoices",
            ShowHeader = true
        };

        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Id"], Name = "Id", IsFilterable = false, PropertyNmae = "Id", IsSortable = false, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["InvoiceNumber"], Name = "InvoiceNumber", IsFilterable = true, PropertyNmae = "InvoiceNumber", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["InvoicePrice"], Name = "InvoicePrice", IsFilterable = true, PropertyNmae = "InvoicePrice", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["InvoiceType"], Name = "InvoiceType", IsFilterable = true, PropertyNmae = "InvoiceType", IsSortable = true, TypeFilter = TypeFilter.Text });



        foreach (var item in invoices.Items)
        {
            var rowDef = new RowDef();
            rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["InvoiceNumber"], item.InvoiceNumber));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["InvoicePrice"], item.InvoicePrice.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["InvoiceType"], item.InvoiceType.ToString()));


            tableData.Rows.Add(rowDef);
        }
        itemData = tableData;
    }

    private void HandleAdd()
    {
        Navigation.NavigateTo("/invoices/add", true);
    }
    private void HandleView(RowDef row)
    {
        Navigation.NavigateTo("/invoices/details/" + row.Cells[0].Value, true);
    }
    /// <summary>

    /// Displays a snackbar notification with the specified message and severity.

    /// </summary>

    /// <param name="message">The message to display in the snackbar.</param>

    /// <param name="severity">The severity level of the snackbar (e.g., Success, Error).</param>

    void ShowSnackbar(string message, Severity severity)

    {

        Snackbar.Clear();

        Snackbar.Configuration.PositionClass = Defaults.Classes.Position.TopRight;

        Snackbar.Add(message, severity, c => c.SnackbarVariant = Variant.Filled);

    }
}
