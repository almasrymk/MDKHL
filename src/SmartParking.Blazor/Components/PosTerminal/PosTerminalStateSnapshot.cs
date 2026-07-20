using SmartParking.Enums.PosTerminal;

namespace SmartParking.Blazor.Components.PosTerminal;

/// <summary>
/// Immutable snapshot of POS terminal session state for Blazor UI binding.
/// Intentionally excludes RawJsonResult and full EMV tag data — that data is
/// potentially large and sensitive (merchant info, cryptograms, etc.) and belongs
/// exclusively in the persisted <see cref="SmartParking.Entities.Payments.PosTerminalTransactionDetail"/> audit
/// record (TASK-007). It must never reach the UI layer.
/// </summary>
public sealed record PosTerminalStateSnapshot(
    PosSessionState SessionState,
    GeideaTerminalAction? LastTerminalAction = null,
    GeideaTerminalStatus? TerminalStatus = null,
    string? LastErrorMessage = null,
    string? SamaCode = null,
    SamaResponseOutcome? Outcome = null,
    string? SamaDescription = null,
    string? MaskedPan = null,
    string? CardScheme = null,
    string? AuthCode = null,
    string? Rrn = null);
