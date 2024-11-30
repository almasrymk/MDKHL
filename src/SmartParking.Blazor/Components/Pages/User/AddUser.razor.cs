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
    private TimeSpan? _time = new TimeSpan();
    private List<OrganizationDto> Organizations = new List<OrganizationDto>();
    private string userRole { get; set; }
    private string[] Roles = new string[1];
    private MudForm form;


    TimeEditMode editMode = new TimeEditMode();

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
        int orgId = 0;
        Guid? TenantId = null;
        if (currrentTenant.Id is not null)
        {
            var org = await OrganizationAppService.GetOrganizationForCurrentTenat();
            orgId = org.Id;
            TenantId = org.TenantId;
        }
        if(userRole == RolesType.Organization.ToString())
        {
            var org = await OrganizationAppService.GetAsync(userCreate.OrganizationId);
            orgId = org.Id;
            TenantId = org.TenantId;
        }
        if (userRole == RolesType.Keeper.ToString())
        {
            var org = await OrganizationAppService.GetAsync(userCreate.OrganizationId);
            orgId = org.Id;
            TenantId = org.TenantId;
        }
        var successedMessage = L["UserCreatedSuccessfully"];
        var errorMessage = L["Can'tCreateUser"];
        var validationErrorMessage = L["PleaseFixTheValidationErrors"];
        await AddUserAsync(orgId, TenantId, successedMessage, errorMessage, validationErrorMessage);
    }

    private async Task AddUserAsync(int orgId, Guid? TenantId, LocalizedString successedMessage, LocalizedString errorMessage, LocalizedString validationErrorMessage)
    {
        try
        {
            CheckUser(orgId, TenantId);

            var user = await UsersAppService.CreateAsync(userCreate);
            if (user != null)
            {
                ShowSnackbar($"{successedMessage}", Severity.Success);
                Task.Delay(1000);
                NavigationManager.NavigateTo("/users", true);
            }
            else
            {
                ShowSnackbar($"{errorMessage}", Severity.Error);


            }
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

            ShowSnackbar($"{errorMessage}: {ex.Code} + ", Severity.Error);
        }
        catch (AuthenticationException ex)
        {
            ShowSnackbar($"{errorMessage} : {ex.Message} + ", Severity.Error);
        }
        catch (Exception ex)
        {

            ShowSnackbar($"{errorMessage} : {ex.Message} + ", Severity.Error);
        }
    }

    private void CheckUser(int orgId, Guid? TenantId)
    {
        if (userRole == RolesType.Mdkhl.ToString())
        {
            userCreate.TenantId = TenantId;
            userCreate.UserType = 0;
        }
        else if (userRole == RolesType.Organization.ToString() )
        {
            userCreate.OrganizationId = orgId;
            userCreate.TenantId = TenantId;
            userCreate.UserType = 1;
            userRole = RolesType.Organization.ToString();
        }
        else if(userRole == RolesType.Keeper.ToString())
        {
            userCreate.OrganizationId = orgId;
            userCreate.TenantId = TenantId;
            userCreate.UserType = 4;
            userRole = RolesType.Keeper.ToString();
        }

        userCreate.Password = "P@ssw0rd";
        Roles[0] = userRole;
        userCreate.RoleNames = Roles;
        userCreate.UserName = userCreate.PhoneNumber;
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
