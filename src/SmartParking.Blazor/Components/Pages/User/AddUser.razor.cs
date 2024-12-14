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

namespace SmartParking.Blazor.Components.Pages.User;

public partial class AddUser
{
    public UserCreateDto userCreate { get; set; }
    private List<OrganizationDto> Organizations = new List<OrganizationDto>();
    private string userRole { get; set; }
    private string[] Roles = new string[1];
    private MudForm form;

    protected override async Task OnInitializedAsync()
    {
        userCreate = new UserCreateDto();
        if (currrentUser.IsInRole(Enums.RolesType.Mdkhl.ToString()) || currrentUser.IsInRole("admin"))
        {

             Organizations = await OrganizationAppService.GetAllMobileAsync();
           
        }
    }
    private async void SubmitCreate()
    {
        var (orgId, tenantId) = await GetOrganizationDetailsAsync();
        var successMessage = L["User CreatedSuccessfully"];
        var errorMessage = L["Can'tCreateUser "];
        var validationErrorMessage = L["PleaseFixTheValidationErrors"];

        await AddUserAsync(orgId, tenantId, successMessage, errorMessage, validationErrorMessage);
    }

    private async Task<(int orgId, Guid? tenantId)> GetOrganizationDetailsAsync()
    {
        int orgId = 0;
        Guid? tenantId = null;

        if (currrentTenant.Id is not null)
        {
            var org = await OrganizationAppService.GetOrganizationForCurrentTenat();
            orgId = org.Id;
            tenantId = org.TenantId;
        }
        else if (userRole == RolesType.Organization.ToString() || userRole == RolesType.Keeper.ToString())
        {
            var org = await OrganizationAppService.GetAsync(userCreate.OrganizationId);
            orgId = org.Id;
            tenantId = org.TenantId;
        }

        return (orgId, tenantId);
    }

    private async Task AddUserAsync(int orgId, Guid? tenantId, LocalizedString successMessage, LocalizedString errorMessage, LocalizedString validationErrorMessage)
    {
        try
        {
            CheckUser(orgId, tenantId);
            var role = await UsersAppService.FindRoleByTenantIdAsync(userRole, tenantId);
            var user = await UsersAppService.CreateWithRoleAsync(userCreate,role);

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

    private void CheckUser(int orgId, Guid? tenantId)
    {
        userCreate.Password = "P@ssw0rd";
        userCreate.UserName = userCreate.PhoneNumber;

        switch (userRole)
        {
            case var role when role == RolesType.Mdkhl.ToString():
                userCreate.TenantId = tenantId;
                userCreate.UserType = 0;
                break;
            case var role when role == RolesType.Organization.ToString():
                userCreate.OrganizationId = orgId;
                userCreate.TenantId = tenantId;
                userCreate.UserType = 1;
                break;
            case var role when role == RolesType.Keeper.ToString():
                userCreate.OrganizationId = orgId;
                userCreate.TenantId = tenantId;
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
