using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Pages.Customer;
using SmartParking.Blazor.Components.Pages.Garages;
using SmartParking.Blazor.Components.SPComponents;
using SmartParking.Cameras;
using SmartParking.Garages;
using SmartParking.Localization;
using SmartParking.Organizations;
using SmartParking.OrganizationsGarages;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Validation;
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

    private readonly DialogOptions _backdropClick = new() { BackdropClick = false };
    private readonly IStringLocalizer<SmartParkingResource> _localizer;

    public Camera(IStringLocalizer<SmartParkingResource> localizer)
    {
       _localizer = localizer;
    }

    /// <summary>
    /// filter for cameras on the server side using the camera columns.
    /// </summary>
    /// <param name="columnFilter">The name of column to filter for among cameras.</param>
    /// <returns>Returns a task that represents the asynchronous operation.</returns>
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
        if (CurrentTenant.Id != null)
        {

        var camerassDto = await CamerasAppService.Filter(input);
        // Calculate the total number of pages required to display all results
        TotalItem = (int)camerassDto.TotalCount;
        CreateTable(camerassDto);
        }
        else
        {

            var camerassDto = await CamerasAppService.FilterForMDKHL(input);
            // Calculate the total number of pages required to display all results
            TotalItem = (int)camerassDto.TotalCount;
            CreateTable(camerassDto);
        }



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
        if (CurrentTenant.Id != null)
        {

            // Call the service to perform the search based on the input parameters
            var camerasDto = await CamerasAppService.Search(input);

            // Calculate the total number of pages required to display all results
            TotalItem = (int)camerasDto.TotalCount;
            CreateTable(camerasDto);
        }
        else
        {

            var camerassDto = await CamerasAppService.SearchForMDKHL(input);
            // Calculate the total number of pages required to display all results
            TotalItem = (int)camerassDto.TotalCount;
            CreateTable(camerassDto);
        }
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


        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Id"], Name = "Id", IsFilterable = false, PropertyName = "Id", IsSortable = false, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Code"], Name = "Code", IsFilterable = false, PropertyName = "DeviceId", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["CameraName"], Name = "CameraName", IsFilterable = false, PropertyName = "Name", IsSortable = true, TypeFilter = TypeFilter.Text });
        if (CultureInfo.CurrentCulture.Name == "en")
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["GarageNameEn"], Name = "GarageNameEn", IsFilterable = false, PropertyName = "Garage.NameEn", IsSortable = true, TypeFilter = TypeFilter.Text });
        }
        else
        {

        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["GarageNameAr"], Name = "GarageNameAr", IsFilterable = false, PropertyName = "Garage.NameAr", IsSortable = true, TypeFilter = TypeFilter.Text });
        }

        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["MacAddress"], Name = "MacAddress", IsFilterable = false, PropertyName = "MacAddress", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["CameraType"], Name = "CameraType", IsFilterable = true, PropertyName = "CameraType", IsSortable = true, TypeFilter = TypeFilter.Enum });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Status"], Name = "Status", IsFilterable = true, PropertyName = "Status", IsSortable = true, TypeFilter = TypeFilter.Enum });
        //tableData.Columns.Add(new ColumnDef() { DisplayName = "Max Capacity", Name = "MaxCapacity", IsFilterable = false, IsSortable = false, TypeFilter = TypeFilter.Number });
        foreach (var item in cameras.Items)
        {
            var rowDef = new RowDef();
            rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Code"], item.DeviceId));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["CameraName"], item.Name));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["MacAddress"], item.MacAddress));
            if(item.Garage is not null)
            {
                if (CultureInfo.CurrentCulture.Name == "en")
                {
                    rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["GarageNameEn"], item.Garage.NameEn));
                }
                else
                {
                    rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["GarageNameAr"], item.Garage.NameAr));


                }

            }

            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["CameraType"], _localizer[$"{item.CameraType.ToString()}"]));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Status"], _localizer[$"{item.Status.ToString()}"]));


            tableData.Rows.Add(rowDef);
        }
        itemData = tableData;
    }

    private Task OpenDialogAsync(DialogOptions options)//Add Camera
    {
        return Dialog.ShowAsync<AddDialog>("Add Camera", options);
    }
	public async Task<IDialogReference> AssignGarage(string id)
	{
		var Gid = int.Parse(id);
		var cameraDto = await CamerasAppService.GetAsync(Gid);

		var createUpdateCamera = new CreateUpdateCameraDto
		{
			Name = cameraDto.Name,
            DeviceId = cameraDto.DeviceId,
			Status = cameraDto.Status,
			BrandName = cameraDto.BrandName,
			CameraIP = cameraDto.CameraIP,
			GarageId = cameraDto.GarageId,
			MacAddress = cameraDto.MacAddress,
			MainType = cameraDto.MainType,
			Model = cameraDto.Model,
			TenantId = cameraDto.TenantId,
			//Type = cameraDto.Type,
            CameraType=cameraDto.CameraType,

		};



		var parameters = new DialogParameters
		{
			["UpdatedId"] = Gid,
            ["Camera"] = createUpdateCamera
		};

		return await Dialog.ShowAsync<AssignGarageDialog>("Assign Garage", parameters);

	}
	private async Task<IDialogReference> OnClickRowEdit(RowDef row)
    {
        var id = int.Parse(row.Cells[0].Value);

        var cameraDto = await CamerasAppService.GetAsync(id);

        var createUpdateCamera = new CreateUpdateCameraDto
        {
            Name = cameraDto.Name,
            DeviceId = cameraDto.DeviceId,
            Status = cameraDto.Status,
            BrandName = cameraDto.BrandName,
            CameraIP = cameraDto.CameraIP,
            GarageId = cameraDto.GarageId,
            MacAddress = cameraDto.MacAddress,
            MainType = cameraDto.MainType,
            Model = cameraDto.Model,
            TenantId = cameraDto.TenantId,
            //Type=cameraDto.Type,
           CameraType = cameraDto.CameraType

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
            DeviceId = cameraDto.DeviceId,
			Status = cameraDto.Status,
			BrandName = cameraDto.BrandName,
			CameraIP = cameraDto.CameraIP,
			GarageId = cameraDto.GarageId,
			MacAddress = cameraDto.MacAddress,
			MainType = cameraDto.MainType,
			Model = cameraDto.Model,
			TenantId = cameraDto.TenantId,
			CameraType = cameraDto.CameraType,


		};

		var parameters = new DialogParameters
        {
            ["Camera"] = createUpdateCamera,
			["UpdatedId"] = id

		};


        return await Dialog.ShowAsync<ViewCameraDialog>( "",parameters);
    }

    private async Task OnClickRowDelete(RowDef row)
    {
        var id = int.Parse(row.Cells[0].Value);
        var Name = _localizer["Camera with Mac Address : "] + row.Cells[2].Value;

        var parameters = new DialogParameters
        {
            ["Name"] = Name,

        };

        var dialog = await Dialog.ShowAsync<SPDialogDelete>("DeleteCamera", parameters);
        var result = await dialog.Result;
        try
        {
            if (!result.Canceled)
            {
                await CamerasAppService.DeleteAsync(id);
                ShowSnackbar($"{L["Camera Deleted Successfully"]} ", Severity.Error);

                await Task.Delay(1000);

                Navigation.NavigateTo("/cameras", forceLoad: true);

            }


        }
        catch (BusinessException ex)
        {

            ShowSnackbar($"Failed to add organization: {ex.Code} + ", Severity.Error);
        }
        catch (AuthenticationException ex)
        {
            ShowSnackbar($"Failed to add organization: {ex.Message} + ", Severity.Error);
        }
        catch (Exception ex)
        {

            ShowSnackbar($"Failed to add organization: {ex.Message} + ", Severity.Error);
        }

    }
	/// <summary>

	/// Displays a snackbar notification with the specified message and severity.

	/// </summary>

	/// <param name="message">The message to display in the snackbar.</param>

	/// <param name="severity">The severity level of the snackbar (e.g., Success, Error).</param>

	void ShowSnackbar(string message, Severity severity)

    {

        Snackbar.Clear();
        Snackbar.Add(message, severity, c => c.SnackbarVariant = Variant.Filled );

    }


  


}
