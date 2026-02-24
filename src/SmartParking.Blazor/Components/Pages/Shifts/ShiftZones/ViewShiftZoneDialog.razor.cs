using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Shared.Utilities;
using SmartParking.Currency;
using SmartParking.CustomerPackages;
using SmartParking.Enums;
using SmartParking.Localization;
using SmartParking.Shifts;
using SmartParking.Shifts.ShiftDetail;
using SmartParking.Shifts.ShiftsMangers;
using SmartParking.Shifts.ShiftZones;
using System.Globalization;
using System.Runtime.Intrinsics.Arm;
using Volo.Abp.Application.Dtos;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Blazor.Components.Pages.Shifts.ShiftZones;

public partial class ViewShiftZoneDialog
{
    [CascadingParameter] IMudDialogInstance MudDialog { get; set; }
    [Parameter] public ShiftZoneDto ShiftZone { get; set; } = new ShiftZoneDto();
    private bool IsArabic => CultureInfo.CurrentCulture.Name.StartsWith("ar", StringComparison.OrdinalIgnoreCase);
    private int TotalItem;
    private TableData itemData = new TableData();
    private readonly IStringLocalizer<SmartParkingResource> _localizer;


    private string GetLocalized(string en, string ar)
    {
        if (IsArabic)
            return !string.IsNullOrWhiteSpace(ar) ? ar : en ?? string.Empty;

        return !string.IsNullOrWhiteSpace(en) ? en : ar ?? string.Empty;
    }


    protected override Task OnParametersSetAsync()
    {
        ShiftZone ??= new ShiftZoneDto();
        ShiftZone.ShiftDto ??= new ShiftDto();
        ShiftZone.ShiftMangerDto ??= new ShiftMangerDto();

        return base.OnParametersSetAsync();
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }

    private async Task FetchTableData(PageChangeEventArgs pageInfo)
    {
        var input = new PagedAndSortedAndSearchResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn
        };

        PagedResultDto<ShiftDetailsDto> StaffDevice = new();

        var x = ShiftZone.ShiftDetailsDto;
        StaffDevice = 

        TotalItem = (int)StaffDevice.TotalCount;
        CreateTable(StaffDevice.Items.ToList());
    }


    private void CreateTable(List<ShiftDetailsDto> shiftDetails)
    {
        var tableData = new TableData()
        {
            Name = L["CustomerPackageHeader"],
            ShowHeader = true
        };

        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Staff"], Name = "Staff", IsFilterable = false, PropertyName = "Staff", IsSortable = false, TypeFilter = TypeFilter.Text });

        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Device"], Name = "Device", IsFilterable = false, PropertyName = "Device", IsSortable = false, TypeFilter = TypeFilter.Text });
        
        
        foreach (var item in shiftDetails)
        {
            var rowDef = new RowDef();
            rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
            
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Staff"], item.ShiftStaffDetails.ToString()));
            
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Device"], item!.ShiftDevices.ToString()));
            



            tableData.Rows.Add(rowDef);
        }
        itemData = tableData;
    }

}