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
    Task<PagedResultDto<IdentityRoleDto>> GetListAndPagedOrganization(PagedResultRequestDto input,Guid? tenantId);
    Task<PagedResultDto<RoleDto>> GetListAndPagedMdkhl(PagedAndSortedResultRequestDto input);
    Task<PagedResultDto<IdentityRoleDto>> SearchOrganization(PagedAndSortedAndSearchResultRequestDto input,Guid? tenantId);
    Task<PagedResultDto<RoleDto>> SearchMdkhl(PagedAndSortedAndSearchResultRequestDto input);
    Task<IdentityRoleDto> GetRoleById(Guid Id);
    Task<IdentityRole> GetIdentityRoleById(Guid Id);
    Task AssignRole(Guid UserId, Guid RoleId);
}
