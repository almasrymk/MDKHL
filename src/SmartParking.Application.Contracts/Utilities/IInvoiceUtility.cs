using SmartParking.Invoices;
using System.Threading.Tasks;

namespace SmartParking.Utilities;

public interface IInvoiceUtility
{
    Task<QRCodeDto> GetQrCodeAsync(string? token, int? invoiceId);
    Task<QRCodeDto> GetQrCodeAsync(decimal amount, int organizationId);
    Task<string?> GetTokenAsync();
}
