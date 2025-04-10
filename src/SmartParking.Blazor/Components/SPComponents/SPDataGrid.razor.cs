using Blazorise.DataGrid;
using Humanizer.Localisation;
using iText.Html2pdf;
using iText.IO.Font;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Annot;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Font;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using MudBlazor;
using OfficeOpenXml;
using Polly;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Enums;
using SmartParking.Events;
using SmartParking.Localization;
using SmartParking.Utilities;

namespace SmartParking.Blazor.Components.SPComponents;

public partial class SPDataGrid
{
    [Parameter] public List<MudMenuData> Menus { get; set; } = new List<MudMenuData>();
    [Parameter] public string PageName { get; set; }
    [Parameter] public TableData Data { get; set; } = new TableData();
    public IList<RowDef> Rows { get; set; } = new List<RowDef>();
    [Parameter] public EventCallback<PageChangeEventArgs> OnPageChanged { get; set; }
    [Parameter] public EventCallback OnAdd { get; set; }
    [Parameter] public bool ShowAdd { get; set; } = true;
    [Parameter] public EventCallback<PageChangeEventArgs> OnSearching { get; set; }
    [Parameter] public bool ShowSearching { get; set; } = true;
    [Parameter] public EventCallback<PageChangeEventArgs> OnFiltering { get; set; }
    [Parameter] public bool ShowFiltering { get; set; } = false;
    [Parameter] public bool ShowCustomerTransactions { get; set; } = false;
    [Parameter] public bool ShowVehicleTransactions { get; set; } = false;
    [Parameter] public EventCallback<RowDef> ShowCustomerTransactionsMethod { get; set; }
    [Parameter] public EventCallback<(RowDef, string)> ShowVehicleTransactionsMethod { get; set; }
    [Parameter] public EventCallback<RowDef> OnClickRowEdit { get; set; }
    [Parameter] public EventCallback<RowDef> OnClickButton { get; set; }
    [Parameter] public bool ShowEdit { get; set; } = true;
    [Parameter] public bool ShowButton { get; set; } = true;
    [Parameter] public bool ShowExport { get; set; } = true;
    [Parameter] public bool ShowMenue { get; set; } = false;
    [Parameter] public EventCallback<RowDef> OnClickCar { get; set; }
    [Parameter] public bool ShowAssignCar { get; set; } = true;
    [Parameter] public EventCallback<RowDef> OnClickRowView { get; set; }
    [Parameter] public bool ShowView { get; set; } = true;
    [Parameter] public EventCallback<RowDef> OnClickRowDelete { get; set; }
    [Parameter] public bool ShowDelete { get; set; } = true;
    [Parameter] public EventCallback<string> OnClickAssign { get; set; }
    [Parameter] public EventCallback<RowDef> OnClickStart { get; set; }
    [Parameter] public EventCallback<RowDef> OnClickEnd { get; set; }

    [Parameter] public bool ShowAssign { get; set; } = true;
    [Parameter] public bool ShowStart { get; set; } = true;
    [Parameter] public bool ShowEnd { get; set; } = true;
    [Parameter] public EventCallback<RowDef> OnClickViewLogs { get; set; }
    [Parameter] public bool ShowViewLogs { get; set; } = true;
    [Parameter] public EventCallback<RowDef> OnClickViewUserShifts { get; set; }
    [Parameter] public bool ShowUserShifts { get; set; } = true;
    [Inject] public IStringLocalizer<SmartParkingResource> _stringLocalizer { get; set; }

    [Parameter] public EventCallback<string> HandleCategory { get; set; }

    public List<FilterRequestDto> ColumnsFilter { get; set; } = new List<FilterRequestDto>();
    private List<string> ColumnsFilterApply { get; set; } = new List<string>();
    [Parameter] public int TotalItemsInTable { get; set; }
    public string _searchString { get; set; } = string.Empty;
    private string sort;
    public MudDataGrid<RowDef> dataGrid { get; set; }
    private bool _ascending = true;
    public string sortString = "Id";
    public bool ClickFilter { get; set; } = false;
    private string SelectedColumn { get; set; }

    private bool IsShowValue  { get; set; } = true;
    private ColumnDef SelectedColumnDef { get; set; } = new ColumnDef();
    private KeyValuePair<string, string> SelectOperator { get; set; } = new KeyValuePair<string, string>();
    private List<KeyValuePair<string, string>> SelectProprtyName { get; set; } = new List<KeyValuePair<string, string>>();

