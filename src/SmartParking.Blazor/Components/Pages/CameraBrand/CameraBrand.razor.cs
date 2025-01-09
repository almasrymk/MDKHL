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

namespace SmartParking.Blazor.Components.Pages.CameraBrand;

partial class CameraBrand
{
    private int TotalItem;
    public int CurrentPage { get; set; } = 1; // Default current page value is 10
    public int RowPerPage { get; set; } = 10; // Default value is 10

    private TableData itemData = new TableData();

    private string termSearch;

    private DialogOptions options = new DialogOptions { CloseOnEscapeKey = true };

    private readonly IStringLocalizer<SmartParkingResource> _localizer;

    public CameraBrand(IStringLocalizer<SmartParkingResource> localizer)
    {
        _localizer = localizer;
    }

    private async Task HandleFilter(PageChangeEventArgs pageInfo)
    {

        var input = new PagedAndSortedAndFilteredResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn,
            SearchingTerm = termSearch, // The term to use for filtering results
            filters = pageInfo.Filters
        };
        // Call the service to perform the search based on the input parameters
        var brandsDto = await AppService.Filter(input);

        // Calculate the total number of pages required to display all results
        TotalItem = (int)brandsDto.TotalCount;
        CreateTable(brandsDto);


    }

    private async Task HandleSearch(PageChangeEventArgs pageInfo)
    {


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
        var brandsDto = await AppService.Search(input);

        // Calculate the total number of pages required to display all results
        TotalItem = (int)brandsDto.TotalCount;
        CreateTable(brandsDto);
    }


    private async Task FetchTableData(PageChangeEventArgs pageInfo)
    {

        CurrentPage = pageInfo.PageNumber;
        RowPerPage = pageInfo.RowsPerPage;
        // Create a request object for paged and sorted data
        var input = new PagedAndSortedResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn
        };

        // Retrieve the list of brands from the server
        var camerabrandsDto = await AppService.GetListAsync(input);
        TotalItem = (int)camerabrandsDto.TotalCount;
        CreateTable(camerabrandsDto);

    }

    private void CreateTable(PagedResultDto<CameraBrandDto> camerabrands)
    {
        // Initialize table data to be displayed, setting the table's name and whether to show the header
        var tableData = new TableData()
        {
            Name = "CameraBrands",
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

    private Task<IDialogReference> CreateCameraBrand()
    {


        return Dialog.ShowAsync<AddCameraBrandDialog>("Simple Dialog", options);


    }
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
        catch (BusinessException ex)
        {

            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.DELETE]} : {ex.Message}", Severity.Error);
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
