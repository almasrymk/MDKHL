using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Blazor.Components.SPComponents;
using SmartParking.Entities.Analysis;
using SmartParking.Enums;
using SmartParking.Helpers;
using SmartParking.Localization;

namespace SmartParking.Blazor.Components.Pages.Reports;

partial class VehicleReport
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
    private SPDataGrid DataGridComponent { get; set; }

    public VehicleReport(IStringLocalizer<SmartParkingResource> localizer)
    {
        _localizer = localizer;

    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !_dialogShown)
        {
            _dialogShown = true;
            StateHasChanged();

        }
    }

    private async Task ShowDialogReports(PageChangeEventArgs paged)
    {
        var parameters = new DialogParameters
        {
            ["Name"] = "Vehicles Reports",
            ["AddRange"] = false,
            ["QueryText"] = true,
            ["AddCustomerCategory"] = false
        };

        var dialog = await Dialog.ShowAsync<ReportDialog>("Vehcile Reports", parameters, _closeButton);
        var result = await dialog.Result;
        if (result.Data is not null && result.Data is SPReportResult data)
        {
            sPReportResult = data;
            var pagedResult = new PageChangeEventArgs
            {
                PageNumber = CurrentPage,
                RowsPerPage = RowPerPage,

            };
            _dialogOpen = sPReportResult.HasBeenOpened;
            await FetchData();
        }


    }

    private async Task FetchData()
    {

        List<GarageTransactionAnalysis> garageTransactionAnalyses = new List<GarageTransactionAnalysis>();
        if (CurrentTenant.Id==null)
            garageTransactionAnalyses = await AnalysisAppService.ReportGarageTransactionForMDKHLPlateAsync(sPReportResult.SearchingText);
        else 
            garageTransactionAnalyses = await AnalysisAppService.ReportGarageTransactionForOrganization(sPReportResult.SearchingText);


        TotalItem = garageTransactionAnalyses.Count;
        itemData = await CreateTableHelper.CreateDataTableForGarageTransaction(garageTransactionAnalyses);
        await Task.Yield();


    }



}
