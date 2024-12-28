using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Organizations;
using System.Threading.Tasks;
using System;
using Volo.Abp;
using Volo.Abp.Validation;
using System.Linq;
using SmartParking.Blazor.Components.Shared;
using System.Security.Authentication;

namespace SmartParking.Blazor.Components.Pages.ContactsData;

public partial class UpdateContactDataDialog
{
    private MudForm form;

    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; }
    bool _clearable;
    [Parameter] public int organizationId { get; set; }
    [Parameter] public int contactDataId { get; set; }
    [Parameter] public ContactInfoDto ContactDataDto { get; set; }

    private async Task Submit()
    {
        


        // Call your service to edit the ContactData
        try
        {
            ContactInfoCreateUpdateDto editUpdateContactDataDto = new ContactInfoCreateUpdateDto()
            {
                Name = ContactDataDto.Name,
                ContactMail = ContactDataDto.ContactMail,
                OrganizationId = organizationId,
                PhoneNum = ContactDataDto.PhoneNum,
                TenantId = CurrentTenant.Id,
                WhatsAppNum = ContactDataDto.WhatsAppNum
            };

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
    protected override Task OnInitializedAsync()
    {
        return base.OnInitializedAsync();

    }
    private void Cancel()
    {
        MudDialog.Cancel();
    }
}
