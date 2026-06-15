using SmartParking.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartParking.Payments.PaymentIntegrations;

/// <summary>
/// Common contract for all payment gateways.
/// Registered via a <c>Func&lt;GatewayKey, IPaymentGateway&gt;</c> factory delegate in DI,
/// which is compatible with both standard MS DI and ABP's Autofac adapter.
/// </summary>
public interface IPaymentGatewayIntegrationService
{
    /// <summary>Gets the unique gateway key identifying this implementation.</summary>
    GatewayKey Gateway { get; }
    /// <summary>
    /// Saves a pending payment record, calls the gateway to create a checkout session,
    /// and returns a unified result containing whatever the UI needs to render
    /// the payment widget (Oppwa checkoutId, Geidea sessionId, etc.).
    /// </summary>
    Task<GatewayInitiateResult> InitiateAsync(PaymentCheckoutRequestDto request);

    /// <summary>
    /// Verifies a completed payment using a gateway-specific reference
    /// (Oppwa: resourcePath, Geidea: orderId).
    /// Updates the Payment and Invoice records on success.
    /// </summary>
    Task<PaymentVerificationResult> VerifyAsync(string gatewayReference);

    /// <summary>
    /// Entry point for the gateway's callback/redirect.
    /// Receives query-string / form parameters already parsed by the controller
    /// so this interface stays free of any ASP.NET Core dependency.
    /// </summary>
    Task<PaymentVerificationResult> HandleCallbackAsync(IReadOnlyDictionary<string, string> callbackParams);

    /// <summary>Returns true if this gateway can process the given PaymentMethod.</summary>
    bool SupportsMethod(PaymentMethod method);
}
