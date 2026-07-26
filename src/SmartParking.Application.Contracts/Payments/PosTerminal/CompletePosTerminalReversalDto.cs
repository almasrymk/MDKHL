using System;

namespace SmartParking.Payments.PosTerminal;

public class CompletePosTerminalReversalDto
{
    public Guid PaymentId { get; set; }
    public string SamaCode { get; set; } = default!;
    public string RawJsonResult { get; set; } = default!;
    public string? TerminalId { get; set; }
    public string? TransactionStan { get; set; }
    public string? Rrn { get; set; }
    public string? AuthCode { get; set; }
    public string? MaskedPan { get; set; }
    public string? CardScheme { get; set; }
}
