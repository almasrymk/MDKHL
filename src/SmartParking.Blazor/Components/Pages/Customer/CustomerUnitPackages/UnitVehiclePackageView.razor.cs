using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Pages.UnitVehiclePackages;
using SmartParking.Localization;
using SmartParking.UnitVehiclePackages;
using System.Globalization;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Blazor.Components.Pages.Customer.CustomerUnitPackages;

public partial class UnitVehiclePackageView
{
    private int TotalItem;
    public int CurrentPage { get; set; } = 1;
    public int RowPerPage { get; set; } = 10;
    private string sortColumn;
    private TableData itemData = new TableData();
    private string termSearch;
    private readonly DialogOptions _backdropClick = new() { BackdropClick = false };
    private readonly IStringLocalizer<SmartParkingResource> _localizer;
    [Parameter] public string CustomerId { get; set; }


    private IMudDialogInstance MudDialog { get; set; }


    public UnitVehiclePackageView(IStringLocalizer<SmartParkingResource> localizer)
    {
        _localizer = localizer;
        //CreatePolicyName = UnitVehiclePackagePermissions.Create;
        //   UpdatePolicyName = UnitVehiclePackagePermissions.Edit;
        //DeletePolicyName = UnitVehiclePackagePermissions.Delete;
    }

    protected async override Task OnInitializedAsync()
    {

        await base.OnInitializedAsync();
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

        var unitsDto = await UnitVehiclePackageAppService.GetAllByCustomerId(Guid.Parse(CustomerId), input);
        TotalItem = (int)unitsDto.TotalCount;
        await CreateTable(unitsDto);
    }

    private async Task HandleSearch(PageChangeEventArgs pageInfo)
    {
        termSearch = pageInfo.SearchTerm;
        CurrentPage = pageInfo.PageNumber;
        RowPerPage = pageInfo.RowsPerPage;
        sortColumn = pageInfo.SortColumn;

        var input = new PagedAndSortedAndSearchResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn,
            SearchingTerm = termSearch
        };

        var unitsDto = await UnitVehiclePackageAppService.GetAllByCustomerId(Guid.Parse(CustomerId) ,input);
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

        var unitsDto = await UnitVehiclePackageAppService.GetAllByCustomerId(Guid.Parse(CustomerId), input);
        TotalItem = (int)unitsDto.TotalCount;
        await CreateTable(unitsDto);
    }

    private async Task CreateTable(PagedResultDto<UnitVehiclePackageDto> packages)
    {
        var tableData = new TableData()
        {
            Name = L["UnitVehiclePackages"],
            ShowHeader = true
        };

        tableData.Columns.Add(new ColumnDef() { DisplayName = L["Id"], Name = "Id", IsFilterable = false, PropertyName = "Id", IsSortable = true, TypeFilter = TypeFilter.Text });
        if (CultureInfo.CurrentCulture.Name == "en")
            tableData.Columns.Add(new ColumnDef() { DisplayName = L["EnglishName"], Name = "EnglishName", IsFilterable = true, PropertyName = "EnglishName", IsSortable = true, TypeFilter = TypeFilter.Text });
        else
            tableData.Columns.Add(new ColumnDef() { DisplayName = L["ArabicName"], Name = "ArabicName", IsFilterable = true, PropertyName = "ArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });

        tableData.Columns.Add(new ColumnDef() { DisplayName = L["UnitCode"], Name = "UnitCode", IsFilterable = true, PropertyName = "UnitCode", IsSortable = true, TypeFilter = TypeFilter.Text });
        if (CultureInfo.CurrentCulture.Name == "en")
            tableData.Columns.Add(new ColumnDef() { DisplayName = L["EnPlateNumber"], Name = "EnPlateNumber", IsFilterable = true, PropertyName = "EnPlateNumber", IsSortable = true, TypeFilter = TypeFilter.Text });
        else
            tableData.Columns.Add(new ColumnDef() { DisplayName = L["ArPlateNumber"], Name = "ArPlateNumber", IsFilterable = true, PropertyName = "ArPlateNumber", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = L["ActivationDate"], Name = "ActivationDate", IsFilterable = true, PropertyName = "ActivationDate", IsSortable = true, TypeFilter = TypeFilter.Date });
        tableData.Columns.Add(new ColumnDef() { DisplayName = L["DeactivationDate"], Name = "DeactivationDate", IsFilterable = true, PropertyName = "DeactivationDate", IsSortable = true, TypeFilter = TypeFilter.Date });
        tableData.Columns.Add(new ColumnDef() { DisplayName = L["CustomerUnitPackageType"], Name = "CustomerUnitPackageType", IsFilterable = true, PropertyName = "CustomerUnitPackageType", IsSortable = true, TypeFilter = TypeFilter.Text });


        foreach (var item in packages.Items)
        {
            var customerUnitPackage = await CustomerUnitPackageAppService.GetAsync(item.CustomerUnitPackageId);
            var englishName = customerUnitPackage.EnglishName;
            var arabicName = customerUnitPackage.ArabicName;
            var customer = await CustomerAppService.GetAsync(item.CustomerId);
            var customerName = customer.FullName;
            var rowDef = new RowDef();
            rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(L["UnitCode"], item.UnitCode ?? ""));
            if (CultureInfo.CurrentCulture.Name == "en")
                rowDef.Cells.Add(new KeyValuePair<string, string>(L["EnglishName"], englishName ?? ""));
            else
                rowDef.Cells.Add(new KeyValuePair<string, string>(L["ArabicName"], arabicName ?? ""));
            if (CultureInfo.CurrentCulture.Name == "en")
                rowDef.Cells.Add(new KeyValuePair<string, string>(L["EnPlateNumber"], item.EnPlateNumber ?? ""));
            else
                rowDef.Cells.Add(new KeyValuePair<string, string>(L["ArPlateNumber"], item.ArPlateNumber ?? ""));
            rowDef.Cells.Add(new KeyValuePair<string, string>(L["ActivationDate"], item.ActivationDate.ToString("yyyy-MM-dd")));
            rowDef.Cells.Add(new KeyValuePair<string, string>(L["DeactivationDate"], item.DectivationDate.ToString("yyyy-MM-dd")));
            rowDef.Cells.Add(new KeyValuePair<string, string>(L["CustomerUnitPackageType"], item.CustomerUnitPackageType.ToString()));
            tableData.Rows.Add(rowDef);
        }

        itemData = tableData;
        await Task.Yield();
    }

    private async Task<IDialogReference> OnClickRowView(RowDef row)
    {
        var id = Guid.Parse(row.Cells[0].Value);
        var packageDto = await UnitVehiclePackageAppService.GetAsync(id);

        var parameters = new DialogParameters
        {
            ["Package"] = packageDto
        };

        return await Dialog.ShowAsync<DetailsUnitVehiclePackageDialog>("View Unit Vehicle Package", parameters);
    }

    public string Truncate(string str, int maxLength)
    {
        if (string.IsNullOrEmpty(str)) return str;
        return str.Length > maxLength ? str.Substring(0, maxLength) + "..." : str;
    }
    private void Cancel()
    {
        MudDialog.Cancel();
    }
}
