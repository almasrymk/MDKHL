using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SmartParking.DispenserAppService;

public interface IBarrierAppService : IApplicationService
{
    Task<bool> OpenBarrier(int organizationId, BarrierRequest request);
}
