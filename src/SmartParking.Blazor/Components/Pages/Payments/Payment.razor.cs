using Microsoft.Extensions.Localization;
using MudBlazor;
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

namespace SmartParking.Blazor.Components.Pages.Payments;

public partial class Payment
{
    private int TotalItem;
    public int CurrentPage { get; set; } = 1; // Default current page value is 10
    public int RowPerPage { get; set; } = 10; // Default value is 10

    private TableData itemData = new TableData();

    private string termSearch;
    private readonly IStringLocalizer<SmartParkingResource> _localizer;

    public Payment(IStringLocalizer<SmartParkingResource> localizer)
    {
        _localizer = localizer;
    }

   
    /// <summary>
    /// Displays a snackbar notification with the specified message and severity.
    /// </summary>
    /// <param name="message">The message to display in the snackbar.</param>
    /// <param name="severity">The severity level of the snackbar (e.g., Success, Error).</param>
    void ShowSnackbar(string message, Severity severity)
    {
        Snackbar.Clear();
        Snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomRight;
        Snackbar.Add(message, severity, c => c.SnackbarVariant = Variant.Filled);
    }

    /// <summary>
    /// filter for hotels on the server side using the hotel columns.
    /// </summary>
    /// <param name="columnFilter">The name of column to filter for among hotel.</param>
    /// <returns>Returns a task that represents the asynchronous operation.</returns>
    //private async Task HandleFilter(PageChangeEventArgs pageInfo)
    //{
    //    // Log the search term to the console for debugging purposes

    //    // Create a new search input object with pagination, sorting, and search term parameters
    //    var input = new PagedAndSortedAndFilteredResultRequestDto
    //    {
    //        MaxResultCount = pageInfo.RowsPerPage,
    //        SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
    //        Sorting = pageInfo.SortColumn,
    //        SearchingTerm = termSearch, // The term to use for filtering results
    //        filters = pageInfo.Filters
    //    };
    //    // Call the service to perform the search based on the input parameters
    //    var organizationsDto = await OrganizationsAppService.Filter(input);

    //    // Calculate the total number of pages required to display all results
    //    TotalItem = (int)organizationsDto.TotalCount;
    //    CreateTable(organizationsDto);


    //}
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
        PagedResultDto<PaymentResponseDto> pagedResultDto;
        if (CurrentTenant.Id == null)
        {
            pagedResultDto = await PaymentWebAppService.SearchMdkhlAsync(input);
        }
        else
        {
            pagedResultDto = await PaymentWebAppService.SearchMdkhlAsync(input);
        }

        // Calculate the total number of pages required to display all results
        TotalItem = (int)pagedResultDto.TotalCount;
        CreateTable(pagedResultDto);
    }
    /// <summary>
    /// Fetches the table data for the specified page and updates the table with the retrieved data.
    /// </summary>
    /// <param name="page">The page number for which to fetch the data.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task FetchTableData(PageChangeEventArgs pageInfo)
    {

        string sort;
        if (pageInfo.SortColumn == "Id")
            sort = "CreationTime DESC";
        else
            sort = pageInfo.SortColumn;

        CurrentPage = pageInfo.PageNumber;
        RowPerPage = pageInfo.RowsPerPage;
        // Create a request object for paged and sorted data
        var input = new PagedAndSortedResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = sort
        };

        // Retrieve the list of payments from the server
        PagedResultDto<PaymentResponseDto> paymentsDto;
        if (CurrentTenant.Id == null) 
         paymentsDto = await PaymentWebAppService.GetListMdkhlAsync(input);
        else
            paymentsDto = await PaymentWebAppService.GetListOrganizationAsync(input);
        TotalItem = (int)paymentsDto.TotalCount;
        CreateTable(paymentsDto);

    }

    private void CreateTable(PagedResultDto<PaymentResponseDto> organizations)
    {
        // Initialize table data to be displayed, setting the table's name and whether to show the header
        var tableData = new TableData()
        {
            Name = "Payment",
            ShowHeader = true
        };


        tableData.Columns.Add(new ColumnDef() { DisplayName = "Id", Name = "Id", IsFilterable = false, PropertyName = "Id", IsSortable = false, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["PaymentCategory"], Name = "PaymentCategory", IsFilterable = true, PropertyName = "PaymentCategory", IsSortable = true, TypeFilter = TypeFilter.Enum });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["PaymentMethod"], Name = "PaymentMethod", IsFilterable = true, PropertyName = "PaymentMethod", IsSortable = true, TypeFilter = TypeFilter.Enum });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["PaymentStatus"], Name = "PaymentStatus", IsFilterable = true, PropertyName = "PaymentStatus", IsSortable = true, TypeFilter = TypeFilter.Enum });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["customer category"], Name = "customer category", IsFilterable = true, PropertyName = "Customer.CustomerCategoryStatus", IsSortable = true, TypeFilter = TypeFilter.Enum });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Amount"], Name = "Amount", IsFilterable = true, PropertyName = "Amount", IsSortable = true, TypeFilter = TypeFilter.Number });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Currency"], Name = "Currency", IsFilterable = true, PropertyName = "Currency", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["CustomerName"], Name = "CustomerName", IsFilterable = false, PropertyName = "Customer.IdentityUser.UserName", IsSortable = false, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["CreationTime"], Name = "CreationTime", IsFilterable = false, PropertyName = "CreationTime", IsSortable = false, TypeFilter = TypeFilter.Text });

       
        foreach (var item in organizations.Items)
        {
            
            var rowDef = new RowDef();
            rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["PaymentCategory"], item.PaymentCategory));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["PaymentMethod"], item.PaymentMethod));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["PaymentStatus"], item.PaymentStatus));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["customer category"], item.Customers?.CustomerCategory.ToString() ?? _localizer["Customer"]));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Amount"], item.Amount.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Currency"], item.Currency.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["CustomerName"], item.Customers?.UserName ?? _localizer["Customer"]));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["CreationTime"], item.CreationTime.ToString()));



            tableData.Rows.Add(rowDef);
        }
        itemData = tableData;
    }
    private async Task HandleView(RowDef rowDef)
    {
        var id = rowDef.Cells[0].Value;
        Navigation.NavigateTo("/payments/view/" +id, true);
    }
}
