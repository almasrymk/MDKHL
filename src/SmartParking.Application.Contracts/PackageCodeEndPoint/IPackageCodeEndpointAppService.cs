using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SmartParking.PackageCodeEndPoint;
public interface IPackageCodeEndpointAppService : IApplicationService
{
    Task<bool> PackageCodeEntry(PackageCodeEntryDto packageCodeEntry);
    Task<bool> PackageCodeExit(PackageCodeExitDto packageCodeExit);
}
