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

namespace SmartParking.Blazor.Components.Pages.Subscriptions;

public partial class EditSubscriptionDialog
{
    [CascadingParameter] MudDialogInstance MudDialog { get; set; }
    [Parameter] public SubscriptionDto subscription { get; set; } = new SubscriptionDto();

    public List<SubscriptionTypeLookupDto> subscriptions = new List<SubscriptionTypeLookupDto>();

    private MudForm editForm;

    private DateTime? ActivationDate = DateTime.Now;

    private DateTime? InActivationDate = DateTime.Now;

    bool _clearable;
    [Parameter] public int UpdatedId { get; set; }
    protected override async Task OnInitializedAsync()
    {
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
                ActivationDate = DateOnly.FromDateTime(ActivationDate ?? DateTime.MinValue),
                InActivationDate = DateOnly.FromDateTime(ActivationDate ?? DateTime.MaxValue),
                NumberOfCameras = subscription.NumberOfCameras,
                NumberOfCustomers = subscription.NumberOfCustomers, 
                NumberOfUsers = subscription.NumberOfUsers ,
                NumbersOfGarages = subscription.NumbersOfGarages,
                SubscriptionSlotNumber = subscription.SubscriptionSlotNumber,
                ParkingSlot = subscription.ParkingSlot,
                CreationTime = subscription.CreationTime
            };

            // Call the update method

            var subscriptions = await SubscriptionAppService.UpdateAsync(UpdatedId, input);

            if (subscriptions != null)
            {
                Snackbar.Add(successedMessage, Severity.Success);
                MudDialog.Close(DialogResult.Ok(true));
                await Task.Delay(1000);
                Navigation.NavigateTo("/subscriptions", true);
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
