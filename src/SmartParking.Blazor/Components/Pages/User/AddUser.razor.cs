using MudBlazor;
using SmartParking.Organizations;
using SmartParking.OrganizationsGarages;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Threading.Tasks;
using System;
using Volo.Abp;
using SmartParking.Authentication;
using System.Linq;
using Volo.Abp.Validation;
using SmartParking.Enums;
using Volo.Abp.Identity;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Localization;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace SmartParking.Blazor.Components.Pages.User;

public partial class AddUser
{
    public UserCreateDto userCreate { get; set; }
    private List<OrganizationDto> Organizations = new List<OrganizationDto>();
    private List<IdentityRoleDto> Roles = new List<IdentityRoleDto>();
    private MudForm form;
    private IdentityRoleDto role;
    private bool IsOrganization { get; set; }
    private bool IsSelectOrganization { get; set; } = true;
    private MudTextField<string> textField;
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await textField.FocusAsync();
        }
    }
    private async Task HandleBlur()
    {
      var user = await UsersAppService.CheckPhoneIsUnique(textField.Value);
        if (user != null) {
            Snackbar.Configuration.PositionClass = Defaults.Classes.Position.TopCenter;

            Snackbar.Add(LocalizationHelper.GetLocalizedString("User") + " " +
                LocalizationHelper.GetLocalizedString("DuplicateValidation") + " , " +
                LocalizationHelper.GetLocalizedString("MessageEdit"), MudBlazor.Severity.Normal, config =>
                    {
                        config.Action = LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.UPDATE);
                        config.ActionColor = Color.Primary;
                        config.Onclick = snackbar =>
                        {
                            HandleEdit(user);
                            return Task.CompletedTask;
                        };
                    });
        }

    }
    public async Task<IDialogReference> HandleEdit(UserDto userData)
    {
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
            IdentificationTypes = userData.IdentificationTypes
        };
        var parameters = new DialogParameters
        {
            ["updateUserDto"] = userUpdate,
            ["userId"] = userData.Id.ToString()
        };
        return await DialogService.ShowAsync<EditUserDialog>("User Edit", parameters);
    }
    protected override async Task OnInitializedAsync()
    {
        userCreate = new UserCreateDto();
        Roles = await RoleAppService.GetListByTenant(null);
       await GetOrganizationDetailsAsync();

    }
    private async Task IsMdkhlSelect(bool isOrganization)
    {
        IsOrganization = isOrganization;
        IsSelectOrganization = !isOrganization;
        if (isOrganization)
        {
            Organizations = await OrganizationAppService.GetAllMobileAsync();
        }
        Roles = await RoleAppService.GetListByTenant(null);

    }    
    private async Task SelectOrganization(int organizationId)
    {
        IsSelectOrganization = true;
        var orag = await OrganizationAppService.GetAsync(organizationId);
        userCreate.OrganizationId = organizationId;
        userCreate.TenantId = orag.TenantId;
        Roles = await RoleAppService.GetListByTenant(orag.TenantId);
    }
    private async void SubmitCreate()
    {
        

        await AddUserAsync();
    }

    private async Task<(int orgId, Guid? tenantId)> GetOrganizationDetailsAsync()
    {
        int orgId = 0;
        Guid? tenantId = null;

        if (currrentTenant.Id is not null)
        {
            var org = await OrganizationAppService.GetOrganizationForCurrentTenat();
            Roles = await RoleAppService.GetListByTenant(org.TenantId);
            userCreate.OrganizationId = org.Id;
            userCreate.TenantId = org.TenantId;
        }

        return (orgId, tenantId);
    }

    private async Task AddUserAsync()
    {
        try
        {
            await GetOrganizationDetailsAsync();
            CheckUser();
            var user = await UsersAppService.CreateWithRoleAsync(userCreate,role.Id);
            SnackBar.ShowSnackbar(Snackbar, LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.SUCCESS) + " " +
            LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.ADD) + " " +
            LocalizationHelper.GetLocalizedString("User"), Severity.Success);
            await Task.Delay(1000); // Awaiting the delay to ensure it completes before navigation
            NavigationManager.NavigateTo("/users", true);
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

    private void CheckUser()
    {
        userCreate.Password = "P@ssw0rd";
        userCreate.UserName = userCreate.PhoneNumber;

        switch (role.Name)
        {
            case var name when name == RolesType.Organization.ToString():
                userCreate.UserType = 1;
                break;

            case var name when name == RolesType.Keeper.ToString():
                userCreate.UserType = 4;
                break;

            default:
                userCreate.UserType = 5;
                break;

        }
    }
    public void CancelAdd()
    {
        NavigationManager.NavigateTo("/users", true);

    }



}
