using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Identity;

namespace SmartParking.Authentication;

public interface IRoleAppService: IApplicationService
{
    Task<List<IdentityRoleDto>> GetListByTenant(Guid? tenantId);
    Task<PagedResultDto<IdentityRoleDto>> GetListAndPagedByTenant(PagedResultRequestDto input,Guid? tenantId);
    Task<PagedResultDto<IdentityRoleDto>> SearchByTenant(PagedAndSortedAndSearchResultRequestDto input,Guid? tenantId);
}
