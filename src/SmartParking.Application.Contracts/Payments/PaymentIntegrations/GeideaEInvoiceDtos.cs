using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SmartParking.Payments.PaymentIntegrations;

// ─── Shared base response ─────────────────────────────────────────────────────

/// <summary>
/// Every Geidea API response includes these standard fields.
/// </summary>
public class GeideaBaseResponse
{
    [JsonPropertyName("responseCode")]
    public string? ResponseCode { get; init; }

    [JsonPropertyName("responseMessage")]
    public string? ResponseMessage { get; init; }

    [JsonPropertyName("detailedResponseCode")]
    public string? DetailedResponseCode { get; init; }

    [JsonPropertyName("detailedResponseMessage")]
    public string? DetailedResponseMessage { get; init; }

    /// <summary>Returns true when Geidea signals a successful operation (code "000").</summary>
    [JsonIgnore]
    public bool IsSuccess => ResponseCode == "000";
}

// ─── eInvoice line items ──────────────────────────────────────────────────────

/// <summary>
/// A single line item on a Geidea eInvoice.
/// </summary>
public class GeideaInvoiceItem
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = default!;

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("count")]
    public int Count { get; init; }

    [JsonPropertyName("price")]
    public decimal Price { get; init; }
}

// ─── Customer info ────────────────────────────────────────────────────────────

/// <summary>
/// Customer contact details attached to a Geidea eInvoice.
/// </summary>
public class GeideaCustomerDetails
{
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("email")]
    public string? Email { get; init; }

    [JsonPropertyName("phone")]
    public string? Phone { get; init; }
}

// ─── Create eInvoice ──────────────────────────────────────────────────────────

/// <summary>
/// Request body for POST /payment-intent/api/v1/direct/eInvoice.
/// </summary>
public class GeideaCreateInvoiceRequest
{
    [JsonPropertyName("amount")]
    public decimal Amount { get; init; }

    [JsonPropertyName("currency")]
    public string Currency { get; init; } = default!;

    [JsonPropertyName("merchantReferenceId")]
    public string MerchantReferenceId { get; init; } = default!;

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("expiryDate")]
    public string? ExpiryDate { get; init; }

    [JsonPropertyName("customer")]
    public GeideaCustomerDetails? Customer { get; init; }

    [JsonPropertyName("items")]
    public List<GeideaInvoiceItem>? Items { get; init; }

    [JsonPropertyName("language")]
    public string Language { get; init; } = "en";
}

/// <summary>
/// Response from POST /payment-intent/api/v1/direct/eInvoice.
/// The most important field is <see cref="PaymentIntentId"/> which must be stored
/// so you can reference the invoice in all subsequent calls.
/// </summary>
public class GeideaCreateInvoiceResponse : GeideaBaseResponse
{
    [JsonPropertyName("paymentIntentId")]
    public string? PaymentIntentId { get; init; }

    [JsonPropertyName("status")]
    public string? Status { get; init; }

    [JsonPropertyName("merchantReferenceId")]
    public string? MerchantReferenceId { get; init; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; init; }

    [JsonPropertyName("currency")]
    public string? Currency { get; init; }

    [JsonPropertyName("createdDate")]
    public string? CreatedDate { get; init; }
}

// ─── Update eInvoice ──────────────────────────────────────────────────────────

/// <summary>
/// Request body for PUT /payment-intent/api/v1/direct/eInvoice.
/// Use to change amount/items or transition the invoice status
/// (e.g. "Created" → "Sent", "Sent" → "Cancelled").
/// </summary>
public class GeideaUpdateInvoiceRequest
{
    [JsonPropertyName("paymentIntentId")]
    public string PaymentIntentId { get; init; } = default!;

    [JsonPropertyName("amount")]
    public decimal? Amount { get; init; }

    [JsonPropertyName("currency")]
    public string? Currency { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("expiryDate")]
    public string? ExpiryDate { get; init; }

