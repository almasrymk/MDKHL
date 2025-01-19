using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Authentication;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Blazor.Components.SPComponents;
using SmartParking.Entities;
using SmartParking.Localization;
using System.Globalization;
using System.Security.Authentication;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace SmartParking.Blazor.Components.Pages.User;

public partial class User
{
    protected PermissionManagementModal PermissionManagementModal;
    private int TotalItem;
    public int CurrentPage { get; set; } = 1; // Default current page value is 10
    public int RowPerPage { get; set; } = 10; // Default value is 10

    private TableData itemData = new TableData();

    public bool ViewAdd { get; set; } = true;
    protected bool HasManagePermissionsPermission { get; set; } = true;
    protected const string PermissionProviderName = "U";

    private string termSearch;
    private readonly IStringLocalizer<SmartParkingResource> _localizer;
    private readonly ICurrentUser _currentUser;
    protected string ManagePermissionsPolicyName;

    public User(IStringLocalizer<SmartParkingResource> localizer, ICurrentUser currentUser)
    {
        _localizer = localizer;
        _currentUser = currentUser;
        CreatePolicyName = IdentityPermissions.Users.Create;
        UpdatePolicyName = IdentityPermissions.Users.Update;
        DeletePolicyName = IdentityPermissions.Users.Delete;
        ManagePermissionsPolicyName = IdentityPermissions.Users.ManagePermissions;

    }

    protected override Task OnInitializedAsync()
    {

        return base.OnInitializedAsync();
    }
    /// <summary>
    /// filter for users on the server side using the user columns.
    /// </summary>
    /// <param name="columnFilter">The name of column to filter for among user.</param>
    /// <returns>Returns a task that represents the asynchronous operation.</returns>
    private async Task HandleFilter(PageChangeEventArgs pageInfo)
    {
        //var input = new PagedAndSortedAndFilteredResultRequestDto
        //{
        //    MaxResultCount = pageInfo.RowsPerPage,
        //    SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
        //    Sorting = pageInfo.SortColumn,
        //    SearchingTerm = termSearch, // The term to use for filtering results
        //    filters = pageInfo.Filters
        //};
        //PagedResultDto<UserDto> usersDto;
        //if (currrentTenant.Id is not null)
        //    usersDto = await UsersAppService.FilterForOrganizationAsync(input);
        //else
        //    usersDto = await UsersAppService.Filter(input);

        //TotalItem = (int)usersDto.TotalCount;
        //CreateTable(usersDto);


    }
    /// <summary>
    /// Searches for users on the server side using the user code search term.
    /// </summary>
    /// <param name="searchTerm">The term to search for among user.</param>
    /// <returns>Returns a task that represents the asynchronous operation.</returns>
    private async Task HandleSearch(PageChangeEventArgs pageInfo)
    {

        // Update the term used for searching with the provided search term
        termSearch = pageInfo.SearchTerm;

        // Create a new search input object with pagination, sorting, and search term parameters
        var input = new PagedAndSortedAndSearchResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn == "Id" ? "User.Id" : pageInfo.SortColumn,
            SearchingTerm = termSearch // The term to use for filtering results
        };

        PagedResultDto<UserWithRole> usersDto;
        if (currrentTenant.Id is not null)
            usersDto = await UsersAppService.SearchForOrganizationAsync(input);
        else
            usersDto = await UsersAppService.Search(input);

