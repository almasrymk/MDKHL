using SmartParking.Cameras;
using SmartParking.Entities.Organizations;
using SmartParking.Invoices;
using System.Threading.Tasks;

namespace SmartParking.Utilities;

public interface IInvoiceUtility
{
    Task<QRCodeDto> GetQrCodeAsync(string? token, int? invoiceId);
    Task<QRCodeDto> GetQrCodeAsync(decimal amount, int organizationId);
    Task<QRCodeDto> GetQrCodeAsync(decimal amount, Organization organization);
    Task<string?> GetTokenAsync();
    Task OpenGate(CameraDto Camera);
}
