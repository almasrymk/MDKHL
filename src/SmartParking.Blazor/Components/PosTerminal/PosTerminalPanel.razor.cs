using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using MudBlazor;
using SmartParking.Enums.PosTerminal;
using SmartParking.Organizations;
using SmartParking.Payments;
using SmartParking.Payments.PosTerminal;
using SmartParking.Payments.PosTerminal.WireContracts;

namespace SmartParking.Blazor.Components.PosTerminal;

public partial class PosTerminalPanel : IAsyncDisposable
{
    // ── Parameters ──────────────────────────────────────────────────────────────

    /// <summary>Full payment request DTO — panel owns CreatePendingPosPaymentAsync internally.</summary>
    [Parameter, EditorRequired] public PaymentCheckoutRequestDto Request { get; set; } = default!;

    [Parameter] public OrganizationSettingsDto? OrgSettings { get; set; }

    /// <summary>Fires when a Pending payment row is created, so parent can track the PaymentId.</summary>
    [Parameter] public EventCallback<Guid> OnPaymentCreated { get; set; }

    /// <summary>Fires when terminal returns a result (Approved or Declined).</summary>
    [Parameter] public EventCallback<PosTerminalStateSnapshot> OnTransactionCompleted { get; set; }

    /// <summary>Fires when cashier explicitly cancels before transaction completes.</summary>
    [Parameter] public EventCallback OnCancel { get; set; }

    // ── Injected ─────────────────────────────────────────────────────────────────

    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private IPaymentAppService PaymentAppService { get; set; } = default!;
    [Inject] private IOptions<PosTerminalOptions> PosTerminalOptions { get; set; } = default!;
    private PosTerminalOptions Options => PosTerminalOptions.Value;
    [Inject] private ILogger<PosTerminalPanel> Logger { get; set; } = default!;
    [Inject] private ILoggerFactory LoggerFactory { get; set; } = default!;

    // ── Private State ─────────────────────────────────────────────────────────────

    private GeideaWebEcrInterop _interop = default!;
    private GeideaWebEcrCallbackHandler _handler = default!;

    private PosTerminalStateSnapshot _state = new(PosSessionState.Disconnected);
    private bool _isConfigured;
    private bool _isBusy;

    /// <summary>True once CreatePendingPosPaymentAsync has fired at least once this session.
    /// Controls which retry path (transaction retry vs connection retry) is shown on Error.</summary>
    private bool _activePaymentWasCreated;

    // ── Lifecycle ────────────────────────────────────────────────────────────────

    protected override async Task OnInitializedAsync()
    {
        _interop = new GeideaWebEcrInterop(JSRuntime);
        _handler = new GeideaWebEcrCallbackHandler(
            PaymentAppService,
            _interop,
            Options,
            LoggerFactory.CreateLogger(nameof(GeideaWebEcrCallbackHandler)),
            OrgSettings,
            isArabic: false); // TODO Phase 6: wire actual culture/locale

        _isConfigured = _handler.IsConfigured;

        // Wire Dispatcher so timer callbacks marshal to Blazor circuit — required by TASK-010 thread safety.
        _handler.Dispatcher = action => InvokeAsync(action);

        _handler.StateChanged += OnStateChanged;

        if (!_isConfigured) return;

        await _interop.InitializeAsync(_handler);
    }

