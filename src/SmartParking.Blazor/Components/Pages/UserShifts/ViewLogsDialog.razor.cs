using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.DailyShifts;
using System.Globalization;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Blazor.Components.Pages.UserShifts;

public partial class ViewLogsDialog
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; }

    [Parameter]
    public string userId { get; set; }

    private int TotalItem;
    private TableData itemData = new TableData();
    public int CurrentPage { get; set; } = 1; // Default current page value is 10
    public int RowPerPage { get; set; } = 10; // Default value is 10


    private string termSearch;

    private async Task FetchTableData(PageChangeEventArgs pageInfo)
    {
        termSearch = pageInfo.SearchTerm;
        CurrentPage = pageInfo.PageNumber;
        RowPerPage = pageInfo.RowsPerPage;
        string sort;
        if (pageInfo.SortColumn == "Id")
            sort = "RequestTime DESC";
        else
            sort = pageInfo.SortColumn;

        CurrentPage = pageInfo.PageNumber;
        RowPerPage = pageInfo.RowsPerPage;
        var input = new PagedAndSortedAndSearchResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = sort,
            SearchingTerm = pageInfo.SearchTerm

        };
        PagedResultDto<DailyShiftDto> userLogs;
        if (CurrentTenant.Id == null)
        {
            userLogs = await DailyShiftsAppService.GetListDailyShiftsAsync(input, null, Guid.Parse(userId));
        }
        else
        {
            userLogs = await DailyShiftsAppService.GetListDailyShiftsOrganizationAsync(input, CurrentTenant.Id, Guid.Parse(userId));
        }
        TotalItem = (int)userLogs.TotalCount;


        await CreateTable(userLogs.Items.ToList());

    }

    private async Task CreateTable(List<DailyShiftDto> dailyShiftDto)
    {
        // Initialize table data to be displayed, setting the table's name and whether to show the header
        var tableData = new TableData()
        {
            Name = L["DailyShifts"],
            ShowHeader = true
        };


        tableData.Columns.Add(new ColumnDef() { DisplayName = L["Id"], Name = "Id", IsFilterable = false, PropertyName = "Id", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = L["Name"], Name = "UserName", IsFilterable = true, PropertyName = "KeeperName", IsSortable = true, TypeFilter = TypeFilter.Text });
        if (CurrentTenant.Id == null)
        {
            if (CultureInfo.CurrentCulture.Name == "ar")
            {
                tableData.Columns.Add(new ColumnDef() { DisplayName = L["OrganizationName"], Name = "OrganizationName", IsFilterable = true, PropertyName = "OrganizationNameAr", IsSortable = true, TypeFilter = TypeFilter.Text });
            }
            else
            {
                tableData.Columns.Add(new ColumnDef() { DisplayName = L["OrganizationName"], Name = "OrganizationName", IsFilterable = true, PropertyName = "OrganizationNameEn", IsSortable = true, TypeFilter = TypeFilter.Text });
            }
        }
        if (CultureInfo.CurrentCulture.Name == "ar")
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = L["GarageName"], Name = "GarageName", IsFilterable = true, PropertyName = "GarageNameAr", IsSortable = true, TypeFilter = TypeFilter.Text });
            tableData.Columns.Add(new ColumnDef() { DisplayName = L["ShiftName"], Name = "ShiftName", IsFilterable = true, PropertyName = "ShiftNameAr", IsSortable = true, TypeFilter = TypeFilter.Text });
        }
        else
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = L["GarageName"], Name = "GarageName", IsFilterable = true, PropertyName = "GarageNameEn", IsSortable = true, TypeFilter = TypeFilter.Text });
            tableData.Columns.Add(new ColumnDef() { DisplayName = L["ShiftName"], Name = "ShiftName", IsFilterable = true, PropertyName = "ShiftNameEn", IsSortable = true, TypeFilter = TypeFilter.Text });
        }
        tableData.Columns.Add(new ColumnDef() { DisplayName = L["StartDate"], Name = "StartDate", IsFilterable = true, PropertyName = "StartDate", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = L["EndDate"], Name = "EndDate", IsFilterable = true, PropertyName = "EndDate", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = L["StartTime"], Name = "StartTime", IsFilterable = true, PropertyName = "StartTime", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = L["EndTime"], Name = "EndTime", IsFilterable = true, PropertyName = "EndTime", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = L["RequestTime"], Name = "RequestTime", IsFilterable = true, PropertyName = "RequestTime", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = L["KeeperShiftRequestType"], Name = "KeeperShiftRequestType", IsFilterable = true, PropertyName = "KeeperShiftRequestType", IsSortable = true, TypeFilter = TypeFilter.Enum });

        foreach (var item in dailyShiftDto)
        {
            var rowDef = new RowDef();
            rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(L["Name"], item.KeeperName));
            if (CurrentTenant.Id == null)
            {
                if (CultureInfo.CurrentCulture.Name == "ar")
                {
                    rowDef.Cells.Add(new KeyValuePair<string, string>(L["OrganizationName"], item.OrganizationNameAr));
                }
                else
                {
                    rowDef.Cells.Add(new KeyValuePair<string, string>(L["OrganizationName"], item.OrganizationNameEn));

                }
            }
            if (CultureInfo.CurrentCulture.Name == "ar")
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(L["GarageName"], item.GarageNameAr));
                rowDef.Cells.Add(new KeyValuePair<string, string>(L["ShiftName"], item.ShiftNameAr));
            }
            else
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(L["GarageName"], item.GarageNameEn));
                rowDef.Cells.Add(new KeyValuePair<string, string>(L["ShiftName"], item.ShiftNameEn));
            }
            rowDef.Cells.Add(new KeyValuePair<string, string>(L["StartDate"], item.StartDate.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(L["EndDate"], item.EndDate.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(L["StartTime"], item.StartTime.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(L["EndTime"], item.EndTime.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(L["RequestTime"], item.RequestTime.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(L["KeeperShiftRequestType"], L[item.KeeperShiftRequestType.ToString()]));
            tableData.Rows.Add(rowDef);
        }
        itemData = tableData;
        await Task.Yield();
    }



    private void Cancel() => MudDialog.Cancel();
}
