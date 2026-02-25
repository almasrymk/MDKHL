using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Pages.User;
using SmartParking.Localization;
using SmartParking.UserShifts;
using System.Globalization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace SmartParking.Blazor.Components.Pages.UserShifts;

public partial class UserShifts
{
    protected PermissionManagementModal PermissionManagementModal;
    private List<MudMenuData> MenuDatas = new List<MudMenuData>();
    private int TotalItem;
    public int CurrentPage { get; set; } = 1;
    public int RowPerPage { get; set; } = 10;
    public Guid? TenantId { get; private set; }
    private string sortColumn;
    private string ValueOfMenue = "";
    private TableData itemData = new TableData();

    // Kept so OnClickViewShiftDetails can look up Role / Device / Garage / Name
    // without needing extra hidden cells in the table.
    private IReadOnlyList<ShiftZoneUserViewDto> _lastUsers = new List<ShiftZoneUserViewDto>();

    public bool ViewAdd { get; set; } = true;
    protected bool HasManagePermissionsPermission { get; set; } = true;

    private string termSearch;
    private readonly IStringLocalizer<SmartParkingResource> _localizer;
    private readonly ICurrentUser _currentUser;
    protected string ManagePermissionsPolicyName;

    public UserShifts(IStringLocalizer<SmartParkingResource> localizer, ICurrentUser currentUser)
    {
        _localizer = localizer;
        _currentUser = currentUser;
        ManagePermissionsPolicyName = IdentityPermissions.Users.ManagePermissions;
    }

    protected async override Task OnInitializedAsync()
    {
        await CreateMenue();
        await base.OnInitializedAsync();
    }

    private async Task CreateMenue()
    {
        var itemsOrganizations = new List<MenuItem>();
        var organizations = await OrganizationAppService.GetListAsync(new PagedAndSortedResultRequestDto());

        itemsOrganizations.Add(new MenuItem
        {
            MenueName = "Organization",
            Label = @L["All"],
            OnClick = EventCallback.Factory.Create<string>(this,
                async (string selectedValue) => await HandleSelectTenantAsync(ValueOfMenue, null))
        });

        bool isArabic = CultureInfo.CurrentCulture.Name == "ar";
        foreach (var org in organizations.Items)
        {
            itemsOrganizations.Add(new MenuItem
            {
                MenueName = "Organization",
                Label = isArabic ? org.ArabicName : org.EnglishName,
                OnClick = EventCallback.Factory.Create<string>(this,
                    async (string selectedValue) => await HandleSelectTenantAsync(ValueOfMenue, org.TenantId))
            });
        }

        MenuDatas = new List<MudMenuData>
        {
            new MudMenuData
            {
                Name = @L["Organizations"],
                Label = @L[ValueOfMenue],
                Items = itemsOrganizations
            }
        };
    }

    public async Task HandleSelectTenantAsync(string selectedValue, Guid? tenantId)
    {
        TenantId = tenantId;
        ValueOfMenue = selectedValue;

        if (sortColumn == "Id") sortColumn = "UserId";

        var request = new PagedAndSortedAndSearchResultRequestDto
        {
            MaxResultCount = RowPerPage,
            SkipCount = (CurrentPage - 1) * RowPerPage,
            Sorting = sortColumn,
            SearchingTerm = termSearch
        };

        PagedResultDto<ShiftZoneUserViewDto> userDto =
            await ShiftZoneActionAppService.GetShiftZoneUsersAsync(TenantId, request);

        TotalItem = (int)userDto.TotalCount;
        await CreateTable(userDto.Items);
    }

    private async Task FetchTableData(PageChangeEventArgs pageInfo)
    {
        termSearch = pageInfo.SearchTerm;
        CurrentPage = pageInfo.PageNumber;
        RowPerPage = pageInfo.RowsPerPage;
        sortColumn = pageInfo.SortColumn;

        if (sortColumn == "Id") sortColumn = "UserId";

        var input = new PagedAndSortedAndSearchResultRequestDto
        {
            MaxResultCount = RowPerPage,
            SkipCount = (CurrentPage - 1) * RowPerPage,
            Sorting = sortColumn,
            SearchingTerm = termSearch
        };

        PagedResultDto<ShiftZoneUserViewDto> userDto =
            await ShiftZoneActionAppService.GetShiftZoneUsersAsync(TenantId, input);

        TotalItem = (int)userDto.TotalCount;
        await CreateTable(userDto.Items);
    }

