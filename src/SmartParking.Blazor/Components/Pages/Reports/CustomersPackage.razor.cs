using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Localization;
using MudBlazor;
using OfficeOpenXml.ConditionalFormatting.Contracts;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Blazor.Components.SPComponents;
using SmartParking.CustomerPackages;
using SmartParking.Entities.Analysis;
using SmartParking.Enums;
using SmartParking.Helpers;
using SmartParking.Localization;
using SmartParking.Permissions.Reports;
using SmartParking.StringLocalizer;
using Volo.Abp.Application.Dtos;


namespace SmartParking.Blazor.Components.Pages.Reports;

public partial class CustomersPackage
{
    public int CurrentPage { get; set; } = 1;
    public int RowPerPage { get; set; } = 10;
    private TableData itemData = new TableData();
    private TableData itemDataIsExport = new TableData();
    private int TotalItem;
    private SPReportResult sPReportResult { get; set; } = new SPReportResult();
    private readonly IStringLocalizer<SmartParkingResource> _localizer;
    public string sordColumn { get; private set; }
    private List<MudMenuData> MenuDatas = new List<MudMenuData>();
    public string ColumnVisibility { get; private set; }
    public bool IsEnter { get; private set; }

    public CustomersPackage(IStringLocalizer<SmartParkingResource> localizer)
    {
        _localizer = localizer;
    }
    protected async override Task OnInitializedAsync()
    {
        if (!await AuthorizationService.IsGrantedAnyAsync(ReportsPermissions.SecurityManager))
        {

            Navigation.NavigateTo("/Account/AccessDenied", true);
        }
    }
    
    
    private async Task FetchData(PageChangeEventArgs paged)
    {
        string selectvalue = "Customer";

        var request = new PagedAndSortedAndFilteredResultRequestDto
        {
            MaxResultCount = paged.RowsPerPage,
            SkipCount = (paged.PageNumber - 1) * paged.RowsPerPage,
            Sorting = CultureInfo.CurrentCulture.Name == "ar" ? "Customer.ArabicName" : "Customer.IdentityUser.Name",
            SearchingTerm = "",
            filters = new List<FilterRequestDto>
                {
                    new FilterRequestDto
                    {
                         FilterProperty ="CustomerCategory",
                         FilterValue = selectvalue,
                         IsNot = false,
                         TypeFilter = TypeFilter.Enum
                    }
                },
        };

        var customerPackages = await AnalysisAppService.ReportCustomerPackage(request, CurrentTenant.Id);
        TotalItem = (int)customerPackages.TotalCount;
        itemData = await CreateTableHelper.CreateDataTableForCustomerPackages(customerPackages.Items.ToList());

    }
}
