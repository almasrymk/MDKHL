
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Pages.GarageTransactions;
using SmartParking.DailyShifts;
using SmartParking.GarageTransactions;
using SmartParking.Localization;
using SmartParking.Permissions.GarageTransactionPermissions;
using System.Globalization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Blazor.Components.Pages.DailyShift;

public partial class DailyShift
{
    private List<DailyShiftDto> dailyShiftDtos = new List<DailyShiftDto>();
    private List<MudMenuData> MenuDatas = new List<MudMenuData>();
    public Guid? TenantId { get; private set; }
    private string sortColumn;
    private string ValueOfMenue = "";
    private TableData itemData = new TableData();

    private int currentPage = 1;
    private int pageSize = 100;

    private int TotalItem;
    public int CurrentPage { get; set; } = 1; // Default current page value is 10
    public int RowPerPage { get; set; } = 10; // Default value is 10


    private string termSearch;
    private readonly DialogOptions _backdropClick = new() { BackdropClick = false };
    private readonly IStringLocalizer<SmartParkingResource> _localizer;

    public DailyShift(IStringLocalizer<SmartParkingResource> localizer)
    {
        _localizer = localizer;
    }
    protected async override Task OnInitializedAsync()
    {
       
        await base.OnInitializedAsync();

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
        string sort;
        if (pageInfo.SortColumn == "Id")
            sort = "KeeperName DESC";
        else
            sort = pageInfo.SortColumn;

        CurrentPage = pageInfo.PageNumber;
        RowPerPage = pageInfo.RowsPerPage;
        // Create a request object for paged and sorted data
        var input = new PagedAndSortedAndSearchResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = sort,
            SearchingTerm = pageInfo.SearchTerm

        };
        if (CurrentTenant.Id == null)
        {
            // Retrieve the list of organizations from the server
            var dailyShiftDto = await DailyShiftAppService.GetListDailyShiftsAsync(input, null);
            TotalItem = (int)dailyShiftDto.TotalCount;
            await CreateTable(dailyShiftDto);
        }

