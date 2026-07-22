using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using SmartParking.Organizations;
using SmartParking.Enums.PosTerminal;
using SmartParking.Payments;
using SmartParking.Payments.PosTerminal;
using SmartParking.Payments.PosTerminal.WireContracts;

namespace SmartParking.Blazor.Components.PosTerminal;

public class GeideaWebEcrCallbackHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IPaymentAppService _paymentAppService;
    private readonly GeideaWebEcrInterop _interop;
    private readonly PosTerminalOptions _options;
    private readonly ILogger _logger;
    private readonly bool _isArabic;
    private readonly OrganizationSettingsDto? _orgSettings;

    private CancellationTokenSource? _transactionCts;
    private CancellationTokenSource? _connectionCts;

    private Guid? _activePaymentId;
    private string? _pendingCommandPayload;
    private bool _hasAttemptedFullConnect;

    public bool IsConfigured { get; }
    public PosTerminalStateSnapshot State { get; private set; }

    /// <summary>
    /// Component sets this to InvokeAsync(Action) to marshal timer callbacks to the Blazor sync context.
    /// </summary>
    public Action<Action>? Dispatcher { get; set; }

    public event Action<PosTerminalStateSnapshot>? StateChanged;

    public GeideaWebEcrCallbackHandler(
        IPaymentAppService paymentAppService,
        GeideaWebEcrInterop interop,
        PosTerminalOptions options,
        ILogger logger,
        OrganizationSettingsDto? orgSettings,
        bool isArabic)
    {
        _paymentAppService = paymentAppService;
        _interop = interop;
        _options = options;
        _logger = logger;
        _orgSettings = orgSettings;
        _isArabic = isArabic;

        IsConfigured = true;

        State = new PosTerminalStateSnapshot(PosSessionState.Disconnected);
    }

    private void Dispatch(Action action)
    {
        if (Dispatcher != null)
        {
            Dispatcher(action);
        }
        else
        {
            throw new InvalidOperationException("Dispatcher not configured — GeideaWebEcrCallbackHandler requires InvokeAsync marshaling. Ensure the Blazor component sets the Dispatcher property during initialization.");
        }
    }

    private void UpdateState(PosTerminalStateSnapshot newState)
    {
        var prevState = State.SessionState;
        State = newState;
        var nextState = State.SessionState;

        _logger.LogInformation(
            "[Geidea] State transition | PreviousState={PreviousState} NewState={NewState} LastTerminalAction={Action} TerminalStatus={Status}",
            prevState, nextState, State.LastTerminalAction, State.TerminalStatus);

        StateChanged?.Invoke(State);
    }

    public void SetActivePaymentId(Guid paymentId)
    {
        _activePaymentId = paymentId;
        _logger.LogInformation("[Geidea Lifecycle] Active Payment ID set: {PaymentId}", paymentId);
    }

    public void SetPendingCommandPayload(string payload)
    {
        _pendingCommandPayload = payload;
        _logger.LogInformation("[Geidea Lifecycle] Pending command payload set.");
    }

    public string? BuildConnectPayloadJson()
    {
        return null;
    }

    private string? BuildConnectJson()
    {
        return null;
    }

    public void PrepareForTransaction()
    {
        _logger.LogInformation("[Geidea Lifecycle] Preparing for transaction — updating state to TransactionInProgress.");
        UpdateState(new PosTerminalStateSnapshot(PosSessionState.TransactionInProgress, TerminalStatus: State.TerminalStatus));
    }

    public void StopConnectionTimeout()
    {
        if (_connectionCts != null)
        {
            _connectionCts.CancelAfter(Timeout.InfiniteTimeSpan);
            _connectionCts.Dispose();
            _connectionCts = null;
            _logger.LogInformation("[Geidea Lifecycle] Connection timeout stopped.");
        }
    }

    public void StopTransactionTimeout()
    {
        if (_transactionCts != null)
        {
            _transactionCts.CancelAfter(Timeout.InfiniteTimeSpan);
            _transactionCts.Dispose();
            _transactionCts = null;
            _logger.LogInformation("[Geidea Lifecycle] Transaction timeout stopped.");
        }
    }

    public void StartConnectionTimeout()
    {
        StopConnectionTimeout();
        _connectionCts = new CancellationTokenSource();
        _connectionCts.Token.Register(() =>
        {
            Dispatch(() =>
            {
                var prevState = State.SessionState;
                _logger.LogWarning("[Geidea Lifecycle] Connection to terminal timed out (ConnectionTimeoutMs={Timeout}ms).", _options.ConnectionTimeoutMs);
                UpdateState(State with
                {
                    SessionState = PosSessionState.Error,
                    LastErrorMessage = "Connection to terminal timed out"
                });
                LogStructuredEvent("ConnectionTimeout", "TIMEOUT", prevState, PosSessionState.Error);
            });
        });
        _connectionCts.CancelAfter(_options.ConnectionTimeoutMs);
        _logger.LogInformation("[Geidea Lifecycle] Connection timeout started ({Timeout}ms).", _options.ConnectionTimeoutMs);
    }

    public void StartTransactionTimeout()
    {
        StopTransactionTimeout();
        _transactionCts = new CancellationTokenSource();
        _transactionCts.Token.Register(() =>
        {
            Dispatch(() =>
            {
                var prevState = State.SessionState;
                _logger.LogWarning("[Geidea Lifecycle] Transaction timed out — terminal did not respond within {Seconds}s. PaymentId={PaymentId}",
                    _options.TransactionTimeoutMs / 1000, _activePaymentId);
                _activePaymentId = null;
                UpdateState(State with
                {
                    SessionState = PosSessionState.Error,
                    LastErrorMessage = $"Terminal did not respond within {_options.TransactionTimeoutMs / 1000} seconds"
                });
                LogStructuredEvent("TransactionTimeout", "TIMEOUT", prevState, PosSessionState.Error);
            });
        });
        _transactionCts.CancelAfter(_options.TransactionTimeoutMs);
        _logger.LogInformation("[Geidea Lifecycle] Transaction timeout started ({Timeout}ms).", _options.TransactionTimeoutMs);
    }

    public async Task ResetForTransactionRetryAsync()
    {
        _logger.LogInformation("[Geidea Lifecycle] Resetting for transaction retry...");
        _activePaymentId = null;
        StopTransactionTimeout();
        await _interop.DisconnectAsync();
        await _interop.ConnectAsync(_options.ServiceUrl);
    }

    public async Task RetryConnectionAsync()
    {
        _logger.LogInformation("[Geidea Lifecycle] Retrying connection...");
        StopConnectionTimeout();
        await _interop.DisconnectAsync();
        await _interop.ConnectAsync(_options.ServiceUrl);
        StartConnectionTimeout();
    }

    [JSInvokable]
    public async Task OnSocketOpen()
    {
        _hasAttemptedFullConnect = false;
        _logger.LogInformation("[Geidea Lifecycle] WebSocket connected successfully to service URL.");

        var prevState = State.SessionState;
        UpdateState(State with
        {
            SessionState = PosSessionState.Connecting,
            LastErrorMessage = "Checking terminal connection..."
        });
        LogStructuredEvent("OnSocketOpen", "WEBSOCKET_CONNECTED", prevState, PosSessionState.Connecting);

        // First attempt CONNECT if org settings exist, or check settings
        var connectJson = BuildConnectJson();
        if (!string.IsNullOrEmpty(connectJson))
        {
            _logger.LogInformation("[Geidea Lifecycle] Sending terminal CONNECT command: {Payload}", connectJson);
            await _interop.SendAsync(connectJson);
        }
        else
        {
            _logger.LogInformation("[Geidea Lifecycle] Sending GET_TERMINAL_SETTINGS to check status...");
            var checkSettingsDto = new GetTerminalSettingsRequestDto();
            var json = JsonSerializer.Serialize(checkSettingsDto, JsonOptions);
            await _interop.SendAsync(json);
        }
    }

    [JSInvokable]
    public void OnSocketError(string message)
    {
        string uiMessage = message switch
        {
            string m when m.StartsWith("MIXED_CONTENT") =>
                "Connection blocked: The app is running on HTTPS but the ECR service uses an unencrypted connection. " +
                "Ensure the ECR service URL points to localhost/127.0.0.1, not a remote hostname.",
            string m when m.StartsWith("ECR_SERVICE_UNREACHABLE") =>
                "ECR service not reachable: Ensure GeideaWebECR.exe is running on this machine (check Task Manager). " +
                "Then verify port 5000 is not blocked by Windows Firewall.",
            string m when m.StartsWith("ECR_CONNECT_ERROR") =>
                "Could not connect to ECR service. Verify GeideaWebECR.exe is running and the ServiceUrl in settings is correct.",
            string m when m.StartsWith("ECR_EXCEPTION") =>
                "ECR connection failed with an unexpected error. Check browser console for details.",
            _ => message
        };

        var prevState = State.SessionState;
        StopConnectionTimeout();
        StopTransactionTimeout();
        _logger.LogWarning("[Geidea Lifecycle] Socket error — raw: {Raw} | ui: {UI}", message, uiMessage);
        UpdateState(State with { SessionState = PosSessionState.Error, LastErrorMessage = uiMessage });
        LogStructuredEvent("OnSocketError", "SOCKET_ERROR", prevState, PosSessionState.Error);
    }

    [JSInvokable]
    public void OnSocketClose(int code, string reason)
    {
        var prevState = State.SessionState;
        _logger.LogInformation("[Geidea Lifecycle] WebSocket close event received. Code={Code} Reason={Reason}", code, reason);

        if (code == 1006)
        {
            _logger.LogWarning("[POS ECR] WebSocket closed abnormally (1006): {Reason}", reason);
            if (State.SessionState == PosSessionState.Connecting)
            {
                StopConnectionTimeout();
                UpdateState(State with
                {
                    SessionState = PosSessionState.Error,
                    LastErrorMessage = "ECR service unreachable (connection refused). " +
                                       "Ensure GeideaWebECR.exe is running on this machine and port 5000 is accessible."
                });
                LogStructuredEvent("OnSocketClose", "ABNORMAL_CLOSE_1006", prevState, PosSessionState.Error);
                return;
            }
        }

        StopConnectionTimeout();
        StopTransactionTimeout();
        UpdateState(State with { SessionState = PosSessionState.Disconnected });
        LogStructuredEvent("OnSocketClose", "DISCONNECTED", prevState, PosSessionState.Disconnected);
    }

    [JSInvokable]
    public async Task OnMessageReceived(string rawJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(rawJson);
            string? eventStr = null;
            if (doc.RootElement.TryGetProperty("Event", out var eventElem))
            {
                eventStr = eventElem.GetString();
            }

            string? eventName = null;
            if (doc.RootElement.TryGetProperty("EventName", out var eventNameElem))
            {
                eventName = eventNameElem.GetString();
            }

            var callbackEvent = !string.IsNullOrEmpty(eventStr)
                ? GeideaEventMapper.ParseCallbackEvent(eventStr)
                : GeideaCallbackEvent.OnDataReceive;

            GeideaTerminalAction? parsedAction = null;
            if (doc.RootElement.TryGetProperty("TerminalAction", out var actionElem))
            {
                var actStr = actionElem.GetString();
                if (actStr != null) parsedAction = GeideaEventMapper.ParseTerminalAction(actStr);
            }

            GeideaTerminalStatus? parsedStatus = null;
            if (doc.RootElement.TryGetProperty("TerminalStatus", out var statusElem))
            {
                var statStr = statusElem.GetString();
                if (statStr != null) parsedStatus = GeideaEventMapper.ParseTerminalStatus(statStr);
            }

            LogMessageDiagnostic(callbackEvent, eventName, parsedAction, parsedStatus, State.SessionState, rawJson);

            switch (callbackEvent)
            {
                case GeideaCallbackEvent.OnConnect:
                    await HandleOnConnectAsync(rawJson, eventName);
                    break;

                case GeideaCallbackEvent.OnDisConnect:
                    await HandleOnDisconnectAsync(rawJson, eventName);
                    break;

                case GeideaCallbackEvent.OnTerminalAction:
                    HandleOnTerminalAction(rawJson, parsedAction);
                    break;

                case GeideaCallbackEvent.OnTerminalStatus:
                    HandleOnTerminalStatus(rawJson, parsedStatus);
                    break;

                case GeideaCallbackEvent.OnDataReceive:
                    await HandleOnDataReceiveAsync(doc, rawJson, eventName, parsedStatus);
                    break;

                case GeideaCallbackEvent.OnError:
                    HandleOnError(rawJson, eventName);
                    break;

                case GeideaCallbackEvent.OnWrite:
                    _logger.LogDebug("[Geidea Lifecycle] OnWrite payload received: {Raw}", rawJson);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Geidea Diagnostic] Exception while processing incoming message: {RawJson}", rawJson);
        }
    }

    private async Task HandleOnConnectAsync(string rawJson, string? eventName)
    {
        var prevState = State.SessionState;
        ConnectionStatusResponseDto? connectDto = null;
        try
        {
            connectDto = JsonSerializer.Deserialize<ConnectionStatusResponseDto>(rawJson, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Geidea Diagnostic] Failed to parse ConnectionStatusResponseDto");
        }

        _logger.LogInformation("[Geidea Lifecycle] OnConnect received: IsConnected={IsConnected} Status={Status} Message={Message}",
            connectDto?.IsConnected, connectDto?.Status, connectDto?.Message);

        if (connectDto?.IsConnected == "True")
        {
            StopConnectionTimeout();
            var sessionState = (prevState == PosSessionState.TransactionInProgress ||
                                prevState == PosSessionState.AwaitingCard ||
                                prevState == PosSessionState.AwaitingPin ||
                                prevState == PosSessionState.Processing)
                ? prevState
                : PosSessionState.Connected;

            UpdateState(State with { SessionState = sessionState, LastErrorMessage = null });
            LogStructuredEvent("OnConnect", eventName ?? "CONNECT_SUCCESS", prevState, sessionState);

            if (_pendingCommandPayload != null && sessionState == PosSessionState.Connected)
            {
                var payload = _pendingCommandPayload;
                _pendingCommandPayload = null;
                _logger.LogInformation("[Geidea Lifecycle] Sending pending command payload after terminal connect...");
                await _interop.SendAsync(payload);
                StartTransactionTimeout();
            }
        }
        else
        {
            if (!_hasAttemptedFullConnect)
            {
                _hasAttemptedFullConnect = true;
                _logger.LogInformation("[Geidea Lifecycle] OnConnect indicated disconnected. Sending ONE-TIME CONNECT command...");
                var connectJson = BuildConnectJson();
                if (!string.IsNullOrEmpty(connectJson))
                {
                    _logger.LogInformation("[Geidea Lifecycle] Terminal CONNECT command sent: {Payload}", connectJson);
                    await _interop.SendAsync(connectJson);
                }
            }
            else
            {
                StopConnectionTimeout();
                UpdateState(State with
                {
                    SessionState = PosSessionState.Error,
                    LastErrorMessage = "Terminal not connected in Geidea ECR agent — please open GeideaWebECR.exe and verify terminal connection."
                });
                LogStructuredEvent("OnConnect", eventName ?? "CONNECT_FAILED", prevState, PosSessionState.Error);
            }
        }
    }

    private async Task HandleOnDisconnectAsync(string rawJson, string? eventName)
    {
        var prevState = State.SessionState;
        _logger.LogInformation("[Geidea Lifecycle] OnDisConnect received raw: {RawJson}", rawJson);

        if (prevState == PosSessionState.TransactionInProgress ||
            prevState == PosSessionState.AwaitingCard ||
            prevState == PosSessionState.AwaitingPin ||
            prevState == PosSessionState.Processing)
        {
            // Transient TCP port flap from Geidea service during transaction.
            // DO NOT stop transaction timeout or clear active payment ID!
            _logger.LogWarning("[Geidea Lifecycle] Transient OnDisConnect (PORT_CLOSED) received during active transaction. Retaining active payment ID={PaymentId} and keeping transaction active.", _activePaymentId);
            LogStructuredEvent("OnDisConnect", "PORT_CLOSED_TRANSIENT", prevState, prevState);
            return;
        }

        if (!_hasAttemptedFullConnect && State.SessionState == PosSessionState.Connecting)
        {
            _hasAttemptedFullConnect = true;
            _logger.LogInformation("[Geidea Lifecycle] Disconnect received during initial check. Attempting CONNECT...");
            var connectJson = BuildConnectJson();
            if (!string.IsNullOrEmpty(connectJson))
            {
                await _interop.SendAsync(connectJson);
                return;
            }
        }

        StopConnectionTimeout();
        StopTransactionTimeout();
        _activePaymentId = null;
        UpdateState(State with { SessionState = PosSessionState.Disconnected, LastErrorMessage = "Terminal disconnected" });
        LogStructuredEvent("OnDisConnect", eventName ?? "DISCONNECT", prevState, PosSessionState.Disconnected);
    }

    private void HandleOnTerminalAction(string rawJson, GeideaTerminalAction? parsedAction)
    {
        var action = parsedAction ?? GeideaTerminalAction.Unknown;
        var prevState = State.SessionState;
        _logger.LogInformation("[Geidea Lifecycle] OnTerminalAction received: Action={Action} Raw={RawJson}", action, rawJson);

        if (GeideaEventMapper.IsTerminalErrorAction(action))
        {
            StopTransactionTimeout();
            _activePaymentId = null;
            var errMsg = GetTerminalActionPrompt(action);
            UpdateState(State with
            {
                SessionState = PosSessionState.Error,
                LastTerminalAction = action,
                LastErrorMessage = errMsg
            });
            LogStructuredEvent("OnTerminalAction", action.ToString(), prevState, PosSessionState.Error);
            _logger.LogWarning("[Geidea Lifecycle] Payment transaction failed due to terminal error action: {Action} | {Msg}", action, errMsg);
        }
        else if (action == GeideaTerminalAction.IdleScreen)
        {
            if (prevState == PosSessionState.TransactionInProgress ||
                prevState == PosSessionState.AwaitingCard ||
                prevState == PosSessionState.AwaitingPin ||
                prevState == PosSessionState.Processing)
            {
                StopTransactionTimeout();
                _activePaymentId = null;
                UpdateState(State with
                {
                    SessionState = PosSessionState.Error,
                    LastTerminalAction = action,
                    LastErrorMessage = "Transaction cancelled on terminal (returned to idle screen)"
                });
                LogStructuredEvent("OnTerminalAction", "IDLE_SCREEN_CANCEL", prevState, PosSessionState.Error);
                _logger.LogWarning("[Geidea Lifecycle] Payment cancelled: Terminal returned to IDLE_SCREEN during active transaction.");
            }
            else
            {
                UpdateState(State with { LastTerminalAction = action });
                LogStructuredEvent("OnTerminalAction", "IDLE_SCREEN", prevState, prevState);
            }
        }
        else
        {
            var sessionState = prevState;
            if (action == GeideaTerminalAction.SwipeOrInsert) sessionState = PosSessionState.AwaitingCard;
            else if (action == GeideaTerminalAction.PinEntry) sessionState = PosSessionState.AwaitingPin;
            else if (action == GeideaTerminalAction.SendingRequest || action == GeideaTerminalAction.CardRemoved || action == GeideaTerminalAction.WaitingResponse) sessionState = PosSessionState.Processing;

            if (action == GeideaTerminalAction.Connecting || action == GeideaTerminalAction.ReConnect)
            {
                _logger.LogInformation("[Geidea Lifecycle] Terminal handshake in progress ({Action}). Refreshing connection timeout.", action);
                StartConnectionTimeout();
            }

            UpdateState(State with { SessionState = sessionState, LastTerminalAction = action });
            LogStructuredEvent("OnTerminalAction", action.ToString(), prevState, sessionState);
        }
    }

    private void HandleOnTerminalStatus(string rawJson, GeideaTerminalStatus? parsedStatus)
    {
        var status = parsedStatus ?? GeideaTerminalStatus.Unknown;
        var prevState = State.SessionState;
        var sessionState = prevState;

        _logger.LogInformation("[Geidea Lifecycle] OnTerminalStatus received: Status={Status}", status);

        if (status == GeideaTerminalStatus.Available && prevState == PosSessionState.Connecting)
        {
            StopConnectionTimeout();
            sessionState = PosSessionState.Connected;
            _logger.LogInformation("[Geidea Lifecycle] Terminal status is AVAILABLE — transitioning state to Connected.");
        }

        UpdateState(State with { TerminalStatus = status, SessionState = sessionState });
        LogStructuredEvent("OnTerminalStatus", status.ToString(), prevState, sessionState);
    }

    private async Task HandleOnDataReceiveAsync(JsonDocument doc, string rawJson, string? eventName, GeideaTerminalStatus? parsedStatus)
    {
        _logger.LogInformation("[Geidea Lifecycle] OnDataReceive event: EventName={EventName}", eventName);

        if (!string.IsNullOrEmpty(eventName) && !GeideaEventMapper.IsSupportedEventName(eventName))
        {
            _logger.LogWarning("[Geidea Lifecycle] Unsupported EventName in OnDataReceive: {EventName}", eventName);
        }

        if (eventName == "TERMINAL_RESPONSE")
        {
            await ProcessTerminalResponseAsync(rawJson);
        }
        else if (eventName == "TERMINAL_SETTINGS")
        {
            await ProcessTerminalSettingsAsync(rawJson);
        }
        else if (eventName == "TERMINAL_STATUS")
        {
            HandleOnTerminalStatus(rawJson, parsedStatus);
        }
        else
        {
            _logger.LogInformation("[Geidea Lifecycle] Handled OnDataReceive {EventName} event.", eventName);
            LogStructuredEvent("OnDataReceive", eventName ?? "GENERIC_DATA", State.SessionState, State.SessionState);
        }
    }

    private async Task ProcessTerminalResponseAsync(string rawJson)
    {
        StopTransactionTimeout();
        var prevState = State.SessionState;

        if (_activePaymentId == null)
        {
            _logger.LogWarning("[Geidea Lifecycle] Orphaned TERMINAL_RESPONSE received — no active payment ID!");
            return;
        }

        TerminalResponseDto? terminalResponseDto = null;
        try
        {
            terminalResponseDto = JsonSerializer.Deserialize<TerminalResponseDto>(rawJson, JsonOptions);
        }
        catch (Exception parseEx)
        {
            _logger.LogError(parseEx, "[Geidea Diagnostic] Failed to deserialize TerminalResponseDto from raw JSON: {RawJson}", rawJson);
        }

        if (terminalResponseDto?.JsonResult == null)
        {
            _logger.LogError("[Geidea Diagnostic] TerminalResponseDto.JsonResult is null or empty. Raw JSON: {RawJson}", rawJson);
            UpdateState(State with { SessionState = PosSessionState.Error, LastErrorMessage = "Invalid terminal response payload" });
            LogStructuredEvent("OnDataReceive", "TERMINAL_RESPONSE_INVALID", prevState, PosSessionState.Error);
            _activePaymentId = null;
            return;
        }

        TerminalResponseInnerDto? innerDto = null;
        try
        {
            innerDto = JsonSerializer.Deserialize<TerminalResponseInnerDto>(terminalResponseDto.JsonResult, JsonOptions);
        }
        catch (Exception innerEx)
        {
            _logger.LogError(innerEx, "[Geidea Diagnostic] Failed to deserialize TerminalResponseInnerDto from JsonResult string: {JsonResult}", terminalResponseDto.JsonResult);
        }

        if (innerDto == null)
        {
            _logger.LogError("[Geidea Diagnostic] TerminalResponseInnerDto deserialized to null. JsonResult: {JsonResult}", terminalResponseDto.JsonResult);
            UpdateState(State with { SessionState = PosSessionState.Error, LastErrorMessage = "Corrupted transaction result payload" });
            LogStructuredEvent("OnDataReceive", "TERMINAL_RESPONSE_NULL_INNER", prevState, PosSessionState.Error);
            _activePaymentId = null;
            return;
        }

        var completeDto = new CompletePosTerminalPaymentDto
        {
            PaymentId = _activePaymentId.Value,
            SamaCode = innerDto.TransactionResponse,
            RawJsonResult = terminalResponseDto.JsonResult,
            TerminalId = innerDto.CardAcceptorTerminalId,
            TransactionStan = innerDto.TransactionSTAN,
            Rrn = innerDto.RetrievalReferenceNumber,
            AuthCode = innerDto.TransactionAuthCode,
            MaskedPan = innerDto.PrimaryAccountNumber,
            CardScheme = innerDto.CardScheme
        };

        await _paymentAppService.CompletePosTerminalPaymentAsync(completeDto);

        var outcome = SamaResponseCodeMapping.Evaluate(innerDto.TransactionResponse);
        var desc = GeideaEventMapper.GetSamaResponseDescription(innerDto.TransactionResponse, _isArabic);

        UpdateState(State with
        {
            SessionState = PosSessionState.Completed,
            SamaCode = innerDto.TransactionResponse,
            Outcome = outcome,
            SamaDescription = desc,
            MaskedPan = innerDto.PrimaryAccountNumber,
            CardScheme = innerDto.CardScheme,
            AuthCode = innerDto.TransactionAuthCode,
            Rrn = innerDto.RetrievalReferenceNumber
        });

        LogStructuredEvent("OnDataReceive", "TERMINAL_RESPONSE", prevState, PosSessionState.Completed);
        _logger.LogInformation("[Geidea Lifecycle] Payment completed successfully. PaymentId={PaymentId} SamaCode={SamaCode} Outcome={Outcome}",
            _activePaymentId.Value, innerDto.TransactionResponse, outcome);

        _activePaymentId = null;
    }

    private async Task ProcessTerminalSettingsAsync(string rawJson)
    {
        var prevState = State.SessionState;
        try
        {
            var terminalSettingsDto = JsonSerializer.Deserialize<TerminalSettingsResponseDto>(rawJson, JsonOptions);
            if (terminalSettingsDto?.JsonResult != null)
            {
                var innerDto = JsonSerializer.Deserialize<TerminalSettingsInnerDto>(terminalSettingsDto.JsonResult, JsonOptions);
                _logger.LogInformation("[Geidea Lifecycle] Terminal settings received: Menu={Menu}, Printer={Printer}", innerDto?.IsMenuEnabled, innerDto?.IsPrinterEnabled);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Geidea Diagnostic] Could not parse TerminalSettings payload");
        }

        StopConnectionTimeout();
        UpdateState(State with { SessionState = PosSessionState.Connected, LastErrorMessage = null });
        LogStructuredEvent("OnDataReceive", "TERMINAL_SETTINGS", prevState, PosSessionState.Connected);

        if (_pendingCommandPayload != null)
        {
            var payload = _pendingCommandPayload;
            _pendingCommandPayload = null;
            _logger.LogInformation("[Geidea Lifecycle] Sending pending command payload after settings check...");
            await _interop.SendAsync(payload);
            StartTransactionTimeout();
        }
    }

    private void HandleOnError(string rawJson, string? eventName)
    {
        ErrorResponseDto? errorDto = null;
        try
        {
            errorDto = JsonSerializer.Deserialize<ErrorResponseDto>(rawJson, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Geidea Diagnostic] Could not parse ErrorResponseDto");
        }

        var message = errorDto?.Message ?? string.Empty;
        var reason = errorDto?.Reason ?? string.Empty;

        if (message.Equals("PORT_ERROR", StringComparison.OrdinalIgnoreCase) ||
            reason.Contains("Already connected", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("[Geidea Lifecycle] OnError received PORT_ERROR ('Already connected'). Treating as CONNECTED.");
            StopConnectionTimeout();
            var prevState = State.SessionState;
            var sessionState = (prevState == PosSessionState.TransactionInProgress ||
                                prevState == PosSessionState.AwaitingCard ||
                                prevState == PosSessionState.AwaitingPin ||
                                prevState == PosSessionState.Processing)
                ? prevState
                : PosSessionState.Connected;

            UpdateState(State with { SessionState = sessionState, LastErrorMessage = null });
            LogStructuredEvent("OnError", "ALREADY_CONNECTED", prevState, sessionState);
            return;
        }

        StopConnectionTimeout();
        StopTransactionTimeout();
        _activePaymentId = null;

        var errMsg = !string.IsNullOrEmpty(reason) ? $"{message}: {reason}" : (string.IsNullOrEmpty(message) ? "Terminal Error" : message);
        var previousState = State.SessionState;
        UpdateState(State with { SessionState = PosSessionState.Error, LastErrorMessage = errMsg });
        LogStructuredEvent("OnError", eventName ?? "ERROR", previousState, PosSessionState.Error);
        _logger.LogError("[Geidea Lifecycle] OnError received: Message={Message} Reason={Reason}", message, reason);
    }

    private void LogMessageDiagnostic(
        GeideaCallbackEvent callbackEvent,
        string? eventName,
        GeideaTerminalAction? action,
        GeideaTerminalStatus? status,
        PosSessionState sessionState,
        string rawJson)
    {
        _logger.LogInformation(
            "[Geidea Diagnostic] Timestamp={Timestamp:o} | Event={Event} | EventName={EventName} | TerminalAction={TerminalAction} | TerminalStatus={TerminalStatus} | SessionState={SessionState} | RawJson={RawJson}",
            DateTime.UtcNow,
            callbackEvent,
            eventName ?? "(none)",
            action?.ToString() ?? "(none)",
            status?.ToString() ?? "(none)",
            sessionState,
            rawJson);
    }

    private void LogStructuredEvent(
        string callbackEvent,
        string eventName,
        PosSessionState previousState,
        PosSessionState newState)
    {
        _logger.LogInformation(
            "[Geidea]\nEvent={Event}\nEventName={EventName}\nPreviousState={PreviousState}\nNewState={NewState}",
            callbackEvent, eventName, previousState, newState);
    }

    private static string GetTerminalActionPrompt(GeideaTerminalAction action) => action switch
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
        _ => $"Terminal Action: {action}"
    };
}
