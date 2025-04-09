using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using SmartParking.Analysis;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Enums;
using SmartParking.Helpers;
using SmartParking.Localization;
using SmartParking.Organizations;
using SmartParking.Utilities;

namespace SmartParking.Blazor.Components.Pages;

public partial class Index
{
    ChartOptions options = new ChartOptions();
    AnalysisNamesCountDto value = new AnalysisNamesCountDto();
    public EventCallback<string> SelectValueChanged { get; set; }
    private int _organizationsCount;
    private int _garagesCount;
    private int _customersCount;
    private int grid;
    private int totalGrid;
    private GetNumberOfParkingAndEmptyParkingDto getNumberOfParking { get; set; } = new GetNumberOfParkingAndEmptyParkingDto();
    double[] data = { 0, 0, 0, 0 };

    string[] labels = { "Customers", "Employees", "Visitors", "NoPackage" };
    string msg;
    private List<OrganizationDto> Organizations = new List<OrganizationDto>();
    public string[] XAxisLabels = { " " };
    private int index = -1;

    public List<ChartSeries> Series = new List<ChartSeries>();

    private string[] inputLabels { get; set; } = { " " };
    private double[] inputData { get; set; } = { 0 };
    private string? Day = default;
    private string collectedMoneyToday;

    int totalOfParking;
    int totalEmptyOfParking;
    protected async override Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Save the authentication state ID to local storage
            await JSRuntime.InvokeVoidAsync("localStorage.setItem", "authentication-state-id", CurrentUser.Id);
        }

        // You can call the base method if needed (but it's optional)
        await base.OnAfterRenderAsync(firstRender);
    }
    protected async override Task OnInitializedAsync()
    {

        // Fetch the data from backend


        _organizationsCount = await OrganizationAppService.GetOrganizationsCount();
        _garagesCount = await GarageAppService.GetAllGaragesCount();

        var name = CurrentUser.UserName;
        if (currenttenant.Id == null)
        {
            var dataForCount = await AnalysisAppService.GetCountOfCustomersAndVisitorsAndEmployeesMdkhl();
            ShowData(dataForCount.totalCountOfCustomers, dataForCount.totalCountOfVisitors, dataForCount.totalCountOfEmployees, dataForCount.totalCountOfNoPackage);
            grid = 4;
            totalGrid = 4;
            _customersCount = await CustomerAppService.GetCountCustomersCountForMDKHL();

            var dataForSeries = await AnalysisAppService.CalculateWeeklyCollectedMoneyMdkhlAsync();
            showDataGraph(dataForSeries.Days, dataForSeries.AllTotalAmounts, dataForSeries.InvoiceTotalAmounts, dataForSeries.PackageTotalAmounts);

            var dataForToday = await AnalysisAppService.CalculateDailyCollectedMoneyMdKhlAsync();
            Day = dataForToday.Day;
            collectedMoneyToday = $"{Math.Round((decimal)dataForToday.totalAmounts, 2).ToString()} {@L["SAR"]}";
        }
        else
        {

            var organization = await OrganizationAppService.GetOrganizationForCurrentTenatAsync();
            await GetNumberOfParkingAndEmptyAsync(organization.Id);
            var dataForCount = await AnalysisAppService.GetCountOfCustomersAndVisitorsAndEmployeesOrganization(organization.Id);
            ShowData(dataForCount.totalCountOfCustomers, dataForCount.totalCountOfVisitors, dataForCount.totalCountOfEmployees, dataForCount.totalCountOfNoPackage);
            grid = 4;
            totalGrid = 6;
            _customersCount = await CustomerAppService.GetCountCustomersCountForOrganization(currenttenant.Id);
            var dataForSeries = await AnalysisAppService.CalculateWeeklyCollectedMoneyOrganizationAsync(organization.Id);
            showDataGraph(dataForSeries.Days, dataForSeries.AllTotalAmounts, dataForSeries.InvoiceTotalAmounts, dataForSeries.PackageTotalAmounts);

            var dataForToday = await AnalysisAppService.CalculateDailyCollectedMoneyOrganizationAsync(organization.Id);
            Day = dataForToday.Day;
            collectedMoneyToday = $"{Math.Round((decimal)dataForToday.totalAmounts, 2).ToString()} {@L["SAR"]}";
        }

        string[] colors = { "#0D5480", "#16B7D3", "#CE3333", "#ca06e1" };
        options.ChartPalette = colors;
        options.ShowLegend = true;
        options.LineStrokeWidth = 20;
    }

    private void ShowData(int NumberOfCustomers, int NumberOfVisitors, int NumberOfEmployees, int? NumberOfNoPackage)
    {
        data[0] = NumberOfCustomers;
        data[1] = NumberOfEmployees;
        data[2] = NumberOfVisitors;
        labels[0] = $"{LocalizationHelper.GetLocalizedString("Beneficiaries")} {NumberOfCustomers}";
        labels[1] = $"{LocalizationHelper.GetLocalizedString("Employees")} {NumberOfEmployees}";
        labels[2] = $"{LocalizationHelper.GetLocalizedString("Visitors")} {NumberOfVisitors}";

        if (NumberOfNoPackage is not null)
        {
            data[3] = (double)NumberOfNoPackage;
            labels[3] = $"{LocalizationHelper.GetLocalizedString("NoPackages")} {NumberOfNoPackage}";
        }




    }

    private async Task GetNumberOfParkingAndEmptyAsync(int organizationId)
    {
        getNumberOfParking = await AnalysisAppService.GetNumberOfParkingSlotsAndEmptySolts(organizationId);
        totalOfParking = getNumberOfParking.NumberOfParking;
        totalEmptyOfParking = getNumberOfParking.NumberOfEmptyParking;


    }

    private void showDataGraph(string[] days, double[] allTotalAmounts, double[] invoiceTotalAmounts, double[] packageTotalAmounts)
    {
        XAxisLabels = days;
        Series = new List<ChartSeries>()
     {
         new ChartSeries() { Name = LocalizationHelper.GetLocalizedString("TotalCollectedMoney"), Data =  allTotalAmounts},
         new ChartSeries() { Name = LocalizationHelper.GetLocalizedString("InvoiceCollectedMoney"), Data =  invoiceTotalAmounts},
         new ChartSeries() { Name = LocalizationHelper.GetLocalizedString("PackageCollectedMoney"), Data =  packageTotalAmounts}
     };
    }


    private async Task ExportData(TableData data)
    {
        var folderName = FileHelper.CreateImageDirectory("Folders/Excel");

        string file = System.IO.Path.Combine(folderName, $"{data.Name}.xlsx");

        await GenerateFilesAppService.ExportToExcel(data, file);

        SnackBar.ShowSnackbar(Snackbar, LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.SUCCESS) + " " +
                    LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.EXPORT), Severity.Success);

        await Task.Delay(1000);
        await OpenNewBlank($"Folders/Excel/{data.Name}.xlsx");
    }

    private async Task ExportToExcel(CustomerCategoryStatus category)
    {
        try
        {

            TableData data;
            if (currenttenant.Id == null)
            {
                var garageTransactionAnalysis = await AnalysisAppService.ReportGarageTransactionMdkhlAsync(category, null);
                data = await CreateTableHelper.CreateDataTableForGarageTransactionCustomer(garageTransactionAnalysis, currenttenant.Id);


            }
            else
            {
                var garageTransactionAnalysis = await AnalysisAppService.ReportGarageTransactionMdkhlAsync(category, currenttenant.Id);
                data = await CreateTableHelper.CreateDataTableForGarageTransactionCustomer(garageTransactionAnalysis, currenttenant.Id);

            }
            data.Name = category.ToString() + "Transactions";
            await ExportData(data);
        }
        catch
        {
            SnackBar.ShowSnackbar(Snackbar, LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.FAILED) + " " +
                        LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.EXPORT), Severity.Error);
        }


    }
    private async Task OpenNewBlank(string url)
    {

        await JSRuntime.InvokeVoidAsync("window.open", url, "_blank");
    }
}
