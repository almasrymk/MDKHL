using System.Threading.Tasks;

namespace SmartParking.Invoices;

public interface IInvoiceActionsAppService
{
    /// <summary>
    /// Searches for an invoice that matches the specified search criteria.
    /// </summary>
    /// <param name="invoiceSearchDto">An object containing the criteria used to search for the invoice. Must not be <c>null</c>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="InvoiceDto"/>
    /// representing the matching invoice, or <c>null</c> if no invoice is found.</returns>
    Task<InvoiceDto> Search(InvoiceSearchDto invoiceSearchDto);

    /// <summary>
    /// Determines whether the specified <see cref="InvoiceSearchDto"/> can be handled by the current handler.
    /// </summary>
    /// <param name="invoiceSearchDto">The invoice search criteria to evaluate. Cannot be <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the handler can process the specified search criteria; otherwise, <see
    /// langword="false"/>.</returns>
    bool CanHandle(InvoiceSearchDto invoiceSearchDto);
}
