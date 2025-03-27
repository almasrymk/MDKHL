using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Localization;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Volo.Abp.Application.Dtos;
using SmartParking.Vehicles;
using System.Globalization;
using Blazorise;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SmartParking.Blazor.Components.SPComponents;
using System.Security.Authentication;
using Volo.Abp;
using Volo.Abp.MultiTenancy;
using SmartParking.Entities.Vehicles;
using System.Text.RegularExpressions;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Analysis;
using Microsoft.JSInterop;
using SmartParking.PDF;
using SmartParking.Utilities;
using SmartParking.Helpers;
using SmartParking.Results;
using SmartParking.Blazor.Components.Pages.Customer.CustomerPackage;

namespace SmartParking.Blazor.Components.Pages.Customer.Vehicles;

public partial class ViewCustomerVehicleDialog
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; }
    [Parameter] public string CustomerId { get; set; }
    private void Cancel() => MudDialog.Cancel();

    private int TotalItem;
    public int CurrentPage { get; set; } = 1; // Default current page value is 10
    public int RowPerPage { get; set; } = 10; // Default value is 10

    private TableData itemData = new TableData();

    private string termSearch;
    private readonly IStringLocalizer<SmartParkingResource> _localizer;
    private bool ShowEdit;

    public ViewCustomerVehicleDialog(IStringLocalizer<SmartParkingResource> localizer)
    {
        _localizer = localizer;
    }
    protected override Task OnInitializedAsync()
    {
        if (CurrentTenant.Id is null)
            ShowEdit = true;
        return base.OnInitializedAsync();
    }

    /// <summary>
    /// Fetches the table data for the specified page and updates the table with the retrieved data.
    /// </summary>
    /// <param name="page">The page number for which to fetch the data.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task FetchTableData(PageChangeEventArgs pageInfo)
    {

        var input = new PagedAndSortedAndSearchResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn
        };

        var vehicleDto = await CustomerAppService.GetVehiclesRelatedToSpecificCustomerMdkhl(Guid.Parse(CustomerId), input);
        TotalItem =(int) vehicleDto.TotalCount;
        
        CreateTable(vehicleDto.Items.ToList());

    }

    private void CreateTable(List<VehicleDto> vehicles)
    {
        // Initialize table data to be displayed, setting the table's name and whether to show the header
        var tableData = new TableData()
        {
            Name = L["CustomerVehicleHeader"],
            ShowHeader = true
        };

        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Id"], Name = "Id", IsFilterable = false, PropertyName = "Id", IsSortable = false, TypeFilter = TypeFilter.Text });
        if(CultureInfo.CurrentCulture.Name == "ar")
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["PlateNumber"], Name = "ArPlateNumber", IsFilterable = false, PropertyName = "ArPlateNumber", IsSortable = false, TypeFilter = TypeFilter.Text });
            tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["LicenceType"], Name = "LicenceType", IsFilterable = false, PropertyName = "LicenceType.ArabicName", IsSortable = false, TypeFilter = TypeFilter.Text });
            tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["BrandName"], Name = "BrandName", IsFilterable = false, PropertyName = "VehicleBrand.ArabicName", IsSortable = false, TypeFilter = TypeFilter.Text });
            tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["ModelName"], Name = "ModelName", IsFilterable = false, PropertyName = "VehicleModel.ArabicName", IsSortable = false, TypeFilter = TypeFilter.Text });

        }
        else
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["PlateNumber"], Name = "EnPlateNumber", IsFilterable = false, PropertyName = "EnPlateNumber", IsSortable = false, TypeFilter = TypeFilter.Text });
            tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["LicenceType"], Name = "LicenceType", IsFilterable = false, PropertyName = "LicenceType.EnglishName", IsSortable = false, TypeFilter = TypeFilter.Text });
            tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["BrandName"], Name = "BrandName", IsFilterable = false, PropertyName = "VehicleBrand.EnglishName", IsSortable = false, TypeFilter = TypeFilter.Text });
            tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["ModelName"], Name = "ModelName", IsFilterable = false, PropertyName = "VehicleModel.EnglishName", IsSortable = false, TypeFilter = TypeFilter.Text });

        }
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["ModelYear"], Name = "ModelYear", IsFilterable = false, PropertyName = "ModelYear", IsSortable = false, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["CompanyName"], Name = "CompanyName", IsFilterable = false, PropertyName = "CompanyName", IsSortable = false, TypeFilter = TypeFilter.Text });
        foreach (var item in vehicles)
        {
            var rowDef = new RowDef();
            rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
            if (CultureInfo.CurrentCulture.Name == "ar")
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["PlateNumber"], item.ArPlateNumber));
                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["LicenceType"], item.LicenceArabicName));
                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["BrandName"], item.BrandArabicName));
                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["ModelName"], item.ModelArabicName));
            }
            else
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["PlateNumber"], item.EnPlateNumber));
                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["LicenceType"], item.LicenceEnglishName));
                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["BrandName"], item.BrandEnglishName));
                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["ModelName"], item.ModelEnglishName));
            }


            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["ModelYear"], item.ModelYear));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["CompanyName"], item.CompanyName));


            tableData.Rows.Add(rowDef);
        }
        itemData = tableData;
    }
    public async Task ShowCustomerTransactions((RowDef row, string TypeDownload) args)
    {
        var (row, TypeDownload) = args;
        var plateNumber = row.Cells[1].Value;
        TableData Data;
        if (CurrentTenant.Id == null)
        {
            var garageTransactionAnalsisDto = await AnalysisAppService.ReportGarageTransactionForMDKHLPlateAsync(plateNumber);
            Data =await CreateTableHelper.CreateDataTableForGarageTransactionCustomer(garageTransactionAnalsisDto , CurrentTenant.Id);
        }
        else
        {
            var garageTransactionAnalsisDto = await AnalysisAppService.ReportGarageTransactionForOrganization(plateNumber,CurrentTenant.Id);
            Data =await CreateTableHelper.CreateDataTableForGarageTransactionCustomer(garageTransactionAnalsisDto , CurrentTenant.Id);

        }
        Data.Name = L["VicheleTransactionsReport"];
        var exportTask = TypeDownload.ToLower() switch
        {
            "pdf" => JSRuntime.ExportToPdfAsync(Data, GenerateFilesAppService, Snackbar),
            "excel" => JSRuntime.ExportToExcelAsync(Data, GenerateFilesAppService, Snackbar),
            "csv" => JSRuntime.ExportToCsvAsync(Data, GenerateFilesAppService, Snackbar),
            _ => Task.CompletedTask
        };

        await exportTask;
        

    }

    public async Task DownloadExcel(TableData Data, string Name)
    {
        var folderName = FileHelper.CreateImageDirectory(PathHelper.ReportsFolder);
        string file = System.IO.Path.Combine(folderName, $"{Name} Transactions.xlsx");
        await GenerateFilesAppService.ExportToExcel(Data, file);
        SnackBar.ShowSnackbar(Snackbar, LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.SUCCESS) + " " +
                    LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.EXPORT), Severity.Success);

        await JSRuntime.InvokeVoidAsync("window.open", $"{PathHelper.ReportsFolder}/{Name} Transactions.xlsx", "_blank");
    }     
    
    private async Task<IDialogReference> HandleAdd()
    {


        var parameters = new DialogParameters
        {
            ["CustomerId"] = Guid.Parse(CustomerId),
        };


        return await DialogService.ShowAsync<AddVehicleDialog>("view customer vehicle", parameters);
    }

    private async Task<CreateUpdateVehicleDto> CreateVehicleEditDto(VehicleDto vehicle)
    {
        var ArPlate = await VehiclesAppService.SplitPlateAsync(vehicle.ArPlateNumber);
        var EnPlate = await VehiclesAppService.SplitPlateAsync(vehicle.EnPlateNumber);

        return new CreateUpdateVehicleDto
        {
            ArPlateLetters = ArPlate.letters,
            ArPlateNumber = ArPlate.numbers,
            BelongToCompany = vehicle.BelongToCompany,
            CompanyName = vehicle.CompanyName,
            EnPlateLetters = EnPlate.letters,
            EnPlateNumber = EnPlate.numbers,
            LicenceTypeId = vehicle.LicenceTypeId,
            ModelYear = vehicle.ModelYear,
            PlateColorId = vehicle.PlateColorId,
            VehicleBrandId = vehicle.VehicleBrandId,
            VehicleModelId = vehicle.VehicleModelId,
            CustomerId = vehicle.CustomerId,
        };
    }
    private async Task<IDialogReference> HandleEdit(RowDef row)
    {
        var vehicleId = int.Parse(row.Cells[0].Value);
        var vehicle = await VehiclesAppService.GetAsync(vehicleId);
        var vehicleEdit = await CreateVehicleEditDto(vehicle);
         
        var parameters = new DialogParameters
        {
            ["VehicleId"] = vehicleId,
            ["Vehicle"] = vehicleEdit
        };

        return await DialogService.ShowAsync<EditVehicleDialog>("edit customer vehicle", parameters);
    }
    private async Task HandleDelete(RowDef row)
    {

        var id = int.Parse(row.Cells[0].Value);
        var Name = row.Cells[1].Value;

        var parameters = new DialogParameters
        {
            ["Name"] = Name,

        };

        var dialog = await Dialog.ShowAsync<SPDialogDelete>("Delete vehicle", parameters);
        var result = await dialog.Result;
        try
        {
            if (!result.Canceled)
            {
                await VehiclesAppService.DeleteAsync(id);
                SnackBar.ShowSnackbar(Snackbar, LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.SUCCESS) + " " +
                    LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.DELETE) + " " +
                    LocalizationHelper.GetLocalizedString("Vehicle"), Severity.Success);
                await Task.Delay(1000);

                Navigation.Refresh(true);

            }


        }
        catch (BusinessException ex)
        {

            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.ADD]} {L[AlertMessages.AlertMessages.FAILED]}: {ex.Message}", Severity.Error);
        }
        catch (AuthenticationException ex)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.ADD]} {L[AlertMessages.AlertMessages.FAILED]} : {ex.Message} ", Severity.Error);
        }
        catch (Exception ex)
        {

            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.ADD]} {L[AlertMessages.AlertMessages.FAILED]}: {ex.Message} ", Severity.Error);
        }

    }

    public async Task<IDialogReference> HandleAssign(string vehicleId)
    {
        var vehicleIdAfterParse = int.Parse(vehicleId);
        var customer = await CustomerAppService.GetAsync(Guid.Parse(CustomerId));
        var parameters = new DialogParameters
        {
            ["Customer"] = customer,
            ["VehicleId"] = vehicleIdAfterParse,
        };


        return await DialogService.ShowAsync<AssignPackgeDialog>("view customer package", parameters);
    }

}
