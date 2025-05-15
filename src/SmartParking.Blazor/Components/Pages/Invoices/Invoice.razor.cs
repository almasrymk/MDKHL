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
using System.Globalization;
using static NuGet.Client.ManagedCodeConventions;
using Volo.Abp.MultiTenancy;
using SmartParking.Currency;

namespace SmartParking.Blazor.Components.Pages.Invoices;

public partial class Invoice
{
    private int TotalItem;
    public int CurrentPage { get; set; } = 1; // Default current page value is 10
    public int RowPerPage { get; set; } = 10; // Default value is 10

    private TableData itemData = new TableData();

    private string termSearch;
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
        var invoiceDtos = await AppService.Filter(input);

        // Calculate the total number of pages required to display all results
        TotalItem = (int)invoiceDtos.TotalCount;
        CreateTable(invoiceDtos);


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
        if (CurrentTenant.Id == null)
        {
            var invoiceDto = await AppService.SearchForMDKHL(input);
            TotalItem = (int)invoiceDto.TotalCount;
            CreateTable(invoiceDto);
        }
        else
        {
            var invoiceDto = await AppService.Search(input);
            TotalItem = (int)invoiceDto.TotalCount;
            CreateTable(invoiceDto);
        }
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
            sort = "InvoiceTime DESC";
        else
            sort = pageInfo.SortColumn;
        termSearch = pageInfo.SearchTerm;
        CurrentPage = pageInfo.PageNumber;
        RowPerPage = pageInfo.RowsPerPage;
        var input = new PagedAndSortedAndSearchResultRequestDto
        {
            MaxResultCount = RowPerPage,
            SkipCount = (CurrentPage - 1) * pageInfo.RowsPerPage,
            Sorting = sort,
            SearchingTerm = termSearch
        };

        if (CurrentTenant.Id != null)
        {
            var tenantId = CurrentTenant.Id;
            var invoiceDto = await AppService.GetAllInvoicesByOrganizationAsync(input, tenantId);
            TotalItem = (int)invoiceDto.TotalCount;
            CreateTable(invoiceDto);
        }
        else
        {
            // Retrieve the list of cities from the server
            var invoiceDto = await AppService.GetListAsync(input);
            TotalItem = (int)invoiceDto.TotalCount;
            CreateTable(invoiceDto);
        }
    }
    private void CreateTable(PagedResultDto<InvoiceDto> invoices)
    {
        // Initialize table data to be displayed, setting the table's name and whether to show the header
        var tableData = new TableData()
        {
            Name = L["Invoices"],
            ShowHeader = true
        };
        if (CurrentTenant.Id == null) {

            if (CultureInfo.CurrentCulture.Name == "en")
            {
                tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["OrganizationEnglishName"], Name = "OrganizationEnglishName", IsFilterable = false, PropertyName = "Organization.EnglishName", IsSortable = true, TypeFilter = TypeFilter.Text });

                }
            else
            {
                tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["OrganizationArabicName"], Name = "OrganizationArabicName", IsFilterable = true, PropertyName = "Organization.ArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });

            }
        }
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["InvoiceId"], Name = "InvoiceId", IsFilterable = true, PropertyName = "InvoiceId", IsSortable = false, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["InvoiceTime"], Name = "InvoiceTime", IsFilterable = false, PropertyName = "InvoiceTime", IsSortable = true, TypeFilter = TypeFilter.Text });
        if (CultureInfo.CurrentCulture.Name == "en")
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["CustomerName"], Name = "CustomerName", IsFilterable = true, PropertyName = "CustomerEnName", IsSortable = true, TypeFilter = TypeFilter.Text });
        }
        else
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["CustomerName"], Name = "CustomerName", IsFilterable = true, PropertyName = "CustomerArName", IsSortable = true, TypeFilter = TypeFilter.Text });

        }
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["PlateNumber"], Name = "PlateNumber", IsFilterable = true, PropertyName = "PlateNumber", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = $"{_localizer["TotalHours"]} ({_localizer["Hours"]})", Name="TotalHours", IsFilterable = false, PropertyName = "TotalHours", IsSortable = true, TypeFilter = TypeFilter.Number });
        tableData.Columns.Add(new ColumnDef() { DisplayName = $"{_localizer["InvoicePrice"]}", Name = "InvoicePrice", IsFilterable = false, PropertyName = "InvoicePrice", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["InvoiceType"], Name = "InvoiceType", IsFilterable = true, PropertyName = "InvoiceType", IsSortable = true, TypeFilter = TypeFilter.Enum });

        foreach (var item in invoices.Items)
        {
            var rowDef = new RowDef();   
            if (CurrentTenant.Id == null)
            {
                if (CultureInfo.CurrentCulture.Name == "en")
                {
                    rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["OrganizationEnglishName"], item.OrganizationEnglishName));
                }
                else
                {
                    rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["OrganizationArabicName"], item.OrganizationArabicName));
                }
            }
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["InvoiceId"], item.InvoiceId.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["InvoiceTime"], item.InvoiceTime.ToString()));
            if (CultureInfo.CurrentCulture.Name == "en")
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["CustomerName"], item.CustomerEnName));
            }
            else
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["CustomerName"], item.CustomerArName));
            }

            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["PlateNumber"], item.PlateNumber));
            rowDef.Cells.Add(new KeyValuePair<string, string>($"{_localizer["TotalHours"]} ({_localizer["Hours"]})", item.TotalHours.ToString()));
            var IconHtml = CurrencyIcon.ToHtml("icon-Saudi_Riyal_Symbolsvg", "15px");
            rowDef.Cells.Add(new KeyValuePair<string, string>(
                $"{_localizer["InvoicePrice"]}",
                $"{item.InvoicePrice} {IconHtml}"));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["InvoiceType"],!item.IsPackage?_localizer[$"{item.InvoiceType.ToString()}"]: _localizer[$"Package"]));

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
        if (CurrentTenant.Id != null)
        {
            Navigation.NavigateTo("/invoices/details/" + row.Cells[0].Value, true);
        }
        else
        {
            Navigation.NavigateTo("/invoices/details/" + row.Cells[1].Value, true);
        }
    }
}