        else
        {
            var tenantId = CurrentTenant.Id;
            var dailyShiftDto = await DailyShiftAppService.GetListDailyShiftsAsync(input, tenantId);
            TotalItem = (int)dailyShiftDto.TotalCount;
            await CreateTable(dailyShiftDto);
        }
    }
    private async Task<Task> OnClickRowDelete(RowDef row)
    {
        ShowSnackbar("Service Not Allowed", Severity.Warning);
        Navigation.NavigateTo("/dailyshifts");
        return Task.CompletedTask;


    }
    private async Task HandleFilter(PageChangeEventArgs pageInfo)
    {
        termSearch = pageInfo.SearchTerm;
        CurrentPage = pageInfo.PageNumber;
        RowPerPage = pageInfo.RowsPerPage;
        sortColumn = pageInfo.SortColumn;
        string sort;
        if (pageInfo.SortColumn == "Id")
            sort = "KeeperName DESC";
        else
            sort = pageInfo.SortColumn;

        CurrentPage = pageInfo.PageNumber;
        RowPerPage = pageInfo.RowsPerPage;
        // Create a request object for paged and sorted data
        var input = new PagedAndSortedAndSearchResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = sort,
            SearchingTerm = pageInfo.SearchTerm

        };
        if (CurrentTenant.Id == null)
        {
            // Retrieve the list of organizations from the server
            var dailyShiftDto = await DailyShiftAppService.GetListDailyShiftsAsync(input, null);
            TotalItem = (int)dailyShiftDto.TotalCount;
            await CreateTable(dailyShiftDto);
        }

        else
        {
            var tenantId = CurrentTenant.Id;
            var dailyShiftDto = await DailyShiftAppService.GetListDailyShiftsAsync(input, tenantId);
            TotalItem = (int)dailyShiftDto.TotalCount;
            await CreateTable(dailyShiftDto);
        }
    }

    private async Task CreateTable(PagedResultDto<DailyShiftDto> dailyShiftDto)
    {
        // Initialize table data to be displayed, setting the table's name and whether to show the header
        var tableData = new TableData()
        {
            Name = L["DailyShifts"],
            ShowHeader = true
        };


        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Id"], Name = "Id", IsFilterable = false, PropertyName = "Id", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Name"], Name = "Name", IsFilterable = true, PropertyName = "KeeperName", IsSortable = true, TypeFilter = TypeFilter.Text });
        if (CurrentTenant.Id == null)
        {
            if (CultureInfo.CurrentCulture.Name == "ar")
            {
                tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["OrganizationName"], Name = "OrganizationName", IsFilterable = true, PropertyName = "OrganizationNameAr", IsSortable = true, TypeFilter = TypeFilter.Text });
            }
            else
            {
                tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["OrganizationName"], Name = "OrganizationName", IsFilterable = true, PropertyName = "OrganizationNameEn", IsSortable = true, TypeFilter = TypeFilter.Text });
            }
        }
        if (CultureInfo.CurrentCulture.Name == "ar")
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["GarageName"], Name = "GarageName", IsFilterable = true, PropertyName = "GarageNameAr", IsSortable = true, TypeFilter = TypeFilter.Text });
        }
        else
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["GarageName"], Name = "GarageName", IsFilterable = true, PropertyName = "GarageNameEn", IsSortable = true, TypeFilter = TypeFilter.Text });
        }
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["RequestTime"], Name = "RequestTime", IsFilterable = true, PropertyName = "RequestTime", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["KeeperShiftRequestType"], Name = "KeeperShiftRequestType", IsFilterable = true, PropertyName = "KeeperShiftRequestType", IsSortable = true, TypeFilter = TypeFilter.Enum });
        
        foreach (var item in dailyShiftDto.Items)
        {
            var rowDef = new RowDef();
            rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("Name"), item.KeeperName));
            if (CurrentTenant.Id == null)
            {
                if (CultureInfo.CurrentCulture.Name == "ar")
                {
                    rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("OrganizationName"), item.OrganizationNameAr));
                }
                else
                {
                    rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("OrganizationName"), item.OrganizationNameEn));

                }
            }
            if (CultureInfo.CurrentCulture.Name == "ar")
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("GarageName"), item.GarageNameAr));
            }
            else
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("GarageName"), item.GarageNameEn));
            }
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("RequestTime"), item.RequestTime.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("KeeperShiftRequestType"), LocalizationHelper.GetLocalizedString(item.KeeperShiftRequestType.ToString())));
            tableData.Rows.Add(rowDef);
        }
        itemData = tableData;
        await Task.Yield();
    }
    private async Task<IDialogReference> OnClickRowEdit(RowDef row)
    {
        ShowSnackbar("Not Working", Severity.Warning);
        return null;


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

        PagedResultDto<DailyShiftDto> DailyShiftDto = await DailyShiftAppService.GetListDailyShiftsAsync(request, TenantId);

        TotalItem = (int)DailyShiftDto.TotalCount;
        await CreateTable(DailyShiftDto);
    }
    private async Task<IDialogReference> OnClickRowView(RowDef row)
    {
        var id = int.Parse(row.Cells[0].Value);
        var dailyShiftDto = await DailyShiftAppService.GetAsync(id);
        var parameters = new DialogParameters
        {
            ["DailyShift"] = dailyShiftDto,
        };

        return await Dialog.ShowAsync<ViewGarageTransaction>("View DailyShift", parameters);
    }
    private Task CreateDailyShift(DialogOptions options)//Add 
    {
        ShowSnackbar("Not Working", Severity.Warning);

        return Task.CompletedTask;

        //return Dialog.ShowAsync<GarageTransaction>("Add GarageTransaction", options);
    }
    void ShowSnackbar(string message, Severity severity)

    {

        Snackbar.Clear();

        Snackbar.Configuration.PositionClass = Defaults.Classes.Position.TopRight;

        Snackbar.Add(message, severity, c => c.SnackbarVariant = Variant.Filled);

    }
}
