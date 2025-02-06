using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Blazor.Components.SPComponents;
using SmartParking.CustomerPackages;
using SmartParking.Entities.Analysis;
using SmartParking.Enums;
using SmartParking.Helpers;
using SmartParking.Localization;
using SmartParking.Permissions.Reports;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Blazor.Components.Pages.Reports;

public partial class CustomerPackage
{
    public int CurrentPage { get; set; } = 1;
    public int RowPerPage { get; set; } = 10;
    private TableData itemData = new TableData();
    private int TotalItem;
    private SPReportResult sPReportResult { get; set; } = new SPReportResult();
    private readonly IStringLocalizer<SmartParkingResource> _localizer;
    public string sordColumn { get; private set; }
    private List<MudMenuData> MenuDatas = new List<MudMenuData>();
    public string ColumnVisibility { get; private set; }
    public CustomerPackage(IStringLocalizer<SmartParkingResource> localizer)
    {
        _localizer = localizer;
    }
    protected async override Task OnInitializedAsync()
    {
        if (!await AuthorizationService.IsGrantedAnyAsync(ReportsPermissions.SecurityManager))
        {

            Navigation.NavigateTo("/Account/AccessDenied", true);
        }
    }
    private async Task CreateMenue(List<ColumnDef> columns)
    {
        var MenuDatasTable = new List<MudMenuData>
        {
           await CreateColumnVisibilityMenue(columns),

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
    private async Task FetchData(PageChangeEventArgs paged)
    {
        var input = new PagedAndSortedAndFilteredResultRequestDto
        {
            MaxResultCount = paged.RowsPerPage,
            SkipCount = (paged.PageNumber - 1) * paged.RowsPerPage,
            Sorting = paged.SortColumn,
            filters = paged.Filters,
            SearchingTerm = paged.SearchTerm
            
        };
        var customerPackages = await AnalysisAppService.ReportCustomerPackage(input, CurrentTenant.Id);
        TotalItem = (int) customerPackages.TotalCount;
       
        if (!ColumnVisibility.IsNullOrWhiteSpace())
        {
            itemData.Columns.FirstOrDefault(c => c.DisplayName == ColumnVisibility).IsColumnVisible = !itemData.Columns.FirstOrDefault(c => c.DisplayName == ColumnVisibility).IsColumnVisible;

        }
        else
        {
            itemData = await CreateTableHelper.CreateDataTableForCustomerPackages(customerPackages.Items.ToList());
        }
        await CreateMenue(itemData.Columns);

    }
}
