using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Pages.PackageTypes;
using SmartParking.Blazor.Components.SPComponents;
using SmartParking.Localization;
using SmartParking.Lookups;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Threading.Tasks;
using System;
using Volo.Abp.MultiTenancy;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using SmartParking.SubscriptionTypes;
using System.Globalization;
using SmartParking.Blazor.Components.Shared;

namespace SmartParking.Blazor.Components.Pages.SubscriptionType;

public partial class SubscriptionType
{
    private int TotalItem;
    public int CurrentPage { get; set; } = 1; // Default current page value is 10
    public int RowPerPage { get; set; } = 10; // Default value is 10

    private TableData itemData = new TableData();

    private string termSearch;
    public bool ShowAdd { get; set; } = true;

    private DialogOptions options = new DialogOptions { CloseOnEscapeKey = true };

    private readonly IStringLocalizer<SmartParkingResource> _localizer;

    public SubscriptionType(IStringLocalizer<SmartParkingResource> localizer)
    {
        _localizer = localizer;
    }
    

    /// <summary>
    /// filter for cities on the server side using the city columns.
    /// </summary>
    /// <param name="columnFilter">The name of column to filter for among city.</param>
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
        var subscriptionTypeDto = await SubscriptionTypeAppService.FilterForMDKHL(input);

        // Calculate the total number of pages required to display all results
        TotalItem = (int)subscriptionTypeDto.TotalCount;
        CreateTable(subscriptionTypeDto);


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
        var SubscriptionTypeDto = await SubscriptionTypeAppService.SearchForMDKHL(input);

        // Calculate the total number of pages required to display all results
        TotalItem = (int)SubscriptionTypeDto.TotalCount;
        CreateTable(SubscriptionTypeDto);
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

        // Retrieve the list of cities from the server
        var SubscriptionTypeDto = await SubscriptionTypeAppService.GetListAsync(input);
        TotalItem = (int)SubscriptionTypeDto.TotalCount;
        CreateTable(SubscriptionTypeDto);

    }

    private void CreateTable(PagedResultDto<SubscriptionTypeDto> subscriptionType)
    {
        // Initialize table data to be displayed, setting the table's name and whether to show the header
        var tableData = new TableData()
        {
            Name = "SubscriptionType",
            ShowHeader = true
        };

        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Id"], Name = "Id", IsFilterable = false, PropertyName = "Id", IsSortable = true, TypeFilter = TypeFilter.Text });
       
        if(CultureInfo.CurrentCulture.Name == "ar")
			tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Type"], Name = "Type", IsFilterable = true, PropertyName = "TypeAr", IsSortable = true, TypeFilter = TypeFilter.Text });
        else
			tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Type"], Name = "Type", IsFilterable = true, PropertyName = "TypeEn", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Status"], Name = "Status", IsFilterable = false, PropertyName = "Status", IsSortable = true, TypeFilter = TypeFilter.Enum });
		foreach (var item in subscriptionType.Items)
        {
            var rowDef = new RowDef();
            rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
			if (CultureInfo.CurrentCulture.Name == "ar")
				rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Type"], item.TypeAr));
            else
				rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Type"], item.TypeEn));

            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Status"], _localizer[$"{item.Status.ToString()}"]));


			tableData.Rows.Add(rowDef);
        }
        itemData = tableData;
    }

    private Task CreatePackageType()
    {


        return Dialog.ShowAsync<AddSubscriptionTypeDialog>("Simple Dialog", options);


    }
    private async Task<IDialogReference> OnClickRowEdit(RowDef row)
    {
        var id = int.Parse(row.Cells[0].Value);

        var subscriptionTypeDto = await SubscriptionTypeAppService.GetAsync(id);

        var createUpdateSubscriptionTypeDto = new CreateUpdateSubscriptionTypeDto()
        {
            TypeAr = subscriptionTypeDto.TypeAr,
            TypeEn = subscriptionTypeDto.TypeEn,
        };



        var parameters = new DialogParameters
        {
            ["subscriptionType"] = subscriptionTypeDto,
            ["UpdatedId"] = id
        };


        return await Dialog.ShowAsync<EditSubscriptionTypeDialog>("Edit PackageType", parameters);

    }


    private async Task<IDialogReference> OnClickRowView(RowDef row)
    {
        var id = int.Parse(row.Cells[0].Value);

        var subscriptionTypeDto = await SubscriptionTypeAppService.GetAsync(id);


        var parameters = new DialogParameters
        {
            ["subscriptionType"] = subscriptionTypeDto,
        };


        return await Dialog.ShowAsync<DetailsSubscriptionType>("View PackageType", parameters);
    }

    private async Task OnClickRowDelete(RowDef row)
    {
      
        var id = int.Parse(row.Cells[0].Value);
        var Name = row.Cells[1].Value;

        var parameters = new DialogParameters
        {
            ["Name"] = Name,

        };

        var dialog = await Dialog.ShowAsync<SPDialogDelete>("Package type Delete Dialog", parameters);
        var result = await dialog.Result;
        try
        {
            if (!result.Canceled)
            {
                await SubscriptionTypeAppService.DeleteAsync(id);
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]} {L[AlertMessages.AlertMessages.DELETE]} {Name}", Severity.Success);
                await Task.Delay(1000);

                Navigation.NavigateTo("/subscriptiontypes", forceLoad: true);

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
