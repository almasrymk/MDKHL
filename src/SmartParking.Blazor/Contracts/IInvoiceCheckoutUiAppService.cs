using SmartParking.Blazor.Dtos;
using SmartParking.Payments;

namespace SmartParking.Blazor.Contracts;

public interface IInvoiceCheckoutUiAppService
{
    Task<InvoiceCheckoutUiDto> GetInvoiceCheckoutAsync(Guid paymentId);
}
