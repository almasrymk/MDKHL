
using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Blazor.Components.SPComponents;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using SmartParking.Garages;
using SmartParking.Localization;
using SmartParking.OrganizationsGarages;
using SmartParking.Permissions.CameraPermissions;
using SmartParking.Permissions.GaragePermissions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Authentication;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;


namespace SmartParking.Blazor.Components.Pages.Garages;

public partial class Garage
{

    private int TotalItem;
    public int CurrentPage { get; set; } = 1; // Default current page value is 10
    public int RowPerPage { get; set; } = 10; // Default value is 10


    private TableData itemData = new TableData();
    public bool ViewAssign { get; set; } = true;

    private string termSearch;
	private DialogOptions options = new DialogOptions { CloseOnEscapeKey = true };
    private readonly IStringLocalizer<SmartParkingResource> _localizer;

    public  int MaxiumumNumberOfGarageAllowedToBeAdded { get; set; }
    public  int SumOfAllMaximumCapacityforActiveGarages { get; set; }

    public int ActiveGarages { get; set; }

    private bool showOrganizationView =>currentTenant.Id != null;


    public Garage(IStringLocalizer<SmartParkingResource> localizer)
    {
        _localizer = localizer;
        CreatePolicyName = GaragePermissions.Create;
        UpdatePolicyName = GaragePermissions.Edit;
        DeletePolicyName = GaragePermissions.Delete;
    }





    protected async override Task OnInitializedAsync()
    {
        if (currentTenant.Id != null)
        {
            ViewAssign = false;
            MaxiumumNumberOfGarageAllowedToBeAdded = await OrganizationAppService.GetNumberOfGaragesAllowedToBeAdded();
            ActiveGarages = await OrganizationAppService.GetNumberOfActiveGaragesOfOrganization();

        }
        else if (currentTenant.Id == null)
		{
            var result =await GarageAppService.GetMaxCapacityofActiveGaragesAndItsCount();
            ActiveGarages = result.Item2;
            SumOfAllMaximumCapacityforActiveGarages = result.Item1;
		}
		await base.OnInitializedAsync();

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
        if (currentTenant.Id == null)
        {
            // Call the service to perform the search based on the input parameters
            var garagessDto = await GarageAppService.FilterForMDKHL(input);

            // Calculate the total number of pages required to display all results
            TotalItem = (int)garagessDto.TotalCount;
            CreateTable(garagessDto);
        }
        else
        {

            // Call the service to perform the search based on the input parameters
            var garagessDto = await GarageAppService.Filter(input);

            // Calculate the total number of pages required to display all results
            TotalItem = (int)garagessDto.TotalCount;
            CreateTable(garagessDto);

        }

    }
    /// <summary>
    /// Searches for hotels on the server side using the hotel code search term.
    /// </summary>
    /// <param name="searchTerm">The term to search for among hotel.</param>
    /// <returns>Returns a task that represents the asynchronous operation.</returns>
    private async Task HandleSearch(PageChangeEventArgs pageInfo)
    {

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
        if (currentTenant.Id == null)
        {
            var garagessDto = await GarageAppService.SearchForMDKHL(input);

            // Calculate the total number of pages required to display all results
            TotalItem = (int)garagessDto.TotalCount;
            CreateTable(garagessDto);
        }
        else
        {

            var garagessDto = await GarageAppService.Search(input);

            // Calculate the total number of pages required to display all results
            TotalItem = (int)garagessDto.TotalCount;
            CreateTable(garagessDto);

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
        // Create a request object for paged and sorted data
        var input = new PagedAndSortedResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn
        };
      
        var garagessDto = await GarageAppService.GetListAsync(input);
        TotalItem = (int)garagessDto.TotalCount;
        CreateTable(garagessDto);


    }