    private bool IsNot = false;
    private bool IsClickMenu = false;
    private string ValueOfMenue { get; set; } = "All";
    public DateTime? Date { get; set; }
    public TimeSpan? Time { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }   
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public Dictionary<string, string> StringOperatorEn { get; set; } = new Dictionary<string, string>
    {
        ["contains"] = "Contains",
        ["not contains"] = "Contains",
        ["equals"] = "Equals",
        ["not equals"] = "Equals",
        ["starts with"] = "StartsWith",
        ["ends with"] = "EndsWith",
        ["is empty"] = "IsNullOrEmpty",
        ["is not empty"] = "IsNullOrEmpty",
        ["in"] = "In"
    };    
    
    public Dictionary<string, string> DateOperatorEn { get; set; } = new Dictionary<string, string>
    {
        ["greater than"] = "GreaterThan",
        ["greater than or equal"] = "GreaterThanOrEqual",
        ["less than"] = "LessThan",
        ["less than or equal"] = "LessThanOrEqual",
        ["between"] = "Between",
    };
    
    public Dictionary<string, string> NumberOperatorEn { get; set; } = new Dictionary<string, string>
    {
        ["greater than"] = "GreaterThan",
        ["less than"] = "LessThan",
        ["equals"] = "Equals"
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
        ["لا يكون فارغـًا"] = "IsNullOrEmpty",
        ["في"] = "In"
    };
    
    public Dictionary<string, string> DateOperatorAR { get; set; } = new Dictionary<string, string>
    {
        ["اكبر من"] = "GreaterThan",
        ["اكبر من او يساوي"] = "GreaterThanOrEqual",
        ["اقل من"] = "LessThan",
        [" اقل من او يساوي"] = "LessThanOrEqual",
        ["بين"] = "Between",
    };
    
    public Dictionary<string, string> NumberOperatorAR { get; set; } = new Dictionary<string, string>
    {
        ["اكبر من"] = "GreaterThan",
        ["اقل من"] = "LessThan",
        ["يساوي"] = "Equals"
    };
    private string FilterValue { get; set; }
    public bool IsExport { get; private set; }

    protected override Task OnInitializedAsync()
    {
        Rows = Data.Rows;
        return base.OnInitializedAsync();
    }
    public SPDataGrid(IStringLocalizer<SmartParkingResource> stringLocalizer)
    {
        dataGrid = new MudDataGrid<RowDef>();
        _stringLocalizer = stringLocalizer;
    }

    public void OnSelectColumn(string selectColumn)
    {
        SelectedColumn = selectColumn;
        if(Data.Columns.FirstOrDefault(c => c.PropertyName == SelectedColumn).TypeFilter == TypeFilter.Enum)
        IsShowValue = false;

    }
    public async Task ClearItem(string key)
    {
        var removeItem = SelectProprtyName.FirstOrDefault(c => c.Key == key); ;
        ColumnsFilterApply.Remove(key);
        ColumnsFilter.Remove(ColumnsFilter.FirstOrDefault(x => x.FilterProperty == removeItem.Value));
        if (ColumnsFilterApply.Count == 0)
        {

            ClearAll();
        }
        await dataGrid.ReloadServerData();
    }

    public async void ClearAll()
    {

        ColumnsFilter.Clear();
        ColumnsFilterApply.Clear();
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
        if (!SelectedColumn.IsNullOrEmpty())
        {
            if (IsShowValue && (SelectOperator.Key.Contains("not") || SelectOperator.Key.Contains("لا"))  )
                IsNot = true;
            var DisplayFilter = Data.Columns.FirstOrDefault(c => c.PropertyName == SelectedColumn);
            
            FilterRequestDto filterEventArgs = new FilterRequestDto()
            {
                FilterProperty = SelectedColumn,
                FilterValue = FilterValue,
                FilterOperator = SelectOperator.Value??"",
                IsNot = IsNot,
                TypeFilter = Data.Columns.FirstOrDefault(x => x.PropertyName == SelectedColumn).TypeFilter,
                Date = Date ?? DateTime.MinValue,
                EndDate = EndDate ?? DateTime.MinValue,
                StartDate = StartDate ?? DateTime.MinValue,
                Time = Time ?? TimeSpan.MinValue,
                EndTime = EndTime ??TimeSpan.MaxValue,
                StartTime = StartTime ?? TimeSpan.MinValue,
            };
            if (IsShowValue && SelectOperator.Value.Contains("In"))
            {
                var values = FilterValue.Split(',')
                                .Select(x => x.Trim())
                                .ToList();
                filterEventArgs.FilterValues = values;
            }            
            ColumnsFilter.Add(filterEventArgs);
            ColumnsFilterApply.Add(DisplayFilter.Name);
            SelectProprtyName.Add(new KeyValuePair<string, string>(DisplayFilter.Name, filterEventArgs.FilterProperty));
            ClickFilter = false;
            SelectedColumn = "";
            FilterValue = "";
            SelectOperator = default;
            IsNot = false;
            IsShowValue = true;
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
            Filters = ColumnsFilter,
            IsExport = IsExport

        };

        await OnPageChanged.InvokeAsync(pageArgs);

        if (!_searchString.IsNullOrEmpty() && !IsClickMenu)
        {
            await OnSearching.InvokeAsync(pageArgs);
        }
        if (ColumnsFilter.Count > 0)
        {
            await OnFiltering.InvokeAsync(pageArgs);
        }
        if (IsClickMenu)
        {
            await HandleCategory.InvokeAsync(ValueOfMenue);
        }
        return new GridData<RowDef>
        {
            TotalItems = TotalItemsInTable,
            Items = Data.Rows
        };
    }
    public async Task ExportToExcel()
    {
        try
        {
            GridState<RowDef> state = new GridState<RowDef>();
            state.PageSize = int.MaxValue - 1;
            IsExport = true;
            await ServerReload(state);
            var folderName = FileHelper.CreateImageDirectory("Folders/Excel");
            string file = System.IO.Path.Combine(folderName, $"{Data.Name}.xlsx");

            await GenerateFilesAppService.ExportToExcel(Data, file);

            SnackBar.ShowSnackbar(Snackbar, LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.SUCCESS) + " " +
                        LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.EXPORT), Severity.Success);