    public async ValueTask DisposeAsync()
    {
        _handler.StateChanged -= OnStateChanged;
        try
        {
            await _interop.DisconnectAsync();
        }
        catch (JSDisconnectedException) { }
        catch (ObjectDisposedException) { }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "DisconnectAsync failed during PosTerminalPanel dispose");
        }

        try
        {
            await _interop.DisposeAsync();
        }
        catch (JSDisconnectedException) { }
        catch (ObjectDisposedException) { }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "DisposeAsync failed for Interop during PosTerminalPanel dispose");
        }
    }

    // ── Event Handling ──────────────────────────────────────────────────────────

    private void OnStateChanged(PosTerminalStateSnapshot snapshot)
    {
        _state = snapshot;
        _isBusy = false;

        // Render first so Completed state is briefly visible, then notify parent to close.
        StateHasChanged();

        if (snapshot.SessionState == PosSessionState.Completed)
        {
            _ = OnTransactionCompleted.InvokeAsync(snapshot);
        }
    }

    // ── Actions ──────────────────────────────────────────────────────────────────

    /// <summary>Opens WebSocket and sends CONNECTION command to bridge physical terminal.</summary>
    private async Task ConnectAsync()
    {
        if (!_isConfigured || _isBusy) return;
        _isBusy = true;
        StateHasChanged();

        try
        {
            await _interop.ConnectAsync(Options.ServiceUrl);
            _handler.StartConnectionTimeout();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "ConnectAsync failed");
            _isBusy = false;
        }
    }

    /// <summary>Manual cashier trigger: creates Pending payment, sets _activePaymentId, sends PURCHASE.</summary>
    private async Task SendPurchaseAsync()
    {
        if (!_isConfigured || _isBusy) return;
        _isBusy = true;

        // Immediately reset state to clear stale SAMA outcomes/PANs from previous attempts
        _handler.PrepareForTransaction();
        StateHasChanged();

        try
        {
            // Step 1: create Pending payment row in DB
            var pending = await PaymentAppService.CreatePendingPosPaymentAsync(Request);
            var paymentId = pending.PaymentId;

            // Step 2: set _activePaymentId BEFORE any socket operation (ordering guarantee per ADR-7)
            _handler.SetActivePaymentId(paymentId);
            _activePaymentWasCreated = true;

            // Step 3: notify parent so it can track this PaymentId (e.g., show in invoice view)
            await OnPaymentCreated.InvokeAsync(paymentId);

            // Step 4: serialize and send PURCHASE
            var purchaseDto = new PurchaseRequestDto
            {
                Amount = Request.Amount.ToString("F2"),
                ECRNumber = DeriveEcrNumber(pending.PaymentNumber)
            };
            var json = JsonSerializer.Serialize(purchaseDto);
            Logger.LogInformation("Purchase JSON:\n{Json}", json);

            await _interop.SendAsync(json);

            // Step 5: start transaction timeout timer (ADR-6)
            _handler.StartTransactionTimeout();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "SendPurchaseAsync failed");
            _isBusy = false;
        }
    }

    /// <summary>Retry after TRANSACTION timeout (ADR-7): tears down socket, creates new payment row on next Send.</summary>
    private async Task RetryTransactionAsync()
    {
        if (_isBusy) return;
        _isBusy = true;
        _activePaymentWasCreated = false;
        StateHasChanged();

        try
        {
            // ResetForTransactionRetryAsync: clears _activePaymentId, disconnects + reconnects WebSocket
            await _handler.ResetForTransactionRetryAsync();
            _handler.StartConnectionTimeout();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "RetryTransactionAsync failed");
            _isBusy = false;
        }
    }

    private async Task CancelAsync()
    {
        await OnCancel.InvokeAsync();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Derives a 16-digit numeric ECRNumber from PaymentNumber.
    /// PaymentNumber format: "ORG260000042" (alpha prefix + sequential digits).
    /// Extracts digit chars only -> left-pads to 16 digits.
    /// Collision-free for practical operation: digit extraction is deterministic and
    /// PaymentNumber is unique-per-payment; OrgId prefix + sequential counter ensure
    /// no two payments produce identical digit-only strings.
    /// </summary>
    private static string DeriveEcrNumber(string paymentNumber)
    {
        var digits = new string(paymentNumber.Where(char.IsDigit).ToArray());
        return digits.Length >= 16 ? digits[^16..] : digits.PadLeft(16, '0');
    }

    /// <summary>Maps GeideaTerminalAction to cashier-facing prompt text.
    /// TODO Phase 6: localize via IStringLocalizer.</summary>
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

    /// <summary>
    /// Explicitly disconnects the terminal hardware connection.
    /// Called by wrapping dialog on cancellation to ensure clean hardware release.
    /// </summary>
    public async Task CancelTransactionAsync()
    {
        try
        {
            await _interop.DisconnectAsync();
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "DisconnectAsync failed during explicit cancel");
        }
    }
}

