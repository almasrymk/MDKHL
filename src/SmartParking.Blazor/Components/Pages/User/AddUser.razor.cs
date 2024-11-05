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
        Organization org = new Organization();
        if(currrentUser.IsInRole(Enums.RolesType.Organization.ToString()) || currrentUser.IsInRole(Enums.RolesType.OrganizationEmployee.ToString()))
        {

           orgId= await OrganizationAppService.GetOrganizationIdByTenant(currrentTenant.Id);
           org = await OrganizationAppService.GetOrganizationForCurrentTenat();
            

        }
        else
        {
           var result = await OrganizationAppService.GetAsync(userCreate.OrganizationId);
            orgId= result.Id;

        }

        var successedMessage = L["UserCreatedSuccessfully"];
        var errorMessage = L["Can'tCreateUser"];
        var validationErrorMessage = L["PleaseFixTheValidationErrors"];
        try
        {
            if (currrentUser.IsInRole(Enums.RolesType.Mdkhl.ToString()) || currrentUser.IsInRole(Enums.RolesType.MdkhlEmployee.ToString())|| currrentUser.IsInRole("admin"))
            {
                if (userRole == RolesType.Mdkhl.ToString() || userRole == RolesType.MdkhlEmployee.ToString())
                {
                    userCreate.TenantId = null;
                    userCreate.UserType = 1;
                }
                else if (userRole == RolesType.Organization.ToString() || userRole == RolesType.OrganizationEmployee.ToString() || userRole == RolesType.Keeper.ToString())
                {
                    userCreate.OrganizationId=orgId;
                    userCreate.TenantId = org.TenantId;
                    userCreate.UserType = 2;
                }

            }
            else if (currrentUser.IsInRole(Enums.RolesType.Organization.ToString()) || currrentUser.IsInRole(Enums.RolesType.OrganizationEmployee.ToString())){

                if (userRole == RolesType.Organization.ToString() || userRole == RolesType.OrganizationEmployee.ToString() || userRole == RolesType.Keeper.ToString())
                {
                    userCreate.OrganizationId = orgId;
                    userCreate.TenantId = org.TenantId;
                    userCreate.UserType = 2;
                }
            }

            userCreate.Password = "P@ssw0rd";
            Roles[0] = userRole;
            userCreate.RoleNames=Roles;
            userCreate.UserName = userCreate.PhoneNumber;


            var user = await UsersAppService.CreateAsync(userCreate);
            if (user != null)
            {
                ShowSnackbar($"{successedMessage}", Severity.Success);
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
