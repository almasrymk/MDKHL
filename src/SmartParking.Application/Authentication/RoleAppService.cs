using SmartParking.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using System.Linq.Dynamic.Core;
using Volo.Abp.Data;
using System.Runtime.InteropServices;
namespace SmartParking.Authentication;

public class RoleAppService : ApplicationService, IRoleAppService
{
    private readonly ICustomRoleReository _roleReository;
    private readonly IDataFilter _dataFilter;

    public RoleAppService(ICustomRoleReository roleReository,IDataFilter dataFilter)
    {
        _roleReository = roleReository;
        _dataFilter = dataFilter;
    }

    public async Task<PagedResultDto<IdentityRoleDto>> GetListAndPagedByTenant(PagedResultRequestDto input,Guid? tenantId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var queryable = await _roleReository.GetQueryableAsync();
            queryable = GetQueryByTenat(tenantId, queryable);
            return await GetPagedResultAsync(queryable, input);
        }
    }

    public async Task<List<IdentityRoleDto>> GetListByTenant(Guid? tenantId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var queryable = await _roleReository.GetQueryableAsync();
            queryable = GetQueryByTenat(tenantId, queryable);
            return await MapRoles(queryable); ;
        }
    }

    public async Task<PagedResultDto<IdentityRoleDto>> SearchByTenant(PagedAndSortedAndSearchResultRequestDto input, Guid? tenantId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var queryable = await _roleReository.GetQueryableAsync();
            queryable = GetQueryByTenat(tenantId, queryable);
            if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
            {
                queryable = ApplySearchFilter(queryable, input.SearchingTerm);
            }

            if (!input.Sorting.IsNullOrEmpty())
            {
                queryable = ApplySorting(queryable, input.Sorting);
            }

            return await GetPagedResultAsync(queryable, input);
        }
    }

    private static IQueryable<IdentityRole> GetQueryByTenat(Guid? tenantId, IQueryable<IdentityRole> queryable)
    {
        queryable = queryable.Where(r => r.TenantId == tenantId);
        return queryable;
    }

    private async Task<PagedResultDto<IdentityRoleDto>> GetPagedResultAsync(IQueryable<IdentityRole> queryable, PagedResultRequestDto input)
    {
        var totalCount = await AsyncExecuter.CountAsync(queryable);
        queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
        List<IdentityRoleDto> RolesDto = await MapRoles(queryable);

        return new PagedResultDto<IdentityRoleDto>
        {
            TotalCount = totalCount,
            Items = RolesDto
        };
    }

    private async Task<List<IdentityRoleDto>> MapRoles(IQueryable<IdentityRole> queryable)
    {
        var roles = await AsyncExecuter.ToListAsync(queryable);
        var rolesDto = ObjectMapper.Map<List<IdentityRole>, List<IdentityRoleDto>>(roles);
        return rolesDto;
    }

    private IQueryable<IdentityRole> ApplySearchFilter(IQueryable<IdentityRole> queryable, string searchingTerm)
    {
        var lowerTerm = searchingTerm.ToLower();
        return queryable.Where(role =>
            role.Name.ToLower().Contains(lowerTerm));
            
    }

    private IQueryable<IdentityRole> ApplySorting(IQueryable<IdentityRole> queryable, string sorting)
    {
        if (sorting.Contains("DESC"))
        {
            var sortingParts = sorting.Split(' ').ToList();
            return queryable.OrderBy(sortingParts[0] + " descending");
        }
        return queryable.OrderBy(sorting);
    }

    public async Task<IdentityRoleDto> Create(IdentityRoleCreateDto roleCreateDto, [Optional] Guid? tenantId)
    {
        var role = new IdentityRole(GuidGenerator.Create(), roleCreateDto.Name, tenantId);
        role.IsPublic = true;
        var roleCreate = await _roleReository.InsertAsync(role, true);
        var rolesDto = ObjectMapper.Map<IdentityRole, IdentityRoleDto>(roleCreate);
        return rolesDto;
    }

    public async Task<IdentityRoleDto> Update(Guid Id, IdentityRoleUpdateDto updateDto)
    {
        var role = await _roleReository.GetAsync(Id);
        role.ChangeName(updateDto.Name);
        var rolesDto = ObjectMapper.Map<IdentityRole, IdentityRoleDto>(role);
        return rolesDto;
    }
}
