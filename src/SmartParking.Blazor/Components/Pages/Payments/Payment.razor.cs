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
            pagedResultDto = await PaymentWebAppService.SearchMdkhlAsync(input);
        }
        else
        {
            pagedResultDto = await PaymentWebAppService.SearchOrganizationAsync(input);
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
        PagedResultDto<PaymentDto> paymentsDto;
        if (CurrentTenant.Id == null) 
         paymentsDto = await PaymentWebAppService.GetListMdkhlAsync(input);
        else
            paymentsDto = await PaymentWebAppService.GetListOrganizationAsync(input);
        TotalItem = (int)paymentsDto.TotalCount;
        CreateTable(paymentsDto);

    }

    private void CreateTable(PagedResultDto<PaymentDto> organizations)
    {
        // Initialize table data to be displayed, setting the table's name and whether to show the header
        var tableData = new TableData()
        {
            Name = "Payment",
            ShowHeader = true
        };


        tableData.Columns.Add(new ColumnDef() { DisplayName = "Id", Name = "Id", IsFilterable = false, PropertyName = "Id", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Code"], Name = "PaymentNumber", IsFilterable = true, PropertyName = "PaymentNumber", IsSortable = true, TypeFilter = TypeFilter.Enum });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["PlateNumber"], Name = "PlateNumber", IsFilterable = false, PropertyName = "Invoice.PlateNumber", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["PaymentCategory"], Name = "PaymentCategory", IsFilterable = true, PropertyName = "PaymentCategory", IsSortable = true, TypeFilter = TypeFilter.Enum });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["InvoiceId"], Name = "InvoiceId", IsFilterable = false, PropertyName = "Invoice.Id", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["PaymentMethod"], Name = "PaymentMethod", IsFilterable = true, PropertyName = "PaymentMethod", IsSortable = true, TypeFilter = TypeFilter.Enum });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["customer category"], Name = "customer category", IsFilterable = true, PropertyName = "Customers.CustomerCategory", IsSortable = true, TypeFilter = TypeFilter.Enum });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Amount"], Name = "Amount", IsFilterable = true, PropertyName = "Amount", IsSortable = true, TypeFilter = TypeFilter.Number });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Currency"], Name = "Currency", IsFilterable = true, PropertyName = "Currency", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["CustomerName"], Name = "CustomerName", IsFilterable = false, PropertyName = "Customers.ArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["CreationTime"], Name = "CreationTime", IsFilterable = false, PropertyName = "CreationTime", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["PaymentStatus"], Name = "PaymentStatus", IsFilterable = true, PropertyName = "PaymentStatus", IsSortable = true, TypeFilter = TypeFilter.Enum });

       
        foreach (var item in organizations.Items)
        {
            
            var rowDef = new RowDef();
            rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Code"], item.PaymentNumber?.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["PlateNumber"], item.InvoiceId.HasValue ? item.PlateNumber : @L["Pacakge"]));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["PaymentCategory"], _localizer[$"{item.PaymentCategory.ToString()}"]));
            rowDef.Cells.Add(new KeyValuePair<string, string>(
                _localizer["InvoiceId"],
                item.PaymentCategory == Enums.PaymentCategory.PaymentParking
                ?  item.InvoiceId.ToString()
                :  item.PackageId .ToString()
            ));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["PaymentMethod"], _localizer[$"{item.PaymentMethod}"]));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["customer category"], item.Customers?.CustomerCategory.ToString() is null ? _localizer["NotCustomer"]: _localizer[$"{item.Customers?.CustomerCategory.ToString()}"]));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Amount"], item.Amount.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Currency"], _localizer[$"{item.Currency.ToString()}"]));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["CustomerName"], item.Customers?.ArabicName ?? _localizer["NotCustomer"]));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["CreationTime"], item.CreationTime.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["PaymentStatus"], _localizer[$"{item.PaymentStatus.ToString()}"]));



            tableData.Rows.Add(rowDef);
        }
        itemData = tableData;
    }
    private async Task HandleView(RowDef rowDef)
    {
        var id = rowDef.Cells[0].Value;
        Navigation.NavigateTo($"/payments/view/{id}" , true);
    }
}
