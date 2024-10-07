using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SmartParking.GarageTransactionEndPoints
{
    public interface IGarageTransactionEndPointsAppService : IApplicationService
    {
        Task<Boolean> CreateAsync(CreateUpdateGarageTransactionEndPointDto input);
    }
}
