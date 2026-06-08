namespace SmartParking.Payments.PaymentIntegrations;

/// <summary>
/// Configuration root for the Geidea payment gateway.
/// Bound from appsettings.json "Geidea" section.
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
}
