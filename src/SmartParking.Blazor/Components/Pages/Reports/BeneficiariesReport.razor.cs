
using Blazorise.DataGrid;
using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Blazor.Components.SPComponents;
using SmartParking.Entities.Analysis;
using SmartParking.Entities.Organizations;
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
    private readonly IStringLocalizer<SmartParkingResource> _localizer;
    private SPDataGrid DataGridComponent { get; set; }

    public BeneficiariesReport(IStringLocalizer<SmartParkingResource> localizer)
    {
        _localizer = localizer;
        DataGridComponent = new SPDataGrid(localizer);

    }
    private async Task FetchData(PageChangeEventArgs paged)
    {
        
        var input = new PagedAndSortedAndFilteredResultRequestDto
        {
            MaxResultCount = paged.RowsPerPage,
            SkipCount = (paged.PageNumber - 1) * paged.RowsPerPage,
            Sorting = paged.SortColumn=="Id"? "GarageName" : paged.SortColumn,
            filters = paged.Filters,
            SearchingTerm = paged.SearchTerm

        };
        var garageTransactionAnalyses = await AnalysisAppService.ReportGarageTransactionAsync(input,CurrentTenant.Id);
        TotalItem = (int)garageTransactionAnalyses.TotalCount;
        itemData = await CreateTableHelper.CreateDataTableForGarageTransaction(garageTransactionAnalyses.Items.ToList());


    }
}
