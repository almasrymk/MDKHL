using System.Threading.Tasks;

namespace SmartParking.Payments.PaymentIntegrations;

/// <summary>
/// Contract for the Geidea Pay-by-Invoice (eInvoice) REST API.
/// All operations target https://api.ksamerchant.geidea.net and are authorised
/// with Basic Auth (Base64 of PublicKey:ApiPassword).
/// </summary>
public interface IGeideaEInvoiceService
{
    /// <summary>
    /// Creates a new eInvoice via POST /payment-intent/api/v1/direct/eInvoice.
    /// Returns the <c>paymentIntentId</c> assigned by Geidea — persist this value
    /// as it is required by all subsequent invoice operations.
    /// </summary>
    Task<GeideaCreateInvoiceResponse> CreateInvoiceAsync(GeideaCreateInvoiceRequest request);

    /// <summary>
    /// Updates an existing eInvoice (amount, items, status, etc.) via
    /// PUT /payment-intent/api/v1/direct/eInvoice.
    /// Use the <c>status</c> field to transition the invoice lifecycle
    /// (e.g. "Created" → "Cancelled").
    /// </summary>
    Task<GeideaUpdateInvoiceResponse> UpdateInvoiceAsync(GeideaUpdateInvoiceRequest request);

    /// <summary>
    /// Sends the invoice to the customer's e-mail address via
    /// POST /payment-intent/api/v1/direct/eInvoice/{paymentIntentId}/sendByEmail.
    /// </summary>
    Task<GeideaSendInvoiceByEmailResponse> SendInvoiceByEmailAsync(
        string paymentIntentId,
        GeideaSendInvoiceByEmailRequest request);

    /// <summary>
    /// Sends the invoice link to the customer's mobile phone via
    /// POST /payment-intent/api/v1/direct/eInvoice/{paymentIntentId}/sendBySms.
    /// </summary>
    Task<GeideaSendInvoiceBySmsResponse> SendInvoiceBySmsAsync(
        string paymentIntentId,
        GeideaSendInvoiceBySmsRequest request);

    /// <summary>
    /// Retrieves full invoice details via
    /// GET /payment-intent/api/v1/direct/eInvoice/{paymentIntentId}.
    /// </summary>
    Task<GeideaGetInvoiceResponse> GetInvoiceAsync(string paymentIntentId);

    /// <summary>
    /// Deletes an invoice via
    /// DELETE /payment-intent/api/v1/direct/eInvoice/{paymentIntentId}.
    /// Only invoices in <c>Created</c> status can be deleted.
    /// </summary>
    Task<GeideaDeleteInvoiceResponse> DeleteInvoiceAsync(string paymentIntentId);
}
