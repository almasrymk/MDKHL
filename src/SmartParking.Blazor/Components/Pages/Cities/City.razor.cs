using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Blazor.Components.SPComponents;
using SmartParking.Cameras;
using SmartParking.Localization;
using SmartParking.Lookups;
using SmartParking.Lookups.CameraBrands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.Cities;

/// <summary>
/// Blazor component for managing city records, including search, filter, pagination, and CRUD operations.
/// </summary>
public partial class City
{
    private int TotalItem;
    public int CurrentPage { get; set; } = 1;

    public int RowPerPage { get; set; } = 10;

    private TableData itemData = new TableData();

    private string termSearch;

    private DialogOptions options = new DialogOptions { CloseOnEscapeKey = true };

    private readonly IStringLocalizer<SmartParkingResource> _localizer;
    public City(IStringLocalizer<SmartParkingResource> localizer)
    {
        _localizer = localizer;
    }

    /// <summary>
    /// Filters city records based on provided column filters.
    /// </summary>
    /// <param name="pageInfo">Pagination and filter information.</param>
    private async Task HandleFilter(PageChangeEventArgs pageInfo)
    {
        var input = new PagedAndSortedAndFilteredResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn,
            SearchingTerm = termSearch,
            filters = pageInfo.Filters
        };

        var citiesDto = await AppService.Filter(input);
        TotalItem = (int)citiesDto.TotalCount;
        CreateTable(citiesDto);
    }

    /// <summary>
    /// Searches cities using a search term.
    /// </summary>
    /// <param name="pageInfo">Pagination and search term data.</param>
    private async Task HandleSearch(PageChangeEventArgs pageInfo)
    {
        termSearch = pageInfo.SearchTerm;

        var input = new PagedAndSortedAndSearchResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn,
            SearchingTerm = termSearch
        };

        var citiesDto = await AppService.Search(input);
        TotalItem = (int)citiesDto.TotalCount;
        CreateTable(citiesDto);
    }

    /// <summary>
    /// Retrieves paginated city data from the server.
    /// </summary>
    /// <param name="pageInfo">Information about the current page.</param>
    private async Task FetchTableData(PageChangeEventArgs pageInfo)
    {
        CurrentPage = pageInfo.PageNumber;
        RowPerPage = pageInfo.RowsPerPage;

        var input = new PagedAndSortedResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn
        };

        var citiesDto = await AppService.GetListAsync(input);
        TotalItem = (int)citiesDto.TotalCount;
        CreateTable(citiesDto);
    }

    /// <summary>
    /// Builds the table UI from a paginated result of cities.
    /// </summary>
    /// <param name="cities">Paginated result of cities.</param>
    private void CreateTable(PagedResultDto<CityDto> cities)
    {
        var tableData = new TableData
        {
            Name = L["Cities"],
            ShowHeader = true
        };

        tableData.Columns.Add(new ColumnDef { DisplayName = _localizer["Id"], Name = "Id", PropertyName = "Id", IsSortable = true, IsFilterable = false, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef { DisplayName = _localizer["ArabicName"], Name = "City Arabic Name", PropertyName = "ArabicName", IsSortable = true, IsFilterable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef { DisplayName = _localizer["EnglishName"], Name = "City English Name", PropertyName = "EnglishName", IsSortable = true, IsFilterable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef { DisplayName = _localizer["StateArabicName"], Name = "State Arabic Name", PropertyName = "State.ArabicName", IsSortable = true, IsFilterable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef { DisplayName = _localizer["StateEnglishName"], Name = "State English Name", PropertyName = "State.EnglishName", IsSortable = true, IsFilterable = true, TypeFilter = TypeFilter.Text });

        foreach (var item in cities.Items)
        {
            var rowDef = new RowDef();
            rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["ArabicName"], item.ArabicName));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["EnglishName"], item.EnglishName));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["StateArabicName"], item.StateArabicName));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["StateEnglishName"], item.StateEnglishName));
            tableData.Rows.Add(rowDef);
        }

        itemData = tableData;
    }

    /// <summary>
    /// Opens a dialog to create a new city.
    /// </summary>
    private Task CreateCity()
    {
        return Dialog.ShowAsync<AddCityDialog>("Simple Dialog", options);
    }

    /// <summary>
    /// Opens the edit dialog for a selected city row.
    /// </summary>
    /// <param name="row">The selected row to edit.</param>
    /// <returns>A dialog reference to the edit dialog.</returns>
    private async Task<IDialogReference> OnClickRowEdit(RowDef row)
    {
        var id = int.Parse(row.Cells[0].Value);
        var cityDto = await AppService.GetAsync(id);

        var parameters = new DialogParameters
        {
            ["City"] = cityDto,
            ["Id"] = id
        };

        return await Dialog.ShowAsync<EditCityDialog>("Edit City", parameters);
    }

    /// <summary>
    /// Opens a view-only dialog for a selected city row.
    /// </summary>
    /// <param name="row">The selected row to view.</param>
    /// <returns>A dialog reference to the view dialog.</returns>
    private async Task<IDialogReference> OnClickRowView(RowDef row)
    {
        var id = int.Parse(row.Cells[0].Value);
        var cityDto = await AppService.GetAsync(id);

        var parameters = new DialogParameters
        {
            ["City"] = cityDto
        };

        return await Dialog.ShowAsync<DetailsCityDialog>("View City", parameters);
    }

    /// <summary>
    /// Deletes a selected city row after confirmation dialog.
    /// </summary>
    /// <param name="row">The selected row to delete.</param>
    private async Task OnClickRowDelete(RowDef row)
    {
        var id = int.Parse(row.Cells[0].Value);
        var Name = row.Cells[2].Value;

        var parameters = new DialogParameters
        {
            ["Name"] = Name
        };

        var dialog = await Dialog.ShowAsync<SPDialogDelete>("DeleteCity", parameters);
        var result = await dialog.Result;

        try
        {
            if (!result.Canceled)
            {
                await AppService.DeleteAsync(id);
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]} {L[AlertMessages.AlertMessages.DELETE]} {Name}", Severity.Success);
                await Task.Delay(1000);
                Navigation.NavigateTo("/cities", forceLoad: true);
            }
        }
        catch (BusinessException ex)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.DELETE]}: {ex.Code}", Severity.Error);
        }
        catch (AuthenticationException ex)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.DELETE]}", Severity.Error);
        }
        catch (Exception ex)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.DELETE]}", Severity.Error);
        }
    }
}

