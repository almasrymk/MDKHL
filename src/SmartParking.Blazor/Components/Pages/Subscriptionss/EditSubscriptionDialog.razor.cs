using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Lookups;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Volo.Abp;
using SmartParking.Subscriptions;
using SmartParking.SubscriptionTypes;
using Volo.Abp.Validation;
using System.Linq;
using SmartParking.Entities.Subscriptions;

namespace SmartParking.Blazor.Components.Pages.Subscriptionss;

public partial class EditSubscriptionDialog
{
    [CascadingParameter] MudDialogInstance MudDialog { get; set; }
    [Parameter] public SubscriptionDto subscription { get; set; } = new SubscriptionDto();

    public List<SubscriptionTypeLookupDto> subscriptions = new List<SubscriptionTypeLookupDto>();

    private MudForm editForm;

    private DateTime? _activateDate = new DateTime();

    private DateTime? _inActivateDate = new DateTime();

    bool _clearable;
    [Parameter] public int UpdatedId { get; set; }
    protected override async Task OnInitializedAsync()
    {
        _activateDate = subscription.ActivationDate.ToDateTime(TimeOnly.Parse("00:00 AM"));
        _inActivateDate = subscription.InActivationDate.ToDateTime(TimeOnly.Parse("00:00 AM"));

        var result = await SubscriptionTypeAppService.GetSubscriptionTypeLookupAsync();
        subscriptions = result;
    }
    private async Task Save()
    {

        var successedMessage = L["SubscriptionUpdatedSuccessfully"];
        var errorMessage = L["Can'tUpdateSubscription"];
        var validationErrorMessage = L["PleaseFixTheValidationErrors"];
        var failedError = L["FailedToUpdateSubscription :"];

        try
        {

            


            // Create the input object for update
            var input = new CreateUpdateSubscriptionDto
            {
                ArabicName = subscription.ArabicName,
                EnglishName = subscription.EnglishName,
                SubscriptionDescription = subscription.SubscriptionDescription,
                SubscriptionPrice = subscription.SubscriptionPrice,
                Status = subscription.Status,
                SubscriptionTypeId = subscription.SubscriptionTypeId,
                ActivationDate = DateOnly.FromDateTime(_activateDate ?? DateTime.MinValue),
                InActivationDate = DateOnly.FromDateTime(_inActivateDate ?? DateTime.MinValue),
               
            };

            // Call the update method

            var subscriptions = await SubscriptionAppService.UpdateAsync(UpdatedId, input);

            if (subscriptions != null)
            {
                Snackbar.Add(successedMessage, Severity.Success);
                MudDialog.Close(DialogResult.Ok(true));
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
                Snackbar.Add($"Validation failed: {validationError.MemberNames.First()} - {validationError.ErrorMessage}", Severity.Error);
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
