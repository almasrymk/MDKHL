using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Localization;
using MudBlazor;
using OfficeOpenXml.ConditionalFormatting.Contracts;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Blazor.Components.SPComponents;
using SmartParking.CustomerPackages;
using SmartParking.Entities.Analysis;
using SmartParking.Enums;
using SmartParking.Helpers;
using SmartParking.Localization;
using SmartParking.Permissions.Reports;
using SmartParking.StringLocalizer;
using Volo.Abp.Application.Dtos;


namespace SmartParking.Blazor.Components.Pages.Reports;

public partial class CustomersPackage
{
    public int CurrentPage { get; set; } = 1;
    public int RowPerPage { get; set; } = 10;
    private TableData itemData = new TableData();
    private TableData itemDataIsExport = new TableData();
    private int TotalItem;
    private SPReportResult sPReportResult { get; set; } = new SPReportResult();
    private readonly IStringLocalizer<SmartParkingResource> _localizer;
    public string sordColumn { get; private set; }
    private List<MudMenuData> MenuDatas = new List<MudMenuData>();
    public string ColumnVisibility { get; private set; }
    public bool IsEnter { get; private set; }
    public CustomersPackage(IStringLocalizer<SmartParkingResource> localizer)
    {
        _localizer = localizer;
    }
    protected async override Task OnInitializedAsync()
    {
        if (!await AuthorizationService.IsGrantedAnyAsync(ReportsPermissions.SecurityManager))
        {
            Navigation.NavigateTo("/Account/AccessDenied", true);
        }
        await CreateMenue(itemData.Columns);
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

    private async Task FetchData(PageChangeEventArgs paged)
    {
        string selectvalue = "Customer";
        var filters = paged.Filters;
        filters.Add(new FilterRequestDto
        {
            FilterProperty = "CustomerCategory",
            FilterValue = selectvalue,
            IsNot = false,
            TypeFilter = TypeFilter.Enum
        });
        var request = new PagedAndSortedAndFilteredResultRequestDto
        {
            MaxResultCount = paged.RowsPerPage,
            SkipCount = (paged.PageNumber - 1) * paged.RowsPerPage,
            Sorting = paged.SortColumn == "Id" ? CultureInfo.CurrentCulture.Name == "ar" ? "CustomerArabicName" : "CustomerEnglishName" : paged.SortColumn,
            SearchingTerm = "",
            filters  = filters
        };

        var customerPackages = await AnalysisAppService.ReportCustomerPackage(request, CurrentTenant.Id);
        TotalItem = (int)customerPackages.TotalCount;

        if (paged.IsExport)
        {
            itemDataIsExport.Columns = itemData.Columns;
            itemData = await CreateTableHelper.CreateDataTableForCustomerPackages(customerPackages.Items.ToList());
            itemData.Columns = itemDataIsExport.Columns;
        }
        else if (!ColumnVisibility.IsNullOrWhiteSpace() && (IsEnter && paged.IsExport == false))
        {
            itemData.Columns.FirstOrDefault(c => c.DisplayName == ColumnVisibility).IsColumnVisible = !itemData.Columns.FirstOrDefault(c => c.DisplayName == ColumnVisibility).IsColumnVisible;
        }
        else
        {
            itemData = await CreateTableHelper.CreateDataTableForCustomerPackages(customerPackages.Items.ToList());
            IsEnter = true;
        } 

        if (CultureInfo.CurrentCulture.Name == "ar")
        {
            itemData.Name = LocalizationHelper.GetLocalizedString("Report") + " " + CustomStringLocalizer.For + LocalizationHelper.GetLocalizedString("CustomerPackageReport");

        }
        else
        {
            itemData.Name = LocalizationHelper.GetLocalizedString("CustomerPackageReport") + " "+ LocalizationHelper.GetLocalizedString("Report");

        }
        await CreateMenue(itemData.Columns);
    }
}
