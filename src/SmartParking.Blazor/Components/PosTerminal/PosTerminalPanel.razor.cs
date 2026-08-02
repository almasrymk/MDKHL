using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartParking.Enums.PosTerminal;
using SmartParking.Organizations;
using SmartParking.Payments;
using SmartParking.Payments.PosTerminal;

namespace SmartParking.Blazor.Components.PosTerminal;

public partial class PosTerminalPanel : IAsyncDisposable
{
    // ── Parameters ──────────────────────────────────────────────────────────────

    /// <summary>Full payment request DTO — panel owns CreatePendingPosPaymentAsync internally.</summary>
    [Parameter, EditorRequired] public PaymentCheckoutRequestDto Request { get; set; } = default!;


    /// <summary>Fires before creating pending payment and dispatching purchase to POS terminal.</summary>
    [Parameter] public EventCallback OnBeforeSendPurchase { get; set; }

    /// <summary>Fires when a Pending payment row is created, so parent can track the PaymentId.</summary>
    [Parameter] public EventCallback<Guid> OnPaymentCreated { get; set; }

    /// <summary>Fires when terminal returns a result (Approved or Declined).</summary>
    [Parameter] public EventCallback<PosTerminalStateSnapshot> OnTransactionCompleted { get; set; }

    /// <summary>Fires when cashier explicitly cancels before transaction completes.</summary>
    [Parameter] public EventCallback OnCancel { get; set; }

    // ── Injected ─────────────────────────────────────────────────────────────────

    [Inject] private IPaymentAppService PaymentAppService { get; set; } = default!;
    [Inject] private IOptions<PosTerminalOptions> PosTerminalOptions { get; set; } = default!;
    private PosTerminalOptions Options => PosTerminalOptions.Value;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private ILogger<PosTerminalPanel> Logger { get; set; } = default!;

    // ── Private State ─────────────────────────────────────────────────────────────

    private PosTerminalStateSnapshot _state = new(PosSessionState.Connected);
    private bool _isConfigured = true;
    private bool _isBusy;
    private bool _activePaymentWasCreated;

    // ── Lifecycle ────────────────────────────────────────────────────────────────

    protected override async Task OnInitializedAsync()
    {
        _isConfigured = true;
        _state = new PosTerminalStateSnapshot(PosSessionState.Connected);
        await Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await ValueTask.CompletedTask;
    }

    // ── Actions ──────────────────────────────────────────────────────────────────

