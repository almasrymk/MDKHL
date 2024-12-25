using MudBlazor;
using SmartParking.Organizations;
using SmartParking.OrganizationsGarages;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Threading.Tasks;
using System;
using Volo.Abp;
using SmartParking.Authentication;
using Volo.Abp.Application.Dtos;
using System.Linq;
using Volo.Abp.Validation;
using SmartParking.Enums;
using SmartParking.Entities.Organizations;
using Microsoft.Extensions.Localization;
using Volo.Abp.Identity;
using SmartParking.Blazor.Components.Pages.Organizations;
using FluentValidation;
using SmartParking.Blazor.Components.DataGrid;
using Volo.Abp.Users;
using Volo.Abp.MultiTenancy;

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
            Snackbar.Add("That user is Already exists , Click edit button to update", MudBlazor.Severity.Normal, config =>
            {
                config.Action = "Edit";
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
        
        var successMessage = L["User CreatedSuccessfully"];
        var errorMessage = L["Can'tCreateUser "];
        var validationErrorMessage = L["PleaseFixTheValidationErrors"];

        await AddUserAsync(successMessage, errorMessage, validationErrorMessage);
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

    private async Task AddUserAsync(LocalizedString successMessage, LocalizedString errorMessage, LocalizedString validationErrorMessage)
    {
        try
        {
            await GetOrganizationDetailsAsync();
            CheckUser();
            var user = await UsersAppService.CreateWithRoleAsync(userCreate,role.Id);

            ShowSnackbar($"{successMessage}", MudBlazor.Severity.Success);
            await Task.Delay(1000); // Awaiting the delay to ensure it completes before navigation
            NavigationManager.NavigateTo("/users", true);
        }
        catch (AbpValidationException validationException)
        {
            foreach (var validationError in validationException.ValidationErrors)
            {
                ShowSnackbar($"{validationErrorMessage}: {validationError.MemberNames.First()} - {validationError.ErrorMessage}", MudBlazor.Severity.Error);
            }
        }
        catch (BusinessException ex)
        {
            ShowSnackbar($"{errorMessage}: {ex.Code}", MudBlazor.Severity.Error);
        }
        catch (AuthenticationException ex)
        {
            ShowSnackbar($"{errorMessage}: {ex.Message}", MudBlazor.Severity.Error);
        }
        catch (Exception ex)
        {
            ShowSnackbar($"{errorMessage}: {ex.Message}", MudBlazor.Severity.Error);
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


    /// <summary>
    /// Displays a snackbar notification with the specified message and severity.
    /// </summary>
    /// <param name="message">The message to display in the snackbar.</param>
    /// <param name="severity">The severity level of the snackbar (e.g., Success, Error).</param>
    void ShowSnackbar(string message, MudBlazor.Severity severity)
    {
        Snackbar.Clear();
        Snackbar.Configuration.PositionClass = Defaults.Classes.Position.TopRight;
        Snackbar.Add(message, severity, c => c.SnackbarVariant = Variant.Filled);
    }




}
