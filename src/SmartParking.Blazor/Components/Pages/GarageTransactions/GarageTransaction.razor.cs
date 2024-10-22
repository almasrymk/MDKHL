using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Pages.PlateColors;
using SmartParking.Lookups.PlateColor;
using SmartParking.Vehicles.LookUps;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using SmartParking.GarageTransactions;
using Volo.Abp.Application.Dtos;
using Microsoft.Extensions.Localization;
using SmartParking.Localization;

namespace SmartParking.Blazor.Components.Pages.GarageTransactions
{
	public partial class GarageTransaction
	{
		private List<GarageTransactionDto> garageTransactions = new List<GarageTransactionDto>();

		private int currentPage = 1;
		private int pageSize = 100;

		private int TotalItem;
		public int CurrentPage { get; set; } = 1; // Default current page value is 10
		public int RowPerPage { get; set; } = 10; // Default value is 10

		private TableData itemData = new TableData();

		private string termSearch;

		private readonly DialogOptions _backdropClick = new() { BackdropClick = false };
        private readonly IStringLocalizer<SmartParkingResource> _localizer;

        public GarageTransaction(IStringLocalizer<SmartParkingResource> localizer)
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
			var garageTransactionDto = await GarageTransactionsAppService.Filter(input);


			// Calculate the total number of pages required to display all results
			TotalItem = (int)garageTransactionDto.TotalCount;
			CreateTable(garageTransactionDto);


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
			var garageTransactionsDto = await GarageTransactionsAppService.Search(input);

			// Calculate the total number of pages required to display all results
			TotalItem = (int)garageTransactionsDto.TotalCount;
			CreateTable(garageTransactionsDto);
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
			var garageTransactionsDto = await GarageTransactionsAppService.GetListAsync(input);
			TotalItem = (int)garageTransactionsDto.TotalCount;
			CreateTable(garageTransactionsDto);

		}

		private void CreateTable(PagedResultDto<GarageTransactionDto> garageTransactions)
		{
			// Initialize table data to be displayed, setting the table's name and whether to show the header
			var tableData = new TableData()
			{
				Name = "GarageTransactions",
				ShowHeader = true
			};


			tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Id"], Name = "Id", IsFilterable = false, PropertyNmae = "Id", IsSortable = false, TypeFilter = TypeFilter.Text });
			tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["PlateNumber"], Name = "PlateNumber", IsFilterable = true, PropertyNmae = "PlateNumber", IsSortable = true, TypeFilter = TypeFilter.Text });
			tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["AccurateTime"], Name = "AccurateTime", IsFilterable = true, PropertyNmae = "AccurateTime", IsSortable = true, TypeFilter = TypeFilter.Text });
			tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["SnapTime"], Name = "SnapTime", IsFilterable = true, PropertyNmae = "SnapTime", IsSortable = true, TypeFilter = TypeFilter.Text });
            tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Type"], Name = "Type", IsFilterable = true, PropertyNmae = "Type", IsSortable = true, TypeFilter = TypeFilter.Enum });

			foreach (var item in garageTransactions.Items)
			{
				var rowDef = new RowDef();
				rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
				rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["PlateNumber"], item.PlateNumber));
				rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["AccurateTime"], item.AccurateTime.ToString()));
				rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["SnapTime"], item.SnapTime.ToString()));
                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Type"], item.CameraType.ToString()));

                //rowDef.Cells.Add(new KeyValuePair<string, string>("Notes", item.Notes.ToString()));


                tableData.Rows.Add(rowDef);
			}
			itemData = tableData;
		}

		private Task CreateGarageTransaction(DialogOptions options)//Add 
		{
            ShowSnackbar("Not Working", Severity.Warning);

            return Task.CompletedTask;

            //return Dialog.ShowAsync<GarageTransaction>("Add GarageTransaction", options);
		}

		private async Task<IDialogReference> OnClickRowEdit(RowDef row)
		{
			ShowSnackbar("Not Working",Severity.Warning);
			return null;
			var id = int.Parse(row.Cells[0].Value);
			Console.WriteLine(id);
			var garageTransactionDto = await GarageTransactionsAppService.GetAsync(id);

			var createUpdateGarageTransaction = new CreateUpdateGarageTransactionDto
			{
				

			};



			var parameters = new DialogParameters
			{
				["GarageTransaction"] = garageTransactionDto,
				["UpdatedId"] = id
			};


			return await Dialog.ShowAsync<EditPlateColor>("Edit GarageTransaction", parameters);

		}

		private async Task<IDialogReference> OnClickRowView(RowDef row)
		{
           
            Console.WriteLine("View Button");

			var id = int.Parse(row.Cells[0].Value);
			Console.WriteLine(id);

			var garageTransactionDto = await GarageTransactionsAppService.GetAsync(id);

			var createUpdateGarageTransaction = new CreateUpdateGarageTransactionDto
            {
				AccurateTime=garageTransactionDto.AccurateTime,
				SnapTime=garageTransactionDto.SnapTime,
				PlateNumber=garageTransactionDto.PlateNumber,
				VehiclePicture=garageTransactionDto.VehiclePicture,
				//Type=garageTransactionDto.Type,
				CameraType=garageTransactionDto.CameraType,
				CameraId=garageTransactionDto.CameraId,
				GarageId=garageTransactionDto.GarageId,
				OrganizationID=garageTransactionDto.OrganizationID,
			};

			var parameters = new DialogParameters
			{
				["GarageTransaction"] = createUpdateGarageTransaction,
			};


			return await Dialog.ShowAsync<ViewGarageTransaction>("View GarageTransaction", parameters);
		}

		private async Task<Task> OnClickRowDelete(string row)
		{
			ShowSnackbar("Service Not Allowed", Severity.Warning);
			return Task.CompletedTask;
			var id = int.Parse(row);
			try
			{

				await GarageTransactionsAppService.DeleteAsync(id);
				ShowSnackbar("GarageTransaction Deleted Successfully ", Severity.Success);

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
}
