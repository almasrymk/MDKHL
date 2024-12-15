using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Organizations;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Volo.Abp.Application.Dtos;
using MudBlazor;
using System.Security.Authentication;
using Volo.Abp;
using SmartParking.Blazor.Components.SPComponents;
using Microsoft.Extensions.Localization;
using SmartParking.Localization;
using System.Globalization;
using SmartParking.Permissions.GaragePermissions;
using SmartParking.Permissions.OrganizationPermissions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Authorization;

namespace SmartParking.Blazor.Components.Pages.Organizations;

public partial class Organization
{
    private int TotalItem;
    public int CurrentPage { get; set; } = 1; // Default current page value is 10
    public int RowPerPage { get; set; } = 10; // Default value is 10

    private TableData itemData = new TableData();

    private string termSearch;
    private bool HasChangeStatusPermission;
    private readonly IStringLocalizer<SmartParkingResource> _localizer;

    public Organization(IStringLocalizer<SmartParkingResource> localizer)
    {
        _localizer = localizer;
        CreatePolicyName = OrganizationPermissions.Create;
        UpdatePolicyName = OrganizationPermissions.Edit;
        DeletePolicyName = OrganizationPermissions.Delete;
        
    }
   

    private async Task HandleDelete(RowDef row)
    {
        var successMessage = L["OrganizationDeletedSuccessfully"];
        var errorMessage = L["Can'tDeleteOrganiztion"];
        var validationErrorMessage = L["PleaseFixTheValidationErrors"];

        var id = int.Parse(row.Cells[0].Value);
        var Name = row.Cells[2].Value;

        var parameters = new DialogParameters
        {
            ["Name"] = Name,

        };

        var dialog = await Dialog.ShowAsync<SPDialogDelete>("OrganizationDeleteDialog", parameters);
        var result = await dialog.Result;
        try
        {
            if (!result.Canceled)
            {
                await OrganizationsAppService.DeleteByIdAsync(id);
                ShowSnackbar(successMessage, Severity.Error);
                await Task.Delay(1000);

                Navigation.NavigateTo("/organizations", forceLoad: true);

            }


        }
        catch (BusinessException ex)
        {

            ShowSnackbar($"{ errorMessage} {ex.Code} + ", Severity.Error);
        }
        catch (AuthenticationException ex)
        {
            ShowSnackbar($"{errorMessage} {ex.Message} + ", Severity.Error);
        }
        catch (Exception ex)
        {

            ShowSnackbar($"{errorMessage} {ex.Message} + ", Severity.Error);
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
        Snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomRight;
        Snackbar.Add(message, severity, c => c.SnackbarVariant = Variant.Filled);
    }
    private void HandleView(RowDef row)
    {
        Navigation.NavigateTo("/organizations/view/" + row.Cells[0].Value, true);
    }
    private void HandleEdit(RowDef row)
    {
        Navigation.NavigateTo("/organizations/update/" + row.Cells[0].Value,true);
    }
    private void HandleAdd()
    {
        Navigation.NavigateTo("/organizations/add",true);
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
        var organizationsDto = await OrganizationsAppService.Filter(input);

        // Calculate the total number of pages required to display all results
        TotalItem = (int)organizationsDto.TotalCount;
        CreateTable(organizationsDto);


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
        var organizationsDto = await OrganizationsAppService.Search(input);

        // Calculate the total number of pages required to display all results
        TotalItem = (int)organizationsDto.TotalCount;
        CreateTable(organizationsDto);
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
        var organizationsDto = await OrganizationsAppService.GetListAsync(input);
        TotalItem = (int)organizationsDto.TotalCount;
        CreateTable(organizationsDto);
       
    }

    private void CreateTable(PagedResultDto<OrganizationDto> organizations)
    {
        // Initialize table data to be displayed, setting the table's name and whether to show the header
        var tableData = new TableData()
        {
            Name = "Organization",
            ShowHeader = true
        };


        tableData.Columns.Add(new ColumnDef() { DisplayName = "Id", Name = "Id", IsFilterable = false, PropertyName = "Id", IsSortable = false, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Code"], Name = "Code", IsFilterable = false, PropertyName = "Code", IsSortable = true, TypeFilter = TypeFilter.Text });
        if (CultureInfo.CurrentCulture.Name == "en")
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["EnglishName"], Name = "EnglishName", IsFilterable = true, PropertyName = "EnglishName", IsSortable = true, TypeFilter = TypeFilter.Text });
            tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Country Name"], Name = "CountryName", IsFilterable = true, PropertyName = "Country.EnglishName", IsSortable = true, TypeFilter = TypeFilter.Text });
            tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["RegionName"], Name = "RegionName", IsFilterable = true, PropertyName = "Region.EnglishName", IsSortable = true, TypeFilter = TypeFilter.Text });
            tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["State Name"], Name = "StateName", IsFilterable = true, PropertyName = "State.EnglishName", IsSortable = true, TypeFilter = TypeFilter.Text });
            tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["City Name"], Name = "CityName", IsFilterable = true, PropertyName = "City.EnglishName", IsSortable = true, TypeFilter = TypeFilter.Text });

        }
        else
        {
             tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["ArabicName"], Name = "ArabicName", IsFilterable = true, PropertyName = "ArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });
             tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Country Name"], Name = "CountryName", IsFilterable = true, PropertyName = "Country.ArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });
             tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["RegionName"], Name = "RegionName", IsFilterable = true, PropertyName = "Region.ArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });
             tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["State Name"], Name = "StateName", IsFilterable = true, PropertyName = "State.ArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });
             tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["City Name"], Name = "CityName", IsFilterable = true, PropertyName = "City.ArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });

        }
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["NumOfGarages"], Name = "NumOfGarages", IsFilterable = true, IsSortable = true, PropertyName = "Subscription.NumbersOfGarages", TypeFilter = TypeFilter.Number });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["ParkingSlots"], Name = "ParkingSlots", IsFilterable = true,PropertyName= "Subscription.ParkingSlot", IsSortable = true, TypeFilter = TypeFilter.Number });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Num Of Cameras"], Name = "NumOfCameras", IsFilterable = false, IsSortable = false, TypeFilter = TypeFilter.Number });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Status"], Name = "Status", IsFilterable = true, PropertyName = "Status", IsSortable = true, TypeFilter = TypeFilter.Enum });
        foreach (var item in organizations.Items)
        {
            var rowDef = new RowDef();
            rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Code"], item.Code));
            if (CultureInfo.CurrentCulture.Name == "en")
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["EnglishName"], item.EnglishName));
                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Country Name"], item.CountryName));
                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["RegionName"], item.RegionName));
                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["State Name"], item.StateName));
                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["City Name"], item.CityName));
            }
            else
            {

                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["ArabicName"], item.ArabicName));
                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Country Name"], item.CountryNameAr));
                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["RegionName"], item.RegionNameAr));
                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["State Name"], item.StateNameAr));
                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["City Name"], item.CityNameAr));
            }
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["ParkingSlots"], item.ParkingSlots.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["NumOfGarages"], item.NumbersOfGarages.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Num Of Cameras"], item.NumberOfCameras.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Status"], _localizer[$"{item.Status.ToString()}"]));


            tableData.Rows.Add(rowDef);
        }
        itemData = tableData;
    }
}
