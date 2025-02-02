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
using Volo.Abp.Application.Dtos;

namespace SmartParking.Blazor.Components.Pages.Reports;

public partial class CustomerPackage
{
    public int CurrentPage { get; set; } = 1;
    public int RowPerPage { get; set; } = 10;
    private TableData itemData = new TableData();
    private int TotalItem;
    private SPReportResult sPReportResult { get; set; } = new SPReportResult();
    private readonly DialogOptions _closeButton = new() { CloseButton = false };
    private readonly IStringLocalizer<SmartParkingResource> _localizer;
    private bool _dialogShown = false;
    private bool _dialogOpen = false;
    //private SPDataGrid DataGridComponent { get; set; }
    public string sordColumn { get; private set; }

    public CustomerPackage(IStringLocalizer<SmartParkingResource> localizer)
    {
        _localizer = localizer;
        //DataGridComponent = new SPDataGrid(localizer);

    }

    //protected override async Task OnAfterRenderAsync(bool firstRender)
    //{
    //    if (firstRender && !_dialogShown)
    //    {
    //        _dialogShown = true;
    //        StateHasChanged();

    //    }
    //}

    //private async Task ShowDialogReports(PageChangeEventArgs paged)
    //{

    //    CurrentPage = paged.PageNumber;
    //    RowPerPage = paged.RowsPerPage;
    //    sordColumn = paged.SortColumn;

    //    var parameters = new DialogParameters
    //    {
    //        ["Name"] = $"{_localizer["CustomerPackage"]} {_localizer["Reports"]}",
    //        ["AddRange"] = true,
    //        ["QueryText"] = false,
    //        ["AddCustomerCategory"] = false
    //    };

    //    var dialog = await Dialog.ShowAsync<ReportDialog>("CustomerPackage Reports", parameters, _closeButton);
    //    var result = await dialog.Result;
    //    if (result.Data is not null && result.Data is SPReportResult data)
    //    {
    //        sPReportResult = data;

    //        _dialogOpen = sPReportResult.HasBeenOpened;
    //        await FetchData();
    //    }


    //}

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
        var customerPackages = await AnalysisAppService.ReportCustomerPackage(input, CurrentTenant.Id,null,null);
        TotalItem = (int) customerPackages.TotalCount;
        itemData = await CreateTableHelper.CreateDataTableForCustomerPackages(customerPackages.Items.ToList());
        await Task.Yield();


    }
}
