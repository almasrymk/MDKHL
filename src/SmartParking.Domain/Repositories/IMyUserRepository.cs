using SmartParking.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace SmartParking.Repositories;

public interface IMyUserRepository : IRepository<IdentityUser, Guid>
{
    Task<List<IdentityUser>> FindByOrganizationAsync(int oraganizationId);
    Task<List<IdentityUser>> FindByOrganizationAndActiveAsync(int oraganizationId);
    Task<IdentityUser> FindByOrganizationByTenantAsync(Guid? tenantId);
    Task<IdentityUser?> FindUserByOrganizationAsync(int organizationId, Guid userId);
    Task<PagedResult<UserWithRole>> FindUserWithRoleAsync(int skipCount, int maxResultCount,string Sorting, string searchTerm);
    Task<PagedResult<UserWithRole>> SearchUserWithRoleAsync(int skipCount, int maxResultCount, string term , string Sorting);
    Task<PagedResult<UserWithRole>> FindUserWithRoleMdkhlAsync(int skipCount, int maxResultCount,string Sorting, string searchTerm, Guid? tenantId);
    Task<PagedResult<UserWithRole>> SearchUserWithRoleMdkhlAsync(int skipCount, int maxResultCount, string term, string Sorting);
    Task<List<UserRolesForMdkhl>> GetUserWithRolesForMdkhlAsync(Guid userId);


}
