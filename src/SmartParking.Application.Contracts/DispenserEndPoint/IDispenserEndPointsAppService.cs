using Microsoft.AspNetCore.Components;
using SmartParking.PackageCodeEndPoint;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SmartParking.DispenserEndPoint;
public interface IDispenserEndPointsAppService : IApplicationService
{
    Task<IBaseGateResponseDto> Entry(string OrganizationCode, EntryDto request);
    Task<IBaseGateResponseDto> PreEntry(string OrganizationCode, PackageCodeEntryDto request);
    Task<IBaseGateResponseDto> Exit(string OrganizationCode, ExitDto request);
    Task<IBaseGateResponseDto> PreExit(string OrganizationCode, PackageCodeExitDto request);
}
