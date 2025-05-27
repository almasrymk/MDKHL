
using Blazorise.DataGrid;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Pages.Organizations;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Blazor.Components.SPComponents;
using SmartParking.Cutomers;
using SmartParking.Entities.Analysis;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using SmartParking.Helpers;
using SmartParking.Localization;
using SmartParking.PDF;
using SmartParking.Permissions.PaymentsPermissions;
using SmartParking.Permissions.Reports;
using SmartParking.Results;
using SmartParking.StringLocalizer;
using SmartParking.Utilities;
using System.Composition;
using System.Globalization;
using Volo.Abp.MultiTenancy;
using static QRCoder.PayloadGenerator;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SmartParking.Blazor.Components.Pages.Reports;

partial class VisitorReport
{
    public int CurrentPage { get; set; } = 1;
    public int RowPerPage { get; set; } = 10;
    private TableData itemData = new TableData();
    private TableData itemDataIsExport = new TableData();
    private int TotalItem;
    private SPReportResult sPReportResult { get; set; } = new SPReportResult();
    public string ColumnVisibility { get; private set; }
    public bool IsEnter { get; set; }
    private readonly IStringLocalizer<SmartParkingResource> _localizer;
    private List<MudMenuData> MenuDatas = new List<MudMenuData>();
    public VisitorReport(IStringLocalizer<SmartParkingResource> localizer)
    {
        _localizer = localizer;

    }
    protected async override Task OnInitializedAsync()
    {
        if (await AuthorizationService.IsGrantedAnyAsync(ReportsPermissions.SecurityManager) || await AuthorizationService.IsGrantedAnyAsync(ReportsPermissions.IT))
        {
        }
        else
            Navigation.NavigateTo("/Account/AccessDenied", true);
    }
    private async Task FetchData(PageChangeEventArgs paged)
    {
        string selectvalue = "Visitor";
        var filters = paged.Filters;
        filters.Add(new FilterRequestDto
        {
            FilterProperty = "GarageTransaction.CustomerCategory",
            FilterValue = selectvalue,
            IsNot = false,
            TypeFilter = TypeFilter.Enum
        });
        var request = new PagedAndSortedAndFilteredResultRequestDto
        {
            MaxResultCount = paged.RowsPerPage,
            SkipCount = (paged.PageNumber - 1) * paged.RowsPerPage,
            Sorting = paged.SortColumn == "Id" ? CultureInfo.CurrentCulture.Name == "ar" ? "ArabicName" : "UserName" : paged.SortColumn,
            SearchingTerm = "",
            filters = filters
        };

         var garageTransactionAnalyses = await AnalysisAppService.ReportGarageTransactionAsync(request, CurrentTenant.Id);
         TotalItem = (int)garageTransactionAnalyses.TotalCount;
        itemData = await CreateTableHelper.CreateDataTableForGarageTransaction(garageTransactionAnalyses.Items.ToList(), CurrentTenant.Id);
        itemData.Name = LocalizationHelper.GetLocalizedString("VisitorAndTransactionReportName");

    }
}
