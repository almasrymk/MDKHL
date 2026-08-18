using SmartParking.Invoices;
using SmartParking.Mobile.Invoices;
using SmartParking.Results;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Mobile.Keepers;

public interface IKeeperInvoiceAppService : IApplicationService
{
    /// <summary>
    /// Retrieves a paged list of all invoices by organization id asynchronously.
    /// </summary>
    /// <param name="organizationId">ID of the specific organization.</param>
    /// <param name="paged">Pagination, sorting, and search parameters.</param>
    /// <returns>Paged result containing <see cref="InvoiceDto"/> items.</returns>
    Task<Result<PagedResultDto<InvoiceDto>>> GetAllInvoicesByOrganizationIdAsync(int organizationId, PagedAndSortedAndSearchResultRequestDto paged);

    /// <summary>
    /// Retrieves the last unpaid exit invoice by organization id and watcher/camera code asynchronously.
    /// </summary>
    /// <param name="organizationId">ID of the specific organization.</param>
    /// <param name="watcherCode">Watcher/device code.</param>
    /// <returns>A <see cref="InvoiceDto"/> if found.</returns>
    Task<Result<InvoiceDto>> GetLastInvoiceByOrganizationIdAndWatcherCodeAsync(int organizationId, string watcherCode);

    /// <summary>
    /// Initiates the checkout process for a specific invoice, applying any discount and generating payment QR code.
    /// </summary>
    /// <param name="invoiceId">The unique identifier of the invoice.</param>
    /// <param name="paymentMethodType">The payment method enum value.</param>
    /// <param name="DiscountValue">The discount value to be applied (optional, defaults to 0).</param>
    /// <returns>A result containing <see cref="InvoiceMobileDto"/>.</returns>
    Task<Result<InvoiceMobileDto>> CheckoutAsync(int invoiceId, int paymentMethodType, decimal DiscountValue = default);

    /// <summary>
    /// Processes a cash payment checkout for a customer request on a specific invoice.
    /// </summary>
    /// <param name="invoiceId">The unique identifier of the invoice.</param>
    /// <param name="PaymentType">The type of payment description.</param>
    /// <param name="DiscountValue">The discount value to be applied (optional, defaults to 0).</param>
    /// <returns>A result containing <see cref="InvoiceMobileDto"/>.</returns>
    Task<Result<InvoiceMobileDto>> PayCashForCustomerRequestAsync(int invoiceId, string PaymentType, decimal DiscountValue = default);

    /// <summary>
    /// Retrieves all paid invoices associated with a specific organization.
    /// </summary>
    /// <param name="organizationId">The unique identifier of the organization.</param>
    /// <returns>A list of paid <see cref="InvoiceDto"/> records.</returns>
    Task<Result<List<InvoiceDto>>> GetAllInvoicesByOrganizationIdPaidAsync(int organizationId);

    /// <summary>
    /// Retrieves the payment QR code associated with the specified invoice.
    /// </summary>
    /// <param name="InvoiceId">The unique identifier of the invoice.</param>
    /// <returns>A result containing the QR code string if successful.</returns>
    Task<Result<string>> GetPaymentQrCodeByInvoiceId(int InvoiceId);
}
