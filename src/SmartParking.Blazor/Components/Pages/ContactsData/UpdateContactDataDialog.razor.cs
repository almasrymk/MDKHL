using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Organizations;
using System.Security.Authentication;
using Volo.Abp;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.ContactsData;

/// <summary>
/// Dialog for updating contact data.
/// This component allows users to edit the details of a contact (such as Name, Email, Phone Number, etc.)
/// and submit the changes to the backend service.
/// </summary>
public partial class UpdateContactDataDialog
{
    private MudForm form;

    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; }

    bool _clearable;
    [Parameter] public int organizationId { get; set; }

    [Parameter] public int contactDataId { get; set; }

    [Parameter] public ContactInfoDto ContactDataDto { get; set; }

    /// <summary>
    /// Submits the form to update the contact data by calling the backend service to persist the changes.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task Submit()
    {
        try
        {
            ContactInfoCreateUpdateDto editUpdateContactDataDto = Mapper.Map<ContactInfoCreateUpdateDto>(ContactDataDto);
            var contactdata = await ContactInfoAppService.UpdateAsync(contactDataId, editUpdateContactDataDto);
            if (contactdata != null)
            {
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]} {L[AlertMessages.AlertMessages.UPDATE]} {editUpdateContactDataDto.Name}", Severity.Success);
                MudDialog.Close(DialogResult.Ok(true));
                await Task.Delay(1000);
                Navigation.NavigateTo("/contact-data", forceLoad: true);
            }
            else
            {
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.UPDATE]} {editUpdateContactDataDto.Name}", Severity.Error);
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
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.UPDATE]} : {ex.Message}", Severity.Error);
        }
        catch (AuthenticationException ex)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.UPDATE]} : {ex.Message} ", Severity.Error);
        }
        catch (Exception ex)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.UPDATE]} : {ex.Message} ", Severity.Error);
        }
    }

    /// <summary>
    /// Initializes the component, called when the component is initialized.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    protected override Task OnInitializedAsync()
    {
        return base.OnInitializedAsync();
    }

    /// <summary>
    /// Cancels the dialog and closes it without submitting any changes.
    /// </summary>
    private void Cancel()
    {
        MudDialog.Cancel();
    }
}
