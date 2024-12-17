using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Organizations;
using System.Threading.Tasks;
using System;
using Volo.Abp;
using Volo.Abp.Identity;
using Volo.Abp.Validation;
using System.Linq;
using SmartParking.Authentication;

namespace SmartParking.Blazor.Components.Pages.Roles;

public partial class UpdateRoleDialog
{
    private MudForm form;

    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; }
    bool _clearable;
    [Parameter] public IdentityRoleDto RoleDto { get; set; }

    private async Task Submit()
    {
        var successedMessage = L["EditRoleEditedSuccessfully"];
        var errorMessage = L["Can'tEditRole"];
        var validationErrorMessage = L["PleaseFixTheValidationErrors"];


        // Call your service to edit the ContactData
        try
        {
            IdentityRoleUpdateDto updateRoleDto = new IdentityRoleUpdateDto()
            {
                Name = RoleDto.Name,
            };

            var contactdata = await RoleAppService.Update(RoleDto.Id, updateRoleDto);
            if (contactdata != null)
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
            Snackbar.Add($"Failed to add ContactData: {ex.Code} + ", Severity.Error);

        }
        catch (Exception ex)
        {
            Snackbar.Add($"Failed to add ContactData: {ex.Message} + ", Severity.Error);
        }
    }
    protected override Task OnInitializedAsync()
    {
        return base.OnInitializedAsync();

    }
    private void Cancel()
    {
        MudDialog.Cancel();
    }
}
