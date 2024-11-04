using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Localization;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Volo.Abp.Application.Dtos;
using SmartParking.Subscriptions;
using SmartParking.Lookups;
using SmartParking.Blazor.Components.SPComponents;
using SmartParking.Vehicles.LookUps;
using System.Security.Authentication;
using Volo.Abp;
using Volo.Abp.Validation;
using System.Linq;
using System.Globalization;

namespace SmartParking.Blazor.Components.Pages.Subscriptions;

public partial class Subscription
{
    private int TotalItem;
    public int CurrentPage { get; set; } = 1; // Default current page value is 10
    public int RowPerPage { get; set; } = 10; // Default value is 10

    private TableData itemData = new TableData();

    private string termSearch;

    private DialogOptions options = new DialogOptions { CloseOnEscapeKey = true };

    private readonly IStringLocalizer<SmartParkingResource> _localizer;
    public Subscription(IStringLocalizer<SmartParkingResource> localizer)
    {
        _localizer = localizer;
    }

    /// <summary>
    /// filter for subscriptions on the server side using the subscription columns.
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
        var subscriptionDto = await AppService.Filter(input);

        // Calculate the total number of pages required to display all results
        TotalItem = (int)subscriptionDto.TotalCount;
        CreateTable(subscriptionDto);


    }

    /// <summary>
    /// Searches for subscriptions on the server side using the subscription type search term.
    /// </summary>
    /// <param name="searchTerm">The term to search for among subscription.</param>
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
        var subscriptionDto = await AppService.Search(input);

        // Calculate the total number of pages required to display all results
        TotalItem = (int)subscriptionDto.TotalCount;
        CreateTable(subscriptionDto);
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
        var subscriptionDto = await AppService.GetListAsync(input);
        TotalItem = (int)subscriptionDto.TotalCount;
        CreateTable(subscriptionDto);

    }

    private void CreateTable(PagedResultDto<SubscriptionDto> invoices)
    {
        // Initialize table data to be displayed, setting the table's name and whether to show the header
        var tableData = new TableData()
        {
            Name = "Subscriptions",
            ShowHeader = true
        };

        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Id"], Name = "Id", IsFilterable = false, PropertyNmae = "Id", IsSortable = false, TypeFilter = TypeFilter.Text });
        if (CultureInfo.CurrentCulture.Name == "en")
        { 
             tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["English Name"], Name = "English Name", IsFilterable = false, PropertyNmae = "EnglishName", IsSortable = true, TypeFilter = TypeFilter.Text });
        
        }
        else
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Arabic Name"], Name = "Arabic Name", IsFilterable = false, PropertyNmae = "ArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });

        }
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["SubscriptionDescription"], Name = "SubscriptionDescription", IsFilterable = false, PropertyNmae = "SubscriptionDescription", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["SubscriptionType"], Name = "SubscriptionType", IsFilterable = true, PropertyNmae = "SubscriptionType.Type", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["SubscriptionPrice"], Name = "SubscriptionPrice", IsFilterable = false, PropertyNmae = "SubscriptionPrice", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["SubscriptionDescription"], Name = "SubscriptionDescription", IsFilterable = false, PropertyNmae = "SubscriptionDescription", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["SubscriptionStatus"], Name = "Status", IsFilterable = true, PropertyNmae = "Status", IsSortable = true, TypeFilter = TypeFilter.Enum });



        foreach (var item in invoices.Items)
        {
            var rowDef = new RowDef();
            rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
            if (CultureInfo.CurrentCulture.Name == "en")
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["EnglishName"], item.EnglishName));
            
            }
            else
            {

                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["ArabicName"], item.ArabicName));
            }
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["SubscriptionDescription"], item.SubscriptionDescription));
			rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["SubscriptionType"], item.SubscriptionType));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["SubscriptionPrice"], item.SubscriptionPrice.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["SubscriptionStatus"], _localizer[$"{item.Status.ToString()}"]));


            tableData.Rows.Add(rowDef);
        }
        itemData = tableData;
    }


    private Task CreateSubscription()
    {


        return Dialog.ShowAsync<AddSubscriptionDialog>("Simple Dialog", options);


    }
    private async Task<IDialogReference> OnClickRowEdit(RowDef row)
    {
        var id = int.Parse(row.Cells[0].Value);

        var subscriptionDto = await AppService.GetAsync(id);

        var createUpdateSubscriptionDto = new CreateUpdateSubscriptionDto()
        {
           ArabicName = subscriptionDto.ArabicName,
           EnglishName = subscriptionDto.EnglishName,
           SubscriptionDescription = subscriptionDto.SubscriptionDescription,
           SubscriptionPrice = subscriptionDto.SubscriptionPrice,
           ActivationDate =  subscriptionDto .ActivationDate,
           InActivationDate = subscriptionDto.InActivationDate,
           SubscriptionTypeId = subscriptionDto.SubscriptionTypeId,
           Status = subscriptionDto.Status,

        };



        var parameters = new DialogParameters
        {
            ["subscription"] = subscriptionDto,
            ["UpdatedId"] = id
        };


        return await Dialog.ShowAsync<EditSubscriptionDialog>("Edit Subscription", parameters);

    }


    private async Task<IDialogReference> OnClickRowView(RowDef row)
    {
        var id = int.Parse(row.Cells[0].Value);

        var subscriptionDto = await AppService.GetAsync(id);


        var parameters = new DialogParameters
        {
            ["subscription"] = subscriptionDto,
        };


        return await Dialog.ShowAsync<DetailsSubscriptionDialog>("View Subscription", parameters);
    }

    private async Task OnClickRowDelete(RowDef row)
    {
        var successMessage = _localizer["SubscriptionDeletedSuccessfully"];
		var errorMessage = L["Can'tDeleteSubscription"];
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
                await AppService.DeleteAsync(id);
                ShowSnackbar(successMessage, Severity.Error);
                await Task.Delay(1000);

                Navigation.NavigateTo("/subscriptions", forceLoad: true);

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
        Snackbar.Add(message, severity, c => c.SnackbarVariant = Variant.Filled);

    }
}
