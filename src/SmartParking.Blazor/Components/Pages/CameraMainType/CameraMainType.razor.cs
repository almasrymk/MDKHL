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

public partial class CameraMainType
{
    private int currentPage = 1;
    private int pageSize = 100;

    private int TotalItem;
    public int CurrentPage { get; set; } = 1; // Default current page value is 10
    public int RowPerPage { get; set; } = 10; // Default value is 10

    private TableData itemData = new TableData();

    private string termSearch;
    private readonly IStringLocalizer<SmartParkingResource> _localizer;

    public CameraMainType(IStringLocalizer<SmartParkingResource> localizer)
    {
        _localizer = localizer;
    }
    private async Task HandleFilter(PageChangeEventArgs pageInfo)
    {
        // Log the search term to the console for debugging purposes

        // Create a new search input object with pagination, sorting, and search term parameters
        var input = new PagedAndSortedAndFilteredResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn,
            SearchingTerm = termSearch, // The term to use for filtering results
            filters = pageInfo.Filters
        };
        // Call the service to perform the search based on the input parameters
        var cameraMainTypeDto = await AppService.Filter(input);


        // Calculate the total number of pages required to display all results
        TotalItem = (int)cameraMainTypeDto.TotalCount;
        CreateTable(cameraMainTypeDto);


    }
    /// <summary>
    /// Searches for cameras on the server side using the cameras code search term.
    /// </summary>
    /// <param name="searchTerm">The term to search for among cameras.</param>
    /// <returns>Returns a task that represents the asynchronous operation.</returns>
    private async Task HandleSearch(PageChangeEventArgs pageInfo)
    {
        // Log the search term to the console for debugging purposes
        Console.WriteLine(pageInfo.SearchTerm);

        // Update the term used for searching with the provided search term
        termSearch = pageInfo.SearchTerm;

        // Create a new search input object with pagination, sorting, and search term parameters
        var input = new PagedAndSortedAndSearchResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn,
            SearchingTerm = termSearch // The term to use for filtering results
        };

        // Call the service to perform the search based on the input parameters
        var cameraMainTypeDto = await AppService.Search(input);

        // Calculate the total number of pages required to display all results
        TotalItem = (int)cameraMainTypeDto.TotalCount;
        CreateTable(cameraMainTypeDto);
    }
    /// <summary>
    /// Fetches the table data for the specified page and updates the table with the retrieved data.
    /// </summary>
    /// <param name="page">The page number for which to fetch the data.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task FetchTableData(PageChangeEventArgs pageInfo)
    {

        CurrentPage = pageInfo.PageNumber;
        RowPerPage = pageInfo.RowsPerPage;
        Console.WriteLine(RowPerPage);
        // Create a request object for paged and sorted data
        var input = new PagedAndSortedResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn
        };

        // Retrieve the list of organizations from the server
        var camerMainTypeDto = await AppService.GetListAsync(input);
        TotalItem = (int)camerMainTypeDto.TotalCount;
        CreateTable(camerMainTypeDto);

    }

    private void CreateTable(PagedResultDto<CameraMainTypeDto> cameraCamera)
    {
        // Initialize table data to be displayed, setting the table's name and whether to show the header
        var tableData = new TableData()
        {
            Name = "Camera Main Type",
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

    private Task CreateCameraMainType()//Add Camera
    {
        return Dialog.ShowAsync<AddCameraMainTypeDialog>("Add camera main type");
    }

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

            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.DELETE]} {L[AlertMessages.AlertMessages.FAILED]}: {ex.Code}", Severity.Error);
        }
        catch (AuthenticationException ex)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.DELETE]} {L[AlertMessages.AlertMessages.FAILED]} : {ex.Message} ", Severity.Error);
        }
        catch (Exception ex)
        {

            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.DELETE]} {L[AlertMessages.AlertMessages.FAILED]}: {ex.Message} ", Severity.Error);
        }



    }
   
}
