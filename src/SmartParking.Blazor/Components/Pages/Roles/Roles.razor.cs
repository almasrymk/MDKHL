using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Pages.ContactsData;
using SmartParking.Localization;
using SmartParking.Organizations;
using SmartParking.Permissions.ContactPermissions;
using static MudBlazor.CategoryTypes;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Volo.Abp.Identity;
using Volo.Abp.Application.Dtos;
using SmartParking.Blazor.Components.Pages.User;
using Microsoft.AspNetCore.Authorization;
using SmartParking.Authentication;
using SmartParking.Permissions.PackagePermissions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SmartParking.Blazor.Components.Pages.Roles;

partial class Roles
{
    protected PermissionRolesManagementModal PermissionRolesManagementModal;

    private int TotalItem;
    public int CurrentPage { get; set; } = 1; // Default current page value is 10
    public int RowPerPage { get; set; } = 10; // Default value is 10

    private TableData itemData = new TableData();

    private string termSearch;
    private readonly IStringLocalizer<SmartParkingResource> _localizer;
    protected bool HasManagePermissionsPermission { get; set; } = true;
    protected const string PermissionProviderName = "R";
    protected string ManagePermissionsPolicyName;
    public Roles(IStringLocalizer<SmartParkingResource> localizer)
    {
        _localizer = localizer;
        CreatePolicyName = IdentityPermissions.Roles.Create;
        UpdatePolicyName = IdentityPermissions.Roles.Update;
        DeletePolicyName = IdentityPermissions.Roles.Delete;
        ManagePermissionsPolicyName = IdentityPermissions.Roles.ManagePermissions;

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

        var contactInfosDto = await RoleAppService.SearchByTenant(input,CurrentTenant.Id);

        TotalItem = (int)contactInfosDto.TotalCount;
        CreateTable(contactInfosDto);
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

        var input = new PagedAndSortedResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn
        };

        var RolesDto =  await RoleAppService.GetListAndPagedByTenant(input, CurrentTenant.Id);

        
        TotalItem = (int)RolesDto.TotalCount;
        CreateTable(RolesDto);

    }
    

    private void CreateTable(PagedResultDto<IdentityRoleDto> rolesDto)
    {
        // Initialize table data to be displayed, setting the table's name and whether to show the header
        var tableData = new TableData()
        {
            Name = "Roles",
            ShowHeader = true
        };


        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Id"], Name = "Id", IsFilterable = false, PropertyName = "Id", IsSortable = false, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Name"], Name = "Name", IsFilterable = false, PropertyName = "Name", IsSortable = false, TypeFilter = TypeFilter.Text });

        foreach (var item in rolesDto.Items)
        {
            var rowDef = new RowDef();
            rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Name"], item.Name));
   
            tableData.Rows.Add(rowDef);
        }
        itemData = tableData;
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
        await DialogService.ShowAsync<UpdateRoleDialog>("Simple Dialog" , parameters);
    }

    public override bool Equals(object obj)
    {
        return obj is Roles roles &&
               EqualityComparer<PermissionRolesManagementModal>.Default.Equals(PermissionRolesManagementModal, roles.PermissionRolesManagementModal);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(PermissionRolesManagementModal);
    }
}
