using System;
using System.Threading.Tasks;
using SmartParking.ZATCATaxInvoice;
using SmartParking.Utilities;

namespace SmartParking.Application.Utilities
{
    /// <summary>
    /// Null Object implementation of IZatcaTaxInvoiceAppService.
    /// Used when ZATCA integration is disabled via configuration.
    /// Returns an empty successful response without performing any operations.
    /// </summary>
    public class NullZatcaTaxInvoiceAppService : IZatcaTaxInvoiceAppService
    {
        /// <summary>
        /// Returns a successful empty response without sending to ZATCA.
        /// </summary>
        public Task<ResponseInvoice> SendToZACTA(Guid paymentId)
        {
            var response = new ResponseInvoice
            {
                status = true,
                clearanceStatus = "SKIPPED",
                invoiceQRCode = string.Empty
            };

            return Task.FromResult(response);
        }
    }
}
