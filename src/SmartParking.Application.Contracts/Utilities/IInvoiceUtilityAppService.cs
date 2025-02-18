using SmartParking.Cameras;
using SmartParking.Entities.Organizations;
using SmartParking.Invoices;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SmartParking.Utilities;

public interface IInvoiceUtilityAppService : IApplicationService
{
    Task<QRCodeDto> GetQrCodeAsync(string? token, int? invoiceId);
    Task<QRCodeDto> GetQrCodeAsync(decimal amount, int organizationId);
    Task<QRCodeDto> GetQrCodeAsync(decimal amount, Organization organization);
    Task<string?> GetTokenAsync();
    Task<bool> OpenGate(OpenGateRequestDto camera);
    Task<bool> SendRequestToOpenGate(int ExitGarageTransactionId);
}
