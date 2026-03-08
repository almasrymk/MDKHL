using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Localization;
using SmartParking.Shifts;
using SmartParking.Shifts.ShiftDetail;
using SmartParking.Shifts.ShiftsMangers;
using SmartParking.Shifts.ShiftZones;
using System.Globalization;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Blazor.Components.Pages.Shifts.ShiftZones;

public partial class ViewShiftZoneDialog
{
    [CascadingParameter] IMudDialogInstance MudDialog { get; set; }
    [Parameter] public ShiftZoneDto ShiftZone { get; set; } = new ShiftZoneDto();
    private bool IsArabic => CultureInfo.CurrentCulture.Name.StartsWith("ar", StringComparison.OrdinalIgnoreCase);
    private int TotalItem;
    private TableData itemData = new TableData();
    private PagedResultDto<ShiftDetailsDto> StaffDevice = new();

    private string GetLocalized(string en, string ar)
    {
        if (IsArabic)
            return !string.IsNullOrWhiteSpace(ar) ? ar : en ?? string.Empty;

        return !string.IsNullOrWhiteSpace(en) ? en : ar ?? string.Empty;
    }


    protected override Task OnParametersSetAsync()
    {
        ShiftZone ??= new ShiftZoneDto();
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

        StaffDevice = await ShiftDetailsAppService.GetPaginatedByShiftZoneAsync(ShiftZone.Id, input);

        var shiftDetails = StaffDevice.Items.ToList();
        TotalItem = (int)StaffDevice.TotalCount;
        CreateTable(shiftDetails);
    }


    private void CreateTable(List<ShiftDetailsDto> shiftDetails)
    {
        var tableData = new TableData()
        {
            Name = L["ShiftZoneDetails"],
            ShowHeader = true
        };

        tableData.Columns.Add(new ColumnDef() { DisplayName = L["Staff"], Name = "Staff", IsFilterable = false, PropertyName = "Staff", IsSortable = false, TypeFilter = TypeFilter.Text });

        tableData.Columns.Add(new ColumnDef() { DisplayName = L["Device"], Name = "Device", IsFilterable = false, PropertyName = "Device", IsSortable = false, TypeFilter = TypeFilter.Text });
        
        
        foreach (var item in shiftDetails)
        {
            var rowDef = new RowDef();
            rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
            
            rowDef.Cells.Add(new KeyValuePair<string, string>(L["Staff"], item.ShiftStaffDetails.StaffEnName.ToString()));
            
            rowDef.Cells.Add(new KeyValuePair<string, string>(L["Device"], item!.ShiftDevices.DeviceUserName.ToString()));
            



            tableData.Rows.Add(rowDef);
        }
        itemData = tableData;
    }

}