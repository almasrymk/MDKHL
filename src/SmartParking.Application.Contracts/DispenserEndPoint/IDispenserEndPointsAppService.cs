using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SmartParking.DispenserEndPoint;

public interface IDispenserEndPointsAppService : IApplicationService
{
    Task<bool> Entry(EntryDto request);
    Task<bool> PreEntry(PreEntryDto request);
    Task<bool> Exit(ExitDto request);
}
