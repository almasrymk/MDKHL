using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Pages.Cities;
using SmartParking.Blazor.Components.Pages.Governorates;
using SmartParking.Localization;
using SmartParking.Lookups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Blazor.Components.Pages.PackageTypes;

public partial class PackageType
{
    private int TotalItem;
    public int CurrentPage { get; set; } = 1; // Default current page value is 10
    public int RowPerPage { get; set; } = 10; // Default value is 10

    private TableData itemData = new TableData();

    private string termSearch;

    private DialogOptions options = new DialogOptions { CloseOnEscapeKey = true };

    private readonly IStringLocalizer<SmartParkingResource> _localizer;

    public PackageType(IStringLocalizer<SmartParkingResource> localizer)
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
        var packageTypeDto = await AppService.Filter(input);

        // Calculate the total number of pages required to display all results
        TotalItem = (int)packageTypeDto.TotalCount;
        CreateTable(packageTypeDto);


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
        var packageTypeDto = await AppService.Search(input);

        // Calculate the total number of pages required to display all results
        TotalItem = (int)packageTypeDto.TotalCount;
        CreateTable(packageTypeDto);
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
        var packageTypeDto = await AppService.GetListAsync(input);
        TotalItem = (int)packageTypeDto.TotalCount;
        CreateTable(packageTypeDto);

    }

    private void CreateTable(PagedResultDto<PackageTypeDto> packageTypes)
    {
        // Initialize table data to be displayed, setting the table's name and whether to show the header
        var tableData = new TableData()
        {
            Name = "Packages",
            ShowHeader = true
        };

        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Id"], Name = "Id", IsFilterable = false, PropertyNmae = "Id", IsSortable = false, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Arabic Name"] , Name = "ArabicName", IsFilterable = true, PropertyNmae = "ArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["English Name"] , Name = "EnglishName", IsFilterable = true, PropertyNmae = "EnglishName", IsSortable = true, TypeFilter = TypeFilter.Text });
       


        foreach (var item in packageTypes.Items)
        {
            var rowDef = new RowDef();
            rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Arabic Name"], item.ArabicName));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["English Name"], item.EnglishName));
            

            tableData.Rows.Add(rowDef);
        }
        itemData = tableData;
    }

    private Task CreatePackageType()
    {


        return Dialog.ShowAsync<AddPackageTypeDialog>("Simple Dialog", options);


    }
    private async Task<IDialogReference> OnClickRowEdit(RowDef row)
    {
        var id = int.Parse(row.Cells[0].Value);

        var packageTypeDto = await AppService.GetAsync(id);

        var createUpdatePackageTypeDto = new CreateUpdateGovernorateDto()
        {
            ArabicName = packageTypeDto.ArabicName,
            EnglishName = packageTypeDto.EnglishName,
            Notes = packageTypeDto.Notes,
        };



        var parameters = new DialogParameters
        {
            ["packageType"] = packageTypeDto,
            ["UpdatedId"] = id
        };


        return await Dialog.ShowAsync<EditPackageTypeDialog>("Edit PackageType", parameters);

    }


    private async Task<IDialogReference> OnClickRowView(RowDef row)
    {
        var id = int.Parse(row.Cells[0].Value);

        var packageTypeDto = await AppService.GetAsync(id);


        var parameters = new DialogParameters
        {
            ["packageType"] = packageTypeDto,
        };


        return await Dialog.ShowAsync<DetailsPackageTypeDialog>("View PackageType", parameters);
    }

    private async Task OnClickRowDelete(string row)
    {
        var successMessage = _localizer["PackageTypeDeletedSuccessfully"];
        var id = int.Parse(row);
        try
        {

            await AppService.DeleteAsync(id);
            ShowSnackbar(successMessage, Severity.Success);

        }
        catch (Exception ex)
        {

            ShowSnackbar(ex.Message, Severity.Error);
        }

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
