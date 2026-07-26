using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using SmartParking.Enums.PosTerminal;

namespace SmartParking.Payments.PosTerminal;

public static class GeideaEventMapper
{
    private static readonly FrozenDictionary<string, GeideaCallbackEvent> EventsMap = new Dictionary<string, GeideaCallbackEvent>(System.StringComparer.OrdinalIgnoreCase)
    {
        { "OnConnect", GeideaCallbackEvent.OnConnect },
        { "OnDisConnect", GeideaCallbackEvent.OnDisConnect },
        { "OnError", GeideaCallbackEvent.OnError },
        { "OnTerminalAction", GeideaCallbackEvent.OnTerminalAction },
        { "OnTerminalStatus", GeideaCallbackEvent.OnTerminalStatus },
        { "OnDataReceive", GeideaCallbackEvent.OnDataReceive },
        { "OnWrite", GeideaCallbackEvent.OnWrite }
    }.ToFrozenDictionary(System.StringComparer.OrdinalIgnoreCase);

    private static readonly FrozenDictionary<string, GeideaTerminalAction> ActionsMap = new Dictionary<string, GeideaTerminalAction>(System.StringComparer.OrdinalIgnoreCase)
    {
        // Standard Actions
        { "SWIPE_OR_INSERT", GeideaTerminalAction.SwipeOrInsert },
        { "CARD_INSERTED", GeideaTerminalAction.CardInserted },
        { "CARD_REMOVED", GeideaTerminalAction.CardRemoved },
        { "PIN_ENTRY", GeideaTerminalAction.PinEntry },
        { "PIN_QUIT", GeideaTerminalAction.PinQuit },
        { "IDLE_SCREEN", GeideaTerminalAction.IdleScreen },
        { "CONNECTING", GeideaTerminalAction.Connecting },
        { "RECONNECT", GeideaTerminalAction.ReConnect },
        { "RECONNECTING", GeideaTerminalAction.ReConnect },
        { "SENDING_REQUEST", GeideaTerminalAction.SendingRequest },
        { "WAITING_RESPONSE", GeideaTerminalAction.WaitingResponse },
        { "SAF_PROCESSING", GeideaTerminalAction.SafProcessing },
        { "CONNECTION_MADE", GeideaTerminalAction.ConnectionMade },
        { "RECONCILIATION_SUCCESSFUL", GeideaTerminalAction.ReconciliationSuccessful },
        { "CONNECTION_CLOSED", GeideaTerminalAction.ConnectionClosed },

        // Error Codes 1
        { "USER_CANCELLED_AND_TIMEOUT", GeideaTerminalAction.UserCancelledAndTimeout },
        { "CARD_READ_ERROR", GeideaTerminalAction.CardReadError },
        { "DATA_ERROR", GeideaTerminalAction.DataError },
        { "BLOCKED_CARD", GeideaTerminalAction.BlockedCard },
        { "NO_ACTIVE_APPLICATION_FOUND", GeideaTerminalAction.NoActiveApplicationFound },
        { "INSERT_CARD_ONLY_USE_SMART_CARD_READER", GeideaTerminalAction.InsertCardOnlyUseSmartCardReader },
        { "MAXIMUM_AMOUNT_LIMIT_EXCEEDED", GeideaTerminalAction.MaximumAmountLimitExceeded },
        { "TRANSACTION_CANCELLED", GeideaTerminalAction.TransactionCancelled },

        // Error Codes 2
        { "WRONG_PIN", GeideaTerminalAction.WrongPin },
        { "RETRY_LIMIT_EXCEEDED", GeideaTerminalAction.RetryLimitExceeded },
        { "MAXIMUM_SAF_REACHED", GeideaTerminalAction.MaximumSafReached },
        { "MAG_STRIPE_NOT_SUPPORTED", GeideaTerminalAction.MagStripeNotSupported },
        { "CARD_SCHEME_NOT_SUPPORTED", GeideaTerminalAction.CardSchemeNotSupported },
        { "DE_SAF_FAILED", GeideaTerminalAction.DeSafFailed },
        { "RECONCILIATION_NO_BUSINESS", GeideaTerminalAction.ReconciliationNoBusiness },
        { "RECONCILIATION_FAILED", GeideaTerminalAction.ReconciliationFailed },
        { "NO_TXN_TO_REVERSE", GeideaTerminalAction.NoTxnToReverse },
        { "TRY_CONTACTING_INTERFACE", GeideaTerminalAction.TryContactingInterface },
        { "SUPERVISOR_PASSWORD_QUIT", GeideaTerminalAction.SupervisorPasswordQuit },
        { "GPRS_DOWN", GeideaTerminalAction.GprsDown },
        { "REFUND_PAN_NOT_MATCHING", GeideaTerminalAction.RefundPanNotMatching },
        { "INVALID_PASSWORD", GeideaTerminalAction.InvalidPassword },
        { "CARD_NOT_DETECTED", GeideaTerminalAction.CardNotDetected }
    }.ToFrozenDictionary(System.StringComparer.OrdinalIgnoreCase);

