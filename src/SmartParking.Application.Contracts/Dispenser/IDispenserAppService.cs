using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SmartParking.Dispenser;

public interface IDispenserAppService : IApplicationService
{
    Task<bool> Entry(EntryDto request);
    Task<bool> PreEntry(PreEntryDto request);
    Task<bool> Exit(ExitDto request);
}
