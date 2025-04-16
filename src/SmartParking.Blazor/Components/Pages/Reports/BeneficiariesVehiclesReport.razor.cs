using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Helpers;
using SmartParking.Localization;
using SmartParking.Permissions.Reports;
using SmartParking.StringLocalizer;
using System.Globalization;

namespace SmartParking.Blazor.Components.Pages.Reports;

public partial class BeneficiariesVehiclesReport
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
    public BeneficiariesVehiclesReport(IStringLocalizer<SmartParkingResource> localizer)
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


        if (!await AuthorizationService.IsGrantedAnyAsync(ReportsPermissions.SecurityManager) || !await AuthorizationService.IsGrantedAnyAsync(ReportsPermissions.IT))
        {

            Navigation.NavigateTo("/Account/AccessDenied", true);
        }
        await CreateMenue(itemData.Columns);
    }
    private async Task SearchData(PageChangeEventArgs paged)
    {
        var input = new PagedAndSortedAndFilteredResultRequestDto
        {
            MaxResultCount = paged.RowsPerPage,
            SkipCount = (paged.PageNumber - 1) * paged.RowsPerPage,
            Sorting = paged.SortColumn == "Id" ? CultureInfo.CurrentCulture.Name == "ar" ? "CustomerArabicName" : "CustomerEnglishName" : paged.SortColumn,
            filters = paged.Filters,
            SearchingTerm = paged.SearchTerm

        };
        var vehicleCustomer = await AnalysisAppService.ReportVehicleCustomerAsync(input, CurrentTenant.Id);
        TotalItem = (int)vehicleCustomer.TotalCount;
        if (paged.IsExport && !paged.SearchTerm.IsNullOrWhiteSpace())
        {
            itemDataIsExport.Columns = itemData.Columns;
            itemData.Columns.FirstOrDefault(c => c.DisplayName == LocalizationHelper.GetLocalizedString("BeneficiaryName")).IsColumnVisible = false;
            itemData.Name = (CultureInfo.CurrentCulture.Name == "ar" ? vehicleCustomer.Items.FirstOrDefault().CustomerArabicName : vehicleCustomer.Items.FirstOrDefault().CustomerEnglishName) + " " + LocalizationHelper.GetLocalizedString("Vehicles");
            itemData.Columns = itemDataIsExport.Columns;
        }
    }
    private async Task FetchData(PageChangeEventArgs paged)
    {

        var input = new PagedAndSortedAndFilteredResultRequestDto
        {
            MaxResultCount = paged.RowsPerPage,
            SkipCount = (paged.PageNumber - 1) * paged.RowsPerPage,
            Sorting = paged.SortColumn == "Id" ? CultureInfo.CurrentCulture.Name == "ar" ? "CustomerArabicName" : "CustomerEnglishName" : paged.SortColumn,
            filters = paged.Filters,
            SearchingTerm = paged.SearchTerm

        };
        var vehicleCustomer = await AnalysisAppService.ReportVehicleCustomerAsync(input, CurrentTenant.Id);
        TotalItem = (int)vehicleCustomer.TotalCount;

        if (paged.IsExport)
        {
            itemDataIsExport.Columns = itemData.Columns;
            itemData = await CreateTableHelper.CreateDataTableForCustomersVehicles(vehicleCustomer.Items.ToList());

            itemData.Columns = itemDataIsExport.Columns;
        }
        else if (!ColumnVisibility.IsNullOrWhiteSpace() && (IsEnter && paged.IsExport == false))
        {
            itemData.Columns.FirstOrDefault(c => c.DisplayName == ColumnVisibility).IsColumnVisible = !itemData.Columns.FirstOrDefault(c => c.DisplayName == ColumnVisibility).IsColumnVisible;
        }
        else
        {
            itemData = await CreateTableHelper.CreateDataTableForCustomersVehicles(vehicleCustomer.Items.ToList());
            IsEnter = true;
        }
       
        await CreateMenue(itemData.Columns);

    }
}
