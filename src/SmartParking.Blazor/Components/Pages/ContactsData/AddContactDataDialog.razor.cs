using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Organizations;
using System.Security.Authentication;
using Volo.Abp;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.ContactsData;

/// <summary>
/// Represents the dialog component for adding a new contact data entry.
/// </summary>
public partial class AddContactDataDialog
{
    private MudForm form;
    public ContactInfoCreateUpdateDto createUpdateContactDataDto = new ContactInfoCreateUpdateDto();
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; }
    bool _clearable;
    [Parameter]
    public int organizationId { get; set; }
    /// <summary>
    /// Submits the form to create a new contact data entry.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task Submit()
    {
        try
        {
           
            var contactdata = await ContactInfoAppService.CreateAsync(createUpdateContactDataDto);
            if (contactdata != null)
            {
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]} {L[AlertMessages.AlertMessages.ADD]} {createUpdateContactDataDto.Name} ", Severity.Success);
                MudDialog.Close(DialogResult.Ok(true));
                await Task.Delay(1000);
                Navigation.NavigateTo("/contact-data", forceLoad: true);
            }
            else
            {
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.ADD]} {createUpdateContactDataDto.Name} ", Severity.Error);
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
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.DELETE]} : {ex.Message}", Severity.Error);
        }
        catch (AuthenticationException ex)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.DELETE]} : {ex.Message} ", Severity.Error);
        }
        catch (Exception ex)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.DELETE]} : {ex.Message} ", Severity.Error);
        }
    }

    /// <summary>
    /// Cancels the add contact data operation and closes the dialog.
    /// </summary>
    private void Cancel()
    {
        MudDialog.Cancel();
    }
}
