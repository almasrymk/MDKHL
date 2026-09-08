using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Organizations;
using SmartParking.Payments;

namespace SmartParking.Blazor.Components.PosTerminal;

public partial class PosPaymentDialog
{
    private PosTerminalPanel? _panel;

    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;

    [Parameter] public PaymentCheckoutRequestDto Request { get; set; } = default!;
    [Parameter] public OrganizationSettingsDto? OrgSettings { get; set; }
    [Parameter] public Guid? ExistingPaymentId { get; set; }
    [Parameter] public string? ExistingPaymentNumber { get; set; }
    [Parameter] public EventCallback<Guid> OnPaymentCreated { get; set; }
    [Parameter] public EventCallback OnBeforeSendPurchase { get; set; }

    private async Task HandleTransactionCompleted(PosTerminalStateSnapshot snapshot)
    {
        // Small yield so any in-flight Blazor render completes before dialog teardown.
        await Task.Delay(200);
        MudDialog.Close(DialogResult.Ok(snapshot));
    }

    private async Task HandleCancel()
    {
        // Explicitly disconnect terminal hardware BEFORE closing dialog UI.
        // Prevents unmounting from leaving the hardware bridge in a stuck state.
        if (_panel != null)
        {
            await _panel.CancelTransactionAsync();
        }
        MudDialog.Cancel();
    }
}

