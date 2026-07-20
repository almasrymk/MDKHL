namespace SmartParking.Enums.PosTerminal;

/// <summary>
/// Discriminates the type of terminal transaction against a payment.
/// Allows multiple terminal details per payment (Initial Purchase, Refunds, Reversals).
/// </summary>
public enum PosTransactionType
{
    Purchase = 0,
    Refund = 1,
    Reversal = 2
}
