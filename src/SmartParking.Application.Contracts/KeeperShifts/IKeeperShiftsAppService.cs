using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartParking.KeeperShifts;
using SmartParking.Vehicles;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;

namespace SmartParking.KeeperShifts
{
    public interface IKeeperShiftsAppService : IApplicationService,ITransientDependency
    {
        Task<bool> CreateAsync(CreateUpdateKeeperShiftDto input);

        Task<PagedResultDto<KeeperShiftDto>> GetListKeeperShiftAsync(Guid? TenantId, PagedAndSortedResultRequestDto input);

        Task<KeeperShiftDto> GetAsync(int id);
        Task DeleteAsync(int id);

    }


}


