using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Organizations;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Volo.Abp.Application.Dtos;
using System.Linq;

namespace SmartParking.Blazor.Components.SPComponents;

public partial class SPDataGrid
{
    [Parameter] public string PageName { get; set; }
    [Parameter] public TableData Data { get; set; } = new TableData();
    [Parameter] public EventCallback<PageChangeEventArgs> OnPageChanged { get; set; }
    [Parameter] public EventCallback<PageChangeEventArgs> OnSearching { get; set; }
    [Parameter] public EventCallback<PageChangeEventArgs> OnFiltering { get; set; }
    public List<FilterRequestDto> ColumnsFilter { get; set; } = new List<FilterRequestDto>();
    private List<string> ColumnsFilterApply { get; set; } = new List<string>();
    [Parameter] public int TotalItemsInTable { get; set; }
    private string _searchString;
    private string sort;
    MudDataGrid<RowDef> dataGrid;
    private bool _ascending = true;
    public string sortString = "Code";
    public bool ClickFilter { get; set; } = false;
    private string SelectedColumn { get; set; }
    private KeyValuePair<string, string> SelectOperator { get; set; } = new KeyValuePair<string, string>();

    private bool IsNot = false;
    public Dictionary<string, string> StringOperator { get; set; } = new Dictionary<string, string>
    {
        ["contains"] = "Contains",
        ["not contains"] = "Contains",
        ["equals"] = "Equals",
        ["not equals"] = "Equals",
        ["starts with"] = "StartsWith",
        ["ends with"] = "EndsWith",
        ["is empty"] = "IsNullOrEmpty",
        ["is not empty"] = "IsNullOrEmpty"
    };
    private string FilterValue { get; set; }
    private void HandleAdd()
    {
        var uri = "/" + PageName + "/" + "add";
        Navigation.NavigateTo(uri, forceLoad: true); // Reload the page to apply the language change
    }
    public async Task ClearItem(string key)
    {


        ColumnsFilterApply.Remove(key);
        if (ColumnsFilterApply.Count == 0)
        {

            ClearAll();
        }
        await dataGrid.ReloadServerData();
    }

    public async void ClearAll()
    {

        ColumnsFilter.Clear();
        ClickFilter = false;
        await dataGrid.ReloadServerData();
    }
    private void CloseFilter()
    {
        SelectedColumn = "";
        FilterValue = "";
        ClickFilter = false;
    }

    public async Task ApplyFilter()
    {
        if (SelectOperator.Key.Contains("not"))
            IsNot = true;
        FilterRequestDto filterEventArgs = new FilterRequestDto()
        {
            FilterProperty = SelectedColumn,
            FilterValue = FilterValue,
            FilterOperator = SelectOperator.Value,
            IsNot = IsNot,
            TypeFilter = Data.Columns.FirstOrDefault(x => x.PropertyNmae == SelectedColumn).TypeFilter
        };
        ColumnsFilter.Add(filterEventArgs);
        ColumnsFilterApply.Add(SelectedColumn);
        ClickFilter = false;
        await dataGrid.ReloadServerData();
        SelectedColumn = "";
        FilterValue = "";
        SelectOperator = default;
        IsNot = false;
    }
    private void OnClickFilter()
    {
        ClickFilter = true;
    }
    private async Task OnSearch(string text)
    {
        _searchString = text;


        await dataGrid.ReloadServerData();
    }
    private async Task SortTable(string column)
    {


        if (sort == column)
        {
            _ascending = !_ascending;
        }
        else
        {
            sort = column;
            _ascending = true;
        }

        var sortDirection = _ascending ? "" : "DESC";
        var sorting = $"{sort} {sortDirection}";

        sortString = sorting;
        Console.WriteLine(sortString);
        await dataGrid.ReloadServerData();
    }
    private async Task<GridData<RowDef>> ServerReload(GridState<RowDef> state)
    {
        var pageArgs = new PageChangeEventArgs()
        {
            PageNumber = state.Page + 1,
            RowsPerPage = state.PageSize,
            SortColumn = sortString,
            SearchTerm = _searchString,
            Filters = ColumnsFilter
        };
        await OnPageChanged.InvokeAsync(pageArgs);
        if (!_searchString.IsNullOrEmpty())
        {
            await OnSearching.InvokeAsync(pageArgs);
        }
        if (ColumnsFilter.Count > 0)
        {
            await OnFiltering.InvokeAsync(pageArgs);
        }
        return new GridData<RowDef>
        {
            TotalItems = TotalItemsInTable,
            Items = Data.Rows
        };
    }

}