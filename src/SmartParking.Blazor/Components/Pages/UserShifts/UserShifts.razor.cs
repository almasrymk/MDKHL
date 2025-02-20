using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Authentication;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Pages.User;
using SmartParking.Entities;
using SmartParking.Localization;
using System.Globalization;
using System.Security.Authentication;
using Volo.Abp.Identity;
using Volo.Abp.Users;
using Volo.Abp;
using Volo.Abp.Application.Dtos;

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

    public bool ViewAdd { get; set; } = true;
    protected bool HasManagePermissionsPermission { get; set; } = true;
    protected const string PermissionProviderName = "U";

    private string termSearch;
    private readonly IStringLocalizer<SmartParkingResource> _localizer;
    private readonly ICurrentUser _currentUser;
    protected string ManagePermissionsPolicyName;

    public UserShifts(IStringLocalizer<SmartParkingResource> localizer, ICurrentUser currentUser)
    {
        _localizer = localizer;
        _currentUser = currentUser;
        //CreatePolicyName = IdentityPermissions.Users.Create;
        //UpdatePolicyName = IdentityPermissions.Users.Update;
        //DeletePolicyName = IdentityPermissions.Users.Delete;
        ManagePermissionsPolicyName = IdentityPermissions.Users.ManagePermissions;

    }

    protected async override Task OnInitializedAsync()
    {
        //if (!await AuthorizationService.IsGrantedAsync(IdentityPermissions.Users.Default))
        //{
        //    Navigation.NavigateTo("/Account/AccessDenied", true);
        //}
        await CreateMenue();
        await base.OnInitializedAsync();
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
    private Task AssignShift()
    {

    }
    public async Task HandleSelectTenantAsync(string selectedValue, Guid? tenantId)
    {
        TenantId = tenantId;
        ValueOfMenue = selectedValue;
        if (sortColumn == "Id")
            sortColumn = "User.Id";
        var request = new PagedAndSortedAndSearchResultRequestDto
        {
            MaxResultCount = RowPerPage,
            SkipCount = (CurrentPage - 1) * RowPerPage,
            Sorting = sortColumn,
            SearchingTerm = termSearch
        };

        PagedResultDto<UserWithRole> userDto = await KeeperShiftsAppService.GetListUserCanAssignShiftsMdkhlAsync(TenantId, request);

        TotalItem = (int)userDto.TotalCount;
        await CreateTable(userDto.Items);
    }
    private async Task FetchTableData(PageChangeEventArgs pageInfo)
    {
        termSearch = pageInfo.SearchTerm;
        CurrentPage = pageInfo.PageNumber;
        RowPerPage = pageInfo.RowsPerPage;
        sortColumn = pageInfo.SortColumn;
        var input = new PagedAndSortedAndSearchResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn,
            SearchingTerm = pageInfo.SearchTerm
        };
        if (pageInfo.SortColumn == "Id")
            input.Sorting = "User.Id";

        PagedResultDto<UserWithRole> usersDto;
        if (currrentTenant.Id is not null)
            usersDto = await KeeperShiftsAppService.GetListUserCanAssignShiftsOrganizationAsync(input);
        else
            usersDto = await KeeperShiftsAppService.GetListUserCanAssignShiftsMdkhlAsync(null,input);
        TotalItem = (int)usersDto.TotalCount;
        await CreateTable(usersDto.Items);

    }

    private void HandleAdd()
    {
        Navigation.NavigateTo("/add-user", true);
    }

    private async Task CreateTable(IReadOnlyList<UserWithRole> users)
    {
        
        var tableData = new TableData()
        {
            Name = L["UsersCanAssignShifts"],
            ShowHeader = true
        };

        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Id"], Name = "Id", IsFilterable = false, PropertyName = "User.Id", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["UserName"], Name = "UserName", IsFilterable = true, PropertyName = "User.UserName", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["UserFullName"], Name = "Name", IsFilterable = true, PropertyName = "User.Name", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Email"], Name = "Email", IsFilterable = true, PropertyName = "User.Email", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["PhoneNumber"], Name = "PhoneNumber", IsFilterable = true, PropertyName = "User.PhoneNumber", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["UserRole"], Name = "UserRole", IsFilterable = false, PropertyName = "Role.Name", IsSortable = true, TypeFilter = TypeFilter.Text });
        switch (currrentTenant?.Id is null, CultureInfo.CurrentCulture.Name)
        {
            case (true, "ar"):
                tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["OrganizationName"], Name = "Organization Name", IsFilterable = false, PropertyName = "OrganizationEnglishName", IsSortable = true, TypeFilter = TypeFilter.Text });
                break;

            case (true, "en"):
                tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["OrganizationName"], Name = "Organization Name", IsFilterable = false, PropertyName = "OrganizationArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });
                break;
        }


        foreach (var item in users)
        {
            var rowDef = new RowDef();
            rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.User.Id), item.User.Id.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["UserName"], item.User.UserName));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["UserFullName"], item.User.Name));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Email"], item.User.Email));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["PhoneNumber"], item.User.PhoneNumber));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["UserRole"], item.Role?.Name));

            switch (currrentTenant?.Id is null, CultureInfo.CurrentCulture.Name)
            {
                case (true, "ar"):
                    rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["OrganizationName"], item.OrganizationArabicName));
                    break;

                case (true, "en"):
                    rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["OrganizationName"], item.OrganizationEnglishName));
                    break;
            }




            tableData.Rows.Add(rowDef);
        }
        itemData = tableData;
        await Task.Yield();
    }

    //protected override async Task SetPermissionsAsync()
    //{
    //    await base.SetPermissionsAsync();
    //    HasManagePermissionsPermission =
    //        await AuthorizationService.IsGrantedAsync(IdentityPermissions.Users.Default);
    //}

    //public async Task AddPermission(RowDef row)
    //{
    //    var user = await UsersAppService.GetAsync(Guid.Parse(row.Cells[0].Value));
    //    await PermissionManagementModal.OpenAsync(PermissionProviderName,
    //                        row.Cells[0].Value,
    //                        user.TenantId,
    //                        row.Cells[1].Value);
    //}
   
    public async  Task AssignShift(RowDef row)
    {
        var userId = Guid.Parse(row.Cells[0].Value);
        var userData = await UsersAppService.GetAsync(userId);
        var userUpdate = new UserDto
        {
            Name = userData.Name,
            Email = userData.Email,
            Surname = userData.Surname,
            PhoneNumber = userData.PhoneNumber,
            UserName = userData.UserName,
            TenantId = userData.TenantId,
            RoleId = userData.RoleId,
            ConcurrencyStamp = userData.ConcurrencyStamp,
            IsActive = userData.IsActive,
            IdentificationTypes = userData.IdentificationTypes,
            NationalId = userData.NationalId,
        };
        var parameters = new DialogParameters
        {
            ["userDto"] = userUpdate,
            ["userId"] = userId.ToString()
        };

    //    return await DialogService.ShowAsync<ViewUserDialog>("User View", parameters);
    //}

    //private async Task OnClickRowDelete(RowDef row)
    //{
    //    var successMessage = _localizer["UserDeletedSuccessfully"];

    //    var id = Guid.Parse(row.Cells[0].Value);
    //    var Name = row.Cells[2].Value;
    //    var userData = await UsersAppService.GetAsync(id);
    //    if (row.Cells[5].Value == null)
    //        return;
    //    var roleData = currrentTenant.Id == null ?
    //        await RoleAppService.GetRoleByNameMdkhlAsync((row.Cells[5].Value).ToString())
    //        : await RoleAppService.GetRoleByNameAsync((row.Cells[5].Value).ToString());
    //    var userUpdate = new UpdateUserDto
    //    {
    //        Name = userData.Name,
    //        Email = userData.Email,
    //        Surname = userData.Surname,
    //        PhoneNumber = userData.PhoneNumber,
    //        UserName = userData.UserName,
    //        TenantId = userData.TenantId,
    //        RoleId = roleData.Id,
    //        ConcurrencyStamp = userData.ConcurrencyStamp,
    //        IsActive = userData.IsActive,
    //        NationalId = userData.NationalId,
    //        IdentificationTypes = userData.IdentificationTypes,
    //    };
    //    var parameters = new DialogParameters
    //    {
    //        ["Name"] = Name,

    //    };

    //    var dialog = await DialogService.ShowAsync<SPDialogDelete>("Delete user", parameters);
    //    var result = await dialog.Result;
    //    try
    //    {
    //        if (!result.Canceled)
    //        {

    //            if (currrentTenant.Id != null)
    //            {
    //                await UsersAppService.UpdateUserWithNoRoleAsync(id, userUpdate);
    //            }
    //            else
    //            {
    //                await UsersAppService.RemoveRoleMdkhl(id, userUpdate.RoleId);
    //            }

    //            SnackBar.ShowSnackbar(Snackbar, LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.SUCCESS) + " " +
    //                LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.DELETE) + " " +
    //                LocalizationHelper.GetLocalizedString("User") + " " + userData.Name, Severity.Success);
    //            await Task.Delay(1000);

    //            Navigation.NavigateTo("/users", forceLoad: true);

    //        }


    //    }

    //    catch (BusinessException ex)
    //    {

    //        SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.ADD]} {L[AlertMessages.AlertMessages.FAILED]}: {ex.Code}", Severity.Error);
    //    }
    //    catch (AuthenticationException ex)
    //    {
    //        SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.ADD]} {L[AlertMessages.AlertMessages.FAILED]} : {ex.Message} ", Severity.Error);
    //    }
    //    catch (Exception ex)
    //    {

    //        SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.DELETE]}: {ex.Message} ", Severity.Error);
    //    }


    //}
}
