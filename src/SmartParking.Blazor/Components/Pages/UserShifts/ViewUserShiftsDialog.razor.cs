using Volo.Application.Services.Dto;
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
    [Parameter]
    public Guid UserId { get; set; }

    private int TotalItem;
    public int CurrentPage { get; set; } = 1; 
    public int RowPerPage { get; set; } = 10;

    private TableData itemData = new TableData();

    private string termSearch;
    private readonly IStringLocalizer<SmartParkingResource> _localizer;
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
            Sorting = pageInfo.SortColumn
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

        CreateTable(keeperShiftDto.Items.ToList());

    }

    private void CreateTable(List<KeeperShiftDto> keeperShiftDtos)
    {
        // Initialize table data to be displayed, setting the table's name and whether to show the header
        var tableData = new TableData()
        {
            Name = L["KeeperShift"],
            ShowHeader = true
        };

        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Id"], Name = "Id", IsFilterable = false, PropertyName = "Id", IsSortable = false, TypeFilter = TypeFilter.Text });
        if (CultureInfo.CurrentCulture.Name == "ar")
        {
        }
        foreach (var item in keeperShiftDtos)
        {
            var rowDef = new RowDef();
            rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
            if (CultureInfo.CurrentCulture.Name == "ar")
            {
            }
             

            

            tableData.Rows.Add(rowDef);
        }
        itemData = tableData;
    }

}