            await Task.Delay(1000);
            await OpenNewBlank($"Folders/Excel/{Data.Name}.xlsx");
            IsExport = false;
        }
        catch
        {
            SnackBar.ShowSnackbar(Snackbar, LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.FAILED) + " " +
                        LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.EXPORT), Severity.Error);
        }


    }

    private async Task ExportToCSV()
    {
        try
        {
            GridState<RowDef> state = new GridState<RowDef>();
            state.PageSize = int.MaxValue - 1;
            IsExport = true;
            await ServerReload(state);
            var folderName = FileHelper.CreateImageDirectory("Folders/Csv");
            string file = System.IO.Path.Combine(folderName, $"{Data.Name}.csv");

            await GenerateFilesAppService.ExportToExcel(Data, file);

            SnackBar.ShowSnackbar(Snackbar, LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.SUCCESS) + " " +
                        LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.EXPORT), Severity.Success);
            await Task.Delay(1000);
            await OpenNewBlank($"Folders/Csv/{Data.Name}.csv");
            IsExport = false;

        }
        catch
        {
            SnackBar.ShowSnackbar(Snackbar, LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.FAILED) + " " +
                        LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.EXPORT), Severity.Error);
        }
    }

    private async Task OpenNewBlank(string url)
    {

        await JSRuntime.InvokeVoidAsync("window.open", url, "_blank");
    }

    private async Task ExportToPDF()
    {
        try
        {
            await _eventBus.PublishAsync(new FileExportEvent(EventStatus.Started.ToString(), LocalizationHelper.GetLocalizedString("exportstarted")));

            GridState<RowDef> state = new GridState<RowDef>();
            state.PageSize = int.MaxValue - 1;
            IsExport = true;
            await ServerReload(state);
            var folderName = FileHelper.CreateImageDirectory("Folders/PDF");
            string file = System.IO.Path.Combine(folderName, $"{Data.Name}.pdf");
            await GenerateFilesAppService.ExportToPdf(Data, file);

            //SnackBar.ShowSnackbar(Snackbar, LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.SUCCESS) + " " +
            //            LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.EXPORT), Severity.Success);
            await Task.Delay(1000);
            await OpenNewBlank($"Folders/PDF/{Data.Name}.pdf");
            IsExport = false;

        }
        catch
        {
            SnackBar.ShowSnackbar(Snackbar, LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.FAILED) + " " +
                        LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.EXPORT), Severity.Error);
        }


    }

    public async Task HandleMunue(string value, EventCallback<string> action, bool isExport)
    {
        HandleCategory = action;
        IsClickMenu = true;
        ValueOfMenue = value;

        await action.InvokeAsync(value);
        if (!isExport)
            await dataGrid.ReloadServerData();
        else
            IsClickMenu = false;

    }
    public async Task HandleMunueColumnVisibility(string columnSelect, EventCallback<string> action)
    {

        SelectedColumnDef = Data.Columns.FirstOrDefault(c => c.DisplayName == columnSelect);
        await action.InvokeAsync(columnSelect);
        await dataGrid.ReloadServerData();
    }
}

