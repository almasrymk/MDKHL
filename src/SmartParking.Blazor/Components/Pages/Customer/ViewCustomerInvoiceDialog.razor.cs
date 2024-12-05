using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.SPComponents;
using SmartParking.Customers;
using SmartParking.Localization;
using SmartParking.Vehicles;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Authentication;
using System.Threading.Tasks;
using System;
using Volo.Abp;
using SmartParking.Invoices;

namespace SmartParking.Blazor.Components.Pages.Customer;

public partial class ViewCustomerInvoiceDialog
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

    public ViewCustomerInvoiceDialog(IStringLocalizer<SmartParkingResource> localizer)
    {
        _localizer = localizer;
    }

    /// <summary>
    /// Fetches the table data for the specified page and updates the table with the retrieved data.
    /// </summary>
    /// <param name="page">The page number for which to fetch the data.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task FetchTableData(PageChangeEventArgs pageInfo)
    {


        var invoiceDto = await InvoiceAppService.GetInvoiceForCustomer(Guid.Parse(CustomerId));

        CreateTable(invoiceDto);

    }

    private void CreateTable(List<InvoiceDto> invoice)
    {
        // Initialize table data to be displayed, setting the table's name and whether to show the header
        var tableData = new TableData()
        {
            Name = "customer invoice Data",
            ShowHeader = true
        };
        
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Id"], Name = "Id", IsFilterable = false, PropertyName = "Id", IsSortable = false, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["InvoiceNumber"], Name = "InvoiceNumber", IsFilterable = false, PropertyName = "InvoiceNumber", IsSortable = false, TypeFilter = TypeFilter.Text });
        if (CultureInfo.CurrentCulture.Name == "ar")
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["CustomerArName"], Name = "CustomerArName", IsFilterable = false, PropertyName = "CustomerArName", IsSortable = false, TypeFilter = TypeFilter.Text });
        }
        else
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["CustomerEnName"], Name = "CustomerEnName", IsFilterable = false, PropertyName = "CustomerEnName", IsSortable = false, TypeFilter = TypeFilter.Text });
        }
        
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["TotalHours"], Name = "TotalHours", IsFilterable = false, PropertyName = "TotalHours", IsSortable = false, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["InvoicePrice"], Name = "InvoicePrice", IsFilterable = false, PropertyName = "InvoicePrice", IsSortable = false, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["InvoiceType"], Name = "InvoiceType", IsFilterable = false, PropertyName = "InvoiceType", IsSortable = false, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["GarageName"], Name = "GarageName", IsFilterable = false, PropertyName = "GarageName", IsSortable = false, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["PlateNumber"], Name = "PlateNumber", IsFilterable = false, PropertyName = "PlateNumber", IsSortable = false, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["InvoiceTime"], Name = "InvoiceTime", IsFilterable = false, PropertyName = "InvoiceTime", IsSortable = false, TypeFilter = TypeFilter.Text });
        foreach (var item in invoice)
        { 
           
            var rowDef = new RowDef();
            rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["InvoiceNumber"], item.InvoiceNumber));
            if (CultureInfo.CurrentCulture.Name == "ar")
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["CustomerArName"], item.CustomerArName));
            }
            else
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["CustomerEnName"], item.CustomerEnName));
            }
                
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["TotalHours"], item.TotalHours.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["InvoicePrice"], item.InvoicePrice.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["InvoiceType"], item.InvoiceType));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["GarageName"], item.GarageName));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["PlateNumber"], item.PlateNumber));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["InvoiceTime"], item.InvoiceTime.ToString()));


            tableData.Rows.Add(rowDef);
        }
        itemData = tableData;
    }
   

}
