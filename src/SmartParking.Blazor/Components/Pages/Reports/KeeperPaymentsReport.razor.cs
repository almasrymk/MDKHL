
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
using SmartParking.StringLocalizer;
using SmartParking.Utilities;
using System.Composition;
using System.Globalization;
using Volo.Abp.MultiTenancy;
using static QRCoder.PayloadGenerator;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SmartParking.Blazor.Components.Pages.Reports;

partial class KeeperPaymentsReport
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
    public KeeperPaymentsReport(IStringLocalizer<SmartParkingResource> localizer)
    {
        _localizer = localizer;

    }
    private async Task CreateMenue(List<ColumnDef> columns)
    {
        var MenuDatasTable = new List<MudMenuData>
        {
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
    private async Task HandleSelectColumnVisibilityAsync(string valueOfMenue)
    {
        ColumnVisibility = valueOfMenue;
        await Task.Yield();
    }


    protected async override Task OnInitializedAsync()
    {


        if (!await AuthorizationService.IsGrantedAnyAsync(ReportsPermissions.SecurityManager) &&
            !await AuthorizationService.IsGrantedAnyAsync(ReportsPermissions.IT)&&
            !await AuthorizationService.IsGrantedAnyAsync(ReportsPermissions.FinancialManagement)
            )
        {

            Navigation.NavigateTo("/Account/AccessDenied", true);
        }
    }
    private async Task FetchData(PageChangeEventArgs paged)
    {

        var request = new PagedAndSortedAndFilteredResultRequestDto
        {
            MaxResultCount = paged.RowsPerPage,
            SkipCount = (paged.PageNumber - 1) * paged.RowsPerPage,
            Sorting = paged.SortColumn == "Id" ? "UserName" : paged.SortColumn,
            SearchingTerm = "",
            filters = paged.Filters

        };

        var keeperPaymentAnalyses = await AnalysisAppService.ReportPaymentCollectedMoneyWithKeeperAndShiftAsync(request, CurrentTenant.Id);
        TotalItem = (int)keeperPaymentAnalyses.TotalCount;
        if(CurrentTenant.Id == null)
        {
            var items = await CreateTableHelper.CreateDataTableForPaymentUserShift(keeperPaymentAnalyses.Items.ToList(),null);

        }
        else
        {
            var items = await CreateTableHelper.CreateDataTableForPaymentUserShift(keeperPaymentAnalyses.Items.ToList(), CurrentTenant.Id);

        }

        if (paged.IsExport)
        {
            itemDataIsExport.Columns = itemData.Columns;
            if (CurrentTenant.Id == null)
            {

                itemData = await CreateTableHelper.CreateDataTableForPaymentUserShift(keeperPaymentAnalyses.Items.ToList(),null);
                
            }
            else
            {
                var items = await CreateTableHelper.CreateDataTableForPaymentUserShift(keeperPaymentAnalyses.Items.ToList(), CurrentTenant.Id);

            }
            itemData.Columns = itemDataIsExport.Columns;
        }
        else if (!ColumnVisibility.IsNullOrWhiteSpace() && (IsEnter && paged.IsExport == false))
        {
            itemData.Columns.FirstOrDefault(c => c.DisplayName == ColumnVisibility).IsColumnVisible = !itemData.Columns.FirstOrDefault(c => c.DisplayName == ColumnVisibility).IsColumnVisible;
        }
        else
        {
            if (CurrentTenant.Id == null)
            {
                itemData = await CreateTableHelper.CreateDataTableForPaymentUserShift(keeperPaymentAnalyses.Items.ToList(),null);

            }
            else
            {
                itemData = await CreateTableHelper.CreateDataTableForPaymentUserShift(keeperPaymentAnalyses.Items.ToList(), CurrentTenant.Id);

            }
            IsEnter = true;
        }
        await CreateMenue(itemData.Columns);

    }
}
