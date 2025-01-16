using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Pages.LicenceTypes;
using SmartParking.Lookups.LicenceType;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using SmartParking.Lookups.VehicleModel;
using SmartParking.Vehicles.LookUps;
using Volo.Abp.Application.Dtos;
using SmartParking.Blazor.Components.Pages.VehicleBrands;
using Microsoft.Extensions.Localization;
using SmartParking.Localization;
using SmartParking.Blazor.Components.SPComponents;
using System.Security.Authentication;
using Volo.Abp;
using Volo.Abp.Validation;
using System.Linq;
using SmartParking.Blazor.Components.Shared;

namespace SmartParking.Blazor.Components.Pages.VehicleModels;

public partial class VehicleModel
{
	private List<VehicleModelDto> licenceTypes = new List<VehicleModelDto>();

	private int currentPage = 1;
	private int pageSize = 100;

	private int TotalItem;
	public int CurrentPage { get; set; } = 1; // Default current page value is 10
	public int RowPerPage { get; set; } = 10; // Default value is 10

	private TableData itemData = new TableData();

	private string termSearch;

	private readonly DialogOptions _backdropClick = new() { BackdropClick = false };
    private readonly IStringLocalizer<SmartParkingResource> _localizer;

    public VehicleModel(IStringLocalizer<SmartParkingResource> localizer)
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
		var vehicleModelsDto = await VehicleModelsAppService.Filter(input);


		// Calculate the total number of pages required to display all results
		TotalItem = (int)vehicleModelsDto.TotalCount;
		CreateTable(vehicleModelsDto);


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
		var vehicleModelsDto = await VehicleModelsAppService.Search(input);

		// Calculate the total number of pages required to display all results
		TotalItem = (int)vehicleModelsDto.TotalCount;
		CreateTable(vehicleModelsDto);
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
		var vehicleModelsDto = await VehicleModelsAppService.GetListAsync(input);
		TotalItem = (int)vehicleModelsDto.TotalCount;
		CreateTable(vehicleModelsDto);

	}

	private void CreateTable(PagedResultDto<VehicleModelDto> vehicleModels)
	{
		// Initialize table data to be displayed, setting the table's name and whether to show the header
		var tableData = new TableData()
		{
			Name = "VehicleModelس",
			ShowHeader = true
		};


		tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Id"], Name = "Id", IsFilterable = false, PropertyName = "Id", IsSortable = true, TypeFilter = TypeFilter.Text });
		tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["ModelArabicName"], Name = "ArabicName", IsFilterable = true, PropertyName = "ArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });
		tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["ModelEnglishName"], Name = "EnglishName", IsFilterable = true, PropertyName = "EnglishName", IsSortable = true, TypeFilter = TypeFilter.Text });
		tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["VehicleBrandArabicName"], Name = "VehicleBrandArabicName", IsFilterable = true, PropertyName = "VehiclesBrand.ArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });
		tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["VehicleBrandEnglishName"], Name = "VehicleBrandEnglishName", IsFilterable = true, PropertyName = "VehiclesBrand.EnglishName", IsSortable = true, TypeFilter = TypeFilter.Text });

		//tableData.Columns.Add(new ColumnDef() { DisplayName = "Notes", Name = "Notes", IsFilterable = true, PropertyNmae = "Notes", IsSortable = true, TypeFilter = TypeFilter.Text });
		//tableData.Columns.Add(new ColumnDef() { DisplayName = "Max Capacity", Name = "MaxCapacity", IsFilterable = false, IsSortable = true, TypeFilter = TypeFilter.Number });
		foreach (var item in vehicleModels.Items)
		{
			var rowDef = new RowDef();
			rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
			rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["ModelArabicName"], item.ArabicName));
			rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["ModelEnglishName"], item.EnglishName));
			rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["VehicleBrandArabicName"], item.VehiclesBrand.ArabicName));
			rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["VehicleBrandEnglishName"],item.VehiclesBrand.EnglishName));

			//rowDef.Cells.Add(new KeyValuePair<string, string>("Notes", item.Notes.ToString()));


			tableData.Rows.Add(rowDef);
		}
		itemData = tableData;
	}

	private Task CreateVehicleModel(DialogOptions options)//Add Camera
	{
		return Dialog.ShowAsync<AddVehicleModel>("Add VehicleModel", options);
	}

	private async Task<IDialogReference> OnClickRowEdit(RowDef row)
	{
		var id = int.Parse(row.Cells[0].Value);
		Console.WriteLine(id);
		var vehicleModelDto = await VehicleModelsAppService.GetAsync(id);

		var createUpdateVehicleModel = new CreateUpdateVehicleModelDto
		{
			ArabicName = vehicleModelDto.ArabicName,
			EnglishName = vehicleModelDto.EnglishName,
			VehiclesBrandId = vehicleModelDto.VehiclesBrandId,
			Image = vehicleModelDto.Image,

		};



		var parameters = new DialogParameters
		{
			["VehicleModel"] = createUpdateVehicleModel,
            ["UpdatedId"] = id
        };


		return await Dialog.ShowAsync<EditVehicleModel>("Edit VehicleModel", parameters);

	}

	private async Task<IDialogReference> OnClickRowView(RowDef row)
	{

		var id = int.Parse(row.Cells[0].Value);

		var vehicleModelDto = await VehicleModelsAppService.GetAsync(id);

		var createUpdateVehicleModel = new CreateUpdateVehicleModelDto
		{
			ArabicName = vehicleModelDto.ArabicName,
			EnglishName = vehicleModelDto.EnglishName,
			VehiclesBrandId = vehicleModelDto.VehiclesBrandId,
			Image = vehicleModelDto.Image

		};

		var parameters = new DialogParameters
		{
			["VehicleModel"] = createUpdateVehicleModel,
		};


		return await Dialog.ShowAsync<ViewVehicleModelDialog>("View VehicleModelDialog", parameters);
	}

	private async Task OnClickRowDelete(RowDef row)
	{
		

		var id = int.Parse(row.Cells[0].Value);
        var Name = row.Cells[2].Value;

        var parameters = new DialogParameters
        {
            ["Name"] = Name,

        };

        var dialog = await Dialog.ShowAsync<SPDialogDelete>("Vehicle Model Delete Dialog", parameters);
        var result = await dialog.Result;
        try
        {
            if (!result.Canceled)
            {
                await VehicleModelsAppService.DeleteAsync(id);
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]} {L[AlertMessages.AlertMessages.DELETE]} {Name} ", Severity.Success);
                await Task.Delay(1000);

                Navigation.NavigateTo("/vehiclemodels", forceLoad: true);

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
