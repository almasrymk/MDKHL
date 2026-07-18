using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Customers;
using SmartParking.CustomerUnitPackages;
using SmartParking.UnitVehiclePackages;
using System.Globalization;
using Volo.Abp.Application.Dtos;
using static MudBlazor.CategoryTypes;

namespace SmartParking.Blazor.Components.Pages.Customer.CustomerUnitPackages;

public partial class CustomerUnitPackagePage
{
    private int TotalItem;
    public int CurrentPage { get; set; } = 1;
    public int RowPerPage { get; set; } = 10;
    private string sortColumn;
    private TableData itemData = new TableData();
    private List<MudMenuData> MenuDatas = new List<MudMenuData>();
    public string ColumnVisibility { get; private set; }


    private string termSearch;
    private readonly DialogOptions _backdropClick = new() { BackdropClick = false };

    protected async override Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await FetchTableData(new PageChangeEventArgs { PageNumber = 1, RowsPerPage = 10, SortColumn = "" });
    }

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

        var unitsDto = await CustomerUnitPackageAppService.GetListAsync(input);
        TotalItem = (int)unitsDto.TotalCount;
        await CreateTable(unitsDto);
    }



    private async Task FetchTableData(PageChangeEventArgs pageInfo)
    {
        var input = new PagedAndSortedAndSearchResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn,
            SearchingTerm = pageInfo.SearchTerm
        };

        var unitsDto = await CustomerUnitPackageAppService.GetListAsync(input);
        TotalItem = (int)unitsDto.TotalCount;
        await CreateTable(unitsDto);
    }

    private async Task CreateTable(PagedResultDto<CustomerUnitPackageDto> packages)
    {
        var tableData = new TableData()
        {
            Name = L["UnitVehiclePackages"],
            ShowHeader = true
        };

        tableData.Columns.Add(new ColumnDef() { DisplayName = L["Id"], Name = "Id", IsFilterable = false, PropertyName = "Id", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = L["CustomerName"], Name = "CustomerName", IsFilterable = true, PropertyName = CultureInfo.CurrentCulture.Name == "en" ? "Customer.IdentityUser.Name" : "Customer.ArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = L["PackageName"], Name = "PackageName", IsFilterable = true, PropertyName = CultureInfo.CurrentCulture.Name == "en" ? "EnglishName" : "ArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });

        tableData.Columns.Add(new ColumnDef() { DisplayName = L["UnitCode"], Name = "UnitCode", IsFilterable = true, PropertyName = "UnitCode", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = L["ActivationDate"], Name = "ActivationDate", IsFilterable = true, PropertyName = "ActivationDate", IsSortable = true, TypeFilter = TypeFilter.Date });
        tableData.Columns.Add(new ColumnDef() { DisplayName = L["DeactivationDate"], Name = "DeactivationDate", IsFilterable = true, PropertyName = "DectivationDate", IsSortable = true, TypeFilter = TypeFilter.Date });
        tableData.Columns.Add(new ColumnDef() { DisplayName = L["CustomerUnitPackageType"], Name = "CustomerUnitPackageType", IsFilterable = true, PropertyName = "CustomerUnitPackageType", IsSortable = true, TypeFilter = TypeFilter.Text });


        foreach (var item in packages.Items)
        {
            
            var rowDef = new RowDef();
            rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(L["UnitCode"], item.UnitCode ?? ""));
            rowDef.Cells.Add(new KeyValuePair<string, string>(L["CustomerName"], CultureInfo.CurrentCulture.Name == "en" ? item.CustomerFullName : item.CustomerArabicName ?? ""));
            rowDef.Cells.Add(new KeyValuePair<string, string>(L["PackageName"], CultureInfo.CurrentCulture.Name == "en" ? item.EnglishName : item.ArabicName ?? ""));

            rowDef.Cells.Add(new KeyValuePair<string, string>(L["ActivationDate"], item.ActivationDate.ToString("yyyy-MM-dd")));
            rowDef.Cells.Add(new KeyValuePair<string, string>(L["DeactivationDate"], item.DectivationDate.ToString("yyyy-MM-dd")));
            rowDef.Cells.Add(new KeyValuePair<string, string>(L["CustomerUnitPackageType"], L[item.UnitPackageType.ToString()]));
            tableData.Rows.Add(rowDef);
        }

        itemData = tableData;
        await CreateMenue(tableData.Columns);
        await Task.Yield();
    }

    private async Task<IDialogReference> OnClickRowView(RowDef row)
    {
        var id = int.Parse(row.Cells[0].Value);
        var packageDto = await CustomerUnitPackageAppService.GetAsync(id);

        var parameters = new DialogParameters
        {
            ["Package"] = packageDto
        };

        return await Dialog.ShowAsync<CustomerUnitPackageDetailsDialog>(L["PackageDetails"], parameters);
    }

    public string Truncate(string str, int maxLength)
    {
        if (string.IsNullOrEmpty(str)) return str;
        return str.Length > maxLength ? str.Substring(0, maxLength) + "..." : str;
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
    private async Task CreateMenue(List<ColumnDef> columns)
    {
        var MenuDatasTable = new List<MudMenuData>
        {
           await CreateColumnVisibilityMenue(columns)

        };
        MenuDatas = MenuDatasTable;
    }
    private async Task HandleSelectColumnVisibilityAsync(string valueOfMenue)
    {
        ColumnVisibility = valueOfMenue;
        await Task.Yield();
    }
}
