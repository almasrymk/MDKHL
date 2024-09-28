using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Organizations;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Blazor.Components.Pages.Organizations;

public partial class Organization
{

    private int TotalItem;
    public int CurrentPage { get; set; } = 1; // Default current page value is 10
    public int RowPerPage { get; set; } = 10; // Default value is 10

    private TableData itemData = new TableData();

    private string termSearch;

    private void HandleEdit(RowDef row)
    {
        Navigation.NavigateTo("/organizations/update/" + row.Cells[0].Value,true);
    }
    private void HandleAdd()
    {
        Navigation.NavigateTo("/organizations/add",true);
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
        // Call the service to perform the search based on the input parameters
        var organizationsDto = await OrganizationsAppService.Filter(input);

        // Calculate the total number of pages required to display all results
        TotalItem = (int)organizationsDto.TotalCount;
        CreateTable(organizationsDto);


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
        var organizationsDto = await OrganizationsAppService.Search(input);

        // Calculate the total number of pages required to display all results
        TotalItem = (int)organizationsDto.TotalCount;
        CreateTable(organizationsDto);
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

        // Retrieve the list of organizations from the server
        var organizationsDto = await OrganizationsAppService.GetListAsync(input);
        TotalItem = (int)organizationsDto.TotalCount;
        CreateTable(organizationsDto);

    }

    private void CreateTable(PagedResultDto<OrganizationDto> organizations)
    {
        // Initialize table data to be displayed, setting the table's name and whether to show the header
        var tableData = new TableData()
        {
            Name = "Organization",
            ShowHeader = true
        };


        tableData.Columns.Add(new ColumnDef() { DisplayName = "Id", Name = "Id", IsFilterable = false, PropertyNmae = "Id", IsSortable = false, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = "Code", Name = "Code", IsFilterable = false, PropertyNmae = "Code", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = "English Name", Name = "EnglishName", IsFilterable = true, PropertyNmae = "EnglishName", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = "Country Name", Name = "CountryName", IsFilterable = true, PropertyNmae = "Country.ArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = "State Name", Name = "StateName", IsFilterable = true, PropertyNmae = "State.ArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = "City Name", Name = "CityName", IsFilterable = true, PropertyNmae = "City.ArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = "Status", Name = "Status", IsFilterable = true, PropertyNmae = "Status", IsSortable = true, TypeFilter = TypeFilter.Enum });
        tableData.Columns.Add(new ColumnDef() { DisplayName = "Max Capacity", Name = "MaxCapacity", IsFilterable = false, IsSortable = false, TypeFilter = TypeFilter.Number });
        tableData.Columns.Add(new ColumnDef() { DisplayName = "Num Of Cameras", Name = "NumOfCameras", IsFilterable = false, IsSortable = false, TypeFilter = TypeFilter.Number });
        foreach (var item in organizations.Items)
        {
            var rowDef = new RowDef();
            rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>("Code", item.Code));
            rowDef.Cells.Add(new KeyValuePair<string, string>("English Name", item.EnglishName));
            rowDef.Cells.Add(new KeyValuePair<string, string>("Country Name", item.CountryName));
            rowDef.Cells.Add(new KeyValuePair<string, string>("State Name", item.StateName));
            rowDef.Cells.Add(new KeyValuePair<string, string>("City Name", item.StateName));
            rowDef.Cells.Add(new KeyValuePair<string, string>("Status", item.Status.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>("Max Capacity", item.MaxCapacity.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>("Num Of Cameras", item.NumOfCameras.ToString()));


            tableData.Rows.Add(rowDef);
        }
        itemData = tableData;
    }
}
