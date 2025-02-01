
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Blazor.Components.SPComponents;
using SmartParking.Entities.Analysis;
using SmartParking.Enums;
using SmartParking.Helpers;
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



    private bool _dialogShown = false;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !_dialogShown)
        {
            _dialogShown = true; // Prevent multiple dialog openings
            await ShowDialogReports();
        }
    }

    private async Task  ShowDialogReports()
    {
        var parameters = new DialogParameters
        {
            ["Name"] = "Customers Reports",
            ["AddRange"] = true,
            ["QueryText"]=false,
            ["AddCustomerCategory"] = true
        };
        var dialog= await Dialog.ShowAsync<ReportDialog>("Customers Reports", parameters);
       var result = await dialog.Result;
        if (!result.Canceled && result.Data is SPReportResult data)
        {
            await FetchData(data);
        }


    }

    private async Task FetchData(SPReportResult data)
    {
        CustomerCategoryStatus customerCategory = data.CustomerCategory;
        DateRange dateRange = data.DateRange;
        List<GarageTransactionAnalysis> garageTransactionAnalyses = new List<GarageTransactionAnalysis>();
        if (data.InRange)
            garageTransactionAnalyses = await AnalysisAppService.ReportGarageTransactionDateCustomeCategoryAsync(customerCategory, dateRange.Start.Value, dateRange.End.Value);
        else
            garageTransactionAnalyses = await AnalysisAppService.ReportGarageTransactionMdkhlAsync(customerCategory, CurrentTenant.Id);


        TotalItem = garageTransactionAnalyses.Count;
        itemData = await CreateTableHelper.CreateDataTableForGarageTransaction(garageTransactionAnalyses);
    }
}
