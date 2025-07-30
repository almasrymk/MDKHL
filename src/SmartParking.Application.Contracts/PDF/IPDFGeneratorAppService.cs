using Abp.Application.Services;
using SmartParking.CustomerPackages.QRGenerator;
using System.IO;
using System.Threading.Tasks;
namespace SmartParking.PDF;

public interface IPDFGeneratorAppService : IApplicationService 
{
    Task<byte[]> GenerateInvoicePdfAsync(int invoiceId,string commericialRegisteratio,string qrCode);
    byte[] GeneratePdfFromHtmlAsync(string htmlContent, int invoiceId);
    Task<string> GenerateInvoiceHtmlAsync(int invoiceId, string commericialRegisteratio, string qrCode);
    Task<string> GeneratePdfForCustomerPackage(CreateUpdateQrCodeDto input, byte[] qrCodePath);
    Task<Stream> SaveInvoicePdf(int invoiceId);
}
