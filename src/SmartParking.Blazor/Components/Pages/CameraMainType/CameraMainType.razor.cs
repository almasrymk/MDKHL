using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Pages.CameraMainType;
using SmartParking.Blazor.Components.SPComponents;
using SmartParking.Localization;
using SmartParking.Lookups.CameraMainType;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Threading.Tasks;
using System;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using SmartParking.Lookups.CameraMainType;
using SmartParking.Lookups.CameraBrands;
using SmartParking.Blazor.Components.Shared;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.CameraMainType;
/// <summary>
/// Component for managing camera main types including listing, searching, filtering, and CRUD operations.
/// </summary>
public partial class CameraMainType
{
    private int TotalItem;

    /// <summary>
    /// The current page number in pagination.
    /// </summary>
    public int CurrentPage { get; set; } = 1;

    /// <summary>
    /// Number of rows per page to display.
    /// </summary>
    public int RowPerPage { get; set; } = 10;

    private TableData itemData = new TableData();
    private string termSearch;
    private readonly IStringLocalizer<SmartParkingResource> _localizer;

    /// <summary>
    /// Constructor to inject localization support.
    /// </summary>
    /// <param name="localizer">String localizer for SmartParking resources.</param>
    public CameraMainType(IStringLocalizer<SmartParkingResource> localizer)
    {
        _localizer = localizer;
    }

    /// <summary>
    /// Applies filters to the camera main types list and updates the displayed table.
    /// </summary>
    /// <param name="pageInfo">The current pagination, sorting, and filtering information.</param>
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

        var cameraMainTypeDto = await AppService.Filter(input);
        TotalItem = (int)cameraMainTypeDto.TotalCount;
        CreateTable(cameraMainTypeDto);
    }

    /// <summary>
    /// Handles the search functionality for camera main types based on user input.
    /// </summary>
    /// <param name="pageInfo">Pagination and search parameters.</param>
    private async Task HandleSearch(PageChangeEventArgs pageInfo)
    {
        Console.WriteLine(pageInfo.SearchTerm);
        termSearch = pageInfo.SearchTerm;

        var input = new PagedAndSortedAndSearchResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn,
            SearchingTerm = termSearch
        };

        var cameraMainTypeDto = await AppService.Search(input);
        TotalItem = (int)cameraMainTypeDto.TotalCount;
        CreateTable(cameraMainTypeDto);
    }

    /// <summary>
    /// Fetches and displays paged data for camera main types.
    /// </summary>
    /// <param name="pageInfo">Pagination and sorting data.</param>
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

        var camerMainTypeDto = await AppService.GetListAsync(input);
        TotalItem = (int)camerMainTypeDto.TotalCount;
        CreateTable(camerMainTypeDto);
    }

    /// <summary>
    /// Builds the table data structure from the result list.
    /// </summary>
    /// <param name="camerMainType">Paged result DTO containing camera main types.</param>
    private void CreateTable(PagedResultDto<CameraMainTypeDto> camerMainType)
    {
        var tableData = new TableData()
        {
            Name = L["Cameramaintypes"],
            ShowHeader = true
        };


        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Id"], Name = "Id", IsFilterable = false, PropertyName = "Id", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["ArabicName"], Name = "ArabicName", IsFilterable = true, PropertyName = "ArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["EnglishName"], Name = "EnglishName", IsFilterable = true, PropertyName = "EnglishName", IsSortable = true, TypeFilter = TypeFilter.Text });

        foreach (var item in camerMainType.Items)
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
    /// method to open add camera main type dialog
    /// </summary>
    private Task CreateCameraMainType()
    {
        return Dialog.ShowAsync<AddCameraMainTypeDialog>("Add camera main type");
    }
    /// <summary>
    /// Opens the Edit dialog for the selected camera main type row.
    /// Fetches the camera main type details using its ID and displays them in an edit dialog.
    /// </summary>
    /// <param name="row">The row representing the camera main type to edit.</param>
    private async Task<IDialogReference> OnClickRowEdit(RowDef row)
    {
        var id = int.Parse(row.Cells[0].Value);
        var cameraMainTypeDto = await AppService.GetAsync(id);

        var createUpdateCameraMainType = new CreateUpdateCameraMainTypeDto
        {
            ArabicName = cameraMainTypeDto.ArabicName,
            EnglishName = cameraMainTypeDto.EnglishName,
        };

        var parameters = new DialogParameters
        {
            ["cameraMainType"] = createUpdateCameraMainType,
            ["Id"] = id
        };

        return await Dialog.ShowAsync<UpdateCameraMainTypeDialog>("Edit Camera MainType", parameters);
    }

    /// <summary>
    /// Opens a view dialog displaying details of the selected camera main type row.
    /// </summary>
    /// <param name="row">The row representing the camera main type to view.</param>
    private async Task<IDialogReference> OnClickRowView(RowDef row)
    {
        var id = int.Parse(row.Cells[0].Value);
        var cameraMainTypeDto = await AppService.GetAsync(id);

        var parameters = new DialogParameters
        {
            ["cameraMainType"] = cameraMainTypeDto,
        };

        return await Dialog.ShowAsync<ViewCameraMainTypeDialog>("View CameraMainTypeDialog", parameters);
    }

    /// <summary>
    /// Handles deletion of the selected camera main type row after confirmation.
    /// Displays a confirmation dialog before proceeding with deletion.
    /// If deletion succeeds, reloads the page and shows a success snackbar.
    /// Handles various exceptions and shows appropriate error messages.
    /// </summary>
    /// <param name="row">The row representing the camera main type to delete.</param>
    private async Task OnClickRowDelete(RowDef row)
    {
        var id = int.Parse(row.Cells[0].Value);
        var Name = row.Cells[2].Value;

        var parameters = new DialogParameters
        {
            ["Name"] = Name,
        };

        var dialog = await Dialog.ShowAsync<SPDialogDelete>("CameraMainType Delete Dialog", parameters);
        var result = await dialog.Result;

        try
        {
            if (!result.Canceled)
            {
                await AppService.DeleteAsync(id);
                SnackBar.ShowSnackbar(Snackbar, $"{Name} {L[AlertMessages.AlertMessages.DELETE]} {L[AlertMessages.AlertMessages.SUCCESS]}", Severity.Success);
                await Task.Delay(1000);
                Navigation.NavigateTo("/Cameramaintypes", forceLoad: true);
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
   

