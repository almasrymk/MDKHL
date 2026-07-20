using System.Collections.Frozen;

namespace SmartParking.Enums.PosTerminal;

/// <summary>
/// Maps 3-digit SAMA response codes to Approved/Declined outcome.
/// Source: Geidea Web ECR spec §8.2.
/// Approved (purchase) codes: 000, 001, 003, 007, 087, 089, 400.
/// Reconciliation codes: 500 (successful), 501 (failed).
/// All other 3-digit codes = Declined.
/// </summary>
public static class SamaResponseCodeMapping
{
    private static readonly FrozenSet<string> ApprovedCodes = new[]
    {
        "000", "001", "003", "007", "087", "089", "400"
    }.ToFrozenSet();

    public static SamaResponseOutcome Evaluate(string? threeDigitCode)
    {
        if (string.IsNullOrEmpty(threeDigitCode)) return SamaResponseOutcome.Declined;
        if (ApprovedCodes.Contains(threeDigitCode)) return SamaResponseOutcome.Approved;
        if (threeDigitCode == "500") return SamaResponseOutcome.ReconciliationSuccessful;
        if (threeDigitCode == "501") return SamaResponseOutcome.ReconciliationFailed;
        return SamaResponseOutcome.Declined;
    }
}
