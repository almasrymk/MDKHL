
using Blazorise.DataGrid;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Pages.Organizations;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Blazor.Components.SPComponents;
using SmartParking.Cutomers;
using SmartParking.Entities.Analysis;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using SmartParking.Helpers;
using SmartParking.Localization;
using SmartParking.PDF;
using SmartParking.Permissions.PaymentsPermissions;
using SmartParking.Permissions.Reports;
using SmartParking.Results;
using SmartParking.Utilities;
using System.Composition;
using System.Globalization;
using Volo.Abp.MultiTenancy;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SmartParking.Blazor.Components.Pages.Reports;

partial class BeneficiariesReport
{
    public int CurrentPage { get; set; } = 1;
    public int RowPerPage { get; set; } = 10;
    private TableData itemData = new TableData();
    private TableData itemDataIsExport = new TableData();
    private int TotalItem;
    private SPReportResult sPReportResult { get; set; } = new SPReportResult();
    public string ColumnVisibility { get; private set; }
    public bool IsEnter { get; set; }
    private readonly IStringLocalizer<SmartParkingResource> _localizer;
    private List<MudMenuData> MenuDatas = new List<MudMenuData>();
    public BeneficiariesReport(IStringLocalizer<SmartParkingResource> localizer)
    {
        _localizer = localizer;

    }
    private async Task CreateMenue(List<ColumnDef> columns)
    {
        var MenuDatasTable = new List<MudMenuData>
        {
           await CreateReportsMenue() ,
           await CreateColumnVisibilityMenue(columns)


        };
        MenuDatas = MenuDatasTable;


    }
    private async Task<MudMenuData> CreateColumnVisibilityMenue(List<ColumnDef> columns)
    {
        var items = new List<MenuItem>();
        foreach (var item in columns)
        {
            items.Add(new MenuItem { MenueName = "ColumnVisibility", Label = item.DisplayName, ISColumnVisibility = true, OnClick = EventCallback.Factory.Create<string>(this, async (string selectedValue) => await HandleSelectColumnVisibilityAsync(selectedValue)) });
        }
        await Task.Yield();
        return new MudMenuData
        {
            Name = L["ColumnVisibility"],
            Label = L["ColumnVisibilitySelector"],
            Items = items,
            IsMenueColumnVisible = true
        };
    }
    private async Task<MudMenuData> CreateReportsMenue()
    {
        var itemsInMenue = new List<MenuItem>
        {
            new MenuItem { MenueName = "Reports",
                Label = LocalizationHelper.GetLocalizedString("CustomerAndTransactionReport"), ISColumnVisibility = false,
                OnClick = EventCallback.Factory.Create<string>(this, async (string selectedValue) => await CustomerAndTransactionReportsAsync("Customer"))
            },
            new MenuItem { MenueName = "Reports",
                Label = LocalizationHelper.GetLocalizedString("EmployeeAndTransactionReport"),
                ISColumnVisibility = true,
                OnClick = EventCallback.Factory.Create<string>(this, async (string selectedValue) => await CustomerAndTransactionReportsAsync("Employee")) }

        };

        if (await AuthorizationService.IsGrantedAnyAsync(ReportsPermissions.SecurityManager))
            itemsInMenue.Add(new MenuItem { MenueName = "Reports", Label = "VisitorAndTransactionReport", ISColumnVisibility = false, OnClick = EventCallback.Factory.Create<string>(this, async (string selectedValue) => await CustomerAndTransactionReportsAsync("Visitor")) });

        await Task.Yield();
        return new MudMenuData
        {
            Name = L["Reports"],
            Label = L["Reports"],
            Items = itemsInMenue,
            IsMenueColumnVisible = false,
            IsExport = true
            
        };
    }
    private async Task HandleSelectColumnVisibilityAsync(string valueOfMenue)
    {
        ColumnVisibility = valueOfMenue;
        await Task.Yield();
    }
    private async Task CustomerAndTransactionReportsAsync(string selectvalue)
    {
        try
        {

            var request = new PagedAndSortedAndFilteredResultRequestDto
            {
                MaxResultCount = 500,
                SkipCount = 0,
                Sorting = "GarageName",
                SearchingTerm = "",
                filters = new List<FilterRequestDto>
                {
                    new FilterRequestDto
                    {
                        FilterProperty ="GarageTransaction.CustomerCategory",
                         FilterValue = selectvalue,
                         IsNot = false,
                         TypeFilter = TypeFilter.Enum
                    }
                },
            };

            var garageTransactionAnalyses = await AnalysisAppService.ReportGarageTransactionAsync(request, CurrentTenant.Id);
            var items = await CreateTableHelper.CreateDataTableForGarageTransaction(garageTransactionAnalyses.Items.ToList());
            items.Name+=selectvalue;
            await ExportExcel(items);
        }
        catch
        {
            SnackBar.ShowSnackbar(Snackbar, LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.FAILED) + " " +
                        LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.EXPORT), Severity.Error);
        }
    }

    private async Task ExportExcel(TableData items)
    {
        var folderName = FileHelper.CreateImageDirectory("Folders/Excel");
        string file = System.IO.Path.Combine(folderName, $"{items.Name}.xlsx");

        await GenerateFilesAppService.ExportToExcel(items, file);

        SnackBar.ShowSnackbar(Snackbar, LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.SUCCESS) + " " +
                    LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.EXPORT), Severity.Success);

        await Task.Delay(1000);
        await OpenNewBlank($"Folders/Excel/{items.Name}.xlsx");
    }

    private async Task OpenNewBlank(string url)
    {

        await JSRuntime.InvokeVoidAsync("window.open", url, "_blank");
    }
    protected async override Task OnInitializedAsync()
    {


        if (!await AuthorizationService.IsGrantedAnyAsync(ReportsPermissions.SecurityManager) || !await AuthorizationService.IsGrantedAnyAsync(ReportsPermissions.IT))
        {

            Navigation.NavigateTo("/Account/AccessDenied", true);
        }
        await CreateMenue(itemData.Columns);
    }
    private async Task FetchData(PageChangeEventArgs paged)
    {

        var input = new PagedAndSortedAndFilteredResultRequestDto
        {
            MaxResultCount = paged.RowsPerPage,
            SkipCount = (paged.PageNumber - 1) * paged.RowsPerPage,
            Sorting = paged.SortColumn == "Id" ? "GarageName" : paged.SortColumn,
            filters = paged.Filters,
            SearchingTerm = paged.SearchTerm

        };
        var garageTransactionAnalyses = await AnalysisAppService.ReportGarageTransactionAsync(input, CurrentTenant.Id);
        TotalItem = (int)garageTransactionAnalyses.TotalCount;

        if (paged.IsExport)
        {
            itemDataIsExport.Columns = itemData.Columns;
            itemData = await CreateTableHelper.CreateDataTableForGarageTransaction(garageTransactionAnalyses.Items.ToList());
            itemData.Columns = itemDataIsExport.Columns;
        }
        else if (!ColumnVisibility.IsNullOrWhiteSpace() && (IsEnter && paged.IsExport == false))
        {
            itemData.Columns.FirstOrDefault(c => c.DisplayName == ColumnVisibility).IsColumnVisible = !itemData.Columns.FirstOrDefault(c => c.DisplayName == ColumnVisibility).IsColumnVisible;
        }
        else
        {
            itemData = await CreateTableHelper.CreateDataTableForGarageTransaction(garageTransactionAnalyses.Items.ToList());
            IsEnter = true;
        }
        await CreateMenue(itemData.Columns);

    }
}
