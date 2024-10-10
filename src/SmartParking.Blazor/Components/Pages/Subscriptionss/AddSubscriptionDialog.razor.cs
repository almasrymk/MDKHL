using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Lookups.Governorate;
using SmartParking.Lookups;
using SmartParking.LookupsAppServices;
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

public partial class AddSubscriptionDialog
{
    private MudForm form;
    private DateTime? _activateDate = new DateTime();
    private DateTime? _inActivateDate = new DateTime();

    public CreateUpdateSubscriptionDto input = new CreateUpdateSubscriptionDto();
    public List<SubscriptionTypeLookupDto> subscriptions = new List<SubscriptionTypeLookupDto>();

    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; }
    bool _clearable;

    protected override async Task OnInitializedAsync()
    {
        var result = await SubscriptionTypeAppService.GetSubscriptionTypeLookupAsync();
        subscriptions = result;
    }
   

    private async Task Submit()
    {
        var successedMessage = L["SubscriptionCreatedSuccessfully"];
        var errorMessage = L["Can'tCreateSubscription"];
        var validationErrorMessage = L["PleaseFixTheValidationErrors"];



        // Call your service to create the subscription
        try
        {
            input.ActivationDate = DateOnly.FromDateTime(_activateDate ?? DateTime.Now);

            input.InActivationDate = DateOnly.FromDateTime(_inActivateDate ?? DateTime.MinValue);

            var subscription = await SubscriptionAppService.CreateAsync(input);

            if (subscription != null)
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
                Snackbar.Add($"{validationErrorMessage} {validationError.MemberNames.First()} - {validationError.ErrorMessage}", Severity.Error);
            }
        }
        catch (BusinessException ex)
        {
            Snackbar.Add($"Failed to add subscription: {ex.Code} + ", Severity.Error);

        }
        catch (Exception ex)
        {
            Snackbar.Add($"Failed to add subscription: {ex.Message} + ", Severity.Error);
        }

    }
    private void Cancel()
    {
        MudDialog.Cancel();
    }
}
