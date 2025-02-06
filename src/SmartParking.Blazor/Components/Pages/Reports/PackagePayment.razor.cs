using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Localization;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Helpers;
using SmartParking.Localization;
using SmartParking.Permissions.Reports;

namespace SmartParking.Blazor.Components.Pages.Reports;

public partial class PackagePayment
{

    public int CurrentPage { get; set; } = 1;
    public int RowPerPage { get; set; } = 10;
    private TableData itemData = new TableData();
    private int TotalItem;
    private SPReportResult sPReportResult { get; set; } = new SPReportResult();
    private readonly IStringLocalizer<SmartParkingResource> _localizer;
    public string sordColumn { get; private set; }

    public PackagePayment(IStringLocalizer<SmartParkingResource> localizer)
    {
        _localizer = localizer;
    }
    protected async override Task OnInitializedAsync()
    {
        if (!await AuthorizationService.IsGrantedAnyAsync(ReportsPermissions.FinancialManagement))
        {

            Navigation.NavigateTo("/Account/AccessDenied", true);
        }
    }
    private async Task FetchData(PageChangeEventArgs paged)
    {
        var input = new PagedAndSortedAndFilteredResultRequestDto
        {
            MaxResultCount = paged.RowsPerPage,
            SkipCount = (paged.PageNumber - 1) * paged.RowsPerPage,
            Sorting = paged.SortColumn == "Id" ? "Package.Id" : paged.SortColumn,
            filters = paged.Filters,
            SearchingTerm = paged.SearchTerm

        };
        var packagePayment = await AnalysisAppService.ReportPackagePaymentAsync(input, CurrentTenant.Id);
        TotalItem = (int)packagePayment.TotalCount;
        itemData = await CreateTableHelper.CreateDataTablePackagePayments(packagePayment.Items.ToList());

    }
}
