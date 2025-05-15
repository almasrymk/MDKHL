using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.SPComponents;
using SmartParking.Localization;
using SmartParking.Lookups;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Threading.Tasks;
using System;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using SmartParking.Lookups.CameraBrands;
using SmartParking.Blazor.Components.Shared;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.CameraBrand;
/// <summary>
/// Represents a camera brand component.
/// </summary>
public partial class CameraBrand
{
    private int TotalItem;
    public int CurrentPage { get; set; } = 1;
    public int RowPerPage { get; set; } = 10;

    private TableData itemData = new TableData();

    private string termSearch;

    private DialogOptions options = new DialogOptions { CloseOnEscapeKey = true };

    private readonly IStringLocalizer<SmartParkingResource> _localizer;

    public CameraBrand(IStringLocalizer<SmartParkingResource> localizer)
    {
        _localizer = localizer;
    }

    #region Private Mehtods 
    /// <summary>
    /// Handles filtering of camera brand data based on the provided page change event arguments.
    /// </summary>
    /// <param name="pageInfo">Page change event arguments.</param>
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
        var brandsDto = await AppService.Filter(input);

        TotalItem = (int)brandsDto.TotalCount;
        CreateTable(brandsDto);
    }
    /// <summary>
    /// Handles searching of camera brand data based on the provided page change event arguments.
    /// </summary>
    /// <param name="pageInfo">Page change event arguments.</param>
    private async Task HandleSearch(PageChangeEventArgs pageInfo)
    {
        termSearch = pageInfo.SearchTerm;

        var input = new PagedAndSortedAndSearchResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn,
            SearchingTerm = termSearch
        };

        var brandsDto = await AppService.Search(input);

        TotalItem = (int)brandsDto.TotalCount;
        CreateTable(brandsDto);
    }
    /// <summary>
    /// Fetches camera brand data based on the provided page change event arguments.
    /// </summary>
    /// <param name="pageInfo">Page change event arguments.</param>
    private async Task FetchTableData(PageChangeEventArgs pageInfo)
    {

        CurrentPage = pageInfo.PageNumber;
        RowPerPage = pageInfo.RowsPerPage;
        var input = new PagedAndSortedResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn
        };

        var camerabrandsDto = await AppService.GetListAsync(input);
        TotalItem = (int)camerabrandsDto.TotalCount;
        CreateTable(camerabrandsDto);

    }

    /// <summary>
    /// Creates a table data object from the  camera brand list.
    /// </summary>
    /// <param name="camerabrands">Camera brand data.</param>
    private void CreateTable(PagedResultDto<CameraBrandDto> camerabrands)
    {
        var tableData = new TableData()
        {
            Name = L["CameraBrands"],
            ShowHeader = true
        };

        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Id"], Name = "Id", IsFilterable = false, PropertyName = "Id", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["ArabicName"], Name = "Brand Arabic Name", IsFilterable = true, PropertyName = "ArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["EnglishName"], Name = "Brand English Name", IsFilterable = true, PropertyName = "EnglishName", IsSortable = true, TypeFilter = TypeFilter.Text });


        foreach (var item in camerabrands.Items)
        {
            var rowDef = new RowDef();
            rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["ArabicName"], item.ArabicName));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["EnglishName"], item.EnglishName));

            tableData.Rows.Add(rowDef);
        }
        itemData = tableData;
    }
    /// <summary>
    /// Opens a dialog to create a new camera brand.
    /// </summary>
    /// <returns>An <see cref="IDialogReference"/> representing the dialog instance.</returns>
    private Task<IDialogReference> CreateCameraBrand()
    {
        return Dialog.ShowAsync<AddCameraBrandDialog>("Simple Dialog", options);
    }

    /// <summary>
    /// Handles the edit action when a row is clicked. Opens an edit dialog pre-filled with camera brand data.
    /// </summary>
    /// <param name="row">The selected row containing the brand ID.</param>
    /// <returns>An <see cref="IDialogReference"/> representing the dialog instance.</returns>
    private async Task<IDialogReference> OnClickRowEdit(RowDef row)
    {
        var id = int.Parse(row.Cells[0].Value);

        var brandDto = await AppService.GetAsync(id);

        var createUpdateBrandDto = new CreateUpdateCameraBrandDto
        {
            ArabicName = brandDto.ArabicName,
            EnglishName = brandDto.EnglishName
        };

        var parameters = new DialogParameters
        {
            ["CameraBrand"] = createUpdateBrandDto,
            ["Id"] = id
        };

        return await Dialog.ShowAsync<UpdateCameraBrandDialog>("Edit Brand", parameters);
    }

    /// <summary>
    /// Handles the view action when a row is clicked. Opens a read-only dialog with brand details.
    /// </summary>
    /// <param name="row">The selected row containing the brand ID.</param>
    /// <returns>An <see cref="IDialogReference"/> representing the dialog instance.</returns>
    private async Task<IDialogReference> OnClickRowView(RowDef row)
    {
        var id = int.Parse(row.Cells[0].Value);

        var brandDto = await AppService.GetAsync(id);

        var parameters = new DialogParameters
        {
            ["CameraBrand"] = brandDto,
        };

        return await Dialog.ShowAsync<ViewCameraBrandDialog>("View Brand", parameters);
    }

    /// <summary>
    /// Handles the delete action when a row is clicked. Opens a confirmation dialog and deletes the camera brand if confirmed.
    /// </summary>
    /// <param name="row">The selected row containing the brand ID and name.</param>
    private async Task OnClickRowDelete(RowDef row)
    {
        var id = int.Parse(row.Cells[0].Value);
        var Name = row.Cells[2].Value;

        var parameters = new DialogParameters
        {
            ["Name"] = Name,
        };

        var dialog = await Dialog.ShowAsync<SPDialogDelete>("DeleteBrand", parameters);
        var result = await dialog.Result;

        try
        {
            if (!result.Canceled)
            {
                await AppService.DeleteAsync(id);
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]} {L[AlertMessages.AlertMessages.DELETE]} {Name} ", Severity.Success);
                await Task.Delay(1000);
                Navigation.NavigateTo("/Camerabrands", forceLoad: true);
            }
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
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.DELETE]} : {ex.Code}", Severity.Error);
        }
        catch (AuthenticationException ex)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.DELETE]} : {ex.Message} ", Severity.Error);
        }
        catch (Exception ex)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.DELETE]} : {ex.Message} ", Severity.Error);
        }
    }
    #endregion
}



