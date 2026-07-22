using SmartParking.Enums.PosTerminal;

namespace SmartParking.Blazor.Components.PosTerminal;

/// <summary>
/// Immutable snapshot of POS terminal session state for Blazor UI binding.
/// Intentionally excludes raw response payloads from UI binding.
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
