using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartParking.Entities;
using SmartParking.KeeperShifts;
using SmartParking.Vehicles;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;

namespace SmartParking.KeeperShifts
{
    public interface IKeeperShiftsAppService : IApplicationService , ITransientDependency
    {
        Task<bool> CreateAsync(CreateUpdateKeeperShiftDto input);
        Task<PagedResultDto<KeeperShiftDto>> GetMyShiftsForCurrentUser(Guid? TenantId, PagedAndSortedAndSearchResultRequestDto input);
        Task<KeeperShiftDto?> GetAsync(int id);
        Task DeleteAsync(int id);
        Task<PagedResultDto<KeeperShiftDto>> GetListAssignedShiftsToUserForMdkhal(Guid? tenantId, Guid? userId, PagedAndSortedAndSearchResultRequestDto input);
        Task<PagedResultDto<KeeperShiftDto>> GetListAssignedShiftsToUserForOrganization(Guid? tenantId, Guid? userId, PagedAndSortedAndSearchResultRequestDto input);
        Task<PagedResultDto<UserWithRole>> GetListUserCanAssignShiftsOrganizationAsync(PagedAndSortedAndSearchResultRequestDto input);
        Task<PagedResultDto<UserWithRole>> GetListUserCanAssignShiftsMdkhlAsync(Guid? tenantId, PagedAndSortedAndSearchResultRequestDto input);
    }


}


