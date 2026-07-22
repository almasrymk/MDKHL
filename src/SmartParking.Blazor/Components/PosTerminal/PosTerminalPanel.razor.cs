using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MudBlazor;
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

    [Parameter] public OrganizationSettingsDto? OrgSettings { get; set; }

    /// <summary>Fires when a Pending payment row is created, so parent can track the PaymentId.</summary>
    [Parameter] public EventCallback<Guid> OnPaymentCreated { get; set; }

    /// <summary>Fires when terminal returns a result (Approved or Declined).</summary>
    [Parameter] public EventCallback<PosTerminalStateSnapshot> OnTransactionCompleted { get; set; }

    /// <summary>Fires when cashier explicitly cancels before transaction completes.</summary>
    [Parameter] public EventCallback OnCancel { get; set; }

    // ── Injected ─────────────────────────────────────────────────────────────────

    [Inject] private IPaymentAppService PaymentAppService { get; set; } = default!;
    [Inject] private IPosServiceManager PosServiceManager { get; set; } = default!;
    [Inject] private IOptions<PosTerminalOptions> PosTerminalOptions { get; set; } = default!;
    private PosTerminalOptions Options => PosTerminalOptions.Value;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private ILogger<PosTerminalPanel> Logger { get; set; } = default!;

    // ── Private State ─────────────────────────────────────────────────────────────

    private PosTerminalStateSnapshot _state = new(PosSessionState.Disconnected);
    private bool _isConfigured;
    private bool _isBusy;
    private bool _activePaymentWasCreated;

    // ── Lifecycle ────────────────────────────────────────────────────────────────

    protected override async Task OnInitializedAsync()
    {
        _isConfigured = true;

        if (!_isConfigured) return;

        PosServiceManager.OnEventReceived += HandlePosEventReceived;

        await CheckConnectionStateAsync();
    }

    private Task CheckConnectionStateAsync()
    {
        bool isConnected = PosServiceManager.IsConnected(Request.OrganizationId.ToString());
        if (isConnected)
        {
            _state = new PosTerminalStateSnapshot(PosSessionState.Connected);
            Logger.LogInformation("[PosTerminalPanel] Connected to POS Service for Organization {OrgId}", Request.OrganizationId);
        }
        else
        {
            _state = new PosTerminalStateSnapshot(
                PosSessionState.Error,
                LastErrorMessage: "No active POS service connected for this organization. Ensure standalone Node.js POS service is running.");
            Logger.LogWarning("[PosTerminalPanel] No active POS Service connected for Organization {OrgId}", Request.OrganizationId);
        }
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        PosServiceManager.OnEventReceived -= HandlePosEventReceived;
        await ValueTask.CompletedTask;
    }

    private void HandlePosEventReceived(PosEventMessage eventMsg)
    {
        if (eventMsg.OrganizationId.Equals(Request.OrganizationId.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            InvokeAsync(() =>
            {
                GeideaTerminalAction? action = null;
                if (!string.IsNullOrEmpty(eventMsg.TerminalAction))
                {
                    action = GeideaEventMapper.ParseTerminalAction(eventMsg.TerminalAction);
                }

                GeideaTerminalStatus? status = null;
                if (!string.IsNullOrEmpty(eventMsg.TerminalStatus))
                {
                    status = GeideaEventMapper.ParseTerminalStatus(eventMsg.TerminalStatus);
                }

                _state = _state with
                {
                    LastTerminalAction = action,
                    TerminalStatus = status
                };
                StateHasChanged();
            });
        }
    }

    // ── Actions ──────────────────────────────────────────────────────────────────

    private async Task ConnectAsync()
    {
        if (!_isConfigured || _isBusy) return;
        _isBusy = true;
        StateHasChanged();

        try
        {
            await CheckConnectionStateAsync();
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

    /// <summary>Manual cashier trigger: creates Pending payment, dispatches PAYMENT command via HTTP.</summary>
    private async Task SendPurchaseAsync()
    {
        if (!_isConfigured || _isBusy) return;
        _isBusy = true;

        _state = new PosTerminalStateSnapshot(PosSessionState.TransactionInProgress);
        StateHasChanged();

        try
        {
            // Step 1: create Pending payment row in DB
            var pending = await PaymentAppService.CreatePendingPosPaymentAsync(Request);
            var paymentId = pending.PaymentId;
            _activePaymentWasCreated = true;

            // Step 3: obtain organization API key securely for cashier session dispatch
            string apiKey = await PosServiceManager.GetOrganizationApiKeyAsync(Request.OrganizationId.ToString());

            // Step 4: construct PosCommandMessage
            var commandPayload = new
            {
                requestId = Guid.NewGuid().ToString(),
                command = "PAYMENT",
                payload = new
                {
                    amount = Request.Amount.ToString("F2"),
                    ecrNumber = DeriveEcrNumber(pending.PaymentNumber),
                    paymentId = paymentId
                }
            };

            Logger.LogInformation("[PosTerminalPanel] Dispatching PAYMENT command for PaymentId={PaymentId} via Browser JS Interop fetch to {Url}...", paymentId, Options.ServiceBaseUrl);

            PosResponseMessage response;
            try
            {
                response = await JSRuntime.InvokeAsync<PosResponseMessage>(
                    "posTerminalClient.sendCommand",
                    Options.ServiceBaseUrl,
                    Request.OrganizationId.ToString(),
                    apiKey,
                    commandPayload);

                if (!response.Success && response.Error?.Code == "FETCH_ERROR")
                {
                    Logger.LogWarning("[PosTerminalPanel] Direct JS fetch failed ({Msg}). Falling back to ASP.NET Core server dispatch...", response.Error.Message);
                    var cmdMsg = new PosCommandMessage
                    {
                        RequestId = commandPayload.requestId,
                        Command = "PAYMENT",
                        Payload = commandPayload.payload
                    };
                    response = await PosServiceManager.SendCommandAsync(Request.OrganizationId.ToString(), cmdMsg);
                }
            }
            catch (Exception jsEx)
            {
                Logger.LogWarning(jsEx, "[PosTerminalPanel] JS Interop invocation failed. Falling back to ASP.NET Core server dispatch...");
                var cmdMsg = new PosCommandMessage
                {
                    RequestId = commandPayload.requestId,
                    Command = "PAYMENT",
                    Payload = commandPayload.payload
                };
                response = await PosServiceManager.SendCommandAsync(Request.OrganizationId.ToString(), cmdMsg);
            }

            Logger.LogInformation("[PosTerminalPanel] Correlated response received for RequestId={ReqId}, Success={Success}, Status={Status}",
                response.RequestId, response.Success, response.Status);

            // Step 5: Evaluate response
            if (response.Success && response.Status == "APPROVED")
            {
                _state = new PosTerminalStateSnapshot(
                    PosSessionState.Completed,
                    Outcome: SamaResponseOutcome.Approved,
                    SamaDescription: "Transaction Approved");
            }
            else
            {
                _state = new PosTerminalStateSnapshot(
                    PosSessionState.Completed,
                    Outcome: SamaResponseOutcome.Declined,
                    SamaDescription: response.Error?.Message ?? response.Status ?? "Transaction Declined");
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
