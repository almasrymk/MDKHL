using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Blazor.Components.SPComponents;
using SmartParking.Localization;
using SmartParking.Permissions.UnitsPermissions;
using SmartParking.Units;
using System.Globalization;
using System.Security.Authentication;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.Units;

public partial class Unit
{
    private int TotalItem;
    public int CurrentPage { get; set; } = 1;
    public int RowPerPage { get; set; } = 10;
    private string sortColumn;
    private TableData itemData = new TableData();
    private string termSearch;
    private readonly DialogOptions _backdropClick = new() { BackdropClick = false };
    private readonly IStringLocalizer<SmartParkingResource> _localizer;

    public Unit(IStringLocalizer<SmartParkingResource> localizer)
    {
    _localizer = localizer;
        CreatePolicyName = UnitPermissions.Create;
        UpdatePolicyName = UnitPermissions.Edit;
        DeletePolicyName = UnitPermissions.Delete;
    }

    protected async override Task OnInitializedAsync()
    {
   //     if (!await AuthorizationService.IsGrantedAsync(UnitPermissions.View))
   //{
   //      Navigation.NavigateTo("/Account/AccessDenied", true);
   //}

   await base.OnInitializedAsync();
    }

    private async Task HandleFilter(PageChangeEventArgs pageInfo)
    {
        var input = new PagedAndSortedAndFilteredResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn,
            SearchingTerm = termSearch,
            filters = pageInfo.Filters
        };

        var unitsDto = await UnitAppService.GetListAsync(input);
        TotalItem = (int)unitsDto.TotalCount;
        await CreateTable(unitsDto);
    }

    private async Task HandleSearch(PageChangeEventArgs pageInfo)
    {
        termSearch = pageInfo.SearchTerm;
        CurrentPage = pageInfo.PageNumber;
        RowPerPage = pageInfo.RowsPerPage;
        sortColumn = pageInfo.SortColumn;

        var input = new PagedAndSortedAndSearchResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn,
            SearchingTerm = termSearch
        };

        var unitsDto = await UnitAppService.Search(input);
        TotalItem = (int)unitsDto.TotalCount;
        await CreateTable(unitsDto);
    }

    private async Task FetchTableData(PageChangeEventArgs pageInfo)
    {
        var input = new PagedAndSortedAndSearchResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn,
            SearchingTerm = pageInfo.SearchTerm
        };

        var unitsDto = await UnitAppService.GetListAsync(input);
        TotalItem = (int)unitsDto.TotalCount;
        await CreateTable(unitsDto);
    }

    private async Task CreateTable(PagedResultDto<UnitDto> units)
    {
        var tableData = new TableData()
        {
            Name = L["Units"],
            ShowHeader = true
        };

        tableData.Columns.Add(new ColumnDef() { DisplayName = L["Id"], Name = "Id", IsFilterable = false, PropertyName = "Id", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = L["Code"], Name = "Code", IsFilterable = true, PropertyName = "Code", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = L["OwnerSlots"], Name = "OwnerSlots", IsFilterable = true, PropertyName = "OwnerSlots", IsSortable = true, TypeFilter = TypeFilter.Number });
        tableData.Columns.Add(new ColumnDef() { DisplayName = L["RenterSlots"], Name = "RenterSlots", IsFilterable = true, PropertyName = "RenterSlots", IsSortable = true, TypeFilter = TypeFilter.Number });
        tableData.Columns.Add(new ColumnDef() { DisplayName = L["Description"], Name = "Description", IsFilterable = true, PropertyName = "Description", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = L["Status"], Name = "Status", IsFilterable = false, PropertyName = "Status", IsSortable = true, TypeFilter = TypeFilter.Enum });

        foreach (var item in units.Items)
        {
            var rowDef = new RowDef();
            rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(L["Code"], item.Code));
            rowDef.Cells.Add(new KeyValuePair<string, string>(L["OwnerSlots"], item.OwnerSlots.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(L["RenterSlots"], item.RenterSlots.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(L["Description"], Truncate(item.Description, 15)));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Status"], _localizer[$"{item.Status.ToString()}"]));
            tableData.Rows.Add(rowDef);
        }

        itemData = tableData;
        await Task.Yield();
    }

    private Task OpenDialogAsync(DialogOptions options)
    {
        return Dialog.ShowAsync<AddUnitDialog>("Add Unit", options);
    }

 private async Task<IDialogReference> OnClickRowEdit(RowDef row)
    {
        var id = int.Parse(row.Cells[0].Value);
        var unitDto = await UnitAppService.GetAsync(id);

        var createUpdateUnit = new CreateUpdateUnitDto
        {
            Code = unitDto.Code,
            OwnerSlots = unitDto.OwnerSlots,
            RenterSlots = unitDto.RenterSlots,
            Description = unitDto.Description,
            OrganizationId = unitDto.OrganizationId
        };

        var parameters = new DialogParameters
        {
            ["Unit"] = createUpdateUnit,
            ["UpdatedId"] = id
        };

        return await Dialog.ShowAsync<EditUnitDialog>("Edit Unit", parameters);
    }

    private async Task<IDialogReference> OnClickRowView(RowDef row)
    {
        var id = int.Parse(row.Cells[0].Value);
        var unitDto = await UnitAppService.GetAsync(id);

        var parameters = new DialogParameters
     {
          ["Unit"] = unitDto
        };

        return await Dialog.ShowAsync<DetailsUnitDialog>("View Unit", parameters);
    }

    private async Task OnClickRowDelete(RowDef row)
    {
        var id = int.Parse(row.Cells[0].Value);
        var code = row.Cells[1].Value;

        var parameters = new DialogParameters
        {
            ["Name"] = code
        };

        var dialog = await Dialog.ShowAsync<SPDialogDelete>("UnitDeleteDialog", parameters);
        var result = await dialog.Result;

        try
        {
            if (!result.Canceled)
            {
                await UnitAppService.DeleteAsync(id);
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]} {L[AlertMessages.AlertMessages.DELETE]}", Severity.Success);
                await Task.Delay(1000);
                Navigation.NavigateTo("/units", true);
            }
        }
        catch (BusinessException ex)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.DELETE]} : {ex.Message}", Severity.Error);
        }
        catch (AuthenticationException ex)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.DELETE]} : {ex.Message}", Severity.Error);
        }
        catch (Exception ex)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.DELETE]} : {ex.Message}", Severity.Error);
        }
    }

    public string Truncate(string str, int maxLength)
    {
        if (string.IsNullOrEmpty(str)) return str;
        return str.Length > maxLength ? str.Substring(0, maxLength) + "..." : str;
    }
}
