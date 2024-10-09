using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Invoices;
using SmartParking.Localization;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Volo.Abp.Application.Dtos;
using SmartParking.Subscriptions;

namespace SmartParking.Blazor.Components.Pages.Subscriptions;

public partial class Subscription
{
    private int TotalItem;
    public int CurrentPage { get; set; } = 1; // Default current page value is 10
    public int RowPerPage { get; set; } = 10; // Default value is 10

    private TableData itemData = new TableData();

    private string termSearch;

    private DialogOptions options = new DialogOptions { CloseOnEscapeKey = true };

    private readonly IStringLocalizer<SmartParkingResource> _localizer;
    public Subscription(IStringLocalizer<SmartParkingResource> localizer)
    {
        _localizer = localizer;
    }

    /// <summary>
    /// filter for subscriptions on the server side using the subscription columns.
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
        var subscriptionDto = await AppService.Filter(input);

        // Calculate the total number of pages required to display all results
        TotalItem = (int)subscriptionDto.TotalCount;
        CreateTable(subscriptionDto);


    }

    /// <summary>
    /// Searches for subscriptions on the server side using the subscription type search term.
    /// </summary>
    /// <param name="searchTerm">The term to search for among subscription.</param>
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
        var subscriptionDto = await AppService.Search(input);

        // Calculate the total number of pages required to display all results
        TotalItem = (int)subscriptionDto.TotalCount;
        CreateTable(subscriptionDto);
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
        var subscriptionDto = await AppService.GetListAsync(input);
        TotalItem = (int)subscriptionDto.TotalCount;
        CreateTable(subscriptionDto);

    }

    private void CreateTable(PagedResultDto<SubscriptionDto> invoices)
    {
        // Initialize table data to be displayed, setting the table's name and whether to show the header
        var tableData = new TableData()
        {
            Name = "Subscriptions",
            ShowHeader = true
        };

        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Id"], Name = "Id", IsFilterable = false, PropertyNmae = "Id", IsSortable = false, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["SubscriptionName"], Name = "SubscriptionName", IsFilterable = false, PropertyNmae = "SubscriptionName", IsSortable = false, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["SubscriptionType"], Name = "SubscriptionType", IsFilterable = true, PropertyNmae = "SubscriptionType", IsSortable = false, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["SubscriptionPrice"], Name = "SubscriptionPrice", IsFilterable = false, PropertyNmae = "SubscriptionPrice", IsSortable = false, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["SubscriptionStatus"], Name = "Status", IsFilterable = true, PropertyNmae = "Status", IsSortable = true, TypeFilter = TypeFilter.Text });



        foreach (var item in invoices.Items)
        {
            var rowDef = new RowDef();
            rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["SubscriptionName"], item.SubscriptionName));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["SubscriptionType"], item.SubscriptionType));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["SubscriptionPrice"], item.SubscriptionPrice.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["SubscriptionStatus"], item.Status.ToString()));


            tableData.Rows.Add(rowDef);
        }
        itemData = tableData;
    }
}
