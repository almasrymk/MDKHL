using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Organizations;
using System.Security.Authentication;
using Volo.Abp;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.ContactsData;

public partial class AddContactDataDialog
{
    private MudForm form;
    public ContactInfoCreateUpdateDto input = new ContactInfoCreateUpdateDto();

    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; }
    bool _clearable;
    [Parameter] public int organizationId { get; set; }

    private async Task Submit()
    {

        try
        {
            ContactInfoCreateUpdateDto createUpdateContactDataDto = new ContactInfoCreateUpdateDto()
            {
                Name = input.Name,
                ContactMail = input.ContactMail,
                OrganizationId = organizationId,
                PhoneNum = input.PhoneNum,
                TenantId = CurrentTenant.Id,
                WhatsAppNum = input.WhatsAppNum
            };


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
    private void Cancel()
    {
        MudDialog.Cancel();
    }
}
