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
using Microsoft.AspNetCore.Identity;
using SmartParking.Entities.Customers;
using SmartParking.Organizations;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Uow;
namespace SmartParking.Authentication;
using IdentityRole = Volo.Abp.Identity.IdentityRole;
using IdentityUser = Volo.Abp.Identity.IdentityUser;

public class RoleAppService : ApplicationService, IRoleAppService
{
    private readonly ICustomRoleReository _roleReository;
    private readonly IDataFilter _dataFilter;
    private readonly IUsersAppService _usersAppService;
    private readonly IOrganizationsAppService _organizationAppService;
    private readonly IMyUserRepository _myUserRepository;
    private readonly UserManager<IdentityUser> _userManager;



    public RoleAppService(ICustomRoleReository roleReository, IDataFilter dataFilter,
        IUsersAppService usersAppService, UserManager<IdentityUser> userManager,
        IOrganizationsAppService organizationAppService,IMyUserRepository myUserRepository)
    {
        _roleReository = roleReository;
        _dataFilter = dataFilter;
        _usersAppService = usersAppService;
        _userManager = userManager;
        _organizationAppService = organizationAppService;
        _myUserRepository = myUserRepository;
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
        if(tenantId == null)
        {
            return queryable;
        }
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
        await IsFoundRole(roleCreateDto.Name, tenantId);
        var role = new IdentityRole(GuidGenerator.Create(), roleCreateDto.Name, tenantId);
        role.IsPublic = true;
        var roleCreate = await _roleReository.InsertAsync(role, true);
        var rolesDto = ObjectMapper.Map<IdentityRole, IdentityRoleDto>(roleCreate);
        return rolesDto;
    }

    private async Task IsFoundRole(string roleName, Guid? tenantId)
    {
        var roleFound = await _roleReository.FindRoleByTenantAsync(roleName, tenantId);
        if (roleFound is not null)
        {
            throw new Exception("This role is already found.");
        }
    }

    public async Task<IdentityRoleDto> Update(Guid Id, IdentityRoleUpdateDto updateDto)
    {
        try
        {
            var role = await _roleReository.GetAsync(Id);
            await IsFoundRole(updateDto.Name, role.TenantId);
            role.ChangeName(updateDto.Name);
            await _roleReository.UpdateAsync(role);
            var rolesDto = ObjectMapper.Map<IdentityRole, IdentityRoleDto>(role);
            return rolesDto;
        }catch(Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    public async Task<IdentityRoleDto> GetRoleById(Guid Id)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _roleReository.GetQueryableAsync();
            query = query.Where(x => x.Id == Id);
           var role= await AsyncExecuter.FirstOrDefaultAsync(query);
            return ObjectMapper.Map<IdentityRole,IdentityRoleDto>(role);
        }
        
    }

    public async Task<IdentityRole?> GetIdentityRoleById(Guid Id)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _roleReository.GetQueryableAsync();
            query = query.Where(x => x.Id == Id);
            var role = await AsyncExecuter.FirstOrDefaultAsync(query);
            return role;
        }
    }

    public async Task AssignRole(Guid UserId, Guid RoleId)
    {
        IdentityUser user;
       
             user = await _myUserRepository.FindUserAsNoTracking(UserId);
        
        
        //var userUpdate = new IdentityUser(user.Id, user.UserName, user.Email, CurrentTenant.Id);
        //user = userUpdate;
        var org =await _organizationAppService.GetOrganizationForCurrentTenat();
        //user.SetProperty("TenantId", CurrentTenant.Id);
        user.SetProperty("OrganizationId", org.Id);
        user.AddRole(RoleId);
        
        await _userManager.RemovePasswordAsync(user);
        await _userManager.AddPasswordAsync(user, "P@ssw0rd");
        await _userManager.UpdateAsync(user);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    public async Task<IdentityRole?> GetRoleByNameAndTenant(string RoleName, Guid? TenantId)
    {
        var query = await _roleReository.GetQueryableAsync();
        query = query.Where(r => r.Name == RoleName && r.TenantId == TenantId);
        var role = await query.FirstOrDefaultAsync();
        return role;
    }
}
