using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Pages.PlateColors;
using SmartParking.Lookups.PlateColor;
using SmartParking.Vehicles.LookUps;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using SmartParking.Lookups.VehicleBrand;
using Volo.Abp.Application.Dtos;
using Microsoft.Extensions.Localization;
using SmartParking.Localization;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SmartParking.Blazor.Components.SPComponents;
using System.Security.Authentication;
using Volo.Abp;
using Volo.Abp.Validation;
using System.Linq;
using SmartParking.Lookups;
using SmartParking.Blazor.Components.Shared;

namespace SmartParking.Blazor.Components.Pages.VehicleBrands;

public partial class VehicleBrand
{
	private List<VehicleBrandDto> plateColors = new List<VehicleBrandDto>();

	private int currentPage = 1;
	private int pageSize = 100;

	private int TotalItem;
	public int CurrentPage { get; set; } = 1; // Default current page value is 10
	public int RowPerPage { get; set; } = 10; // Default value is 10

	private TableData itemData = new TableData();

	private string termSearch;

	private readonly DialogOptions _backdropClick = new() { BackdropClick = false };
    private readonly IStringLocalizer<SmartParkingResource> _localizer;

    public VehicleBrand(IStringLocalizer<SmartParkingResource> localizer)
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
		var vehicleBrandsDto = await VehicleBrandsAppService.Filter(input);


		// Calculate the total number of pages required to display all results
		TotalItem = (int)vehicleBrandsDto.TotalCount;
		CreateTable(vehicleBrandsDto);


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
		var vehicleBrandsDto = await VehicleBrandsAppService.Search(input);

		// Calculate the total number of pages required to display all results
		TotalItem = (int)vehicleBrandsDto.TotalCount;
		CreateTable(vehicleBrandsDto);
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
		var vehicleBrandsDto = await VehicleBrandsAppService.GetListAsync(input);
		TotalItem = (int)vehicleBrandsDto.TotalCount;
		CreateTable(vehicleBrandsDto);

	}

	private void CreateTable(PagedResultDto<VehicleBrandDto> vehicleBrands)
	{
		// Initialize table data to be displayed, setting the table's name and whether to show the header
		var tableData = new TableData()
		{
			Name = "VehicleBrands",
			ShowHeader = true
		};


		tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Id"], Name = "Id", IsFilterable = false, PropertyName = "Id", IsSortable = true, TypeFilter = TypeFilter.Text });
		tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["ArabicName"], Name = "ArabicName", IsFilterable = true, PropertyName = "ArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });
		tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["EnglishName"], Name = "EnglishName", IsFilterable = true, PropertyName = "EnglishName", IsSortable = true, TypeFilter = TypeFilter.Text });
		
		foreach (var item in vehicleBrands.Items)
		{
			var rowDef = new RowDef();
			rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
			rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["ArabicName"], item.ArabicName));
			rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["EnglishName"], item.EnglishName));
			//rowDef.Cells.Add(new KeyValuePair<string, string>("Notes", item.Notes.ToString()));


			tableData.Rows.Add(rowDef);
		}
		itemData = tableData;
	}

	private Task CreateVehicleBrand(DialogOptions options)//Add Camera
	{
		return Dialog.ShowAsync<AddVehicleBrand>("Add VehicleBrand", options);
	}

	private async Task<IDialogReference> OnClickRowEdit(RowDef row)
	{
		var id = int.Parse(row.Cells[0].Value);
		Console.WriteLine(id);
		var vehicleBrandDto = await VehicleBrandsAppService.GetAsync(id);

		var createUpdateVehicleBrand = new CreateUpdateVehicleBrandDto
		{
			ArabicName = vehicleBrandDto.ArabicName,
			EnglishName = vehicleBrandDto.EnglishName,


		};



		var parameters = new DialogParameters
		{
			["VehicleBrand"] = createUpdateVehicleBrand,
			["UpdatedId"] = id
		};


		return await Dialog.ShowAsync<EditVehicleBrand>("Edit VehicleBrand", parameters);

	}

	private async Task<IDialogReference> OnClickRowView(RowDef row)
	{

		var id = int.Parse(row.Cells[0].Value);

		var vehicleBrandDto = await VehicleBrandsAppService.GetAsync(id);

		var createUpdateVehicleBrand = new CreateUpdateVehicleBrandDto
		{
			ArabicName = vehicleBrandDto.ArabicName,
			EnglishName = vehicleBrandDto.EnglishName,

		};

		var parameters = new DialogParameters
		{
			["VehicleBrand"] = createUpdateVehicleBrand,
		};


		return await Dialog.ShowAsync<ViewVehicleBrandDialog>("View VehicleBrandDialog", parameters);
	}

	private async Task OnClickRowDelete(RowDef row)
	{
		
		var id = int.Parse(row.Cells[0].Value);
        var Name = row.Cells[2].Value;

        var parameters = new DialogParameters
        {
            ["Name"] = Name,

        };

        var dialog = await Dialog.ShowAsync<SPDialogDelete>("VehicleBrand Delete Dialog", parameters);
        var result = await dialog.Result;
        try
        {
            if (!result.Canceled)
            {
                await VehicleBrandsAppService.DeleteAsync(id);
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]} {L[AlertMessages.AlertMessages.DELETE]} {Name}", Severity.Success);
                await Task.Delay(1000);

                Navigation.NavigateTo("/vehiclebrands", forceLoad: true);

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
