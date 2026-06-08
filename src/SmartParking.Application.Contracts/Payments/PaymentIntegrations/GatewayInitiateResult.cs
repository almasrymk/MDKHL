namespace SmartParking.Payments.PaymentIntegrations;

/// <summary>
/// Unified result returned by <see cref="IPaymentGatewayIntegrationService.InitiateAsync"/>.
/// The Blazor page inspects <see cref="Gateway"/> to decide which JS widget to load.
/// </summary>
public sealed class GatewayInitiateResult
{
    /// <summary>Oppwa: the checkoutId passed to the HyperPay JS widget.</summary>
    public string? OppwaCheckoutId { get; init; }

    /// <summary>Geidea: the sessionId passed to Geidea.js.</summary>
    public string? GeideaSessionId { get; init; }

    /// <summary>Which gateway produced this result.</summary>
    public GatewayKey Gateway { get; init; }

    /// <summary>Internal sequential payment reference number (e.g. "PAY-0001").</summary>
    public string PaymentNumber { get; init; } = default!;

    /// <summary>The name of the JavaScript function to invoke to load the checkout widget.</summary>
    public string JsMethodName { get; set; } = default!;

    /// <summary>Array of arguments to pass to the JS method.</summary>
    public object[] JsArguments { get; set; } = default!;

    public static GatewayInitiateResult ForOppwa(string checkoutId, string paymentNumber, string shopperResultUrl, string brands, string scriptUrl) =>
        new()
        {
            Gateway = GatewayKey.Oppwa,
            OppwaCheckoutId = checkoutId,
            PaymentNumber = paymentNumber,
            JsMethodName = "hyperPay.loadWidget",
            JsArguments = new object[] { checkoutId, shopperResultUrl, brands, scriptUrl }
        };

    public static GatewayInitiateResult ForGeidea(string sessionId, string paymentNumber, string merchantPublicKey, string callbackUrl, string jsUrl) =>
        new()
        {
            Gateway = GatewayKey.Geidea,
            GeideaSessionId = sessionId,
            PaymentNumber = paymentNumber,
            JsMethodName = "geidea.loadCheckout",
            JsArguments = new object[] { sessionId, merchantPublicKey, callbackUrl, jsUrl }
        };
}
