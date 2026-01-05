using SmartParking.Blazor.Dtos;
using SmartParking.Invoices;

namespace SmartParking.Blazor.Contracts;

public interface IInvoiceUiAppService
{
    Task<GarageTransactionUiDto> GetGarageTransaction(InvoiceSearchDto invoiceSearchDto);
    Task<CreateUpdateInvoiceUiDto> PreCheckout(GarageTransactionUiDto garageTransactionUiDto);
    Task<CreateUpdateInvoiceUiDto> GetInvoiceAfterPreCheckout(InvoiceSearchDto invoiceSearchDto);
    Task<InvoiceUiDto> Checkout(GarageTransactionUiDto garageTransactionUiDto);
    Task<InvoiceUiDto> GetInvoiceAfterCheckout(InvoiceSearchDto invoiceSearchDto);

}
