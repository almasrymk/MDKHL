using Abp.Application.Services;
using SmartParking.CustomerPackages.QRGenerator;
using SmartParking.Payments;
using System.IO;
using System.Threading.Tasks;
namespace SmartParking.PDF.Contracts;

public interface IPDFGeneratorAppService : IApplicationService 
{
    Task<Stream> CreatePdf(PaymentGetResponseDto payment , string PageSize);
}
