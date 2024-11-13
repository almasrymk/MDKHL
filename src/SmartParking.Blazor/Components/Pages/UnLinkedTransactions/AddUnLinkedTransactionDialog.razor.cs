using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Subscriptions;
using SmartParking.SubscriptionTypes;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Volo.Abp;
using SmartParking.GarageTransactionEndPoints;
using Volo.Abp.Validation;
using System.Linq;

namespace SmartParking.Blazor.Components.Pages.UnLinkedTransactions;

public partial class AddUnLinkedTransactionDialog
{
    private MudForm form;
    public CreateUpdateGarageTransactionEndPointDto input = new CreateUpdateGarageTransactionEndPointDto();

    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; }
    bool _clearable;
    protected override async Task OnInitializedAsync()
    {
     
    }


    private async Task Submit()
    {
        var successedMessage = L["UnLinkedTransactionCreatedSuccessfully"];
        var errorMessage = L["Can'tCreateUnLinkedTransaction"];
        var validationErrorMessage = L["PleaseFixTheValidationErrors"];
        var failedError = L["FailedToAddUnLinkedTransaction :"];

        // Call your service to create the UnLinkedTransaction
        try
        {
                   

        var unLinkedTransactionDto = await UnLinkedTransactionAppService.CreateAsync(input);

            if (unLinkedTransactionDto != null)
            {
                Snackbar.Add(successedMessage, Severity.Success);

                MudDialog.Close(DialogResult.Ok(true));
                await Task.Delay(1000);
                Navigation.NavigateTo("/unLinkedTransaction", true);
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
            Snackbar.Add($"{failedError} {ex.Code} + ", Severity.Error);

        }
        catch (Exception ex)
        {
            Snackbar.Add($"{failedError} {ex.Message} + ", Severity.Error);
        }

    }
    private void Cancel()
    {
        MudDialog.Cancel();
    }
}
