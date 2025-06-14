using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SmartParking.BarrierAppService;

public interface IBarrierAppService : IApplicationService
{
    Task<bool> CallingBarrier(int organizationId, BarrierRequest request);
}
