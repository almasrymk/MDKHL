
using Blazorise.DataGrid;
using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Blazor.Components.SPComponents;
using SmartParking.Entities.Analysis;
using SmartParking.Enums;
using SmartParking.Helpers;
using SmartParking.Localization;
using SmartParking.Results;
using Volo.Abp.MultiTenancy;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SmartParking.Blazor.Components.Pages.Reports;

partial class BeneficiariesReport
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

    public BeneficiariesReport(IStringLocalizer<SmartParkingResource> localizer)
    {
        _localizer = localizer;
        DataGridComponent = new SPDataGrid(localizer);

    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !_dialogShown)
        {
            _dialogShown = true;
           // await ShowDialogReports();
           // await DataGridComponent.Reload();
            StateHasChanged();

        }
    }

    private async Task  ShowDialogReports(PageChangeEventArgs paged)
    {
        var parameters = new DialogParameters
        {
            ["Name"] = "Customers Reports",
            ["AddRange"] = true,
            ["QueryText"]=false,
            ["AddCustomerCategory"] = true
        };

       var dialog = await Dialog.ShowAsync<ReportDialog>("Customers Reports", parameters, _closeButton);
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
     
        CustomerCategoryStatus customerCategory = sPReportResult.CustomerCategory;
        DateRange dateRange = sPReportResult.DateRange;
        List<GarageTransactionAnalysis> garageTransactionAnalyses = new List<GarageTransactionAnalysis>();
        if (sPReportResult.InRange && _dialogOpen)
            garageTransactionAnalyses = await AnalysisAppService.ReportGarageTransactionDateCustomeCategoryAsync(customerCategory, dateRange.Start.Value, dateRange.End.Value);
        else if(_dialogOpen)
            garageTransactionAnalyses = await AnalysisAppService.ReportGarageTransactionMdkhlAsync(customerCategory, CurrentTenant.Id);


        TotalItem = garageTransactionAnalyses.Count;
        itemData = await CreateTableHelper.CreateDataTableForGarageTransaction(garageTransactionAnalyses);
        await Task.Yield();


    }
}