    [JsonPropertyName("status")]
    public string? Status { get; init; }

    [JsonPropertyName("customer")]
    public GeideaCustomerDetails? Customer { get; init; }

    [JsonPropertyName("items")]
    public List<GeideaInvoiceItem>? Items { get; init; }
}

/// <summary>
/// Response from PUT /payment-intent/api/v1/direct/eInvoice.
/// </summary>
public class GeideaUpdateInvoiceResponse : GeideaBaseResponse
{
    [JsonPropertyName("paymentIntentId")]
    public string? PaymentIntentId { get; init; }

    [JsonPropertyName("status")]
    public string? Status { get; init; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; init; }

    [JsonPropertyName("currency")]
    public string? Currency { get; init; }

    [JsonPropertyName("updatedDate")]
    public string? UpdatedDate { get; init; }
}

// ─── Send by Email ────────────────────────────────────────────────────────────

/// <summary>
/// Request body for POST /payment-intent/api/v1/direct/eInvoice/{paymentIntentId}/sendByEmail.
/// </summary>
public class GeideaSendInvoiceByEmailRequest
{
    [JsonPropertyName("email")]
    public string Email { get; init; } = default!;

    [JsonPropertyName("language")]
    public string Language { get; init; } = "en";
}

/// <summary>
/// Response from POST .../sendByEmail.
/// </summary>
public class GeideaSendInvoiceByEmailResponse : GeideaBaseResponse
{
    [JsonPropertyName("paymentIntentId")]
    public string? PaymentIntentId { get; init; }
}

// ─── Send by SMS ──────────────────────────────────────────────────────────────

/// <summary>
/// Request body for POST /payment-intent/api/v1/direct/eInvoice/{paymentIntentId}/sendBySms.
/// </summary>
public class GeideaSendInvoiceBySmsRequest
{
    [JsonPropertyName("phone")]
    public string Phone { get; init; } = default!;

    [JsonPropertyName("language")]
    public string Language { get; init; } = "en";
}

/// <summary>
/// Response from POST .../sendBySms.
/// </summary>
public class GeideaSendInvoiceBySmsResponse : GeideaBaseResponse
{
    [JsonPropertyName("paymentIntentId")]
    public string? PaymentIntentId { get; init; }
}

// ─── Get eInvoice ─────────────────────────────────────────────────────────────

/// <summary>
/// Response from GET /payment-intent/api/v1/direct/eInvoice/{paymentIntentId}.
/// </summary>
public class GeideaGetInvoiceResponse : GeideaBaseResponse
{
    [JsonPropertyName("paymentIntentId")]
    public string? PaymentIntentId { get; init; }

    [JsonPropertyName("status")]
    public string? Status { get; init; }

    [JsonPropertyName("merchantReferenceId")]
    public string? MerchantReferenceId { get; init; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; init; }

    [JsonPropertyName("currency")]
    public string? Currency { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("expiryDate")]
    public string? ExpiryDate { get; init; }

    [JsonPropertyName("customer")]
    public GeideaCustomerDetails? Customer { get; init; }

    [JsonPropertyName("items")]
    public List<GeideaInvoiceItem>? Items { get; init; }

    [JsonPropertyName("createdDate")]
    public string? CreatedDate { get; init; }

    [JsonPropertyName("updatedDate")]
    public string? UpdatedDate { get; init; }

    [JsonPropertyName("orderId")]
    public string? OrderId { get; init; }
}

// ─── Delete eInvoice ──────────────────────────────────────────────────────────

/// <summary>
/// Response from DELETE /payment-intent/api/v1/direct/eInvoice/{paymentIntentId}.
/// Only invoices in "Created" status can be deleted.
/// </summary>
public class GeideaDeleteInvoiceResponse : GeideaBaseResponse
{
    [JsonPropertyName("paymentIntentId")]
    public string? PaymentIntentId { get; init; }
}