    private async Task ConnectAsync()
    {
        if (!_isConfigured || _isBusy) return;
        _isBusy = true;
        StateHasChanged();

        try
        {
            await JSRuntime.InvokeVoidAsync("posTerminalClient.connect", Options.WebSocketUrl);
            _state = new PosTerminalStateSnapshot(PosSessionState.Connected);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "[PosTerminalPanel] WebSocket connect call failed");
            _state = new PosTerminalStateSnapshot(PosSessionState.Error, LastErrorMessage: ex.Message);
        }
        finally
        {
            _isBusy = false;
            StateHasChanged();
        }
    }

    private async Task RetryTransactionAsync()
    {
        await SendPurchaseAsync();
    }

    /// <summary>Manual cashier trigger: creates Pending payment, dispatches PURCHASE via browser WebSocket JS Interop.</summary>
    private async Task SendPurchaseAsync()
    {
        if (!_isConfigured || _isBusy) return;
        _isBusy = true;

        _state = new PosTerminalStateSnapshot(PosSessionState.TransactionInProgress);
        StateHasChanged();

        try
        {
            if (OnBeforeSendPurchase.HasDelegate)
            {
                await OnBeforeSendPurchase.InvokeAsync();
            }

            // Step 1: Create Pending payment row in DB
            var pending = await PaymentAppService.CreatePendingPosPaymentAsync(Request);
            var paymentId = pending.PaymentId;
            _activePaymentWasCreated = true;

            await OnPaymentCreated.InvokeAsync(paymentId);

            string ecrNumber = DeriveEcrNumber(pending.PaymentNumber);

            Logger.LogInformation("[PosTerminalPanel] Dispatching PURCHASE via browser JS WebSocket Interop to {Url} for PaymentId={PaymentId}, ECR={EcrNumber}, Amount={Amount}",
                Options.WebSocketUrl, paymentId, ecrNumber, Request.Amount);

            // Step 2: Dispatch command directly to Geidea Web ECR via browser WebSocket client
            var response = await JSRuntime.InvokeAsync<PosTerminalJsResponse>(
                "posTerminalClient.sendPurchase",
                Request.Amount,
                ecrNumber);

            Logger.LogInformation("[PosTerminalPanel] Web ECR response received: Success={Success}, Status={Status}, Code={Code}, RRN={Rrn}",
                response.Success, response.Status, response.ResponseCode, response.Rrn);

            // Step 3: Evaluate response
            if (response.Success || response.Status == "APPROVED" || response.ResponseCode == "000")
            {
                _state = new PosTerminalStateSnapshot(
                    PosSessionState.Completed,
                    Outcome: SamaResponseOutcome.Approved,
                    SamaDescription: response.ResponseMessage ?? "Transaction Approved",
                    MaskedPan: response.MaskedPan,
                    AuthCode: response.AuthCode,
                    Rrn: response.Rrn,
                    CardScheme: response.CardScheme,
                    SamaCode: response.ResponseCode);
            }
            else
            {
                _state = new PosTerminalStateSnapshot(
                    PosSessionState.Completed,
                    Outcome: SamaResponseOutcome.Declined,
                    SamaDescription: response.Error?.Message ?? response.ResponseMessage ?? response.Status ?? "Transaction Declined",
                    SamaCode: response.ResponseCode);
            }

            StateHasChanged();
            await OnTransactionCompleted.InvokeAsync(_state);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[PosTerminalPanel] SendPurchaseAsync failed");
            _state = new PosTerminalStateSnapshot(
                PosSessionState.Error,
                LastErrorMessage: ex.Message);
            StateHasChanged();
        }
        finally
        {
            _isBusy = false;
        }
    }

    private async Task CancelAsync()
    {
        await OnCancel.InvokeAsync();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────────

    private static string DeriveEcrNumber(string paymentNumber)
    {
        var digits = new string(paymentNumber.Where(char.IsDigit).ToArray());
        return digits.Length >= 16 ? digits[^16..] : digits.PadLeft(16, '0');
    }

    private static string GetTerminalActionPrompt(GeideaTerminalAction? action) => action switch
    {
        GeideaTerminalAction.SwipeOrInsert => "Ask customer to insert or swipe card",
        GeideaTerminalAction.CardInserted => "Card inserted — processing...",
        GeideaTerminalAction.PinEntry => "Ask customer to enter PIN",
        GeideaTerminalAction.PinQuit => "PIN entry cancelled",
        GeideaTerminalAction.CardRemoved => "Card removed",
        GeideaTerminalAction.SendingRequest => "Sending request to bank...",
        GeideaTerminalAction.WaitingResponse => "Waiting for bank response...",
        GeideaTerminalAction.Connecting => "Connecting to terminal...",
        GeideaTerminalAction.ConnectionMade => "Connection established",
        GeideaTerminalAction.ConnectionClosed => "Terminal connection closed",
        GeideaTerminalAction.IdleScreen => "Terminal idle",
        GeideaTerminalAction.TransactionCancelled => "Transaction cancelled by terminal",
        GeideaTerminalAction.UserCancelledAndTimeout => "Timed out — no card presented",
        GeideaTerminalAction.CardReadError => "Card read error — ask customer to retry",
        GeideaTerminalAction.WrongPin => "Incorrect PIN",
        GeideaTerminalAction.RetryLimitExceeded => "PIN retry limit exceeded",
        GeideaTerminalAction.BlockedCard => "Card blocked",
        GeideaTerminalAction.MaximumAmountLimitExceeded => "Amount exceeds terminal limit",
        _ => "Processing — please wait..."
    };

    public async Task CancelTransactionAsync()
    {
        await Task.CompletedTask;
    }
}
