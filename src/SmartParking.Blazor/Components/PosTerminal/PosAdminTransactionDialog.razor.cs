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

public partial class PosAdminTransactionDialog : IAsyncDisposable
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;

    [Parameter, EditorRequired] public string Operation { get; set; } = "REFUND"; // "REFUND" or "REVERSAL"
    [Parameter, EditorRequired] public Guid PaymentId { get; set; }
    [Parameter] public decimal Amount { get; set; }
    [Parameter] public string? Rrn { get; set; }
    [Parameter] public string? Stan { get; set; }
    [Parameter] public DateTime TxnDate { get; set; } = DateTime.Today;
    [Parameter] public OrganizationSettingsDto? OrgSettings { get; set; }

    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private IPaymentAppService PaymentAppService { get; set; } = default!;
    [Inject] private IOptions<PosTerminalOptions> PosTerminalOptions { get; set; } = default!;
    private PosTerminalOptions Options => PosTerminalOptions.Value;
    [Inject] private ILogger<PosAdminTransactionDialog> Logger { get; set; } = default!;
    [Inject] private ILoggerFactory LoggerFactory { get; set; } = default!;

    private GeideaWebEcrInterop _interop = default!;
    private GeideaWebEcrCallbackHandler _handler = default!;

    private PosTerminalStateSnapshot _state = new(PosSessionState.Disconnected);
    private bool _isConfigured;
    private bool _isBusy;
    private string _operation = "REFUND";

    protected override async Task OnInitializedAsync()
    {
        _operation = Operation.ToUpperInvariant();
        _interop = new GeideaWebEcrInterop(JSRuntime);
        _handler = new GeideaWebEcrCallbackHandler(
            PaymentAppService,
            _interop,
            Options,
            LoggerFactory.CreateLogger(nameof(GeideaWebEcrCallbackHandler)),
            OrgSettings,
            isArabic: false);

        _isConfigured = _handler.IsConfigured;

        // Marshaling dispatcher to Blazor sync context
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
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "DisconnectAsync failed during PosAdminTransactionDialog dispose");
        }
        await _interop.DisposeAsync();
    }

    private void OnStateChanged(PosTerminalStateSnapshot snapshot)
    {
        _state = snapshot;
        _isBusy = false;
        StateHasChanged();
    }

    private async Task ExecuteAdminOperationAsync()
    {
        if (!_isConfigured || _isBusy) return;
        _isBusy = true;
        _handler.PrepareForTransaction();
        StateHasChanged();

        try
        {
            _handler.SetActivePaymentId(PaymentId);

            string requestJson;
            if (_operation == "REVERSAL")
            {
                var dto = new ReversalRequestDto
                {
                    ECRNumber = DeriveEcrNumber(PaymentId),
                    Stan = Stan ?? "000000"
                };
                requestJson = JsonSerializer.Serialize(dto);
            }
            else // REFUND
            {
                var dto = new RefundRequestDto
                {
                    Amount = Amount.ToString("F2"),
                    ECRNumber = DeriveEcrNumber(PaymentId),
                    RRNNumber = Rrn ?? "000000000000",
                    TransactionDate = TxnDate.ToString("ddMMyyyy")
                };
                requestJson = JsonSerializer.Serialize(dto);
            }

            _handler.SetPendingCommandPayload(requestJson);

            await _interop.ConnectAsync(Options.ServiceUrl);
            _handler.StartConnectionTimeout();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "ExecuteAdminOperationAsync failed for {Operation}", _operation);
            _isBusy = false;
        }
    }

    private void CloseDialog()
    {
        MudDialog.Close(DialogResult.Ok(_state));
    }

    private static string DeriveEcrNumber(Guid paymentId)
    {
        var digits = new string(paymentId.ToString("N").Where(char.IsDigit).ToArray());
        return digits.Length >= 16 ? digits[^16..] : digits.PadLeft(16, '0');
    }
}
