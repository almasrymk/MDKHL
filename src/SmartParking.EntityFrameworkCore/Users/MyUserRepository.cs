using Microsoft.EntityFrameworkCore;
using SmartParking.Entities.Organizations;
using SmartParking.EntityFrameworkCore;
using SmartParking.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Users;

public class MyUserRepository
    : EfCoreRepository<SmartParkingDbContext, IdentityUser, Guid>,
      IMyUserRepository
{
    public MyUserRepository(
        IDbContextProvider<SmartParkingDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<List<IdentityUser>> FindByOrganizationAsync(int organizationId)
    {
        var dbContext = await GetDbContextAsync();
        return await dbContext.Users.Where(user => EF.Property<int>(user, "OrganizationId") == organizationId).ToListAsync();
    }    
    public async Task<IdentityUser?> FindUserByOrganizationAsync(int organizationId , Guid userId)
    {
        var dbContext = await GetDbContextAsync();
        return await dbContext.Users.Where(user => EF.Property<int>(user, "OrganizationId") == organizationId && user.Id == userId).FirstOrDefaultAsync();
    }

    public async Task<IdentityUser?> FindByOrganizationByTenantAsync(Guid? tenantId)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            var dbContext = await GetDbContextAsync();
            return await dbContext.Users.Where(user => user.TenantId == tenantId && EF.Property<int>(user, "UserTypeId")!=3).FirstOrDefaultAsync();
        }
    }
}