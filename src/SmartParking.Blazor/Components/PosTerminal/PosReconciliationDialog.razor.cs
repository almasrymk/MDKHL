using System;
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
using SmartParking.Utilities;

namespace SmartParking.Blazor.Components.PosTerminal;

public partial class PosReconciliationDialog : IAsyncDisposable
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;

    [Parameter] public OrganizationSettingsDto? OrgSettings { get; set; }

    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private IPaymentAppService PaymentAppService { get; set; } = default!;
    [Inject] private IOptions<PosTerminalOptions> PosTerminalOptions { get; set; } = default!;
    private PosTerminalOptions Options => PosTerminalOptions.Value;
    [Inject] private ILogger<PosReconciliationDialog> Logger { get; set; } = default!;
    [Inject] private ILoggerFactory LoggerFactory { get; set; } = default!;

    private GeideaWebEcrInterop _interop = default!;
    private GeideaWebEcrCallbackHandler _handler = default!;

    private PosTerminalStateSnapshot _state = new(PosSessionState.Disconnected);
    private bool _isConfigured;
    private bool _isBusy;

    protected override async Task OnInitializedAsync()
    {
        _interop = new GeideaWebEcrInterop(JSRuntime);
        _handler = new GeideaWebEcrCallbackHandler(
            PaymentAppService,
            _interop,
            Options,
            LoggerFactory.CreateLogger(nameof(GeideaWebEcrCallbackHandler)),
            OrgSettings,
            isArabic: false);

        _isConfigured = _handler.IsConfigured;
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
            Logger.LogWarning(ex, "DisconnectAsync failed during PosReconciliationDialog dispose");
        }
        await _interop.DisposeAsync();
    }

    private void OnStateChanged(PosTerminalStateSnapshot snapshot)
    {
        _state = snapshot;
        _isBusy = false;

        // Structured Audit Trail logging for Reconciliation (No PaymentId FK)
        if (snapshot.SessionState == PosSessionState.Completed)
        {
            Logger.LogInformation(
                "POS Terminal Reconciliation Completed at {KsaTime} KSA Local Time | SAMA Code: {SamaCode} | RRN: {Rrn} | Description: {Description}",
                DateTimeTimeZoneExtensions.KsaNow.ToString("yyyy-MM-dd HH:mm:ss"),
                snapshot.SamaCode ?? "N/A",
                snapshot.Rrn ?? "N/A",
                snapshot.SamaDescription ?? "N/A");
        }

        StateHasChanged();
    }

    private async Task ExecuteReconciliationAsync()
    {
        if (!_isConfigured || _isBusy) return;
        _isBusy = true;
        _handler.PrepareForTransaction();
        StateHasChanged();

        try
        {
            var dto = new ReconciliationRequestDto();
            var requestJson = JsonSerializer.Serialize(dto);
            _handler.SetPendingCommandPayload(requestJson);

            await _interop.ConnectAsync(Options.ServiceUrl);
            _handler.StartConnectionTimeout();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "ExecuteReconciliationAsync failed");
            _isBusy = false;
        }
    }

    private void CloseDialog()
    {
        MudDialog.Close(DialogResult.Ok(_state));
    }
}
