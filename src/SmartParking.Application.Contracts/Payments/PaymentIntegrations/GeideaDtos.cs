using System.Text.Json.Serialization;

namespace SmartParking.Payments.PaymentIntegrations;

// ─── Session creation ─────────────────────────────────────────────────────────

/// <summary>
/// Internal DTO sent to POST /payment-intent/api/v2/direct/session.
/// Serialised as JSON. Geidea.js uses the returned sessionId to render the card form.
/// </summary>
public class GeideaCreateSessionRequest
{
    [JsonPropertyName("amount")]
    public decimal Amount { get; init; }

    [JsonPropertyName("currency")]
    public string Currency { get; init; } = default!;

    [JsonPropertyName("merchantReferenceId")]
    public string MerchantReferenceId { get; init; } = default!;

    [JsonPropertyName("callbackUrl")]
    public string CallbackUrl { get; init; } = default!;

    [JsonPropertyName("language")]
    public string Language { get; init; } = "en";

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; init; } = default!;

    [JsonPropertyName("signature")]
    public string Signature { get; init; } = default!;

    [JsonPropertyName("merchantPublicKey")]
    public string MerchantPublicKey { get; init; } = default!;
}

/// <summary>
/// Subset of the Geidea session creation response we care about.
/// Full response also contains merchantPublicKey, signature, etc.
/// </summary>
public class GeideaCreateSessionResponse
{
    [JsonPropertyName("session")]
    public GeideaSessionInfo? Session { get; init; }

    [JsonPropertyName("responseCode")]
    public string? ResponseCode { get; init; }

    [JsonPropertyName("responseMessage")]
    public string? ResponseMessage { get; init; }

    [JsonPropertyName("detailedResponseCode")]
    public string? DetailedResponseCode { get; init; }

    [JsonPropertyName("detailedResponseMessage")]
    public string? DetailedResponseMessage { get; init; }
}

public class GeideaSessionInfo
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("expiryTime")]
    public string? ExpiryTime { get; init; }
}

// ─── Callback ─────────────────────────────────────────────────────────────────

/// <summary>
/// Parameters Geidea sends when redirecting the browser back to your callbackUrl.
/// These arrive as query-string parameters (GET) or form fields (POST).
/// </summary>
public class GeideaCallbackParams
{
    public string? OrderId { get; set; }
    public string? Status { get; set; }
    public string? MerchantReferenceId { get; set; }
    public string? ResponseCode { get; set; }
    public string? ResponseMessage { get; set; }
}