    private static readonly FrozenDictionary<string, GeideaTerminalStatus> StatusesMap = new Dictionary<string, GeideaTerminalStatus>(System.StringComparer.OrdinalIgnoreCase)
    {
        { "BUSY", GeideaTerminalStatus.Busy },
        { "AVAILABLE", GeideaTerminalStatus.Available }
    }.ToFrozenDictionary(System.StringComparer.OrdinalIgnoreCase);

    private static readonly FrozenSet<string> SupportedEventNames = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
    {
        "TERMINAL_RESPONSE",
        "TERMINAL_SETTINGS",
        "TERMINAL_STATUS",
        "TERMINAL_ACTION",
        "MERCHANT_INFO",
        "TERMINAL_ID",
        "AVAILABLE_PORTS",
        "CHECK_STATUS"
    }.ToFrozenSet(System.StringComparer.OrdinalIgnoreCase);

    private static readonly FrozenDictionary<string, (string English, string Arabic)> SamaMessagesMap = new Dictionary<string, (string, string)>
    {
        { "000", ("Approved", "مقبول") },
        { "001", ("Approved", "مقبول") },
        { "003", ("Approved", "مقبول") },
        { "007", ("Approved", "مقبول") },
        { "087", ("Approved", "مقبول") },
        { "089", ("Approved", "مقبول") },
        { "400", ("Approved", "مقبول") },
        { "500", ("Approved", "مقبول") },
        { "501", ("Approved", "مقبول") },
        { "101", ("Expired Card", "بطاقة منتهية الصلاحية") },
        { "116", ("Insufficient Funds", "الرصيد غير كافٍ") },
        { "117", ("Incorrect PIN", "الرقم السري خاطئ") },
        { "121", ("Withdrawal Limit Exceeded", "تجاوز الحد المسموح به") },
        { "200", ("Invalid Card", "بطاقة غير صالحة") },
        { "909", ("System Malfunction", "عطل في النظام") }
    }.ToFrozenDictionary();

    public static GeideaCallbackEvent ParseCallbackEvent(string rawEvent)
        => EventsMap.TryGetValue(rawEvent, out var ev) ? ev : GeideaCallbackEvent.OnWrite;

    public static GeideaTerminalAction ParseTerminalAction(string rawAction)
        => ActionsMap.TryGetValue(rawAction, out var act) ? act : GeideaTerminalAction.Unknown;

    public static GeideaTerminalStatus ParseTerminalStatus(string rawStatus)
        => StatusesMap.TryGetValue(rawStatus, out var stat) ? stat : GeideaTerminalStatus.Unknown;

    public static bool IsSupportedEventName(string eventName)
        => SupportedEventNames.Contains(eventName);

    public static bool IsTerminalErrorAction(GeideaTerminalAction action) => action switch
    {
        GeideaTerminalAction.UserCancelledAndTimeout => true,
        GeideaTerminalAction.CardReadError => true,
        GeideaTerminalAction.DataError => true,
        GeideaTerminalAction.BlockedCard => true,
        GeideaTerminalAction.NoActiveApplicationFound => true,
        GeideaTerminalAction.InsertCardOnlyUseSmartCardReader => true,
        GeideaTerminalAction.MaximumAmountLimitExceeded => true,
        GeideaTerminalAction.TransactionCancelled => true,
        GeideaTerminalAction.WrongPin => true,
        GeideaTerminalAction.RetryLimitExceeded => true,
        GeideaTerminalAction.MaximumSafReached => true,
        GeideaTerminalAction.MagStripeNotSupported => true,
        GeideaTerminalAction.CardSchemeNotSupported => true,
        GeideaTerminalAction.DeSafFailed => true,
        GeideaTerminalAction.ReconciliationNoBusiness => true,
        GeideaTerminalAction.ReconciliationFailed => true,
        GeideaTerminalAction.NoTxnToReverse => true,
        GeideaTerminalAction.TryContactingInterface => true,
        GeideaTerminalAction.SupervisorPasswordQuit => true,
        GeideaTerminalAction.GprsDown => true,
        GeideaTerminalAction.RefundPanNotMatching => true,
        GeideaTerminalAction.InvalidPassword => true,
        GeideaTerminalAction.CardNotDetected => true,
        GeideaTerminalAction.PinQuit => true,
        _ => false
    };

    public static string GetSamaResponseDescription(string responseCode, bool isArabic)
    {
        if (SamaMessagesMap.TryGetValue(responseCode, out var msgs))
        {
            return isArabic ? msgs.Arabic : msgs.English;
        }
        return isArabic ? "عملية مرفوضة" : "Declined";
    }
}
