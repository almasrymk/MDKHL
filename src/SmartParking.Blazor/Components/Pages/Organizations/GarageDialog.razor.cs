using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Pages.Garages;
using SmartParking.Garages;
using SmartParking.Localization;
using SmartParking.OrganizationsGarages;
using System.Globalization;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Blazor.Components.Pages.Organizations;

public partial class GarageDialog
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; }
    [Parameter] public int organizationId { get; set; }
    private void Cancel() => MudDialog.Cancel();

    private int TotalItem;
    public int CurrentPage { get; set; } = 1; // Default current page value is 10
    public int RowPerPage { get; set; } = 10; // Default value is 10

    private TableData itemData = new TableData();

    private string termSearch;
    private readonly IStringLocalizer<SmartParkingResource> _localizer;

    public GarageDialog(IStringLocalizer<SmartParkingResource> localizer)
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
        // Create a request object for paged and sorted data
        var input = new PagedAndSortedResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn
        };

        // Retrieve the list of organizations from the server
        var garagessDto = await GarageAppService.GetGarageByOrganization(input, organizationId);
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


        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Id"], Name = "Id", IsFilterable = false, PropertyName = "Id", IsSortable = false, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Code"], Name = "Code", IsFilterable = true, PropertyName = "Code", IsSortable = false, TypeFilter = TypeFilter.Text });
        if (CultureInfo.CurrentCulture.Name == "en")
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["EnglishName"], Name = "EnglishName", IsFilterable = true, PropertyName = "NameEn", IsSortable = false, TypeFilter = TypeFilter.Text });
        }
        else
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["ArabicName"], Name = "ArabicName", IsFilterable = true, PropertyName = "NameAr", IsSortable = false, TypeFilter = TypeFilter.Text });

        }

        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["ParkingSlot"], Name = "MaxCapacity", IsFilterable = false, IsSortable = false, TypeFilter = TypeFilter.Number });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Status"], Name = "Status", IsFilterable = false, PropertyName = "Status", IsSortable = false, TypeFilter = TypeFilter.Enum });
        foreach (var item in garages.Items)
        {
            var rowDef = new RowDef();
            rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Code"], item.Code));
            if (CultureInfo.CurrentCulture.Name == "en")
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["EnglishName"], item.NameEn));
            }
            else
            {

                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["ArabicName"], item.NameAr));
            }
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["ParkingSlot"], item.MaxCapacity.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Status"], _localizer[item.Status.ToString()]));


            tableData.Rows.Add(rowDef);
        }
        itemData = tableData;
    }
    private async Task<IDialogReference> OnClickRowEdit(RowDef row)
    {
        var id = int.Parse(row.Cells[0].Value);

        var garageDto = await GarageAppService.GetAsync(id);

        var createUpdateGarage = new CreateUpdateGarageDto
        {
            NameEn = garageDto.NameEn,
            NameAr = garageDto.NameAr,
            Code = garageDto.Code,
            Status = garageDto.Status,
            MaxCapacity = garageDto.MaxCapacity,
            ClosingHours = garageDto.ClosingHours,
            OrganizationId = garageDto.OrganizationId,
            MaxSlotsForCustomers = garageDto.MaxSlotsForCustomers,
            MaxSlotsForEmployess = garageDto.MaxSlotsForEmployess,
            MaxSlotsForFree = garageDto.MaxSlotsForFree,
        };



        var parameters = new DialogParameters
        {
            ["Garage"] = createUpdateGarage,
            ["UpdatedId"] = id
        };


        return await Dialog.ShowAsync<UpdateGarageDialog>("Edit Garage", parameters);

    }
}
