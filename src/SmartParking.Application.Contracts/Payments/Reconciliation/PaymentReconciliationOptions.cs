namespace SmartParking.Payments.Reconciliation;

/// <summary>
/// Strongly typed configuration options for payment reconciliation background job and timeouts.
/// Bound from paymentconfig.json "Payment" section.
/// </summary>
public class PaymentReconciliationOptions
{
    public const string SectionName = "Payment";

    /// <summary>
    /// Active payment provider (e.g. Geidea, Oppwa).
    /// </summary>
    public string ActiveProvider { get; set; } = "Geidea";

    /// <summary>
    /// Interval in minutes between reconciliation worker executions.
    /// Default is 10 minutes.
    /// </summary>
    public int ReconciliationIntervalMinutes { get; set; } = 10;

    /// <summary>
    /// Timeout in minutes before a pending payment is considered stale and reconciled.
    /// Default is 10 minutes.
    /// </summary>
    public int PendingTimeoutMinutes { get; set; } = 10;
}
