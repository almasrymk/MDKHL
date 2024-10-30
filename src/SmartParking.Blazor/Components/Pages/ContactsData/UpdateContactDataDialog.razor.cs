using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Organizations;
using System.Threading.Tasks;
using System;
using Volo.Abp;
using Volo.Abp.Validation;
using System.Linq;

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
        var successedMessage = L["EditDataEditdSuccessfully"];
        var errorMessage = L["Can'tEditContactData"];
        var validationErrorMessage = L["PleaseFixTheValidationErrors"];


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
                Snackbar.Add(successedMessage, Severity.Success);
                MudDialog.Close(DialogResult.Ok(true));
                await Task.Delay(1000);
                Navigation.NavigateTo("/contact-data", forceLoad: true);

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
