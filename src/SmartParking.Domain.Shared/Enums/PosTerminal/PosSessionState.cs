namespace SmartParking.Enums.PosTerminal;

/// <summary>
/// Runtime-only state for Blazor component. Never persisted.
/// Drives UI indicators (connection dot, action prompts, result display).
/// </summary>
public enum PosSessionState
{
    Disconnected,
    Connecting,
    Connected,
    TransactionInProgress,
    AwaitingCard,
    AwaitingPin,
    Processing,
    Completed,
    Error
}
