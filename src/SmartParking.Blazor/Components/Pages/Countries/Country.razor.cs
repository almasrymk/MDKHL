using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Blazor.Components.SPComponents;
using SmartParking.Localization;
using SmartParking.Lookups;
using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Blazor.Components.Pages.Countries;

/// <summary>
/// Handles the country management functionality, including fetching, filtering, searching, and managing country data.
/// This class manages the data displayed in a table, including sorting, filtering, and paginated results.
/// </summary>
public partial class Country
{
    private int TotalItem;
    public int CurrentPage { get; set; } = 1;
    public int RowPerPage { get; set; } = 10;
    private TableData itemData = new TableData();
    private string termSearch;
    private DialogOptions options = new DialogOptions { CloseOnEscapeKey = true };

    private readonly IStringLocalizer<SmartParkingResource> _localizer;

    /// <summary>
    /// Initializes the <see cref="Country"/> class with a localizer for translations.
    /// </summary>
    /// <param name="localizer">The localizer instance used for string translations.</param>
    public Country(IStringLocalizer<SmartParkingResource> localizer)
    {
        _localizer = localizer;
    }

    /// <summary>
    /// Filters the countries on the server side based on provided filter parameters.
    /// This method is used for server-side filtering of country data based on the specified columns.
    /// </summary>
    /// <param name="pageInfo">The page change event that contains the filtering, sorting, and paging information.</param>
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
        var countriesDto = await AppService.Filter(input);
        TotalItem = (int)countriesDto.TotalCount;
        CreateTable(countriesDto);
    }

    /// <summary>
    /// Searches for countries on the server side using the search term.
    /// This method is used for searching countries based on a search term provided by the user.
    /// </summary>
    /// <param name="pageInfo">The page change event that contains the search term, paging, and sorting information.</param>
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
        var countryDto = await AppService.Search(input);
        TotalItem = (int)countryDto.TotalCount;
        CreateTable(countryDto);
    }

    /// <summary>
    /// Fetches the table data for the specified page and updates the table with the retrieved data.
    /// This method handles paginated fetching of country data.
    /// </summary>
    /// <param name="pageInfo">The page change event that contains the paging and sorting information.</param>
    private async Task FetchTableData(PageChangeEventArgs pageInfo)
    {
        CurrentPage = pageInfo.PageNumber;
        RowPerPage = pageInfo.RowsPerPage;
        var input = new PagedAndSortedAndSearchResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn , 
            SearchingTerm = termSearch
        };
        var countriesDto = await AppService.GetListAsync(input);
        TotalItem = (int)countriesDto.TotalCount;
        CreateTable(countriesDto);
    }

    /// <summary>
    /// Creates a table from the provided list of countries.
    /// This method generates the table structure, including columns and rows for displaying country information.
    /// </summary>
    /// <param name="countries">The country data used to populate the table.</param>
    private void CreateTable(PagedResultDto<CountryDto> countries)
    {
        var tableData = new TableData()
        {
            Name = L["Countries"],
            ShowHeader = true
        };

        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Id"], Name = "Id", IsFilterable = false, PropertyName = "Id", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["ArabicName"], Name = "ArabicName", IsFilterable = true, PropertyName = "ArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["EnglishName"], Name = "EnglishName", IsFilterable = true, PropertyName = "EnglishName", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Arabic Capital"], Name = "ArabicCapital", IsFilterable = true, PropertyName = "ArabicCapital", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["English Capital"], Name = "EnglishCapital", IsFilterable = true, PropertyName = "EnglishCapital", IsSortable = true, TypeFilter = TypeFilter.Text });

        foreach (var item in countries.Items)
        {
            var rowDef = new RowDef();
            rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["ArabicName"], item.ArabicName));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["EnglishName"], item.EnglishName));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Arabic Capital"], item.ArabicCapital));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["English Capital"], item.EnglishCapital));
            tableData.Rows.Add(rowDef);
        }
        itemData = tableData;
    }

    /// <summary>
    /// Opens a dialog for creating a new country.
    /// </summary>
    private Task CreateCountry()
    {
        return DialogService.ShowAsync<AddCountryDialog>("Simple Dialog", options);
    }

    /// <summary>
    /// Opens a dialog for editing an existing country.
    /// </summary>
    /// <param name="row">The row of the country to edit.</param>
    private async Task<IDialogReference> OnClickRowEdit(RowDef row)
    {
        var id = int.Parse(row.Cells[0].Value);
        var countryDto = await AppService.GetAsync(id);
        var createUpdateCountry = Mapper.Map<CreateUpdateCountryDto>(countryDto);

        var parameters = new DialogParameters
        {
            ["country"] = countryDto,
            ["Id"] = id
        };

        return await DialogService.ShowAsync<EditCountryDialog>("Edit Country", parameters);
    }

    /// <summary>
    /// Opens a dialog to view the details of a country.
    /// </summary>
    /// <param name="row">The row of the country to view.</param>
    private async Task<IDialogReference> OnClickRowView(RowDef row)
    {
        var id = int.Parse(row.Cells[0].Value);
        var countriesDto = await AppService.GetAsync(id);
        var parameters = new DialogParameters
        {
            ["country"] = countriesDto,
        };
        return await DialogService.ShowAsync<DetailsCountryDialog>("View Country", parameters);
    }

    /// <summary>
    /// Deletes a country based on the specified row information.
    /// Opens a dialog to confirm the deletion and performs the delete operation.
    /// </summary>
    /// <param name="row">The row of the country to delete.</param>
    private async Task OnClickRowDelete(RowDef row)
    {
        var id = int.Parse(row.Cells[0].Value);
        var Name = row.Cells[2].Value;
        var parameters = new DialogParameters
        {
            ["Name"] = Name,
        };
        var dialog = await DialogService.ShowAsync<SPDialogDelete>("DeleteCountry", parameters);
        var result = await dialog.Result;

        try
        {
            if (!result.Canceled)
            {
                await AppService.DeleteAsync(id);
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]} {L[AlertMessages.AlertMessages.DELETE]} {Name}", Severity.Success);
                await Task.Delay(1000);
                Navigation.NavigateTo("/countries", forceLoad: true);
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
