using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Pages.PlateColors;
using SmartParking.Lookups.PlateColor;
using SmartParking.Vehicles.LookUps;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using SmartParking.Lookups.LicenceType;
using Volo.Abp.Application.Dtos;
using Microsoft.Extensions.Localization;
using SmartParking.Localization;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SmartParking.Blazor.Components.SPComponents;
using SmartParking.Garages;
using System.Security.Authentication;
using Volo.Abp;
using Volo.Abp.Validation;
using System.Linq;

namespace SmartParking.Blazor.Components.Pages.LicenceTypes
{
    public partial class LicenceType
    {
		private List<LicenceTypeDto> licenceTypes = new List<LicenceTypeDto>();

		private int currentPage = 1;
		private int pageSize = 100;

		private int TotalItem;
		public int CurrentPage { get; set; } = 1; // Default current page value is 10
		public int RowPerPage { get; set; } = 10; // Default value is 10

		private TableData itemData = new TableData();

		private string termSearch;

		private readonly DialogOptions _backdropClick = new() { BackdropClick = false };
        private readonly IStringLocalizer<SmartParkingResource> _localizer;

        public LicenceType(IStringLocalizer<SmartParkingResource> localizer)
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
			var licenceTypesDto = await LicenceTypesAppService.Filter(input);


			// Calculate the total number of pages required to display all results
			TotalItem = (int)licenceTypesDto.TotalCount;
			CreateTable(licenceTypesDto);


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
			var licenceTypesDto = await LicenceTypesAppService.Search(input);

			// Calculate the total number of pages required to display all results
			TotalItem = (int)licenceTypesDto.TotalCount;
			CreateTable(licenceTypesDto);
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
			var licenceTypesDto = await LicenceTypesAppService.GetListAsync(input);
			TotalItem = (int)licenceTypesDto.TotalCount;
			CreateTable(licenceTypesDto);

		}

		private void CreateTable(PagedResultDto<LicenceTypeDto> licenceTypes)
		{
			// Initialize table data to be displayed, setting the table's name and whether to show the header
			var tableData = new TableData()
			{
				Name = "LicenceType",
				ShowHeader = true
			};


			tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Id"], Name = "Id", IsFilterable = false, PropertyNmae = "Id", IsSortable = false, TypeFilter = TypeFilter.Text });
			tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["ArabicName"], Name = "ArabicName", IsFilterable = true, PropertyNmae = "ArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });
			tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["EnglishName"], Name = "EnglishName", IsFilterable = true, PropertyNmae = "EnglishName", IsSortable = true, TypeFilter = TypeFilter.Text });
			tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["PlateColorArabicName"], Name = "PlateColorArabicName", IsFilterable = true, PropertyNmae = "PlateColorArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });
			tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["PlateColorEnglishName"], Name = "PlateColorEnglishName", IsFilterable = true, PropertyNmae = "PlateColorEnglishName", IsSortable = true, TypeFilter = TypeFilter.Text });

			//tableData.Columns.Add(new ColumnDef() { DisplayName = "Notes", Name = "Notes", IsFilterable = true, PropertyNmae = "Notes", IsSortable = true, TypeFilter = TypeFilter.Text });
			//tableData.Columns.Add(new ColumnDef() { DisplayName = "Max Capacity", Name = "MaxCapacity", IsFilterable = false, IsSortable = false, TypeFilter = TypeFilter.Number });
			foreach (var item in licenceTypes.Items)
			{
				var rowDef = new RowDef();
				rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
				rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["ArabicName"], item.ArabicName));
				rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["EnglishName"], item.EnglishName));
				rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["PlateColorArabicName"], item.PlateColorArabicName));
				rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["PlateColorEnglishName"], item.PlateColorEnglishName));

				//rowDef.Cells.Add(new KeyValuePair<string, string>("Notes", item.Notes.ToString()));


				tableData.Rows.Add(rowDef);
			}
			itemData = tableData;
		}

		private Task CreateLicenceType(DialogOptions options)//Add Camera
		{
			return Dialog.ShowAsync<AddLicenceType>("Add LicenceType", options);
		}

		private async Task<IDialogReference> OnClickRowEdit(RowDef row)
		{
			var id = int.Parse(row.Cells[0].Value);
			Console.WriteLine(id);
			var licenceTypeDto = await LicenceTypesAppService.GetAsync(id);

			var createUpdateLicenceType = new CreateUpdateLicenceTypeDto
			{
				ArabicName = licenceTypeDto.ArabicName,
				EnglishName = licenceTypeDto.EnglishName,
				PlateColorId=licenceTypeDto.PlateColorId

			};



			var parameters = new DialogParameters
			{
				["LicenceType"] = createUpdateLicenceType,
				["UpdatedId"] = id
			};


			return await Dialog.ShowAsync<EditLicenceType>("Edit LicenceType", parameters);

		}

		private async Task<IDialogReference> OnClickRowView(RowDef row)
		{

			var id = int.Parse(row.Cells[0].Value);

			var licenceTypeDto = await LicenceTypesAppService.GetAsync(id);

			var createUpdateLicenceType = new CreateUpdateLicenceTypeDto
			{
				ArabicName = licenceTypeDto.ArabicName,
				EnglishName = licenceTypeDto.EnglishName,
				PlateColorId = licenceTypeDto.PlateColorId,

			};

			var parameters = new DialogParameters
			{
				["LicenceType"] = createUpdateLicenceType,
			};


			return await Dialog.ShowAsync<ViewLicenceTypeDialog>("View LicenceTypeDialog", parameters);
		}

		private async Task OnClickRowDelete(RowDef row)
		{
			var successMessage = L["LicenceTypeDeletedSuccessfully"];
			var validationErrorMessage = L["PleaseFixTheValidationErrors"];
			var id = int.Parse(row.Cells[0].Value);
            var Name = row.Cells[2].Value;

            var parameters = new DialogParameters
            {
                ["Name"] = Name,

            };

            var dialog = await Dialog.ShowAsync<SPDialogDelete>("LicenceTypeGarage", parameters);
            var result = await dialog.Result;
            try
            {
				

				if (!result.Canceled)
                {
                    await LicenceTypesAppService.DeleteAsync(id);
                    ShowSnackbar(successMessage, Severity.Error);
                    await Task.Delay(1000);

                    Navigation.NavigateTo("/licencetypes", forceLoad: true);

                }


            }
            catch (AbpValidationException validationException)
            {
                foreach (var validationError in validationException.ValidationErrors)
                {
                    ShowSnackbar($"{validationErrorMessage} {validationError.MemberNames.First()} - {validationError.ErrorMessage}", Severity.Error);
                }
            }
            catch (BusinessException ex)
            {

                ShowSnackbar($"Failed to delete LicenceType : {ex.Code} + ", Severity.Error);
            }
            catch (AuthenticationException ex)
            {
                ShowSnackbar($"Failed to delete LicenceType : {ex.Message} + ", Severity.Error);
            }
            catch (Exception ex)
            {

                ShowSnackbar($"Failed to delete LicenceType : {ex.Message} + ", Severity.Error);
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

			Snackbar.Add(message, severity, c => c.SnackbarVariant = Variant.Filled);

		}

	}
}
