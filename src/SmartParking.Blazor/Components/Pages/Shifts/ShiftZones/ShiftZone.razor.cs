using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Blazor.Components.SPComponents;
using SmartParking.GarageTransactions;
using SmartParking.Localization;
using SmartParking.Shifts.ShiftZones;
using System.Globalization;
using System.Security.Authentication;
using Volo.Abp;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Blazor.Components.Pages.Shifts.ShiftZones;


public partial class ShiftZone
{
    private int TotalItem;
    public int CurrentPage { get; set; } = 1; 
    public int RowPerPage { get; set; } = 10; 

    private TableData itemData = new TableData();

    private string termSearch;
    public bool ShowAdd { get; set; } = true;
    private SPDataGrid spDataGrid;


    private DialogOptions options = new DialogOptions { CloseOnEscapeKey = true };

    private readonly IStringLocalizer<SmartParkingResource> _localizer;

    public ShiftZone(IStringLocalizer<SmartParkingResource> localizer)
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

        var input = new PagedAndSortedAndFilteredResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn,
            SearchingTerm = termSearch, 
            filters = pageInfo.Filters
        };

        if (CurrentTenant.Id != null)
        {
            var subscriptionTypeDto = await ShiftZoneAppService.FilterForMDKHL(input);


            TotalItem = (int)subscriptionTypeDto.TotalCount;
            await CreateTable(subscriptionTypeDto);
        }
        else
        {
            var subscriptionTypeDto = await ShiftZoneAppService.Filter(input);

            TotalItem = (int)subscriptionTypeDto.TotalCount;
            await CreateTable(subscriptionTypeDto);
        }
    }

    ///// <summary>
    ///// Searches for hotels on the server side using the hotel code search term.
    ///// </summary>
    ///// <param name="searchTerm">The term to search for among hotel.</param>
    ///// <returns>Returns a task that represents the asynchronous operation.</returns>
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

        if(CurrentTenant.Id != null)
        {
            var subscriptionTypeDto = await ShiftZoneAppService.SearchForMDKHL(input);
            TotalItem = (int)subscriptionTypeDto.TotalCount;
            await CreateTable(subscriptionTypeDto);
        }
        else
        {
            var subscriptionTypeDto = await ShiftZoneAppService.Search(input);
            TotalItem = (int)subscriptionTypeDto.TotalCount;
            await CreateTable(subscriptionTypeDto);
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
            sort = "Id";
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

        var ShiftStaffDetailDto = await ShiftZoneAppService.GetListAsync(input);
        TotalItem = (int)ShiftStaffDetailDto.TotalCount;
        await CreateTable(ShiftStaffDetailDto);

    }

    private async Task CreateTable(PagedResultDto<ShiftZoneDto> shiftStaffDetail)
    {
        var tableData = new TableData()
        {
            Name = L["ShiftStaffDetails"],
            ShowHeader = true
        };

        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Id"], Name = "Id", IsFilterable = false, PropertyName = "Id", IsSortable = true, TypeFilter = TypeFilter.Text });

        if (CultureInfo.CurrentCulture.Name == "ar")
            tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["ArabicName"], Name = "ArabicName", IsFilterable = true, PropertyName = "ArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });
        else
            tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["EnglishName"], Name = "FullName", IsFilterable = true, PropertyName = "IdentityUser.Name", IsSortable = true, TypeFilter = TypeFilter.Text });

        tableData.Columns.Add(new ColumnDef() { DisplayName = L["StartDate"], Name = "StartDate", IsFilterable = true, PropertyName = "StartDate", IsSortable = true, TypeFilter = TypeFilter.Date });
        tableData.Columns.Add(new ColumnDef() { DisplayName = L["EndDate"], Name = "EndDate", IsFilterable = true, PropertyName = "EndDate", IsSortable = true, TypeFilter = TypeFilter.Date });
        tableData.Columns.Add(new ColumnDef() { DisplayName = L["StartTime"], Name = "StartTime", IsFilterable = true, PropertyName = "StartTime", IsSortable = true, TypeFilter = TypeFilter.Date });
        tableData.Columns.Add(new ColumnDef() { DisplayName = L["EndTime"], Name = "EndTime", IsFilterable = true, PropertyName = "EndTime", IsSortable = true, TypeFilter = TypeFilter.Date });

        foreach (var item in shiftStaffDetail.Items)
        {
            var rowDef = new RowDef();
            rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
            if (CultureInfo.CurrentCulture.Name == "ar")
                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["ArabicName"], item.ShiftDto.ArabicName));
            else
                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["EnglishName"], item.ShiftDto.EnglishName));
            rowDef.Cells.Add(new KeyValuePair<string, string>(L["StartDate"], item.ShiftDto.StartDate.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(L["EndDate"], item.ShiftDto.EndDate.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(L["StartTime"], item.ShiftDto.IsFullDay ? 24 + L[$"Hour"] : $" {item.ShiftDto.StartTime}"));
            rowDef.Cells.Add(new KeyValuePair<string, string>(L["EndTime"], item.ShiftDto.IsFullDay ? 24 + L[$"Hour"] : $" {item.ShiftDto.EndTime}")); tableData.Rows.Add(rowDef);
        }


        itemData = tableData;
        await Task.Yield();
    }

    private async Task CreateNewShiftStaff()
    {
        var dialogRef = await Dialog.ShowAsync<AddShiftZoneDialog>(L["AddShiftZone"], options);
        var dialogResult = await dialogRef.Result;

        if (dialogResult.Canceled) return;

        if (dialogResult.Data is Guid newId)
        {
            var created = await ShiftZoneAppService.GetAsync(newId);
            if (created != null)
            {
                var newRow = new RowDef();
                newRow.Cells.Add(new(nameof(created.Id), created.Id.ToString()));
                newRow.Cells.Add(
                    CultureInfo.CurrentCulture.Name == "ar"
                    ? new(_localizer["ArabicName"], created.ShiftDto?.ArabicName ?? "")
                    : new(_localizer["EnglishName"], created.ShiftDto?.EnglishName ?? "")
                );
                newRow.Cells.Add(new(L["StartDate"], created.ShiftDto?.StartDate.ToString() ?? ""));
                newRow.Cells.Add(new(L["EndDate"], created.ShiftDto?.EndDate.ToString() ?? ""));
                newRow.Cells.Add(new(L["StartTime"], created.ShiftDto?.IsFullDay == true ? 24 + L["Hour"] : $"{created.ShiftDto?.StartTime}"));
                newRow.Cells.Add(new(L["EndTime"], created.ShiftDto?.IsFullDay == true ? 24 + L["Hour"] : $"{created.ShiftDto?.EndTime}"));

                var newTable = new TableData
                {
                    Name = itemData.Name,
                    ShowHeader = itemData.ShowHeader,
                    Columns = itemData.Columns.ToList(),
                    Rows = itemData.Rows.ToList()
                };
                newTable.Rows.Insert(0, newRow);
                itemData = newTable;
                TotalItem++;

                await spDataGrid.Reload();
                return;
            }
        }

        await spDataGrid.Reload();
    }

    private async Task<IDialogReference> OnClickRowEdit(RowDef row)
    {
        var id = Guid.Parse(row.Cells[0].Value);

        var parameters = new DialogParameters
        {
            ["Id"] = id
        };

        var dialog = await Dialog.ShowAsync<UpdateShiftZoneDialog>(L["EditShiftZone"], parameters, options);
        var result = await dialog.Result;

        if (!result.Canceled)
        {
            await spDataGrid.Reload();
        }

        return dialog;
    }
    private async Task<IDialogReference> OnClickRowView(RowDef row)
    {
        var id = Guid.Parse(row.Cells[0].Value);

        var shiftZoneDto = await ShiftZoneAppService.GetAsync(id);

        var parameters = new DialogParameters
        {
            ["ShiftZone"] = shiftZoneDto
        };

        return await Dialog.ShowAsync<ViewShiftZoneDialog>("View Shift Zone", parameters);
    }
    private async Task OnClickRowDelete(RowDef row)
    {

        var id = Guid.Parse(row.Cells[0].Value);
        var Name = row.Cells[1].Value;

        var parameters = new DialogParameters
        {
            ["Name"] = Name,

        };
        var dialog = await Dialog.ShowAsync<SPDialogDelete>("Shift Zone Delete Dialog", parameters);
        var result = await dialog.Result;
        try
        {
            if (!result.Canceled)
            {
                await ShiftZoneAppService.DeleteAsync(id);
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]} {L["ShiftZoneDeleted"]}", Severity.Success);

                await spDataGrid.Reload();
            }
        }
        catch (BusinessException ex)
        {

            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.DELETE]} : {ex.Message}", Severity.Error);
        }
        catch (AuthenticationException ex)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.DELETE]} : {ex.Message} ", Severity.Error);
        }
        catch (Exception ex)
        {
            if (ex.Message == "")
            {
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.DELETE]} : {L["ShiftZoneDeleteMessage"]} ", Severity.Error);

            }
            else
            {
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.DELETE]} : {ex.Message} ", Severity.Error);
            }
        }
    }
}