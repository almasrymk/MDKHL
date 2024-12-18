using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Organizations;
using System.Threading.Tasks;
using System;
using Volo.Abp;
using Volo.Abp.Identity;
using Volo.Abp.Validation;
using System.Linq;
using System.Collections.Generic;

namespace SmartParking.Blazor.Components.Pages.Roles;

partial class AddRoleDialog
{
    private MudForm form;
    public IdentityRoleCreateDto input = new IdentityRoleCreateDto();

    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; }

    private List<OrganizationDto> organizationList = new List<OrganizationDto>();
    bool _clearable;

    bool _addForMdkhl = true;

    private OrganizationDto _organization { get; set; }

    private Guid? _organizationTenant { get; set; }
    protected async override  Task OnInitializedAsync()
    {
        if(CurrentTenant.Id is null)
        {
            var result =await OrganizationAppService.GetAllMobileAsync();
            organizationList=result.Value.ToList();
        }
        else{
            var org =await OrganizationAppService.GetOrganizationForCurrentTenat();
            _organizationTenant = org.TenantId;
        }
    }
    private void OrganizationValueChanged(int value)
    {
        _organization = organizationList.FirstOrDefault(x => x.Id == value);
    }

    private async Task Submit()
    {
        var successedMessage = L["RoleCreatedSuccessfully"];
        var errorMessage = L["Can'tCreateRole"];
        var validationErrorMessage = L["PleaseFixTheValidationErrors"];


        try
        {
            IdentityRoleCreateDto createIdentityDto = new IdentityRoleCreateDto()
            {
                Name = input.Name,
            };
            if(CurrentTenant.Id is null)
            {
              _organizationTenant = _organization?.TenantId ?? null;

            }

            var roleDto = await RoleAppService.Create(createIdentityDto,_organizationTenant);
            if (roleDto != null)
            {
                Snackbar.Add(successedMessage, Severity.Success);
                MudDialog.Close(DialogResult.Ok(true));
                await Task.Delay(1000);
                Navigation.NavigateTo("/roles", forceLoad: true);

            }

            else
            {
                Snackbar.Add(errorMessage, Severity.Error);

            }
        }
        catch (AbpValidationException validationException)
        {
            foreach (var validationError in validationException.ValidationErrors)
            {
                Snackbar.Add($"{validationErrorMessage} {validationError.MemberNames.First()} - {validationError.ErrorMessage}", Severity.Error);
            }
        }
        catch (BusinessException ex)
        {
            Snackbar.Add($"Failed to add Role: {ex.Code} + ", Severity.Error);

        }
        catch (Exception ex)
        {
            Snackbar.Add($"Failed to add Role: {ex.Message} + ", Severity.Error);
        }
    }
    private void Cancel()
    {
        MudDialog.Cancel();
    }


}
