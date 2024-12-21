using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Threading.Tasks;
using System;
using Volo.Abp;
using SmartParking.Organizations;
using Volo.Abp.Validation;
using System.Linq;

namespace SmartParking.Blazor.Components.Pages.ContactsData;

public partial class AddContactDataDialog
{
    private MudForm form;
    public ContactInfoCreateUpdateDto input = new ContactInfoCreateUpdateDto();

    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; }
    bool _clearable;
    [Parameter] public int organizationId { get; set; }

    private async Task Submit()
    {
        var successedMessage = L["ContactDataCreatedSuccessfully"];
        var errorMessage = L["Can'tCreateContactData"];
        var validationErrorMessage = L["PleaseFixTheValidationErrors"];


        // Call your service to create the ContactData
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
                break;
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
    private void Cancel()
    {
        MudDialog.Cancel();
    }
}
