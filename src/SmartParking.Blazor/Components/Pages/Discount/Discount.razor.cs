using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Pages.Countries;
using SmartParking.Blazor.Components.Pages.States;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Blazor.Components.SPComponents;
using SmartParking.Discounts;
using SmartParking.Entities;
using SmartParking.Localization;
using SmartParking.Lookups;
using SmartParking.Permissions.ShiftsPermissions;
using System.ComponentModel;
using System.Globalization;
using System.Security.Authentication;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace SmartParking.Blazor.Components.Pages.Discount;

public partial class Discount
{

    private DialogOptions options = new DialogOptions { CloseOnEscapeKey = true };
    private int TotalItem;
    public int CurrentPage { get; set; } = 1;
    public int RowPerPage { get; set; } = 10;
    public Guid? TenantId { get; private set; }
    private string sortColumn;
    private string ValueOfMenue = "";
    private TableData itemData = new TableData();
    private string termSearch;
    private readonly IStringLocalizer<SmartParkingResource> _localizer;
    public Discount(IStringLocalizer<SmartParkingResource> localizer)
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
        var input = new PagedAndSortedAndSearchResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn,
            SearchingTerm = pageInfo.SearchTerm
        };
        if (pageInfo.SortColumn == "Id")
            input.Sorting = "EnglishName";

        PagedResultDto<DiscountDto> discountDto = new PagedResultDto<DiscountDto>();
        discountDto = await DiscountAppService.GetListAsync(input);
        TotalItem = (int)discountDto.TotalCount;
        await CreateTable(discountDto.Items);

    }

    private async Task CreateTable(IReadOnlyList<DiscountDto> discountDto)
    {
        var tableData = new TableData()
        {
            Name = L["Discount"],
            ShowHeader = true
        };
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Id"], Name = "Id", IsFilterable = false, PropertyName = "Id", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["ArabicName"], Name = "ArabicName", IsFilterable = true, PropertyName = "ArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["EnglishName"], Name = "EnglishName", IsFilterable = true, PropertyName = "EnglishName", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["DiscountPercent"], Name = "DiscountPercent", IsFilterable = true, PropertyName = "DiscountPercent", IsSortable = true, TypeFilter = TypeFilter.Number });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Notes"], Name = "Notes", IsFilterable = true, PropertyName = "Notes", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Status"], Name = "Status", IsFilterable = false, PropertyName = "Status", IsSortable = true, TypeFilter = TypeFilter.Enum });

        foreach (var item in discountDto)
        {
            var rowDef = new RowDef();
            rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["ArabicName"], item.ArabicName));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["EnglishName"], item.EnglishName));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["DiscountPercent"], item.DiscountPercent.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Notes"], item.Notes));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Status"], _localizer[$"{item.Status.ToString()}"]));


            tableData.Rows.Add(rowDef);
        }
        itemData = tableData;
        await Task.Yield();
    }
    private Task CreateDiscount()
    {


        return DialogService.ShowAsync<AddDiscountDialog>("Simple Dialog", options);
    }
    private async Task<IDialogReference> OnClickRowEdit(RowDef row)
    {
        var id = int.Parse(row.Cells[0].Value);

        var discountDto = await DiscountAppService.GetAsync(id);

        var createUpdateDiscout = new CreateUpdateDiscountDto
        {
            ArabicName = discountDto.ArabicName,
            EnglishName = discountDto.EnglishName,
            Notes = discountDto.Notes,
            DiscountPercent = discountDto.DiscountPercent,
            Status=discountDto.Status

        };



        var parameters = new DialogParameters
        {
            ["discount"] = discountDto,
            ["Id"] = id
        };


        return await DialogService.ShowAsync<EditDiscountDialog>("Edit Discount", parameters);

    }
    private async Task<IDialogReference> OnClickRowView(RowDef row)
    {
        var id = int.Parse(row.Cells[0].Value);

        var discountDto = await DiscountAppService.GetAsync(id);


        var parameters = new DialogParameters
        {
            ["discount"] = discountDto,
        };


        return await DialogService.ShowAsync<DetailsDiscountDialog>("View Discount", parameters);
    }

    private async Task OnClickRowDelete(RowDef row)
    {
        var successMessage = _localizer["DiscountDeletedSuccessfully "];
        var id = int.Parse(row.Cells[0].Value);
        var Name = row.Cells[1].Value;

        var parameters = new DialogParameters
        {
            ["Name"] = Name,

        };
        await DiscountAppService.DeleteAsync(id);
        var dialog = await Dialog.ShowAsync<SPDialogDelete>("Discount Delete Dialog", parameters);
        var result = await dialog.Result;
        try
        {
            if (!result.Canceled)
            {
                ShowSnackbar(successMessage, Severity.Success);
                await Task.Delay(1000);

                Navigation.NavigateTo("/discounts", forceLoad: true);

            }


        }
        catch (BusinessException ex)
        {

            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.DELETE]}: {ex.Code}", Severity.Error);
        }
        catch (AuthenticationException ex)
        {

            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.DELETE]}", Severity.Error);
        }
        catch (Exception ex)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.DELETE]}", Severity.Error);
        }


    }
    void ShowSnackbar(string message, Severity severity)

    {

        Snackbar.Clear();

        Snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomRight;

        Snackbar.Add(message, severity, c => c.SnackbarVariant = Variant.Filled);

    }
}
