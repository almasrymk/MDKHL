using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Localization;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Volo.Abp.Application.Dtos;
using Microsoft.AspNetCore.Components;
using SmartParking.Cutomers;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Blazor.Components.Pages.Organizations;

public partial class CustomerDialog
{
	private List<CustomerDto> Customers = new List<CustomerDto>();

	private int currentPage = 1;
	private int pageSize = 100;

	private int TotalItem;
	public int CurrentPage { get; set; } = 1; // Default current page value is 10
	public int RowPerPage { get; set; } = 10; // Default value is 10

	private TableData itemData = new TableData();

	private string termSearch;

	private readonly DialogOptions _backdropClick = new() { BackdropClick = false };
	private readonly IStringLocalizer<SmartParkingResource> _localizer;
	[CascadingParameter]
	private MudDialogInstance MudDialog { get; set; }
	[Parameter] public int organizationId { get; set; }
	private void Cancel() => MudDialog.Cancel();


	public CustomerDialog(IStringLocalizer<SmartParkingResource> localizer)
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
		if (currentTenant.Id == null)
		{
			// Call the service to perform the search based on the input parameters
			var CustomersDto = await CustomersAppService.FilterMDKHL(input);

			// Calculate the total number of pages required to display all results
			TotalItem = (int)CustomersDto.TotalCount;
			CreateTable(CustomersDto);
		}
		else
		{
            // Call the service to perform the search based on the input parameters
            var CustomersDto = await CustomersAppService.Filter(input);

            // Calculate the total number of pages required to display all results
            TotalItem = (int)CustomersDto.TotalCount;
            CreateTable(CustomersDto);
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
		if (currentTenant.Id == null)
		{
			// Call the service to perform the search based on the input parameters
			var CustomersDto = await CustomersAppService.SearchMDKHL(input);
            // Calculate the total number of pages required to display all results
            TotalItem = (int)CustomersDto.TotalCount;
            CreateTable(CustomersDto);
        }
		else
		{
            // Call the service to perform the search based on the input parameters
            var CustomersDto = await CustomersAppService.Search(input);
            // Calculate the total number of pages required to display all results
            TotalItem = (int)CustomersDto.TotalCount;
            CreateTable(CustomersDto);
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
		var customersDto = await CustomersAppService.GetCustomerByOrganization(input, organizationId);
		TotalItem = (int)customersDto.TotalCount;
		CreateTable(customersDto);

	}

	private void CreateTable(PagedResultDto<CustomerDto> Customers)
	{
		// Initialize table data to be displayed, setting the table's name and whether to show the header
		var tableData = new TableData()
		{
			Name = "Customers",
			ShowHeader = true
		};


		tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Id"], Name = "Id", IsFilterable = false, PropertyName = "Id", IsSortable = false, TypeFilter = TypeFilter.Text });
		tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["EnglishName"], Name = "EnglishName", IsFilterable = true, PropertyName = "IdentityUser.UserName", IsSortable = true, TypeFilter = TypeFilter.Text });
		tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["ArabicName"], Name = "ArabicName", IsFilterable = true, PropertyName = "ArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });
		tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Email"], Name = "Email", IsFilterable = true, PropertyName = "IdentityUser.Email", IsSortable = true, TypeFilter = TypeFilter.Text });
		tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["MobileNumber"], Name = "MobileNumber", IsFilterable = true, PropertyName = "IdentityUser.PhoneNumber", IsSortable = true, TypeFilter = TypeFilter.Text });

		foreach (var item in Customers .Items)
		{
			var rowDef = new RowDef();
			rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
			rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["EnglishName"], item.FullName));
			rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["ArabicName"], item.ArabicName));
			rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Email"], item.Email));
			rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["MobileNumber"], item.MobileNumber));

			tableData.Rows.Add(rowDef);
		}
		itemData = tableData;
	}

}