    /// <summary>
    /// Opens the Shift Details dialog, enriching it with the ShiftZoneUserViewDto
    /// data (Role, Device, Garage, Name) that KeeperShiftDto does not carry.
    /// Lookup is by UserName because UserId is never populated by GetShiftZoneUsersAsync.
    /// </summary>
    public async Task<IDialogReference> OnClickViewShiftDetails(RowDef row)
    {
        var userName = row.Cells[0].Value;

        var user = _lastUsers.FirstOrDefault(u =>
            string.Equals(u.UserName, userName, StringComparison.OrdinalIgnoreCase));

        var parameters = new DialogParameters
        {
            ["UserFullName"] = user?.Name ?? string.Empty,
            ["Role"] = user?.Role ?? string.Empty,
            ["Device"] = user?.Device ?? string.Empty,
            ["Devices"] = user?.Devices ?? new List<string>(),   // ← new
            ["Garage"] = user?.Garage ?? string.Empty,
        };

        return await Dialog.ShowAsync<ViewUserShiftsDialog>(
            L["ShiftDetails"],
            parameters,
            new DialogOptions
            {
                MaxWidth = MaxWidth.Small,
                FullWidth = true,
                CloseOnEscapeKey = true
            });
    }

    private async Task CreateTable(IReadOnlyList<ShiftZoneUserViewDto> users)
    {
        _lastUsers = users;   // ← retained so the dialog lookup works

        var tableData = new TableData
        {
            Name = L["UsersCanAssignShifts"],
            ShowHeader = true
        };

        // Hidden UserName column – index 0, used as the lookup key in OnClickViewShiftDetails.
        // We cannot use UserId because GetShiftZoneUsersAsync never populates it.
        tableData.Columns.Add(new ColumnDef
        {
            DisplayName = _localizer["UserName"],
            Name = "HiddenUserName",
            IsFilterable = false,
            PropertyName = "UserName",
            IsSortable = false,
            TypeFilter = TypeFilter.Text,
            IsColumnVisible = false
        });

        tableData.Columns.Add(new ColumnDef { DisplayName = _localizer["UserName"], Name = "UserName", IsFilterable = true, PropertyName = "User.UserName", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef { DisplayName = _localizer["UserFullName"], Name = "Name", IsFilterable = true, PropertyName = "User.Name", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef { DisplayName = _localizer["Device"], Name = "Device", IsFilterable = true, PropertyName = "User.Device", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef { DisplayName = _localizer["Garage"], Name = "Garage", IsFilterable = true, PropertyName = "User.Garage", IsSortable = true, TypeFilter = TypeFilter.Text });

        bool isHost = currrentTenant?.Id is null;
        bool isArabic = CultureInfo.CurrentCulture.Name == "ar";

        if (isHost)
        {
            tableData.Columns.Add(new ColumnDef
            {
                DisplayName = _localizer["OrganizationName"],
                Name = "Organization Name",
                IsFilterable = false,
                PropertyName = isArabic ? "OrganizationArabicName" : "OrganizationEnglishName",
                IsSortable = true,
                TypeFilter = TypeFilter.Text
            });
        }

        foreach (var item in users)
        {
            var rowDef = new RowDef();

            // Cell 0 – hidden UserName (must stay first, used as lookup key)
            rowDef.Cells.Add(new KeyValuePair<string, string>("HiddenUserName", item.UserName ?? string.Empty));

            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["UserName"], item.UserName));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["UserFullName"], item.Name));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Device"], item.Device));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Garage"], item.Garage));

            if (isHost)
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(
                    _localizer["OrganizationName"],
                    isArabic ? item.OrganizationArabicName : item.OrganizationEnglishName));
            }

            tableData.Rows.Add(rowDef);
        }

        itemData = tableData;
        await Task.Yield();
    }
}