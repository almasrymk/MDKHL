using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SmartParking.Payments.PaymentIntegrations;

// ─── Refund ───────────────────────────────────────────────────────────────────

/// <summary>
/// Request body for POST /payment-intent/api/v1/direct/refund.
/// Initiates a full or partial refund for a successfully captured order.
/// </summary>
public class GeideaRefundRequest
{
    [JsonPropertyName("orderId")]
    public string OrderId { get; init; } = default!;

    /// <summary>
    /// Amount to refund. Must not exceed the original captured amount.
    /// When null the gateway performs a full refund.
    /// </summary>
    [JsonPropertyName("refundAmount")]
    public decimal? RefundAmount { get; init; }

    [JsonPropertyName("currency")]
    public string Currency { get; init; } = default!;

    [JsonPropertyName("merchantReferenceId")]
    public string? MerchantReferenceId { get; init; }

    [JsonPropertyName("reason")]
    public string? Reason { get; init; }
}

/// <summary>
/// Response from POST /payment-intent/api/v1/direct/refund.
/// </summary>
public class GeideaRefundResponse : GeideaBaseResponse
{
    [JsonPropertyName("orderId")]
    public string? OrderId { get; init; }

    [JsonPropertyName("merchantReferenceId")]
    public string? MerchantReferenceId { get; init; }

    [JsonPropertyName("status")]
    public string? Status { get; init; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; init; }

    [JsonPropertyName("currency")]
    public string? Currency { get; init; }

    [JsonPropertyName("refundedAmount")]
    public decimal RefundedAmount { get; init; }

    [JsonPropertyName("createdDate")]
    public string? CreatedDate { get; init; }

    [JsonPropertyName("updatedDate")]
    public string? UpdatedDate { get; init; }
}

// ─── Order status ─────────────────────────────────────────────────────────────

/// <summary>
/// A single transaction entry within an order.
/// </summary>
public class GeideaOrderTransaction
{
    [JsonPropertyName("transactionId")]
    public string? TransactionId { get; init; }

    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [JsonPropertyName("status")]
    public string? Status { get; init; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; init; }

    [JsonPropertyName("currency")]
    public string? Currency { get; init; }

    [JsonPropertyName("createdDate")]
    public string? CreatedDate { get; init; }

    [JsonPropertyName("responseCode")]
    public string? ResponseCode { get; init; }

    [JsonPropertyName("responseMessage")]
    public string? ResponseMessage { get; init; }
}

/// <summary>
/// Response from GET /payment-intent/api/v1/direct/orders/{orderId}.
/// Use this for server-to-server order verification, independent of the frontend callback.
/// </summary>
public class GeideaOrderStatusResponse : GeideaBaseResponse
{
    [JsonPropertyName("orderId")]
    public string? OrderId { get; init; }

    [JsonPropertyName("merchantReferenceId")]
    public string? MerchantReferenceId { get; init; }

    [JsonPropertyName("status")]
    public string? Status { get; init; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; init; }

    [JsonPropertyName("currency")]
    public string? Currency { get; init; }

    [JsonPropertyName("paymentMethod")]
    public string? PaymentMethod { get; init; }

    [JsonPropertyName("cardBrand")]
    public string? CardBrand { get; init; }

    [JsonPropertyName("cardholderName")]
    public string? CardholderName { get; init; }

    [JsonPropertyName("last4")]
    public string? Last4 { get; init; }

    [JsonPropertyName("createdDate")]
    public string? CreatedDate { get; init; }

    [JsonPropertyName("updatedDate")]
    public string? UpdatedDate { get; init; }

    [JsonPropertyName("transactions")]
    public List<GeideaOrderTransaction>? Transactions { get; init; }
}
