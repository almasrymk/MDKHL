using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SmartParking.Authentication;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Volo.Abp.Identity;
using Volo.Abp;
using SmartParking.Entities;
using MudBlazor;
using Volo.Abp.Validation;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Customers;
using SmartParking.Vehicles;
using System.Globalization;
using Microsoft.Extensions.Localization;
using SmartParking.Localization;
using SmartParking.Blazor.Components.SPComponents;
using static MudBlazor.CategoryTypes;
using System.Security.Authentication;
using System.Linq;
using SmartParking.Blazor.Components.Shared;

namespace SmartParking.Blazor.Components.Pages.User;

public partial class MDKHLEditUserDialog
{
    [Parameter]
    public string userId { get; set; }
    [Parameter]
    public UpdateUserDto updateUserDto { get; set; }
    private List<UserRolesForMdkhl> Roles = new List<UserRolesForMdkhl>();
    private int TotalItem;
    public int CurrentPage { get; set; } = 1; // Default current page value is 10
    public int RowPerPage { get; set; } = 10; // Default value is 10

    private TableData itemData = new TableData();
    private readonly IStringLocalizer<SmartParkingResource> _localizer;
    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; }


    public MDKHLEditUserDialog(IStringLocalizer<SmartParkingResource> localizer)
    {
        _localizer = localizer;
    }

    protected override async Task OnInitializedAsync()
    {
        Roles = await UsersAppService.GetUserWithRolesForMdkhlAsync(Guid.Parse(userId));
    }
    public async Task SubmitEdit()
    {
        var successedMessage = L["UserUpdateSuccessfullyWith"];
        try
        {
            UserDto userUpdated = new UserDto();
            //updateUserDto.RoleId = Guid.Parse(roleId);
            if (updateUserDto.ISNoRole)
            {
                userUpdated = await UsersAppService.UpdateUserWithNoRoleAsync(Guid.Parse(userId), updateUserDto);
            }
            else
            {
                userUpdated = await UsersAppService.UpdateUserWithRoleChangeAsync(Guid.Parse(userId), updateUserDto);
            }

            if (userUpdated != null)
            {

                SnackBar.ShowSnackbar(Snackbar, LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.SUCCESS) + " " +
                   LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.UPDATE) + " " +
                   LocalizationHelper.GetLocalizedString("User") + " " + userUpdated.Name, Severity.Success);
                await Task.Delay(1000);
                Navigation.NavigateTo("/users", true);



            }


        }
        catch (AbpValidationException validationException)
        {
            foreach (var validationError in validationException.ValidationErrors)
            {
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.VAlIDATION_ERROR]}: {L[validationError.MemberNames.First()]} - {validationError.ErrorMessage}", Severity.Error);
            }
        }
        catch (BusinessException ex)
        {

            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.ADD]} {L[AlertMessages.AlertMessages.FAILED]}: {ex.Message}", Severity.Error);
        }
        catch (AuthenticationException ex)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.ADD]} {L[AlertMessages.AlertMessages.FAILED]} : {ex.Message} ", Severity.Error);
        }
        catch (Exception ex)
        {

            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.ADD]} {L[AlertMessages.AlertMessages.FAILED]}: {ex.Message} ", Severity.Error);
        }

    }
    private void CreateTable(List<UserRolesForMdkhl> UserRoles)
    {
        var tableData = new TableData()
        {
            Name = "User Roles",
            ShowHeader = true
        };
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["ID"], Name = "ID", IsFilterable = false, PropertyName = "ID", IsSortable = false, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["RoleName"], Name = "RoleName", IsFilterable = false, PropertyName = "RoleName", IsSortable = false, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["OrganizationName"], Name = "OrganizationName", IsFilterable = false, PropertyName = "OrganizationName", IsSortable = false, TypeFilter = TypeFilter.Text });
        foreach (var item in UserRoles)
        {
            var rowDef = new RowDef();

            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["RoleId"], item.RoleId.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["RoleName"], item.RoleName));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["OrganizationName"], item.OrganizationName));
            tableData.Rows.Add(rowDef);
        }
        itemData = tableData;
    }


    private async Task FetchTableData(PageChangeEventArgs pageInfo)
    {
        Roles = await UsersAppService.GetUserWithRolesForMdkhlAsync(Guid.Parse(userId));
        CreateTable(Roles);
    }
    private async Task HandleDelete(RowDef row)
    {
        

        var id = Guid.Parse(row.Cells[0].Value);
        var Name = row.Cells[1].Value;

        var parameters = new DialogParameters
        {
            ["Name"] = Name,

        };

        var dialog = await Dialog.ShowAsync<SPDialogDelete>("Delete vehicle", parameters);
        var result = await dialog.Result;
        try
        {
            if (!result.Canceled)
            {
                await UsersAppService.RemoveRoleMdkhl(Guid.Parse(userId), id);
                SnackBar.ShowSnackbar(Snackbar, LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.SUCCESS) + " " +
               LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.DELETE) + " " +
               LocalizationHelper.GetLocalizedString("Role"), Severity.Success);
                await Task.Delay(1000);
                Navigation.Refresh(true);

            }


        }
        catch (AbpValidationException validationException)
        {
            foreach (var validationError in validationException.ValidationErrors)
            {
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.VAlIDATION_ERROR]}: {L[validationError.MemberNames.First()]} - {validationError.ErrorMessage}", Severity.Error);
            }
        }
        catch (BusinessException ex)
        {

            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.ADD]} {L[AlertMessages.AlertMessages.FAILED]}: {ex.Message}", Severity.Error);
        }
        catch (AuthenticationException ex)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.ADD]} {L[AlertMessages.AlertMessages.FAILED]} : {ex.Message} ", Severity.Error);
        }
        catch (Exception ex)
        {

            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.ADD]} {L[AlertMessages.AlertMessages.FAILED]}: {ex.Message} ", Severity.Error);
        }

    }
    public void CancelEdit()
    {
        Navigation.NavigateTo("/users", true);

    }




}
