
using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Pages.Countries;
using SmartParking.Localization;
using SmartParking.Lookups;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Volo.Abp.Application.Dtos;
using SmartParking.Lookups.PlateColor;
using SmartParking.Blazor.Components.Pages.Cameras;
using SmartParking.Cameras;

namespace SmartParking.Blazor.Components.Pages.PlateColors;

public partial class PlateColor
{
	private List<PlateColorDto> plateColors = new List<PlateColorDto>();

	private int currentPage = 1;
	private int pageSize = 100;

	private int TotalItem;
	public int CurrentPage { get; set; } = 1; // Default current page value is 10
	public int RowPerPage { get; set; } = 10; // Default value is 10

	private TableData itemData = new TableData();

	private string termSearch;

	private readonly DialogOptions _backdropClick = new() { BackdropClick = false };
	private readonly IStringLocalizer<SmartParkingResource> _localizer;

	public PlateColor(IStringLocalizer<SmartParkingResource> localizer)
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
		var plateColorsDto = await PlateColorsAppService.Filter(input);
		

		// Calculate the total number of pages required to display all results
		TotalItem = (int)plateColorsDto.TotalCount;
		CreateTable(plateColorsDto);


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
		var plateColorsDto = await PlateColorsAppService.Search(input);

		// Calculate the total number of pages required to display all results
		TotalItem = (int)plateColorsDto.TotalCount;
		CreateTable(plateColorsDto);
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
		var plateColorsDto = await PlateColorsAppService.GetListAsync(input);
		TotalItem = (int)plateColorsDto.TotalCount;
		CreateTable(plateColorsDto);

	}

	private void CreateTable(PagedResultDto<PlateColorDto> plateColors)
	{
		// Initialize table data to be displayed, setting the table's name and whether to show the header
		var tableData = new TableData()
		{
			Name = "PlateColors",
			ShowHeader = true
		};


		tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Id"], Name = "Id", IsFilterable = false, PropertyNmae = "Id", IsSortable = false, TypeFilter = TypeFilter.Text });
		tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["ArabicName"], Name = "ArabicName", IsFilterable = true, PropertyNmae = "ArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });
		tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["EnglishName"], Name = "EnglishName", IsFilterable = true, PropertyNmae = "EnglishName", IsSortable = true, TypeFilter = TypeFilter.Text });
		//tableData.Columns.Add(new ColumnDef() { DisplayName = "Notes", Name = "Notes", IsFilterable = true, PropertyNmae = "Notes", IsSortable = true, TypeFilter = TypeFilter.Text });
		//tableData.Columns.Add(new ColumnDef() { DisplayName = "Max Capacity", Name = "MaxCapacity", IsFilterable = false, IsSortable = false, TypeFilter = TypeFilter.Number });
		foreach (var item in plateColors.Items)
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

	private Task CreatePlateColor(DialogOptions options)//Add Camera
	{
		return Dialog.ShowAsync<AddPlateColor>("Add PlateColor", options);
	}

	private async Task<IDialogReference> OnClickRowEdit(RowDef row)
	{
		var id = int.Parse(row.Cells[0].Value);
        Console.WriteLine(id);
		var plateColorDto = await PlateColorsAppService.GetAsync(id);

		var createUpdatePlateColot = new CreateUpdatePlateColorDto
		{
			ArabicName=plateColorDto.ArabicName,
			EnglishName=plateColorDto.EnglishName,


		};



		var parameters = new DialogParameters
		{
			["PlateColor"] = createUpdatePlateColot,
			["UpdatedId"] = id
		};


		return await Dialog.ShowAsync<EditPlateColor>("Edit PlateColor", parameters);

	}

	private async Task<IDialogReference> OnClickRowView(RowDef row)
	{
		Console.WriteLine("View Button");

		var id = int.Parse(row.Cells[0].Value);
		Console.WriteLine(id);

		var plateColorDto = await PlateColorsAppService.GetAsync(id);

		var createUpdatePlateColor = new CreateUpdatePlateColorDto
		{
			ArabicName =plateColorDto.ArabicName,
			EnglishName=plateColorDto.EnglishName,

		};

		var parameters = new DialogParameters
		{
			["PlateColor"] = createUpdatePlateColor,
		};


		return await Dialog.ShowAsync<ViewPlateColorDialog>("View PlateColorDialog", parameters);
	}

	private async Task OnClickRowDelete(string row)
	{
		var id = int.Parse(row);
		try
		{

			await PlateColorsAppService.DeleteAsync(id);
			ShowSnackbar("Camera Deleted Successfully ", Severity.Success);

		}
		catch (Exception ex)
		{

			ShowSnackbar(ex.Message, Severity.Error);
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

		Snackbar.Configuration.PositionClass = Defaults.Classes.Position.TopRight;

		Snackbar.Add(message, severity, c => c.SnackbarVariant = Variant.Filled);

	}


}
