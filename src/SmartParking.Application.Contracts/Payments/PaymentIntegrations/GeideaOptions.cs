namespace SmartParking.Payments.PaymentIntegrations;

/// <summary>
/// Configuration root for the Geidea payment gateway.
/// Bound from paymentconfig.json "Providers:Geidea" section.
/// </summary>
public class GeideaOptions
{
    public const string SectionName = "Providers:Geidea";

    /// <summary>"Test" or "Live"</summary>
    public string Mode { get; set; } = "Test";

    public GeideaEnvironmentOptions Test { get; set; } = new();
    public GeideaEnvironmentOptions Live { get; set; } = new();
}

public class GeideaEnvironmentOptions
{
    /// <summary>Geidea REST API base URL (e.g. https://api.ksamerchant.geidea.net)</summary>
    public string BaseUrl { get; set; } = "https://api.ksamerchant.geidea.net";

    /// <summary>Merchant public key — also used as the Basic Auth username.</summary>
    public string MerchantPublicKey { get; set; } = string.Empty;

    /// <summary>API password — used as Basic Auth password and as HMAC key for signature.</summary>
    public string ApiPassword { get; set; } = string.Empty;

    /// <summary>ISO-4217 currency code.</summary>
    public string Currency { get; set; } = "SAR";

    /// <summary>
    /// Full URL of your Geidea callback controller endpoint.
    /// Must be registered in the Geidea merchant portal.
    /// Example: https://yourdomain.com/api/PaymentResult/geidea/callback
    /// </summary>
    public string CallbackUrl { get; set; } = string.Empty;

    /// <summary>
    /// API endpoint paths. All values have production-correct defaults so the
    /// section is optional in paymentconfig.json — add it only to override a path.
    /// </summary>
    public GeideaEndpointOptions Endpoints { get; set; } = new();
}

/// <summary>
/// Configurable API endpoint paths for every Geidea REST operation.
/// Default values match the official KSA production API so no changes to
/// paymentconfig.json are required unless Geidea revises their URL structure.
///
/// To override, add an "Endpoints" subsection under
/// Providers:Geidea:{Test|Live} in paymentconfig.json.
/// </summary>
public class GeideaEndpointOptions
{
    // ── Hosted Payment Page (Quick Pay session) ───────────────────────────────

    /// <summary>POST — creates a Geidea.js hosted-page session.</summary>
    public string CreateSession { get; set; } = "/payment-intent/api/v2/direct/session";

    // ── eInvoice ──────────────────────────────────────────────────────────────

    /// <summary>POST — creates a new eInvoice.</summary>
    public string CreateInvoice { get; set; } = "/payment-intent/api/v1/direct/eInvoice";

    /// <summary>PUT — updates amount, items, or status of an existing eInvoice.</summary>
    public string UpdateInvoice { get; set; } = "/payment-intent/api/v1/direct/eInvoice";

    /// <summary>
    /// POST — sends the invoice link by e-mail.
    /// The <c>{paymentIntentId}</c> placeholder is replaced at runtime.
    /// </summary>
    public string SendInvoiceByEmail { get; set; } = "/payment-intent/api/v1/direct/eInvoice/{paymentIntentId}/sendByEmail";

    /// <summary>
    /// POST — sends the invoice link by SMS.
    /// The <c>{paymentIntentId}</c> placeholder is replaced at runtime.
    /// </summary>
    public string SendInvoiceBySms { get; set; } = "/payment-intent/api/v1/direct/eInvoice/{paymentIntentId}/sendBySms";

    /// <summary>
    /// GET — retrieves full eInvoice details.
    /// The <c>{paymentIntentId}</c> placeholder is replaced at runtime.
    /// </summary>
    public string GetInvoice { get; set; } = "/payment-intent/api/v1/direct/eInvoice/{paymentIntentId}";

    /// <summary>
    /// DELETE — deletes a "Created" eInvoice.
    /// The <c>{paymentIntentId}</c> placeholder is replaced at runtime.
    /// </summary>
    public string DeleteInvoice { get; set; } = "/payment-intent/api/v1/direct/eInvoice/{paymentIntentId}";

    // ── Payment management ────────────────────────────────────────────────────

    /// <summary>POST — refunds a successfully captured order.</summary>
    public string Refund { get; set; } = "/payment-intent/api/v1/direct/refund";

    /// <summary>
    /// GET — retrieves authoritative order status server-to-server.
    /// The <c>{orderId}</c> placeholder is replaced at runtime.
    /// </summary>
    public string GetOrder { get; set; } = "/payment-intent/api/v1/direct/orders/{orderId}";
}
