using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.SPComponents;
using SmartParking.Localization;
using SmartParking.Lookups.CameraModel;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Threading.Tasks;
using System;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using SmartParking.Lookups.CameraBrands;
using SmartParking.Blazor.Components.Shared;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.CameraModel;
/// <summary>
/// Represents a camera model component.
/// </summary>
public partial class CameraModel
{
    private int TotalItem;
    public int CurrentPage { get; set; } = 1; 
    public int RowPerPage { get; set; } = 10; 

    private TableData itemData = new TableData();
    private string termSearch;
    private readonly IStringLocalizer<SmartParkingResource> _localizer;

    public CameraModel(IStringLocalizer<SmartParkingResource> localizer)
    {
        _localizer = localizer;
    }
    /// <summary>
    /// Handles filtering of the camera models table based on advanced filter criteria.
    /// </summary>
    /// <param name="pageInfo">The pagination, sorting, and filtering data.</param>
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
        var cameraCameraDto = await AppService.Filter(input);
        TotalItem = (int)cameraCameraDto.TotalCount;
        CreateTable(cameraCameraDto);
    }

    /// <summary>
    /// Searches for camera models on the server using a search term.
    /// </summary>
    /// <param name="pageInfo">Contains the search term and pagination details.</param>
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
        var cameraCameraDto = await AppService.Search(input);
        TotalItem = (int)cameraCameraDto.TotalCount;
        CreateTable(cameraCameraDto);
    }

    /// <summary>
    /// Fetches paginated camera model data from the server without filters or search terms.
    /// </summary>
    /// <param name="pageInfo">Pagination and sorting details.</param>
    private async Task FetchTableData(PageChangeEventArgs pageInfo)
    {
        CurrentPage = pageInfo.PageNumber;
        RowPerPage = pageInfo.RowsPerPage;
        Console.WriteLine(RowPerPage);

        var input = new PagedAndSortedResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn
        };
        var cameraCameraDto = await AppService.GetListAsync(input);
        TotalItem = (int)cameraCameraDto.TotalCount;
        CreateTable(cameraCameraDto);
    }

    /// <summary>
    /// Builds the table structure with camera model data.
    /// </summary>
    /// <param name="cameraCamera">Paged data of camera models.</param>
    private void CreateTable(PagedResultDto<CameraModelDto> cameraCamera)
    {
        var tableData = new TableData()
        {
            Name = L["Cameramodels"],
            ShowHeader = true
        };
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Id"], Name = "Id", IsFilterable = false, PropertyName = "Id", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["ArabicName"], Name = "ArabicName", IsFilterable = true, PropertyName = "ArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["EnglishName"], Name = "EnglishName", IsFilterable = true, PropertyName = "EnglishName", IsSortable = true, TypeFilter = TypeFilter.Text });

        foreach (var item in cameraCamera.Items)
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
    /// Opens the dialog to create a new camera model.
    /// </summary>
    private Task CreateCameraModel()
    {
        return Dialog.ShowAsync<AddCameraModelDialog>("Add camera model");
    }

    /// <summary>
    /// Opens the dialog to edit a selected camera model.
    /// </summary>
    /// <param name="row">The row that was clicked for editing.</param>
    private async Task<IDialogReference> OnClickRowEdit(RowDef row)
    {
        var id = int.Parse(row.Cells[0].Value);
        var cameraModelDto = await AppService.GetAsync(id);
        var createUpdateCameraModel = new CreateUpdateCameraModelDto
        {
            ArabicName = cameraModelDto.ArabicName,
            EnglishName = cameraModelDto.EnglishName,
        };
        var parameters = new DialogParameters
        {
            ["cameraModel"] = createUpdateCameraModel,
            ["Id"] = id
        };
        return await Dialog.ShowAsync<UpdateCameraModelDialog>("Edit Camera Model", parameters);
    }

    /// <summary>
    /// Opens the dialog to view the details of a selected camera model.
    /// </summary>
    /// <param name="row">The row that was clicked for viewing.</param>
    private async Task<IDialogReference> OnClickRowView(RowDef row)
    {
        var id = int.Parse(row.Cells[0].Value);
        var cameraModelDto = await AppService.GetAsync(id);
        var parameters = new DialogParameters
        {
            ["cameraModel"] = cameraModelDto,
        };
        return await Dialog.ShowAsync<ViewCameraModelDialog>("View CameraModelDialog", parameters);
    }

    /// <summary>
    /// Handles the deletion of a camera model after confirmation.
    /// </summary>
    /// <param name="row">The row representing the camera model to delete.</param>
    private async Task OnClickRowDelete(RowDef row)
    {
        var id = int.Parse(row.Cells[0].Value);
        var Name = row.Cells[2].Value;

        var parameters = new DialogParameters
        {
            ["Name"] = Name,
        };

        var dialog = await Dialog.ShowAsync<SPDialogDelete>("CameraModel Delete Dialog", parameters);
        var result = await dialog.Result;

        try
        {
            if (!result.Canceled)
            {
                await AppService.DeleteAsync(id);
                SnackBar.ShowSnackbar(Snackbar, $"{Name} {L[AlertMessages.AlertMessages.DELETE]} {L[AlertMessages.AlertMessages.SUCCESS]}", Severity.Success);
                await Task.Delay(1000);
                Navigation.NavigateTo("/CameraModels", forceLoad: true);
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

}
