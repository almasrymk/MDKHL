using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Localization;

namespace SmartParking.Blazor.Components.Pages.User;

public partial class AssignationDialog
{
    private MudForm form;
    [CascadingParameter]
    private IMudDialogInstance MudDialogAdd { get; set; }
    [Parameter]
    public Guid Id { get; set; }
    private Guid userRoleId { get; set; }
    private List<Volo.Abp.Identity.IdentityRoleDto> identityRoles { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        identityRoles = await RolesAppService.GetListByTenant(currentTenant.Id);
        await base.OnInitializedAsync();
    }


    private async Task SubmitAssign()
    {

        try
        {

            await RolesAppService.AssignRole(Id, userRoleId);
            SnackBar.ShowSnackbar(Snackbar, LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.SUCCESS) + " " +
                LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.ADD) + " " +
                LocalizationHelper.GetLocalizedString("Role"), Severity.Success);

            MudDialogAdd.Close(DialogResult.Ok(true));
            Navigation.NavigateTo("/users");

        }
        catch (Exception ex)
        {

            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.ADD]} {L[AlertMessages.AlertMessages.FAILED]}: {ex.Message} ", Severity.Error);
        }
    }

    private async Task CancelEdit()
    {
        MudDialogAdd.Cancel();
    }
    private async Task ChooseRole(string Value)
    {
        userRoleId = Guid.Parse(Value);
    }


}
