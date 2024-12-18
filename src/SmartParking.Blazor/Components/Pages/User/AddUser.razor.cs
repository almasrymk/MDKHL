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
        var (orgId, tenantId) = await GetOrganizationDetailsAsync();
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
            orgId = org.Id;
            tenantId = org.TenantId;
        }

        return (orgId, tenantId);
    }

    private async Task AddUserAsync(LocalizedString successMessage, LocalizedString errorMessage, LocalizedString validationErrorMessage)
    {
        try
        {
            CheckUser();
            var user = await UsersAppService.CreateWithRoleAsync(userCreate,role.Id);

            ShowSnackbar($"{successMessage}", Severity.Success);
            await Task.Delay(1000); // Awaiting the delay to ensure it completes before navigation
            NavigationManager.NavigateTo("/users", true);
        }
        catch (AbpValidationException validationException)
        {
            foreach (var validationError in validationException.ValidationErrors)
            {
                ShowSnackbar($"{validationErrorMessage}: {validationError.MemberNames.First()} - {validationError.ErrorMessage}", Severity.Error);
            }
        }
        catch (BusinessException ex)
        {
            ShowSnackbar($"{errorMessage}: {ex.Code}", Severity.Error);
        }
        catch (AuthenticationException ex)
        {
            ShowSnackbar($"{errorMessage}: {ex.Message}", Severity.Error);
        }
        catch (Exception ex)
        {
            ShowSnackbar($"{errorMessage}: {ex.Message}", Severity.Error);
        }
    }

    private void CheckUser()
    {
        userCreate.Password = "P@ssw0rd";
        userCreate.UserName = userCreate.PhoneNumber;

        switch (role)
        {
            case var role when role.Name == RolesType.Organization.ToString():
                userCreate.UserType = 1;
                break;
            case var role when role.Name == RolesType.Keeper.ToString():
                userCreate.UserType = 4;
                break;
        }
    }


    /// <summary>
    /// Displays a snackbar notification with the specified message and severity.
    /// </summary>
    /// <param name="message">The message to display in the snackbar.</param>
    /// <param name="severity">The severity level of the snackbar (e.g., Success, Error).</param>
    void ShowSnackbar(string message, Severity severity)
    {
        Snackbar.Clear();
        Snackbar.Configuration.PositionClass = Defaults.Classes.Position.TopRight;
        Snackbar.Add(message, severity, c => c.SnackbarVariant = Variant.Filled);
    }




}
