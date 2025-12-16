using SmartParking.GarageTransactions;
using System.Threading.Tasks;

namespace SmartParking.Invoices.Factory;

public interface IInvoiceActionsFactoryAppService
{
    /// <summary>
    /// Searches for an invoice that matches the specified search criteria.
    /// </summary>
    /// <param name="invoiceSearchDto">An object containing the criteria used to search for the invoice. Must not be <c>null</c>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="InvoiceDto"/>
    /// representing the matching invoice, or <c>null</c> if no invoice is found.</returns>
    Task<InvoiceDto> Search(InvoiceSearchDto invoiceSearchDto);

}
