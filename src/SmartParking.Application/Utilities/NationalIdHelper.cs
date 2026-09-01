namespace SmartParking.Utilities;

/// <summary>
/// Helper methods for NationalId normalization.
/// </summary>
public static class NationalIdHelper
{
    /// <summary>
    /// Normalizes NationalId: empty/whitespace becomes null, otherwise trimmed.
    /// </summary>
    public static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
