using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Blazor.Components.SPComponents;
using SmartParking.Cutomers;
using SmartParking.Enums;
using SmartParking.Localization;
using SmartParking.Lookups;
using SmartParking.Packages;
using SmartParking.Permissions.CustomerPermissions;
using SmartParking.Permissions.PackagePermissions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Authentication;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Blazor.Components.Pages.Packages;

public partial class Package
{
    private List<MudMenuData> MenuDatas = new List<MudMenuData>();

    private string sortColumn;
    private Guid? TenantId;
    private string ValueOfMenue { get; set; } = "All";

    private int TotalItem;
    public int CurrentPage { get; set; } = 1; 
    public int RowPerPage { get; set; } = 10; 

    private TableData itemData = new TableData();

    private string termSearch;

    private readonly DialogOptions _backdropClick = new() { BackdropClick = false };
    private readonly IStringLocalizer<SmartParkingResource> _localizer;

    public Package(IStringLocalizer<SmartParkingResource> localizer)
    {
        _localizer = localizer;
        CreatePolicyName = PackagePermissions.Create;
        UpdatePolicyName = PackagePermissions.Edit;
        DeletePolicyName = PackagePermissions.Delete;
    }

    protected async override Task OnInitializedAsync()
    {
        await CreateMenue();
        if (!await AuthorizationService.IsGrantedAsync(CustomerPermissions.Default))
        {
            Navigation.NavigateTo("/Account/AccessDenied", true);
        }


        await base.OnInitializedAsync();
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
        await CreateTable(packagesDto);


	}
    private async Task CreateMenue()
    {
        var itemsOrganizations = new List<MenuItem>();
        var organizations = await OrganizationAppService.GetListAsync(new PagedAndSortedResultRequestDto());
        itemsOrganizations.Add(new MenuItem { MenueName = "Organization", Label = @L["All"], OnClick = EventCallback.Factory.Create<string>(this, async (string selectedValue) => await HandleSelectTenantAsync(ValueOfMenue, null)) });
        if (CultureInfo.CurrentCulture.Name == "ar")
        {
            foreach (var org in organizations.Items)
            {
                itemsOrganizations.Add(new MenuItem { MenueName = "Organization", Label = org.ArabicName, OnClick = EventCallback.Factory.Create<string>(this, async (string selectedValue) => await HandleSelectTenantAsync(ValueOfMenue, org.TenantId)) });
            }
        }
        else
        {
            foreach (var org in organizations.Items)
            {
                itemsOrganizations.Add(new MenuItem { MenueName = "Organization", Label = org.EnglishName, OnClick = EventCallback.Factory.Create<string>(this, async (string selectedValue) => await HandleSelectTenantAsync(ValueOfMenue, org.TenantId)) });
            }
        }

        MenuDatas = new List<MudMenuData>
        {
            new MudMenuData
            {
                Name = @L["Organizations"],
                Label = @L[ValueOfMenue],
                Items =itemsOrganizations
            },
        };
    }
    public async Task HandleSelectTenantAsync(string selectedValue, Guid? tenantId)
    {
        TenantId = tenantId;
        ValueOfMenue = selectedValue;

        var request = new PagedAndSortedAndSearchResultRequestDto
        {
            MaxResultCount = RowPerPage,
            SkipCount = (CurrentPage - 1) * RowPerPage,
            Sorting = sortColumn,
            SearchingTerm = termSearch
        };

        PagedResultDto<PackageDto> packages = await GetPackagesAsync(request, TenantId);

        TotalItem = (int)packages.TotalCount;
        await CreateTable(packages);
    }

    private async Task<PagedResultDto<PackageDto>> GetPackagesAsync(PagedAndSortedAndSearchResultRequestDto request,Guid? tenantId = null)
    {
        if (CurrentTenant.Id == null)
        {
            return await PackagesAppService.GetListMDKHLAsync(request, tenantId);
        }
        else
        {
            return await PackagesAppService.GetAllListOrganizationAsync(request);
        }
    }
    /// <summary>
    /// Searches for packages on the server side using the packages search term.
    /// </summary>
    /// <param name="searchTerm">The term to search for among packages.</param>
    /// <returns>Returns a task that represents the asynchronous operation.</returns>
    private async Task HandleSearch(PageChangeEventArgs pageInfo)
	{
        termSearch = pageInfo.SearchTerm;
        CurrentPage = pageInfo.PageNumber;
        RowPerPage = pageInfo.RowsPerPage;
        sortColumn = pageInfo.SortColumn;
        // Create a new search input object with pagination, sorting, and search term parameters
        var input = new PagedAndSortedAndSearchResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn,
            SearchingTerm = termSearch // The term to use for filtering results
        };

