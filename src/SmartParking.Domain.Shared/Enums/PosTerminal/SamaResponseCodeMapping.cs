using System.Collections.Frozen;

namespace SmartParking.Enums.PosTerminal;

/// <summary>
/// Maps 3-digit SAMA response codes to Approved/Declined outcome.
/// Source: Geidea Web ECR spec §8.2.
/// Approved codes: 000, 001, 003, 007, 087, 089, 400, 500, 501.
/// All other 3-digit codes = Declined.
/// </summary>
public static class SamaResponseCodeMapping
{
    private static readonly FrozenSet<string> ApprovedCodes = new[]
    {
        "000", "001", "003", "007", "087", "089", "400", "500", "501"
    }.ToFrozenSet();

    public static SamaResponseOutcome Evaluate(string threeDigitCode)
        => ApprovedCodes.Contains(threeDigitCode)
            ? SamaResponseOutcome.Approved
            : SamaResponseOutcome.Declined;
}