        TotalItem = (int)usersDto.TotalCount;
        CreateTable(usersDto.Items);

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
        // Create a request object for paged and sorted data
        var input = new PagedAndSortedResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn
        };
        if (pageInfo.SortColumn == "Id")
            input.Sorting = "User.Id";
        // Retrieve the list of organizations from the server
        PagedResultDto<UserWithRole> usersDto;
        if (currrentTenant.Id is not null)
            usersDto = await UsersAppService.GetListForOrganizationAsync(input);
        else
            usersDto = await UsersAppService.GetListMdklAsync(input);
        TotalItem = (int)usersDto.TotalCount;
        CreateTable(usersDto.Items);

    }

    private void HandleAdd()
    {
        Navigation.NavigateTo("/add-user", true);
    }

    private void CreateTable(IReadOnlyList<UserWithRole> users)
    {
        // Initialize table data to be displayed, setting the table's name and whether to show the header
        var tableData = new TableData()
        {
            Name = "Users",
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
    }

    protected override async Task SetPermissionsAsync()
    {
        await base.SetPermissionsAsync();
        HasManagePermissionsPermission =
            await AuthorizationService.IsGrantedAsync(IdentityPermissions.Users.Default);
    }

    public async Task AddPermission(RowDef row)
    {
        var user = await UsersAppService.GetAsync(Guid.Parse(row.Cells[0].Value));
        await PermissionManagementModal.OpenAsync(PermissionProviderName,
                            row.Cells[0].Value,
                            user.TenantId,
                            row.Cells[1].Value);
    }
    public async Task<IDialogReference> HandleEdit(RowDef row)
    {
        var userId = Guid.Parse(row.Cells[0].Value);
        var userData = await UsersAppService.GetAsync(userId);
        var userUpdate = new UpdateUserDto
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
            NationalId = userData.NationalId,
            IdentificationTypes = userData.IdentificationTypes,
            IsCustomer = userData.IsCustomer
        };
        var parameters = new DialogParameters
        {
            ["updateUserDto"] = userUpdate,
            ["userId"] = userId.ToString()
        };
        return currrentTenant.Id switch
        {
            (not null) => await DialogService.ShowAsync<EditUserDialog>("User Edit", parameters),
            (null) => await DialogService.ShowAsync<MDKHLEditUserDialog>("User Edit", parameters),
        };
    }
    public async Task<IDialogReference> HandleView(RowDef row)
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

        return await DialogService.ShowAsync<ViewUserDialog>("User View", parameters);
    }

    private async Task OnClickRowDelete(RowDef row)
    {
        var successMessage = _localizer["UserDeletedSuccessfully"];

        var id = Guid.Parse(row.Cells[0].Value);
        var Name = row.Cells[2].Value;
        var userData = await UsersAppService.GetAsync(id);
        if (row.Cells[5].Value == null)
            return;
        var roleData = currrentTenant.Id == null ?
            await RoleAppService.GetRoleByNameMdkhlAsync((row.Cells[5].Value).ToString())
            : await RoleAppService.GetRoleByNameAsync((row.Cells[5].Value).ToString());
        var userUpdate = new UpdateUserDto
        {
            Name = userData.Name,
            Email = userData.Email,
            Surname = userData.Surname,
            PhoneNumber = userData.PhoneNumber,
            UserName = userData.UserName,
            TenantId = userData.TenantId,
            RoleId = roleData.Id,
            ConcurrencyStamp = userData.ConcurrencyStamp,
            IsActive = userData.IsActive,
            NationalId = userData.NationalId,
            IdentificationTypes = userData.IdentificationTypes,
        };
        var parameters = new DialogParameters
        {
            ["Name"] = Name,

        };

        var dialog = await DialogService.ShowAsync<SPDialogDelete>("Delete user", parameters);
        var result = await dialog.Result;
        try
        {
            if (!result.Canceled)
            {

                if (currrentTenant.Id != null)
                {
                    await UsersAppService.UpdateUserWithNoRoleAsync(id, userUpdate);
                }
                else
                {
                    await UsersAppService.RemoveRoleMdkhl(id, userUpdate.RoleId);
                }

                SnackBar.ShowSnackbar(Snackbar, LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.SUCCESS) + " " +
                    LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.DELETE) + " " +
                    LocalizationHelper.GetLocalizedString("User") + " " + userData.Name, Severity.Success);
                await Task.Delay(1000);

                Navigation.NavigateTo("/users", forceLoad: true);

            }


        }

        catch (BusinessException ex)
        {

            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.ADD]} {L[AlertMessages.AlertMessages.FAILED]}: {ex.Code}", Severity.Error);
        }
        catch (AuthenticationException ex)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.ADD]} {L[AlertMessages.AlertMessages.FAILED]} : {ex.Message} ", Severity.Error);
        }
        catch (Exception ex)
        {

            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.DELETE]}: {ex.Message} ", Severity.Error);
        }


    }

}
