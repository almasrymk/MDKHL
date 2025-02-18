using Microsoft.Extensions.Localization;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Helpers;
using SmartParking.Localization;

namespace SmartParking.Blazor.Components.Pages.Reports;

public partial class GaragesReports
{
    public int CurrentPage { get; set; } = 1;
    public int RowPerPage { get; set; } = 10;
    private TableData itemData = new TableData();
    private int TotalItem;
    private SPReportResult sPReportResult { get; set; } = new SPReportResult();
    private readonly IStringLocalizer<SmartParkingResource> _localizer;
    public string sordColumn { get; private set; }

    public GaragesReports(IStringLocalizer<SmartParkingResource> localizer)
    {
        _localizer = localizer;

    }

    private async Task FetchData(PageChangeEventArgs paged)
    {
        var input = new PagedAndSortedAndFilteredResultRequestDto
        {
            MaxResultCount = paged.RowsPerPage,
            SkipCount = (paged.PageNumber - 1) * paged.RowsPerPage,
            Sorting = paged.SortColumn == "Id" ? "Garage.Id" : paged.SortColumn,
            filters = paged.Filters,
            SearchingTerm = paged.SearchTerm

        };
        var garages = await AnalysisAppService.ReportGarageAsync(input, CurrentTenant.Id);
        TotalItem = (int)garages.TotalCount;
        itemData = await CreateTableHelper.CreateDataTableForGarages(garages.Items.ToList());
        await Task.Yield();


    }
}
