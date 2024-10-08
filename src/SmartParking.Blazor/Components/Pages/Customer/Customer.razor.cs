using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Pages.Garages;
using SmartParking.Garages;
using SmartParking.Localization;
using SmartParking.OrganizationsGarages;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Volo.Abp.Application.Dtos;
using SmartParking.Cutomers;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace SmartParking.Blazor.Components.Pages.Customer;

public partial class Customer 
{
	private int TotalItem;
	public int CurrentPage { get; set; } = 1; // Default current page value is 10
	public int RowPerPage { get; set; } = 10; // Default value is 10

	private TableData itemData = new TableData();

	private string termSearch;
	private DialogOptions options = new DialogOptions { CloseOnEscapeKey = true };
	private readonly IStringLocalizer<SmartParkingResource> _localizer;

	public Customer(IStringLocalizer<SmartParkingResource> localizer)
	{
		_localizer = localizer;
	}

	/// <summary>
	/// filter for hotels on the server side using the hotel columns.
	/// </summary>
	/// <param name="columnFilter">The name of column to filter for among hotel.</param>
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
		var customersDto = await CustomerAppService.Filter(input);

		// Calculate the total number of pages required to display all results
		TotalItem = (int)customersDto.TotalCount;
		CreateTable(customersDto);


	}
	/// <summary>
	/// Searches for hotels on the server side using the hotel code search term.
	/// </summary>
	/// <param name="searchTerm">The term to search for among hotel.</param>
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
		var customersDto = await CustomerAppService.Search(input);

		// Calculate the total number of pages required to display all results
		TotalItem = (int)customersDto.TotalCount;
		CreateTable(customersDto);
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
		var customersDto = await CustomerAppService.GetListAsync(input);
		TotalItem = (int)customersDto.TotalCount;
		CreateTable(customersDto);

	}

    private void HandleAdd()
    {
        Navigation.NavigateTo("/customers/register", true);
    }

    private void CreateTable(PagedResultDto<CustomerDto> customers)
	{
		// Initialize table data to be displayed, setting the table's name and whether to show the header
		var tableData = new TableData()
		{
			Name = "Garages",
			ShowHeader = true
		};


		tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Id"], Name = "Id", IsFilterable = false, PropertyNmae = "Id", IsSortable = false, TypeFilter = TypeFilter.Text });
		tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["UserName"], Name = "UserName", IsFilterable = true, PropertyNmae = "UserName", IsSortable = true, TypeFilter = TypeFilter.Text });
		tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["FullName"], Name = "FullName", IsFilterable = true, PropertyNmae = "FullName", IsSortable = true, TypeFilter = TypeFilter.Text });
		tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["ArabicName"], Name = "ArabicName", IsFilterable = true, PropertyNmae = "ArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });
		tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Email"], Name = "Email", IsFilterable = true, PropertyNmae = "Email", IsSortable = true, TypeFilter = TypeFilter.Text });
		tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["NationalId"], Name = "NationalId", IsFilterable = true, PropertyNmae = "NationalId", IsSortable = true, TypeFilter = TypeFilter.Enum });
		tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["MobileNumber"], Name = "MobileNumber", IsFilterable = false, PropertyNmae = "MobileNumber", IsSortable = false, TypeFilter = TypeFilter.Number });
		foreach (var item in customers.Items)
		{
			var rowDef = new RowDef();
			rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
			rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["UserName"], item.UserName));
			rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["FullName"], item.FullName));
			rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["ArabicName"], item.ArabicName));
			rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Email"], item.Email));
			rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["NationalId"], item.NationalId));
			rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["MobileNumber"], item.MobileNumber));


			tableData.Rows.Add(rowDef);
		}
		itemData = tableData;
	}

	public async Task<IDialogReference> CreateVehicle(RowDef row)
	{
		var id = Guid.Parse(row.Cells[0].Value);

		var parameters = new DialogParameters
		{
			["CustomerId"] = id,
		};


		return await DialogService.ShowAsync<AddVehicleDialog>("Simple Dialog", parameters);


	}

	public async Task<IDialogReference> ShowVehiclesForCustomer(RowDef row)
	{
		var id = Guid.Parse(row.Cells[0].Value);

		var parameters = new DialogParameters
		{
			["CustomerId"] = id,
		};


		return await DialogService.ShowAsync<CustomerVehicles>("Simple Dialog", parameters);


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
