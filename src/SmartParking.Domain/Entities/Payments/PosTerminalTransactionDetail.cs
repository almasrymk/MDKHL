using SmartParking.Enums.PosTerminal;
using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Entities.Payments;

/// <summary>
/// Stores POS terminal transaction outcome data from Geidea Web ECR.
/// One-to-one with <see cref="Payment"/> via <see cref="PaymentId"/>.
/// Created only when PaymentMethod == POS and a TERMINAL_RESPONSE is received.
/// Absent for Cash, online Geidea, and other payment methods.
/// </summary>
public class PosTerminalTransactionDetail : FullAuditedEntity<Guid>, IMultiTenant
{
    /// <summary>FK to Payment. Required, unique.</summary>
    public Guid PaymentId { get; private set; }

    /// <summary>3-digit SAMA response code (e.g. "000", "116").</summary>
    public string SamaResponseCode { get; private set; }

    /// <summary>Mapped outcome: Approved or Declined.</summary>
    public SamaResponseOutcome Outcome { get; private set; }

    /// <summary>Terminal ID from CardAcceptorTerminalId in response.</summary>
    public string? TerminalId { get; private set; }

    /// <summary>6-digit STAN from TransactionSTAN.</summary>
    public string? TransactionStan { get; private set; }

    /// <summary>12-digit RRN from RetrievalReferenceNumber.</summary>
    public string? RetrievalReferenceNumber { get; private set; }

    /// <summary>Auth code from TransactionAuthCode.</summary>
    public string? AuthCode { get; private set; }

    /// <summary>Masked PAN from PrimaryAccountNumber (e.g. "476173******0010").</summary>
    public string? MaskedPan { get; private set; }

    /// <summary>Card scheme from response (e.g. "VISA CREDIT", "MADA").</summary>
    public string? CardScheme { get; private set; }

    /// <summary>
    /// Full raw JsonResult string from OnDataReceive TERMINAL_RESPONSE event.
    /// Stored as-is for audit/dispute purposes. Contains all EMV tags, merchant info, etc.
    /// </summary>
    public string RawJsonResult { get; private set; }

    public Guid? TenantId { get; set; }

    // Navigation
    public virtual Payment Payment { get; private set; }

    // EF Core requires parameterless constructor
    protected PosTerminalTransactionDetail() { }

    /// <summary>
    /// Factory method. Called once when TERMINAL_RESPONSE arrives from the terminal.
    /// </summary>
    public static PosTerminalTransactionDetail Create(
        Guid paymentId,
        string samaResponseCode,
        SamaResponseOutcome outcome,
        string rawJsonResult,
        string? terminalId = null,
        string? transactionStan = null,
        string? retrievalReferenceNumber = null,
        string? authCode = null,
        string? maskedPan = null,
        string? cardScheme = null,
        Guid? tenantId = null)
    {
        return new PosTerminalTransactionDetail
        {
            PaymentId = paymentId,
            SamaResponseCode = samaResponseCode,
            Outcome = outcome,
            RawJsonResult = rawJsonResult,
            TerminalId = terminalId,
            TransactionStan = transactionStan,
            RetrievalReferenceNumber = retrievalReferenceNumber,
            AuthCode = authCode,
            MaskedPan = maskedPan,
            CardScheme = cardScheme,
            TenantId = tenantId
        };
    }
}