    private void CreateTable(PagedResultDto<GarageDto> garages)
    {
        // Initialize table data to be displayed, setting the table's name and whether to show the header
        var tableData = new TableData()
        {
            Name = "Garages",
            ShowHeader = true
        };


        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Id"], Name = "Id", IsFilterable = false, PropertyName = "Id", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Code"], Name = "Code", IsFilterable = true, PropertyName = "Code", IsSortable = true, TypeFilter = TypeFilter.Text });
        if (CultureInfo.CurrentCulture.Name == "en")
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["EnglishName"], Name = "EnglishName", IsFilterable = true, PropertyName = "NameEn", IsSortable = true, TypeFilter = TypeFilter.Text });
            tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["OrganizationEnglishName"], Name = "OrganizationEnglishName", IsFilterable = false, PropertyName = "Organization.EnglishName", IsSortable = true, TypeFilter = TypeFilter.Text });
        }
        else
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["ArabicName"], Name = "ArabicName", IsFilterable = true, PropertyName = "NameAr", IsSortable = true, TypeFilter = TypeFilter.Text });
            tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["OrganizationArabicName"], Name = "OrganizationArabicName", IsFilterable = true, PropertyName = "Organization.ArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });


        }
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Garage Max Capacity"], Name = "Garage Max Capacity", PropertyName= "MaxCapacity", IsFilterable = true, IsSortable = true, TypeFilter = TypeFilter.Number });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Status"], Name = "Status", IsFilterable = false, PropertyName = "Status", IsSortable = true, TypeFilter = TypeFilter.Enum });
        foreach (var item in garages.Items)
        {
            var rowDef = new RowDef();
            rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Code"], item.Code));
            if (CultureInfo.CurrentCulture.Name == "en")
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["EnglishName"], item.NameEn));
                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["OrganizationEnglishName"], item.OrganizationNameEn));

            }
            else
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["ArabicName"], item.NameAr));
                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["OrganizationArabicName"], item.OrganizationNameAr));

            }
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Garage Max Capacity"], item.MaxCapacity.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Status"], _localizer[item.Status.ToString()]));


            tableData.Rows.Add(rowDef);
        }
        itemData = tableData;
    }

    private Task CreateGarage()
    {
        return DialogService.ShowAsync<AddGarageDialog>("Simple Dialog", options);

    }
	public async Task<IDialogReference> AssignOrganization(string id)
	{
		var Gid = int.Parse(id);
		var garageDto = await GarageAppService.GetAsync(Gid);

		var createUpdateGarage = new CreateUpdateGarageDto
		{
	        ClosingHours = garageDto.ClosingHours,
            Code = garageDto.Code,
            MaxCapacity = garageDto.MaxCapacity,
            NameEn = garageDto.NameEn,
            NameAr = garageDto.NameAr,
            OrganizationId = garageDto.OrganizationId,
            Status = garageDto.Status,
            MaxSlotsForCustomers = garageDto.MaxSlotsForCustomers,
            MaxSlotsForEmployess = garageDto.MaxSlotsForEmployess,
            MaxSlotsForFree = garageDto.MaxSlotsForFree,
        };



		var parameters = new DialogParameters
		{
			["UpdatedId"] = Gid,
			["Garage"] = createUpdateGarage
		};

		return await DialogService.ShowAsync<AssignOrganizationDialog>("Assign Garage", parameters);

	}
	private async Task<IDialogReference> OnClickRowEdit(RowDef row)
    {
        var id =int.Parse(row.Cells[0].Value);

		var garageDto= await GarageAppService.GetAsync(id);

        var createUpdateGarage=new CreateUpdateGarageDto { 
            NameEn = garageDto.NameEn,
            NameAr = garageDto.NameAr,
            Code = garageDto.Code,
            Status=garageDto.Status,
            MaxCapacity=garageDto.MaxCapacity,
            ClosingHours=garageDto.ClosingHours,
            OrganizationId=garageDto.OrganizationId,
            MaxSlotsForCustomers = garageDto.MaxSlotsForCustomers,
            MaxSlotsForEmployess = garageDto.MaxSlotsForEmployess,
            MaxSlotsForFree = garageDto.MaxSlotsForFree,
        };



		var parameters = new DialogParameters { 
            ["Garage"] = createUpdateGarage,
            ["UpdatedId"]=id
        };


        return await DialogService.ShowAsync<UpdateGarageDialog>("Edit Garage", parameters);
        
    }
    private async Task<IDialogReference> OnClickRowView(RowDef row)
    {
		var id = int.Parse(row.Cells[0].Value);

		var garageDto = await GarageAppService.GetAsync(id);


		var parameters = new DialogParameters
        {
            ["Garage"] = garageDto,
        };


        return await DialogService.ShowAsync<ViewGarageDialog>("View Garage", parameters);
    }

    private async Task OnClickRowDelete(RowDef row)
    {
		var id = int.Parse(row.Cells[0].Value);
        var Name = row.Cells[2].Value;

        var parameters = new DialogParameters
        {
            ["Name"] = Name,

        };

        var dialog = await DialogService.ShowAsync<SPDialogDelete>("DeleteGarage", parameters);
        var result = await dialog.Result;
        try
        {
            if (!result.Canceled)
            {
                await GarageAppService.DeleteAsync(id);
                SnackBar.ShowSnackbar(Snackbar, $"{Name} {L[AlertMessages.AlertMessages.DELETE]} {L[AlertMessages.AlertMessages.SUCCESS]}", Severity.Success);
                await Task.Delay(1000);
                Navigation.NavigateTo("/garages", forceLoad: true);

            }
        }
        catch (BusinessException ex)
        {

            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.DELETE]} {L[AlertMessages.AlertMessages.FAILED]}: {ex.Code}", Severity.Error);
        }
        catch (AuthenticationException ex)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.DELETE]} {L[AlertMessages.AlertMessages.FAILED]} : {ex.Message} ", Severity.Error);
        }
        catch (Exception ex)
        {

            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.DELETE]} {L[AlertMessages.AlertMessages.FAILED]}: {ex.Message} ", Severity.Error);
        }


    }
}
