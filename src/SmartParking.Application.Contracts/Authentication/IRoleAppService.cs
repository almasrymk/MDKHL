using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Identity;

namespace SmartParking.Authentication;

public interface IRoleAppService: IApplicationService
{
    Task<List<IdentityRoleDto>> GetListByTenant(Guid? tenantId);
    Task<IdentityRoleDto> Create(IdentityRoleCreateDto roleCreateDto,[Optional] Guid? tenantId);
    Task<IdentityRoleDto> Update(Guid Id, IdentityRoleUpdateDto updateDto);
    Task<PagedResultDto<IdentityRoleDto>> GetListAndPagedByTenant(PagedResultRequestDto input,Guid? tenantId);
    Task<PagedResultDto<IdentityRoleDto>> SearchByTenant(PagedAndSortedAndSearchResultRequestDto input,Guid? tenantId);
}
