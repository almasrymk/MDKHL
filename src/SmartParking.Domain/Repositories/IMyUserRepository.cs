using SmartParking.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace SmartParking.Repositories;

public interface IMyUserRepository : IRepository<IdentityUser, Guid>
{
    Task<List<IdentityUser>> FindByOrganizationAsync(int oraganizationId);
    Task<IdentityUser> FindByOrganizationByTenantAsync(Guid? tenantId);
    Task<IdentityUser?> FindUserByOrganizationAsync(int organizationId, Guid userId);
    Task<List<UserWithRole>> FindUserWithRoleAsync(int skipCount, int maxResultCount);
    Task<List<UserWithRole>> SearchUserWithRoleAsync(int skipCount, int maxResultCount, string term);


}
