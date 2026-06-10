using System.Threading.Tasks;

namespace SmartParking.Payments.PaymentIntegrations;

/// <summary>
/// Contract for Geidea post-payment operational endpoints.
/// Handles refunds and server-to-server order verification.
/// All operations target https://api.ksamerchant.geidea.net and are authorised
/// with Basic Auth (Base64 of PublicKey:ApiPassword).
/// </summary>
public interface IGeideaPaymentManagementService
{
    /// <summary>
    /// Initiates a full or partial refund for a successfully captured order via
    /// POST /payment-intent/api/v1/direct/refund.
    /// </summary>
    /// <param name="request">
    /// Must contain at minimum <c>OrderId</c> and <c>Currency</c>.
    /// Leave <c>RefundAmount</c> null for a full refund.
    /// </param>
    Task<GeideaRefundResponse> RefundPaymentAsync(GeideaRefundRequest request);

    /// <summary>
    /// Retrieves the authoritative order status directly from Geidea via
    /// GET /payment-intent/api/v1/direct/orders/{orderId}.
    /// Use this server-to-server call to verify transaction outcomes independently
    /// of the frontend callback — particularly useful for reconciliation or
    /// when a callback delivery cannot be guaranteed.
    /// </summary>
    Task<GeideaOrderStatusResponse> GetOrderStatusAsync(string orderId);
}
