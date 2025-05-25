using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Pages.GarageTransactions;
using SmartParking.GarageTransactions;
using SmartParking.Localization;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Volo.Abp.Application.Dtos;
using SmartParking.GarageTransactionEndPoints;
using SmartParking.Blazor.Components.Pages.Subscriptions;
using SmartParking.Subscriptions;
using SmartParking.Blazor.Components.SPComponents;
using System.Security.Authentication;
using Volo.Abp;
using System.Configuration;
using Microsoft.AspNetCore.Authorization;
using SmartParking.Permissions.UnLinkedTransactionPermissions;

namespace SmartParking.Blazor.Components.Pages.UnLinkedTransactions;

public partial class UnLinkedTransaction
{
    private List<UnLinkedTransactionDto> unLinkedTransactions = new List<UnLinkedTransactionDto>();

    private int currentPage = 1;
    private int pageSize = 10;

    private int TotalItem;
    public int CurrentPage { get; set; } = 1; // Default current page value is 10
    public int RowPerPage { get; set; } = 10; // Default value is 10

    private TableData itemData = new TableData();

    private string termSearch;

    private readonly DialogOptions _backdropClick = new() { BackdropClick = false };
    private readonly IStringLocalizer<SmartParkingResource> _localizer;

    public UnLinkedTransaction(IStringLocalizer<SmartParkingResource> localizer)
    {
        _localizer = localizer;
    }
    protected async override Task OnInitializedAsync()
    {
        if (!await AuthorizationService.IsGrantedAsync(UnLinkedTransactionPermission.Default))
        {

            Navigation.NavigateTo("/Account/AccessDenied", true);
        }
        await base.OnInitializedAsync();

    }

    /// <summary>
    /// filter for unLinkedTransaction on the server side using the unLinkedTransaction columns.
    /// </summary>
    /// <param name="pageInfo">The data of column to filter for among unLinkedTransactions.</param>
    /// <returns>Returns a task that represents the asynchronous operation.</returns>
    private async Task HandleFilter(PageChangeEventArgs pageInfo)
    {

       
        var input = new PagedAndSortedAndFilteredResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn,
            SearchingTerm = termSearch, // The term to use for filtering results
            filters = pageInfo.Filters
        };
        // Call the service to perform the search based on the input parameters
       
            var unLinkedTransactionDto = await UnLinkedTransactionAppService.FilterForMDKHL(input);

            // Calculate the total number of pages required to display all results
            TotalItem = (int)unLinkedTransactionDto.TotalCount;
            CreateTable(unLinkedTransactionDto);
    }
    /// <summary>
    /// Searches for unLinkedTransaction on the server side using the unLinkedTransactions code search term.
    /// </summary>
    /// <param name="pageInfo">The term to search for among unLinkedTransactions.</param>
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
            var garageTransactionsDto = await UnLinkedTransactionAppService.SearchForMDKHL(input);

            // Calculate the total number of pages required to display all results
            TotalItem = (int)garageTransactionsDto.TotalCount;
            CreateTable(garageTransactionsDto);
      
    }
    /// <summary>
    /// Fetches the table data for the specified page and updates the table with the retrieved data.
    /// </summary>
    /// <param name="pageInfo">The page number for which to fetch the data.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task FetchTableData(PageChangeEventArgs pageInfo)
    {

        CurrentPage = pageInfo.PageNumber;
        RowPerPage = pageInfo.RowsPerPage;
        termSearch = pageInfo.SearchTerm;
        // Create a request object for paged and sorted data
        var input = new PagedAndSortedAndSearchResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn , 
            SearchingTerm = termSearch
            
        };

        // Retrieve the list of organizations from the server
        var unLinkedTransactions = await UnLinkedTransactionAppService.GetListAsync(input);
        TotalItem = (int)unLinkedTransactions.TotalCount;
        CreateTable(unLinkedTransactions);

    }

    private void CreateTable(PagedResultDto<UnLinkedTransactionDto> unLinkedTransaction)
    {
        // Initialize table data to be displayed, setting the table's name and whether to show the header
        var tableData = new TableData()
        {
            Name = L["UnLinkedTransaction"],
            ShowHeader = true
        };


        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Id"], Name = "Id", IsFilterable = false, PropertyName = "Id", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["PlateNumber"], Name = "PlateNumber", IsFilterable = true, PropertyName = "PlateNumber", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["AccurateTime"], Name = "AccurateTime", IsFilterable = true, PropertyName = "AccurateTime", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["SnapTime"], Name = "SnapTime", IsFilterable = true, PropertyName = "SnapTime", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["DeviceId"], Name = "DeviceId", IsFilterable = true, PropertyName = "DeviceId", IsSortable = true, TypeFilter = TypeFilter.Text });

        foreach (var item in unLinkedTransaction.Items)
        {
            var rowDef = new RowDef();
            rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["PlateNumber"], item.PlateNumber));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["AccurateTime"], item.AccurateTime.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["SnapTime"], item.SnapTime.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["DeviceId"], item.DeviceId));

            tableData.Rows.Add(rowDef);
        }
        itemData = tableData;
    }

    private Task CreateGarageTransaction(DialogOptions options)//Add 
    {
       
        return Dialog.ShowAsync<AddUnLinkedTransactionDialog>("Add UnLinkedTransaction", options);
    }

    private async Task<IDialogReference> OnClickRowView(RowDef row)
    {

        var id = int.Parse(row.Cells[0].Value);

        var unLinkedTransactionDto = await UnLinkedTransactionAppService.GetAsync(id);

        var parameters = new DialogParameters
        {
            ["unLinkedTransactionDto"] = unLinkedTransactionDto
        };


        return await Dialog.ShowAsync<DetailsUnLinkedTransactionDialog>("View unLinked Transaction", parameters);
    }

    private async Task OnClickRowDelete(RowDef row)
    {

        var successMessage = _localizer["UnLinkedTransactionDeletedSuccessfully"];
        var errorMessage = L["Can'tDeleteUnLinkedTransaction"];
        var validationErrorMessage = L["PleaseFixTheValidationErrors"];

        var id = int.Parse(row.Cells[0].Value);
        var Name = row.Cells[2].Value;

        var parameters = new DialogParameters
        {
            ["Name"] = Name,

        };

        var dialog = await Dialog.ShowAsync<SPDialogDelete>("Subscription Delete Dialog", parameters);
        var result = await dialog.Result;
        try
        {
            if (!result.Canceled)
            {
                await UnLinkedTransactionAppService.DeleteAsync(id);
                ShowSnackbar(successMessage, Severity.Error);
                await Task.Delay(1000);

                Navigation.NavigateTo("/unLinkedTransaction", forceLoad: true);

            }


        }
        catch (BusinessException ex)
        {

            ShowSnackbar($"{errorMessage} {ex.Code} + ", Severity.Error);
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

        Snackbar.Configuration.PositionClass = Defaults.Classes.Position.TopRight;

        Snackbar.Add(message, severity, c => c.SnackbarVariant = Variant.Filled);

    }
}
