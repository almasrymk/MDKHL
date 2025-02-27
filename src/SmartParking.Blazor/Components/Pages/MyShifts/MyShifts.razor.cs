using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Pages.DailyShift;
using SmartParking.Blazor.Components.Shared;
using SmartParking.DailyShifts;
using SmartParking.Enums;
using SmartParking.Events;
using SmartParking.KeeperShifts;
using SmartParking.Localization;
using System.Globalization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Validation;
using static MudBlazor.CategoryTypes;

namespace SmartParking.Blazor.Components.Pages.MyShifts;

public partial class MyShifts
{
    private List<KeeperShiftDto> KeeperShiftDto = new List<KeeperShiftDto>();
    private KeeperShiftDto FirstItem = new KeeperShiftDto();
    private List<MudMenuData> MenuDatas = new List<MudMenuData>();
    public Guid? TenantId { get; private set; }
    private string sortColumn;
    private string ValueOfMenue = "";
    private TableData itemData = new TableData();
    private int currentPage = 1;
    private int pageSize = 100;
    private int TotalItem;
    public int CurrentPage { get; set; } = 1;
    public int RowPerPage { get; set; } = 10;
    private string termSearch;
    private readonly DialogOptions _backdropClick = new() { BackdropClick = false };
    private readonly IStringLocalizer<SmartParkingResource> _localizer;
    private bool IsStartShift { get; set; } = true;
    private bool IsEndShift { get; set; } = false;
    public MyShifts(IStringLocalizer<SmartParkingResource> localizer)
    {
        _localizer = localizer;
    }
    protected async override Task OnInitializedAsync()
    {

        await base.OnInitializedAsync();
    }

    private async Task FetchTableData(PageChangeEventArgs pageInfo)
    {
        termSearch = pageInfo.SearchTerm;
        CurrentPage = pageInfo.PageNumber;
        RowPerPage = pageInfo.RowsPerPage;
        sortColumn = pageInfo.SortColumn;
        string sort;
        if (pageInfo.SortColumn == "Id")
            sort = "KeeperName DESC";
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
        var keeperShiftDto = await KeeperShiftAppService.GetMyShiftsForCurrentUser(CurrentUser.Id, input);
        TotalItem = (int)keeperShiftDto.TotalCount;
        await CreateTable(keeperShiftDto);
        FirstItem = keeperShiftDto.Items.ToList().FirstOrDefault();
        if (string.IsNullOrEmpty(input.SearchingTerm) && FirstItem != null)
        {
            
                var state = await keeperShiftSingletonService.GetAsync(FirstItem.Id);
                if (state != null)
                {
                    IsStartShift = false;
                    IsEndShift = true;
                    StateHasChanged();
                }
                else
                {
                    IsStartShift = true;
                    IsEndShift = false;
                    StateHasChanged();
                }
            
        }
        else
        {
            IsStartShift = false;
            IsEndShift = false;
            StateHasChanged();
        }
    }


    private async Task CreateTable(PagedResultDto<KeeperShiftDto> keeperShiftDto)
    {
        var tableData = new TableData()
        {
            Name = L["MyShifts"],
            ShowHeader = true
        };
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Id"], Name = "Id", IsFilterable = false, PropertyName = "Id", IsSortable = true, TypeFilter = TypeFilter.Text });

        if (CultureInfo.CurrentCulture.Name == "ar")
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["GarageName"], Name = "GarageName", IsFilterable = true, PropertyName = "GarageNameAr", IsSortable = true, TypeFilter = TypeFilter.Text });
            tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["ShiftName"], Name = "ShiftName", IsFilterable = true, PropertyName = "ArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });
        }
        else
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["GarageName"], Name = "GarageName", IsFilterable = true, PropertyName = "GarageNameEn", IsSortable = true, TypeFilter = TypeFilter.Text });
            tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["ShiftName"], Name = "ShiftName", IsFilterable = true, PropertyName = "EnglishName", IsSortable = true, TypeFilter = TypeFilter.Text });
        }
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["StartDate"], Name = "StartDate", IsFilterable = true, PropertyName = "StartDate", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["EndDate"], Name = "EndDate", IsFilterable = true, PropertyName = "EndDate", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["StartTime"], Name = "StartTime", IsFilterable = true, PropertyName = "StartTime", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["EndTime"], Name = "EndTime", IsFilterable = true, PropertyName = "EndTime", IsSortable = true, TypeFilter = TypeFilter.Text });

        foreach (var item in keeperShiftDto.Items)
        {
            var rowDef = new RowDef();
            rowDef.Index = keeperShiftDto.Items.ToList().IndexOf(item);
            rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
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
    private async Task<IDialogReference> OnClickRowView(RowDef row)
    {
        var id = int.Parse(row.Cells[0].Value);
        var keeperShiftDto = await KeeperShiftAppService.GetAsync(id);
        var parameters = new DialogParameters
        {
            ["KeeperShiftDto"] = keeperShiftDto,
        };

        return await Dialog.ShowAsync<ViewMyShifts>("View MyShifts", parameters);
    }

    private async Task HandleStartShift(RowDef row)
    {
        try
        {
            var input = await DailyShiftAppService.CreateDailyShiftDto(FirstItem, KeeperShiftRequestType.StartShift);
            var dailyShiftDto = await DailyShiftAppService.CreateAsync(input);
            var state = await keeperShiftSingletonService.SetAsync(int.Parse(row.Cells[0].Value));
            if (state != null)
            {

                IsStartShift = true;
                IsEndShift = false;
                StateHasChanged();

            }
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]} {L[AlertMessages.AlertMessages.START]} {L["Shift"]}", Severity.Success);


        }
        catch (AbpValidationException validationException)
        {
            foreach (var validationError in validationException.ValidationErrors)
            {
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.VAlIDATION_ERROR]}: {L[validationError.MemberNames.First()]} - {validationError.ErrorMessage}", Severity.Error);
            }
        }
        catch (BusinessException ex)
        {

            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.START]} : {ex.Message}", Severity.Error);
        }

        catch (Exception ex)
        {

            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.START]} : {ex.Message} ", Severity.Error);
        }





    }

    private async Task HandleEndShift(RowDef row)
    {

        try
        {
            
            var createDailyShiftDto = await DailyShiftAppService.CreateDailyShiftDto(FirstItem, KeeperShiftRequestType.EndShift);
            var dailyShiftDto = await DailyShiftAppService.CreateAsync(createDailyShiftDto);
            await keeperShiftSingletonService.DisposeAsync();
            if (dailyShiftDto != null)
            {
                IsStartShift = true;
                IsEndShift = false;
                StateHasChanged();
            }
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]} {L[AlertMessages.AlertMessages.END]} {L["Shift"]}", Severity.Success);


        }
        catch (AbpValidationException validationException)
        {
            foreach (var validationError in validationException.ValidationErrors)
            {
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.VAlIDATION_ERROR]}: {L[validationError.MemberNames.First()]} - {validationError.ErrorMessage}", Severity.Error);
            }
        }
        catch (BusinessException ex)
        {

            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.END]} : {ex.Message}", Severity.Error);
        }

        catch (Exception ex)
        {

            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.END]} : {ex.Message} ", Severity.Error);
        }
    }


}
