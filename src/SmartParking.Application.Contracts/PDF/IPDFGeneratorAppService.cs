using Abp.Application.Services;
using SmartParking.CustomerPackages.QRGenerator;
using System.IO;
using System.Threading.Tasks;
namespace SmartParking.PDF;

public interface IPDFGeneratorAppService : IApplicationService 
{
    Task<Stream> CreatePdf(int EntityId , string PageSize);
}
