using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Pages.Packages;
using SmartParking.DefaultPackages;
using SmartParking.Localization;
using SmartParking.Packages;
using static MudBlazor.CategoryTypes;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Volo.Abp.Application.Dtos;
using Microsoft.AspNetCore.Components;
using SmartParking.Garages;

namespace SmartParking.Blazor.Components.Pages.Organizations
{
	public partial class PackageDialog
	{
		private List<PackageDto> packages = new List<PackageDto>();

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

	
		public PackageDialog(IStringLocalizer<SmartParkingResource> localizer)
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
			var packagesDto = await PackagesAppService.Filter(input);

			// Calculate the total number of pages required to display all results
			TotalItem = (int)packagesDto.TotalCount;
			CreateTable(packagesDto);


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
			var packagesDto = await PackagesAppService.Search(input);

			// Calculate the total number of pages required to display all results
			TotalItem = (int)packagesDto.TotalCount;
			CreateTable(packagesDto);
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
			var garagessDto = await PackagesAppService.GetPackagesByOrganization(input, organizationId);
			TotalItem = (int)garagessDto.TotalCount;
			CreateTable(garagessDto);

		}

		private void CreateTable(PagedResultDto<PackageDto> packages)
		{
			// Initialize table data to be displayed, setting the table's name and whether to show the header
			var tableData = new TableData()
			{
				Name = "Packages",
				ShowHeader = true
			};


			tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Id"], Name = "Id", IsFilterable = false, PropertyNmae = "Id", IsSortable = false, TypeFilter = TypeFilter.Text });
			tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["EnglishName"], Name = "EnglishName", IsFilterable = true, PropertyNmae = "EnglishName", IsSortable = true, TypeFilter = TypeFilter.Text });
			tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["PackageTypeEnglishName"], Name = "PackageTypeEnglishName", IsFilterable = true, PropertyNmae = "PackageTypeEnglishName", IsSortable = true, TypeFilter = TypeFilter.Text });
			tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Price"], Name = "Price", IsFilterable = true, PropertyNmae = "Price", IsSortable = true, TypeFilter = TypeFilter.Text });
			tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Description"], Name = "Description", IsFilterable = true, PropertyNmae = "Description", IsSortable = true, TypeFilter = TypeFilter.Text });
			tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["published"], Name = "published", IsFilterable = true, PropertyNmae = "published", IsSortable = true, TypeFilter = TypeFilter.Text });
			tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Status"], Name = "Status", IsFilterable = true, PropertyNmae = "Status", IsSortable = true, TypeFilter = TypeFilter.Enum });

			//tableData.Columns.Add(new ColumnDef() { DisplayName = "Max Capacity", Name = "MaxCapacity", IsFilterable = false, IsSortable = false, TypeFilter = TypeFilter.Number });
			foreach (var item in packages.Items)
			{
				var rowDef = new RowDef();
				rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
				rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["EnglishName"], item.EnglishName));
				//rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["MacAddress"], item.ArabicName));
				rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["PackageTypeEnglishName"], item.PackageType.EnglishName));
				rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Price"], item.Price.ToString()));
				rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Description"], item.Description));
				rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["published"], item.published.ToString()));
				rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Status"], item.Status.ToString()));


				tableData.Rows.Add(rowDef);
			}
			itemData = tableData;
		}

		private Task CreatePackage()//Add Package
		{
			return Dialog.ShowAsync<AddPackageToOrganizationDialog>();
		}
	}
}
