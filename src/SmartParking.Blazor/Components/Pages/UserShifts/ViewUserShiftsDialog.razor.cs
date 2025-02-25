
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Customers;
using SmartParking.KeeperShifts;
using SmartParking.Localization;
using SmartParking.Vehicles;
using System.Globalization;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Blazor.Components.Pages.UserShifts;

public partial class ViewUserShiftsDialog
{
    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; }
    [Parameter]
    public Guid UserId { get; set; }

    private int TotalItem;
    public int CurrentPage { get; set; } = 1; 
    public int RowPerPage { get; set; } = 10;
    private TableData itemData = new TableData();
    private string termSearch;
    private void Cancel() => MudDialog.Cancel();
    public ViewUserShiftsDialog()
    {
        
    }
    protected  override Task OnInitializedAsync()
    {
        return  base.OnInitializedAsync();
    }

    private async Task FetchTableData(PageChangeEventArgs pageInfo)
    {

        var input = new PagedAndSortedAndSearchResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn , 
            SearchingTerm = pageInfo.SearchTerm


        };
        PagedResultDto<KeeperShiftDto> keeperShiftDto;
        if (CurrentTenant.Id == null)
        {
            keeperShiftDto = await KeeperShiftsAppService.GetListAssignedShiftsToUserForMdkhal(null, UserId, input);
        }
        else
        {
            keeperShiftDto = await KeeperShiftsAppService.GetListAssignedShiftsToUserForOrganization(CurrentTenant.Id, UserId, input);

        }

        TotalItem = (int)keeperShiftDto.TotalCount;

        await CreateTable(keeperShiftDto);

    }

    private async Task CreateTable(PagedResultDto<KeeperShiftDto> keeperShiftDto)
    {
        var tableData = new TableData()
        {
            Name = L["UserShifts"],
            ShowHeader = true
        };
        tableData.Columns.Add(new ColumnDef() { DisplayName = L["Id"], Name = "Id", IsFilterable = false, PropertyName = "Id", IsSortable = true, TypeFilter = TypeFilter.Text });
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
            tableData.Columns.Add(new ColumnDef() { DisplayName = L["ShiftName"], Name = "ShiftName", IsFilterable = true, PropertyName = "ArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });
        }
        else
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = L["GarageName"], Name = "GarageName", IsFilterable = true, PropertyName = "GarageNameEn", IsSortable = true, TypeFilter = TypeFilter.Text });
            tableData.Columns.Add(new ColumnDef() { DisplayName = L["ShiftName"], Name = "ShiftName", IsFilterable = true, PropertyName = "EnglishName", IsSortable = true, TypeFilter = TypeFilter.Text });
        }
        tableData.Columns.Add(new ColumnDef() { DisplayName = L["StartDate"], Name = "StartDate", IsFilterable = true, PropertyName = "StartDate", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = L["EndDate"], Name = "EndDate", IsFilterable = true, PropertyName = "EndDate", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = L["StartTime"], Name = "StartTime", IsFilterable = true, PropertyName = "StartTime", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = L["EndTime"], Name = "EndTime", IsFilterable = true, PropertyName = "EndTime", IsSortable = true, TypeFilter = TypeFilter.Text });

        foreach (var item in keeperShiftDto.Items)
        {
            var rowDef = new RowDef();
            rowDef.Index = keeperShiftDto.Items.ToList().IndexOf(item);
            rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
            if (CurrentTenant.Id == null)
            {
                if (CultureInfo.CurrentCulture.Name == "ar")
                {
                    rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("OrganizationName"), item.OrganizationNameAr));
                }
                else
                {
                    rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("OrganizationName"), item.OrganizationNameEn));

                }
            }
            if (CultureInfo.CurrentCulture.Name == "ar")
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("GarageName"), item.GarageNameAr));
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("ShiftName"), item.ArabicName));
            }
            else
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("GarageName"), item.GarageNameEn));
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("ShiftName"), item.EnglishName));
            }
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("StartDate"), item.StartDate.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("EndDate"), item.EndDate.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("StartTime"), item.StartTime.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("EndTime"), item.EndTime.ToString()));
            tableData.Rows.Add(rowDef);
        }
        itemData = tableData;
        await Task.Yield();
    }

}
