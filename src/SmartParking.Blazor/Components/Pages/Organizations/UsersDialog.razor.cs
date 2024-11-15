using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Garages;
using SmartParking.Localization;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Volo.Abp.Application.Dtos;
using SmartParking.Authentication;

namespace SmartParking.Blazor.Components.Pages.Organizations;

public partial class UsersDialog
{
    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; }
    [Parameter] public int organizationId { get; set; }
    private void Cancel() => MudDialog.Cancel();

    private int TotalItem;
    public int CurrentPage { get; set; } = 1; // Default current page value is 10
    public int RowPerPage { get; set; } = 10; // Default value is 10

    private TableData itemData = new TableData();

    private string termSearch;
    private readonly IStringLocalizer<SmartParkingResource> _localizer;

    public UsersDialog(IStringLocalizer<SmartParkingResource> localizer)
    {
        _localizer = localizer;
    }

    //private Task CreateUser()
    //{

    //    var parameters = new DialogParameters
    //    {
    //        ["organizationId"] = organizationId
    //    };
    //    return Dialog.ShowAsync<AddGarageDialogInOrganization>("AddGarageDialogInOrganization", parameters);
    //}
    /// <summary>
    /// filter for hotels on the server side using the hotel columns.
    /// </summary>
    /// <param name="columnFilter">The name of column to filter for among hotel.</param>
    /// <returns>Returns a task that represents the asynchronous operation.</returns>
    private async Task HandleFilter(PageChangeEventArgs pageInfo)
    {
        //// Log the search term to the console for debugging purposes

        //// Create a new search input object with pagination, sorting, and search term parameters
        //var input = new PagedAndSortedAndFilteredResultRequestDto
        //{
        //    MaxResultCount = pageInfo.RowsPerPage,
        //    SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
        //    Sorting = pageInfo.SortColumn,
        //    SearchingTerm = termSearch, // The term to use for filtering results
        //    filters = pageInfo.Filters
        //};
        //// Call the service to perform the search based on the input parameters
        //var garagessDto = await UsersAppService.Filter(input);

        //// Calculate the total number of pages required to display all results
        //TotalItem = (int)garagessDto.TotalCount;
        //CreateTable(garagessDto);


    }
    /// <summary>
    /// Searches for hotels on the server side using the hotel code search term.
    /// </summary>
    /// <param name="searchTerm">The term to search for among hotel.</param>
    /// <returns>Returns a task that represents the asynchronous operation.</returns>
    private async Task HandleSearch(PageChangeEventArgs pageInfo)
    {
        //// Log the search term to the console for debugging purposes
        //Console.WriteLine(pageInfo.SearchTerm);

        //// Update the term used for searching with the provided search term
        //termSearch = pageInfo.SearchTerm;

        //// Create a new search input object with pagination, sorting, and search term parameters
        //var input = new PagedAndSortedAndSearchResultRequestDto
        //{
        //    MaxResultCount = pageInfo.RowsPerPage,
        //    SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
        //    Sorting = pageInfo.SortColumn,
        //    SearchingTerm = termSearch // The term to use for filtering results
        //};

        //// Call the service to perform the search based on the input parameters
        //var garagessDto = await GarageAppService.Search(input);

        //// Calculate the total number of pages required to display all results
        //TotalItem = (int)garagessDto.TotalCount;
        //CreateTable(garagessDto);
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
        var garagessDto = await UsersAppService.GetUserByOrganizationId( organizationId);
        //TotalItem = (int)garagessDto.TotalCount;
        CreateTable(garagessDto);

    }

    private void CreateTable(List<UserDto> garages)
    {
        // Initialize table data to be displayed, setting the table's name and whether to show the header
        var tableData = new TableData()
        {
            Name = "Users",
            ShowHeader = true
        };


        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Id"], Name = "Id", IsFilterable = false, PropertyName = "Id", IsSortable = false, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Name"], Name = "Name", IsFilterable = true, PropertyName = "Name", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Email"], Name = "Email", IsFilterable = true, PropertyName = "Email", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Phone Number"], Name = "PhoneNumber", IsFilterable = true, PropertyName = "PhoneNumber", IsSortable = true, TypeFilter = TypeFilter.Text });
       
        foreach (var item in garages)
        {
            var rowDef = new RowDef();
            rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Name"], item.Name));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Email"], item.Email));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Phone Number"], item.PhoneNumber));


            tableData.Rows.Add(rowDef);
        }
        itemData = tableData;
    }
}
