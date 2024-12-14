using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Organizations;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Volo.Abp.Application.Dtos;
using System.Linq;
using Microsoft.Extensions.Localization;
using SmartParking.Localization;

namespace SmartParking.Blazor.Components.SPComponents;

public partial class SPDataGrid
{
    [Parameter] public string PageName { get; set; }
    [Parameter] public TableData Data { get; set; } = new TableData();
    [Parameter] public EventCallback<PageChangeEventArgs> OnPageChanged { get; set; }
    [Parameter] public EventCallback OnAdd { get; set; }
    [Parameter] public bool ShowAdd { get; set; } = true;
    [Parameter] public EventCallback<PageChangeEventArgs> OnSearching { get; set; }
    [Parameter] public bool ShowSearching { get; set; } = true;
    [Parameter] public EventCallback<PageChangeEventArgs> OnFiltering { get; set; }
    [Parameter] public bool ShowFiltering { get; set; } = true;
    [Parameter] public EventCallback<RowDef> OnClickRowEdit { get; set; }
    [Parameter] public EventCallback<RowDef> OnClickButton { get; set; }
    [Parameter] public bool ShowEdit { get; set; } = true;
    [Parameter] public bool ShowButton { get; set; } = true;
    [Parameter] public EventCallback<RowDef> OnClickCar { get; set; }
    [Parameter] public bool ShowAssignCar { get; set; } = true;
    [Parameter] public EventCallback<RowDef> OnClickRowView { get; set; }
    [Parameter]  public  bool ShowView { get; set; } = true;
    [Parameter] public EventCallback<RowDef> OnClickRowDelete { get; set; }
    public bool ShowDelete { get; set; } = true;
    [Parameter] public EventCallback<string> OnClickAssign { get; set; }

    [Parameter] public bool ShowAssign { get; set; } = true;
    [Inject] public IStringLocalizer<SmartParkingResource> _stringLocalizer { get; set; }

    public SPDataGrid(IStringLocalizer<SmartParkingResource> stringLocalizer)
    {
        _stringLocalizer = stringLocalizer;
    }

    public List<FilterRequestDto> ColumnsFilter { get; set; } = new List<FilterRequestDto>();
    private List<string> ColumnsFilterApply { get; set; } = new List<string>();
    [Parameter] public int TotalItemsInTable { get; set; }
    private string _searchString;
    private string sort;
    MudDataGrid<RowDef> dataGrid;
    private bool _ascending = true;
    public string sortString = "Id";
    public bool ClickFilter { get; set; } = false;
    private string SelectedColumn { get; set; }
    private KeyValuePair<string, string> SelectOperator { get; set; } = new KeyValuePair<string, string>();

    private bool IsNot = false;
    public Dictionary<string, string> StringOperatorEn { get; set; } = new Dictionary<string, string>
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
    public Dictionary<string, string> StringOperatorAR { get; set; } = new Dictionary<string, string>
    {
        ["يحتوي على"] = "Contains",
        ["لا يحتوي على"] = "Contains",
        ["يساوي"] = "Equals",
        ["لا يساوي"] = "Equals",
        ["يبدأ بـ"] = "StartsWith",
        ["لا يبدأ بـ"] = "EndsWith",
        ["فارغـًا"] = "IsNullOrEmpty",
        ["لا يكون فارغـًا"] = "IsNullOrEmpty"
    };
    private string FilterValue { get; set; }
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
        if(Data.Columns.FirstOrDefault(x => x.PropertyName == SelectedColumn).TypeFilter == TypeFilter.Number || Data.Columns.FirstOrDefault(x => x.PropertyName == SelectedColumn).TypeFilter == TypeFilter.Enum)
        {
            SelectOperator = new KeyValuePair<string, string>(SelectedColumn, FilterValue);
        }

		if (!SelectOperator.Key.IsNullOrEmpty()  && !SelectedColumn.IsNullOrEmpty() && (SelectOperator.Value.Contains("IsNullOrEmpty") ||!FilterValue.IsNullOrEmpty()))
        {
            if (SelectOperator.Key.Contains("not") || SelectOperator.Key.Contains("لا"))
                IsNot = true;
            var DisplayFilter = Data.Columns.FirstOrDefault(c => c.PropertyName == SelectedColumn);
            FilterRequestDto filterEventArgs = new FilterRequestDto()
            {
                FilterProperty = SelectedColumn,
                FilterValue = FilterValue,
                FilterOperator = SelectOperator.Value,
                IsNot = IsNot,
                TypeFilter = Data.Columns.FirstOrDefault(x => x.PropertyName == SelectedColumn).TypeFilter
            };
            ColumnsFilter.Add(filterEventArgs);
            ColumnsFilterApply.Add(DisplayFilter.Name);

            ClickFilter = false;
			SelectedColumn = "";
			FilterValue = "";
			SelectOperator = default;
			IsNot = false;
			await dataGrid.ReloadServerData();
            
        }
           
      
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
    protected override Task OnParametersSetAsync()
    {
        return base.OnParametersSetAsync();
    }

}