using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Customs;
using SmartParking.Garages;
using SmartParking.Localization;
using SmartParking.Organizations;
using SmartParking.OrganizationsGarages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;


namespace SmartParking.Blazor.Components.Pages.Garages;

public partial class Garage
{

    private int TotalItem;
    public int CurrentPage { get; set; } = 1; // Default current page value is 10
    public int RowPerPage { get; set; } = 10; // Default value is 10

    private TableData itemData = new TableData();

    private string termSearch;
	private DialogOptions options = new DialogOptions { CloseOnEscapeKey = true };
    private readonly IStringLocalizer<SmartParkingResource> _localizer;

    public Garage(IStringLocalizer<SmartParkingResource> localizer)
    {
        _localizer = localizer;
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
        var garagessDto = await GarageAppService.Filter(input);

        // Calculate the total number of pages required to display all results
        TotalItem = (int)garagessDto.TotalCount;
        CreateTable(garagessDto);


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
        var garagessDto = await GarageAppService.Search(input);

        // Calculate the total number of pages required to display all results
        TotalItem = (int)garagessDto.TotalCount;
        CreateTable(garagessDto);
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
        var garagessDto = await GarageAppService.GetListAsync(input);
        TotalItem = (int)garagessDto.TotalCount;
        CreateTable(garagessDto);

    }

    private void CreateTable(PagedResultDto<GarageDto> garages)
    {
        // Initialize table data to be displayed, setting the table's name and whether to show the header
        var tableData = new TableData()
        {
            Name = "Garages",
            ShowHeader = true
        };


        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Id"], Name = "Id", IsFilterable = false, PropertyNmae = "Id", IsSortable = false, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Code"], Name = "Code", IsFilterable = true, PropertyNmae = "Code", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Name"], Name = "Name", IsFilterable = true, PropertyNmae = "Name", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Status"], Name = "Status", IsFilterable = true, PropertyNmae = "Status", IsSortable = true, TypeFilter = TypeFilter.Enum });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Max Capacity"], Name = "MaxCapacity", IsFilterable = false, IsSortable = false, TypeFilter = TypeFilter.Number });
        foreach (var item in garages.Items)
        {
            var rowDef = new RowDef();
            rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Code"], item.Code));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Name"], item.Name));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Status"], item.Status.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Max Capacity"], item.MaxCapacity.ToString()));


            tableData.Rows.Add(rowDef);
        }
        itemData = tableData;
    }

    private Task CreateGarage()
    {


        return DialogService.ShowAsync<AddGarageDialog>("Simple Dialog", options);


    }
    private async Task<IDialogReference> OnClickRowEdit(RowDef row)
    {
        var id =int.Parse(row.Cells[0].Value);

		var garageDto= await GarageAppService.GetAsync(id);

        var createUpdateGarage=new CreateUpdateGarageDto { 
            Name=garageDto.Name,
            Code=garageDto.Code,
            Status=garageDto.Status,
            MaxCapacity=garageDto.MaxCapacity,
            ClosingHours=garageDto.ClosingHours,
            OrganizationId=garageDto.OrganizationId
        };



		var parameters = new DialogParameters { 
            ["Garage"] = createUpdateGarage,
            ["UpdatedId"]=id
        };


        return await DialogService.ShowAsync<UpdateGarageDialog>("Edit Garage", parameters);
        
    }
    private async Task<IDialogReference> OnClickRowView(RowDef row)
    {
		var id = int.Parse(row.Cells[0].Value);

		var garageDto = await GarageAppService.GetAsync(id);


		var parameters = new DialogParameters
        {
            ["Garage"] = garageDto,
        };


        return await DialogService.ShowAsync<ViewGarageDialog>("View Garage", parameters);
    }

    private async Task OnClickRowDelete(string row)
    {
		var id = int.Parse(row);
        try
        {

            await GarageAppService.DeleteAsync(id);
            ShowSnackbar("Garage Deleted Successfully ", Severity.Success);

        }
        catch (Exception ex) { 
        
           ShowSnackbar(ex.Message,Severity.Error);
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