        // Call the service to perform the search based on the input parameters
        var packagesDto = await PackagesAppService.Search(input );

        // Calculate the total number of pages required to display all results
        TotalItem = (int)packagesDto.TotalCount;
        await CreateTable(packagesDto);
    }
    /// <summary>
    /// Fetches the table data for the specified page and updates the table with the retrieved data.
    /// </summary>
    /// <param name="page">The page number for which to fetch the data.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task FetchTableData(PageChangeEventArgs pageInfo)
    {

        termSearch = pageInfo.SearchTerm;
        CurrentPage = pageInfo.PageNumber;
        RowPerPage = pageInfo.RowsPerPage;
        sortColumn = pageInfo.SortColumn;


        // Create a request object for paged and sorted data
        var input = new PagedAndSortedAndSearchResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn,
            SearchingTerm= termSearch
            
        };

        var packagesDto = (CurrentTenant.Id) switch {
            (not null)=>await PackagesAppService.GetListAsync(input),
            (null)=>await PackagesAppService.GetListMDKHLAsync(input,null),
        };
        TotalItem = (int)packagesDto.TotalCount;
        await CreateTable(packagesDto);

    }

    private async Task CreateTable(PagedResultDto<PackageDto> packages)
    {
        // Initialize table data to be displayed, setting the table's name and whether to show the header
        var tableData = new TableData()
        {
            Name =L["Packages"],
            ShowHeader = true
        };


        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Id"], Name = "Id", IsFilterable = false, PropertyName = "Id", IsSortable = true, TypeFilter = TypeFilter.Text });
        if (CultureInfo.CurrentCulture.Name == "en")
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["EnglishName"], Name = "EnglishName", IsFilterable = true, PropertyName = "EnglishName", IsSortable = true, TypeFilter = TypeFilter.Text });
		}
		else
		{
			tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["ArabicName"], Name = "ArabicName", IsFilterable = true, PropertyName = "ArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });
		}
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Price"], Name = "Price", IsFilterable = false, PropertyName = "Price", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Published"], Name = "published", IsFilterable = false, PropertyName = "published", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Subscribers"], Name = "Subscribers", IsFilterable = false, PropertyName = "CustomerPackages.Count", IsSortable = true, TypeFilter = TypeFilter.Text });
        if (CurrentTenant.Id == null)
        {
            if (CultureInfo.CurrentCulture.Name == "en")
            {
                tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["OrganizationName"], Name = "EnglishName", IsFilterable = true, PropertyName = "EnglishName", IsSortable = true, TypeFilter = TypeFilter.Text });
            }
            else
            {
                tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["OrganizationName"], Name = "ArabicName", IsFilterable = true, PropertyName = "ArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });
            }

        }
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Status"], Name = "Status", IsFilterable = false, PropertyName = "Status", IsSortable = true, TypeFilter = TypeFilter.Enum });

        //tableData.Columns.Add(new ColumnDef() { DisplayName = "Max Capacity", Name = "MaxCapacity", IsFilterable = false, IsSortable = true, TypeFilter = TypeFilter.Number });
        foreach (var item in packages.Items)
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
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Price"],$"{item.Price} {_localizer["SAR"]}"));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Published"], item.Published ? 24 + _localizer[$"Hour"] : $" {item.PublishFrom} " + " - " + $" {item.PublishTo}"));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Subscribers"], item.SubscribersInPackages.ToString()));
            if (CurrentTenant.Id == null)
            {
                if (CultureInfo.CurrentCulture.Name == "en")
                {
                    rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["OrganizationName"], item.OrganizationEnglishName));
                }
                else
                {
                    rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["OrganizationName"], item.OrganizationArabicName));
                }

            }
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Status"], _localizer[$"{item.Status.ToString()}"]));


            tableData.Rows.Add(rowDef);
        }
        itemData = tableData;
        await Task.Yield();

    }

    private Task OpenDialogAsync(DialogOptions options)//Add Package
    {
        return Dialog.ShowAsync<AddPackageDialog>("Add Package", options);
    }

    private async Task<IDialogReference> OnClickRowEdit(RowDef row)
    {

        var id = int.Parse(row.Cells[0].Value);

        var packageDto = await PackagesAppService.GetAsync(id);


        var createUpdatePackage = new CreateUpdatePackageDto
        {
            ArabicName = packageDto.ArabicName,
            PackageTypeId = packageDto.PackageTypeId,
            Description = packageDto.Description,
            EnglishName = packageDto.EnglishName,
            OrganizationId = packageDto.OrganizationId,
            Price = packageDto.Price,
            Published = packageDto.Published,
            PublishFrom = packageDto.PublishFrom,
            PublishTo = packageDto.PublishTo,
            PackageStatus = packageDto.PackageStatus,
            PackageType = packageDto.PackageType,
            CustomerCategory = (CustomerCategoryStatus)Enum.Parse(typeof(CustomerCategoryStatus), packageDto.CustomerCategory),
            DurationOfPackage = packageDto.DurationOfPackage,
            Status = packageDto.Status,
            ActivationDate = packageDto.ActivationDate,
            DeactivationDate = packageDto.DectivationDate,


        };



        var parameters = new DialogParameters
        {
            ["Package"] = createUpdatePackage,
            ["UpdatedId"] = id,
        };


        return await Dialog.ShowAsync<EditPackageDialog>("Edit Package", parameters);

    }

    private async Task<IDialogReference> OnClickRowView(RowDef row)
    {

        var id = int.Parse(row.Cells[0].Value);


        var packageDto = await PackagesAppService.GetAsync(id);

        var package = new PackageDto
        {

            ArabicName = packageDto.ArabicName,
            PackageTypeId = packageDto.PackageTypeId,
            Description = packageDto.Description,
            EnglishName = packageDto.EnglishName,
            OrganizationId = packageDto.OrganizationId,
            Price = packageDto.Price,
            Published = packageDto.Published,
            PublishFrom = packageDto.PublishFrom,
            PublishTo = packageDto.PublishTo,
            PackageStatus = packageDto.PackageStatus,
            PackageType = packageDto.PackageType,
            CustomerCategory = packageDto.CustomerCategory,
            DurationOfPackage = packageDto.DurationOfPackage,
            ActivationDate = packageDto.ActivationDate,
            Status = packageDto.Status,
            DectivationDate = packageDto.DectivationDate,
            ActivedBy=packageDto.ActivedBy,
            DeactivatedBy=packageDto.DeactivatedBy,


        };

        var parameters = new DialogParameters
        {
            ["Package"] = package,
            ["CustomerCategory"] = packageDto.CustomerCategory.ToString(),
            ["UpdatedId"] = id

        };


        return await Dialog.ShowAsync<DetailsPackageDialog>("View Package", parameters);
    }

	private async Task OnClickRowDelete(RowDef row)
	{
        
        var id = int.Parse(row.Cells[0].Value);
        var Name = row.Cells[1].Value;

        var parameters = new DialogParameters
        {
            ["Name"] = Name,

        };
        var Subscribers = int.Parse(row.Cells[4].Value);
        if (Subscribers > 0)
        {
            ShowSnackbar("Can't delete package It Has Subscribers", MudBlazor.Severity.Warning);
            return;

        }



        var dialog = await Dialog.ShowAsync<SPDialogDelete>("PackagesDeleteDialog", parameters);
        var result = await dialog.Result;
        try
        {
            if (!result.Canceled)
            {
                await PackagesAppService.DeleteAsync(id);
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]} {L[AlertMessages.AlertMessages.DELETE]} {Name}", Severity.Success);
                await Task.Delay(1000);

                Navigation.NavigateTo("/package", true);

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


