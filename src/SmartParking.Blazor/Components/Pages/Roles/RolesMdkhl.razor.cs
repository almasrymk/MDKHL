using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Authentication;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Cameras;
using SmartParking.Localization;
using System.Globalization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Identity;

namespace SmartParking.Blazor.Components.Pages.Roles;

public partial class RolesMdkhl
{
    protected PermissionRolesManagementModal PermissionRolesManagementModal;
    protected ViewPermissionRoleModal ViewPermissionRolesManagementModal;

    private List<MudMenuData> MenuDatas = new List<MudMenuData>();
    private int TotalItem;
    public int CurrentPage { get; set; } = 1; // Default current page value is 10
    public int RowPerPage { get; set; } = 10; // Default value is 10
    public Guid? TenantId { get; private set; }
    private string sortColumn;
    private string ValueOfMenue = "";
    private TableData itemData = new TableData();
    private string termSearch;

    private readonly IStringLocalizer<SmartParkingResource> _localizer;
    protected bool HasManagePermissionsPermission { get; set; } = true;
    protected const string PermissionProviderName = "R";
    protected string ManagePermissionsPolicyName;
    public RolesMdkhl(IStringLocalizer<SmartParkingResource> localizer)
    {
        _localizer = localizer;
        CreatePolicyName = IdentityPermissions.Roles.Create;
        UpdatePolicyName = IdentityPermissions.Roles.Update;
        DeletePolicyName = IdentityPermissions.Roles.Delete;
        ManagePermissionsPolicyName = IdentityPermissions.Roles.ManagePermissions;

    }
    protected async override Task OnInitializedAsync()
    {
        if (!await AuthorizationService.IsGrantedAsync(IdentityPermissions.Roles.Default))
        {
            Navigation.NavigateTo("/Account/AccessDenied", true);
        }
        await CreateMenue();
        await base.OnInitializedAsync();
    }
    protected override async Task SetPermissionsAsync()
    {
        await base.SetPermissionsAsync();

        HasManagePermissionsPermission =
            await AuthorizationService.IsGrantedAsync(IdentityPermissions.Roles.ManagePermissions);
    }

    public async Task AddPermission(RowDef row)
    {
        var role = await RoleAppService.GetIdentityRoleById(Guid.Parse(row.Cells[0].Value));
        await PermissionRolesManagementModal.OpenAsync(PermissionProviderName,
                            role.Name,
                            role.TenantId);
    }
    public async Task HandleView(RowDef row)
    {
        var role = await RoleAppService.GetIdentityRoleById(Guid.Parse(row.Cells[0].Value));
        await ViewPermissionRolesManagementModal.OpenAsync(PermissionProviderName,
                            role.Name,
                            role.TenantId);
    }
    /// <summary>
    /// Searches for role on the server side using term search.
    /// </summary>
    /// <param name="searchTerm">The term to search for among roles.</param>
    /// <returns>Returns a task that represents the asynchronous operation.</returns>
    private async Task HandleSearch(PageChangeEventArgs pageInfo)
    {

        termSearch = pageInfo.SearchTerm;

        var input = new PagedAndSortedAndSearchResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn,
            SearchingTerm = termSearch
        };

        var contactInfosDto = await RoleAppService.SearchMdkhl(input);

        TotalItem = (int)contactInfosDto.TotalCount;
        await CreateTable(contactInfosDto);
    }
    /// <summary>
    /// Fetches the table data for the specified page and updates the table with the retrieved data.
    /// </summary>
    /// <param name="page">The page number for which to fetch the data.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task FetchTableData(PageChangeEventArgs pageInfo)
    {
        if(pageInfo.SortColumn == "Id")
            pageInfo.SortColumn = "Role.Id";
        termSearch = pageInfo.SearchTerm;
        CurrentPage = pageInfo.PageNumber;
        RowPerPage = pageInfo.RowsPerPage;
        sortColumn = pageInfo.SortColumn;

        var input = new PagedAndSortedAndSearchResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn,
            SearchingTerm = termSearch
        };
        var RolesDto = await RoleAppService.GetListAndPagedMdkhl(input , TenantId);


        TotalItem = (int)RolesDto.TotalCount;
        await CreateTable(RolesDto);

    }

    private async Task CreateMenue()
    {
        var itemsOrganizations = new List<MenuItem>();
        var organizations = await OrganizationsAppService.GetListAsync(new PagedAndSortedResultRequestDto());
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

        var RolesDto = await RoleAppService.GetListAndPagedMdkhl(request, TenantId);
        TotalItem = (int)RolesDto.TotalCount;
        await CreateTable(RolesDto);
    }
    private async Task CreateTable(PagedResultDto<RoleDto> rolesDto)
    {
        // Initialize table data to be displayed, setting the table's name and whether to show the header
        var tableData = new TableData()
        {
            Name = "Roles",
            ShowHeader = true
        };


        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Id"], Name = "Id", IsFilterable = false, PropertyName = "Role.Id", IsSortable = false, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Name"], Name = "Name", IsFilterable = false, PropertyName = "Role.Name", IsSortable = true, TypeFilter = TypeFilter.Text });
        if(CultureInfo.CurrentCulture.Name == "ar")
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["OrganizationName"], Name = "OrganizationName", IsFilterable = false, PropertyName = "Organization.ArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });
        else
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["OrganizationName"], Name = "OrganizationName", IsFilterable = false, PropertyName = "Organization.EnglishName", IsSortable = true, TypeFilter = TypeFilter.Text });
        foreach (var item in rolesDto.Items)
        {
            var rowDef = new RowDef();
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Id"], item.Id.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Name"], item.Name));
            if(CultureInfo.CurrentCulture.Name == "ar")
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["OrganizationName"], item.OrganizationArabicName.IsNullOrWhiteSpace() ? "مدخل" : item.OrganizationArabicName));
            }
            else
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["OrganizationName"], item.OrganizationEnglishName.IsNullOrWhiteSpace() ? "Mdkhl" : item.OrganizationEnglishName));
            }
            tableData.Rows.Add(rowDef);
        }
        itemData = tableData;
        await Task.Yield();
    }
    private async Task HandleAdd()
    {
        await DialogService.ShowAsync<AddRoleDialog>("Simple Dialog");
    }
    private async Task HandleEdit(RowDef row)
    {
        var id = Guid.Parse(row.Cells[0].Value);
        var role = await RoleAppService.GetRoleById(id);
        var parameters = new DialogParameters
        {
            ["RoleDto"] = role
        };
        await DialogService.ShowAsync<UpdateRoleDialog>("Simple Dialog", parameters);
    }

    public override bool Equals(object obj)
    {
        return obj is RolesMdkhl roles &&
               EqualityComparer<PermissionRolesManagementModal>.Default.Equals(PermissionRolesManagementModal, roles.PermissionRolesManagementModal);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(PermissionRolesManagementModal);
    }
}
