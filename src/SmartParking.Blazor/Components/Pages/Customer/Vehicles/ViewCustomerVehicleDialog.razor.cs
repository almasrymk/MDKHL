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

namespace SmartParking.Blazor.Components.Pages.Customer.Vehicles;

public partial class ViewCustomerVehicleDialog
{
    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; }
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
      
       

           var vehicleDto = await CustomerAppService.GetVehiclesRelatedToSpecificCustomerMdkhl(Guid.Parse(CustomerId));
       
        
        CreateTable(vehicleDto);

    }

    private void CreateTable(List<VehicleDto> vehicles)
    {
        // Initialize table data to be displayed, setting the table's name and whether to show the header
        var tableData = new TableData()
        {
            Name = "customer vehicle Data",
            ShowHeader = true
        };

        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Id"], Name = "Id", IsFilterable = false, PropertyName = "Id", IsSortable = false, TypeFilter = TypeFilter.Text });
        if(CultureInfo.CurrentCulture.Name == "ar")
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["ArPlateNumber"], Name = "ArPlateNumber", IsFilterable = false, PropertyName = "ArPlateNumber", IsSortable = false, TypeFilter = TypeFilter.Text });
        }
        else
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["EnPlateNumber"], Name = "EnPlateNumber", IsFilterable = false, PropertyName = "EnPlateNumber", IsSortable = false, TypeFilter = TypeFilter.Text });
        }
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["ModelYear"], Name = "ModelYear", IsFilterable = false, PropertyName = "ModelYear", IsSortable = false, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["CompanyName"], Name = "CompanyName", IsFilterable = false, PropertyName = "CompanyName", IsSortable = false, TypeFilter = TypeFilter.Text });
        foreach (var item in vehicles)
        {
            var rowDef = new RowDef();
            rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["ArPlateNumber"], item.ArPlateNumber));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["EnPlateNumber"], item.EnPlateNumber));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["ModelYear"], item.ModelYear));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["CompanyName"], item.CompanyName));


            tableData.Rows.Add(rowDef);
        }
        itemData = tableData;
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
        var successMessage = _localizer["VehicleDeletedSuccessfully"];

        var id = int.Parse(row.Cells[0].Value);
        var Name = row.Cells[2].Value;

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
                ShowSnackbar(successMessage, Severity.Success);
                await Task.Delay(1000);

                Navigation.Refresh(true);

            }


        }
        catch (BusinessException ex)
        {

            ShowSnackbar($"Failed to delete vehicle : {ex.Code} + ", Severity.Error);
        }
        catch (AuthenticationException ex)
        {
            ShowSnackbar($"Failed to delete vehicle: {ex.Message} + ", Severity.Error);
        }
        catch (Exception ex)
        {

            ShowSnackbar($"Failed to delete vehicle: {ex.Message} + ", Severity.Error);
        }

    }
    /// <summary>
    /// Displays a snackbar notification with the specified message and severity.
    /// </summary>
    /// <param name="message">The message to display in the snackbar.</param>
    /// <param name="severity">The severity level of the snackbar (e.g., Success, Error).</param>

    void ShowSnackbar(string message, Severity severity)

    {

        Snackbar.Clear();

        Snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomRight;

        Snackbar.Add(message, severity, c => c.SnackbarVariant = Variant.Filled);

    }
}
