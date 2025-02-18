using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Authentication;
using System.Threading.Tasks;
using System;
using Volo.Abp;
using Volo.Abp.Validation;
using System.Collections.Generic;
using Volo.Abp.Identity;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Localization;
using System.Linq;
using System.Security.Authentication;

namespace SmartParking.Blazor.Components.Pages.User;

partial class EditUserDialog
{
    [Parameter]
    public string userId { get; set; }
    [Parameter]
    public UpdateUserDto updateUserDto { get; set; }
    private List<IdentityRoleDto> Roles = new List<IdentityRoleDto>();
    public string UserRole { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Roles = await RoleAppService.GetListByTenant(updateUserDto.TenantId);
        UserRole = await GetRoleNameAsync(updateUserDto.RoleId);
        await base.OnInitializedAsync();

    }
    private async Task<string> GetRoleNameAsync(Guid roleId)
    {
        var identityRoleDto = await RoleAppService.GetIdentityRoleById(updateUserDto.RoleId);
        if (identityRoleDto == null) { 
            return null;
        }
        return identityRoleDto.Name;

    }
    private void ChangeRole(string roleId)
    {
        updateUserDto.RoleId = Guid.Parse(roleId);
        UserRole = Roles.Find(x => x.Id == updateUserDto.RoleId).Name;
    }
    public async Task SubmitEdit()
        {
            
            try
            {
                UserDto userUpdated = new UserDto();
               
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
                LocalizationHelper.GetLocalizedString("User") +" " +userUpdated.Name, Severity.Success);
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

            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.UPDATE]} : {ex.Code}", Severity.Error);
        }
        catch (AuthenticationException ex)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.UPDATE]} : {ex.Message} ", Severity.Error);
        }
        catch (Exception ex)
        {

            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.UPDATE]} : {ex.Message} ", Severity.Error);
        }
    }


        public void CancelEdit()
        {
            Navigation.NavigateTo("/users", true);

        }
    



}
