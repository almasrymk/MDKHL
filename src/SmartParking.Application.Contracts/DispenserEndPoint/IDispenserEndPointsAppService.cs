using Newtonsoft.Json.Linq;
using SmartParking.PackageCodeEndPoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SmartParking.DispenserEndPoint;

public interface IDispenserEndPointsAppService : IApplicationService
{
    Task<IBaseGateResponseDto> Entry(EntryDto request);
    Task<IBaseGateResponseDto> PreEntry(PackageCodeEntryDto request);
    Task<IBaseGateResponseDto> Exit(ExitDto request);
    Task<IBaseGateResponseDto> PreExit(PackageCodeExitDto request);
}
