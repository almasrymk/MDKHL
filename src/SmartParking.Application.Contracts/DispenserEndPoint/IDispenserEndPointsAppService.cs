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
    Task<bool> Entry(EntryDto request);
    Task<bool> PreEntry(PackageCodeEntryDto request);
    Task<bool> Exit(ExitDto request);
    Task<bool> PreExit(PackageCodeExitDto request);
}
