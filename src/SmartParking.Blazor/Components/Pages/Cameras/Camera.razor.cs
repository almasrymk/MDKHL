using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Pages.Garages;
using SmartParking.Cameras;
using SmartParking.Garages;
using SmartParking.OrganizationsGarages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using static MudBlazor.CategoryTypes;

namespace SmartParking.Blazor.Components.Pages.Cameras;

public partial class Camera
{
    private List<CameraDto> cameras=new List<CameraDto>();

    private int currentPage = 1;
    private int pageSize = 100;

    private int TotalItem;
    public int CurrentPage { get; set; } = 1; // Default current page value is 10
    public int RowPerPage { get; set; } = 10; // Default value is 10

    private TableData itemData = new TableData();

    private string termSearch;
    //private DialogOptions options = new DialogOptions { CloseOnEscapeKey = true };

    //Dialog
    private readonly DialogOptions _maxWidth = new() { MaxWidth = MaxWidth.Medium, FullWidth = true };
    private readonly DialogOptions _closeButton = new() { CloseButton = true };
    private readonly DialogOptions _noHeader = new() { NoHeader = true };
    private readonly DialogOptions _backdropClick = new() { BackdropClick = false };
    private readonly DialogOptions _fullScreen = new() { FullScreen = true, CloseButton = true };
    private readonly DialogOptions _topCenter = new() { Position = DialogPosition.TopCenter };
    private DialogOptions options = new DialogOptions { CloseOnEscapeKey = true };


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
        var camerassDto = await CamerasAppService.Filter(input);

        // Calculate the total number of pages required to display all results
        TotalItem = (int)camerassDto.TotalCount;
        CreateTable(camerassDto);


    }

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
        var camerasDto = await CamerasAppService.Search(input);

        // Calculate the total number of pages required to display all results
        TotalItem = (int)camerasDto.TotalCount;
        CreateTable(camerasDto);
    }

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
        var camerassDto = await CamerasAppService.GetListAsync(input);
        TotalItem = (int)camerassDto.TotalCount;
        CreateTable(camerassDto);

    }

    private void CreateTable(PagedResultDto<CameraDto> cameras)
    {
        // Initialize table data to be displayed, setting the table's name and whether to show the header
        var tableData = new TableData()
        {
            Name = "Cameras",
            ShowHeader = true
        };


        tableData.Columns.Add(new ColumnDef() { DisplayName = "Id", Name = "Id", IsFilterable = false, PropertyNmae = "Id", IsSortable = false, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = "MacAddress", Name = "MacAddress", IsFilterable = true, PropertyNmae = "MacAddress", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = "Type", Name = "Type", IsFilterable = true, PropertyNmae = "Type", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = "Status", Name = "Status", IsFilterable = true, PropertyNmae = "Status", IsSortable = true, TypeFilter = TypeFilter.Enum });
        //tableData.Columns.Add(new ColumnDef() { DisplayName = "Max Capacity", Name = "MaxCapacity", IsFilterable = false, IsSortable = false, TypeFilter = TypeFilter.Number });
        foreach (var item in cameras.Items)
        {
            var rowDef = new RowDef();
            rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>("MacAddress", item.MacAddress));
            rowDef.Cells.Add(new KeyValuePair<string, string>("Type", item.Type));
            rowDef.Cells.Add(new KeyValuePair<string, string>("Status", item.Status.ToString()));
            //rowDef.Cells.Add(new KeyValuePair<string, string>("Max Capacity", item.MaxCapacity.ToString()));


            tableData.Rows.Add(rowDef);
        }
        itemData = tableData;
    }

    private Task OpenDialogAsync(DialogOptions options)//Add Camera
    {
        return Dialog.ShowAsync<AddDialog>("Add Camera", options);
    }

    private async Task<IDialogReference> OnClickRowEdit(RowDef row)
    {
        var id = int.Parse(row.Cells[0].Value);

        var cameraDto = await CamerasAppService.GetAsync(id);

        var createUpdateCamera = new CreateUpdateCameraDto
        {
            Name = cameraDto.Name,
            Code = cameraDto.Code,
            Status = cameraDto.Status,
            BrandName = cameraDto.BrandName,
            CameraIP = cameraDto.CameraIP,
            GarageId = cameraDto.GarageId,
            MacAddress = cameraDto.MacAddress,
            MainType = cameraDto.MainType,
            Model = cameraDto.Model,
            TenantId = cameraDto.TenantId,
            Type=cameraDto.Type,
           

        };



        var parameters = new DialogParameters
        {
            ["Camera"] = createUpdateCamera,
            ["UpdatedId"] = id
        };


        return await Dialog.ShowAsync<EditCamera>("Edit Camera", parameters);

    }

    private async Task<IDialogReference> OnClickRowView(RowDef row)
    {
        var id = int.Parse(row.Cells[0].Value);

        var cameraDto = await CamerasAppService.GetAsync(id);

		var createUpdateCamera = new CreateUpdateCameraDto
		{
			Name = cameraDto.Name,
			Code = cameraDto.Code,
			Status = cameraDto.Status,
			BrandName = cameraDto.BrandName,
			CameraIP = cameraDto.CameraIP,
			GarageId = cameraDto.GarageId,
			MacAddress = cameraDto.MacAddress,
			MainType = cameraDto.MainType,
			Model = cameraDto.Model,
			TenantId = cameraDto.TenantId,
			Type = cameraDto.Type,


		};

		var parameters = new DialogParameters
        {
            ["Camera"] = createUpdateCamera,
			["UpdatedId"] = id

		};


        return await Dialog.ShowAsync<ViewCameraDialog>("View Camera", parameters);
    }

    private async Task OnClickRowDelete(string row)
    {
        var id = int.Parse(row);
        try
        {

            await CamerasAppService.DeleteAsync(id);
            ShowSnackbar("Camera Deleted Successfully ", Severity.Success);

        }
        catch (Exception ex)
        {

            ShowSnackbar(ex.Message, Severity.Error);
        }

    }
    void ShowSnackbar(string message, Severity severity)

    {

        Snackbar.Clear();

        Snackbar.Configuration.PositionClass = Defaults.Classes.Position.TopRight;

        Snackbar.Add(message, severity, c => c.SnackbarVariant = Variant.Filled);

    }


    //private async Task GetData()
    //{
    //    var result = await CamerasAppService.GetListAsync(new PagedAndSortedResultRequestDto
    //    {
    //        MaxResultCount = pageSize,
    //        SkipCount = (currentPage - 1) * pageSize
    //    });

    //    // If GetListAsync now returns a plain list, no need for result.Items, just assign directly
    //    cameras = result.Items.ToList(); // This will now be a List<CameraDto>
    //    CreateTable(cameras);

    //    StateHasChanged();
    //}
    //protected override async Task OnInitializedAsync()
    //{

    //    await base.OnInitializedAsync();
    //    await GetData();

    //}

    //private async Task OpenEditDialog(CameraDto camera)
    //{
    //    var parameters = new DialogParameters { ["Camera"] = camera };
    //    var dialog = Dialog.Show<EditCamera>("Edit Camera", parameters);
    //    var result = await dialog.Result;

       
    //}



}
