namespace SmartParking.Enums.PosTerminal;

/// <summary>
/// Maps Geidea terminal action strings from OnTerminalAction events.
/// Includes error codes from spec §8.1 (Error Codes 1 + Error Codes 2).
/// Raw string to enum mapping done in GeideaEventMapper.
/// </summary>
public enum GeideaTerminalAction
{
    // Standard actions
    SwipeOrInsert,
    CardInserted,
    CardRemoved,
    PinEntry,
    PinQuit,
    IdleScreen,
    Connecting,
    ReConnect,
    SendingRequest,
    WaitingResponse,
    SafProcessing,
    ConnectionMade,
    ReconciliationSuccessful,
    ConnectionClosed,

    // Error Codes 1 (before card presented)
    UserCancelledAndTimeout,
    CardReadError,
    DataError,
    BlockedCard,
    NoActiveApplicationFound,
    InsertCardOnlyUseSmartCardReader,
    MaximumAmountLimitExceeded,
    TransactionCancelled,

    // Error Codes 2 (after card presented)
    WrongPin,
    RetryLimitExceeded,
    MaximumSafReached,
    MagStripeNotSupported,
    CardSchemeNotSupported,
    DeSafFailed,
    ReconciliationNoBusiness,
    ReconciliationFailed,
    NoTxnToReverse,
    TryContactingInterface,
    SupervisorPasswordQuit,
    GprsDown,
    RefundPanNotMatching,
    InvalidPassword,
    CardNotDetected,

    // Fallback
    Unknown
}
