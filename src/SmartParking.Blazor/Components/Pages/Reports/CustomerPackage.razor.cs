using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
using SmartParking.Permissions.Reports;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Blazor.Components.Pages.Reports;

public partial class CustomerPackage
{
    public int CurrentPage { get; set; } = 1;
    public int RowPerPage { get; set; } = 10;
    private TableData itemData = new TableData();
    private int TotalItem;
    private SPReportResult sPReportResult { get; set; } = new SPReportResult();
    private readonly IStringLocalizer<SmartParkingResource> _localizer;
    public string sordColumn { get; private set; }

    public CustomerPackage(IStringLocalizer<SmartParkingResource> localizer)
    {
        _localizer = localizer;
    }
    protected async override Task OnInitializedAsync()
    {
        if (await AuthorizationService.IsGrantedAnyAsync(ReportsPermissions.SecurityManager)  || CurrentTenant.Id is null)
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
            Sorting = paged.SortColumn,
            filters = paged.Filters,
            SearchingTerm = paged.SearchTerm
            
        };
        var customerPackages = await AnalysisAppService.ReportCustomerPackage(input, CurrentTenant.Id);
        TotalItem = (int) customerPackages.TotalCount;
        itemData = await CreateTableHelper.CreateDataTableForCustomerPackages(customerPackages.Items.ToList());
        await Task.Yield();


    }
}
