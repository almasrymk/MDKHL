using Microsoft.EntityFrameworkCore;
using Polly;
using SmartParking.Entities;
using SmartParking.Entities.Organizations;
using SmartParking.EntityFrameworkCore;
using SmartParking.Repositories;
using SmartParking.Results;
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
            return await dbContext.Users.Where(user => user.TenantId == tenantId && EF.Property<int>(user, "UserTypeId")==1).FirstOrDefaultAsync();
        }
    }

    public async Task<List<UserWithRole>> FindUserWithRoleAsync(int skipCount,int maxResultCount)
    {
        var dbContext = await GetDbContextAsync();
        var query = from user in dbContext.Users
                    from roleId in user.Roles.Select(r => r.RoleId) // Get RoleIds for the user
                    join role in dbContext.Roles // Join with Roles table
                        on roleId equals role.Id // Match RoleId
                    select new UserWithRole
                    {
                         User = user,
                         Role = role
                        
                    };
        query = query.Skip(skipCount).Take(maxResultCount);
        var result = query.ToList();

        return result;
        

    }
    public async Task<List<UserWithRole>> SearchUserWithRoleAsync(int skipCount,int maxResultCount,string term)
    {
        var dbContext = await GetDbContextAsync();
        var query = from user in dbContext.Users
                    from roleId in user.Roles.Select(r => r.RoleId) // Get RoleIds for the user
                    join role in dbContext.Roles // Join with Roles table
                        on roleId equals role.Id // Match RoleId
                    where user.Name.ToLower().Contains(term.ToLower())
                    || role.Name.ToLower().Contains(term.ToLower())
                    || user.PhoneNumber.ToLower().Contains(term.ToLower())
                    || user.UserName.ToLower().Contains(term.ToLower())
                    || user.Email.ToLower().Contains(term.ToLower())

                    select new UserWithRole
                    {
                        User = user,
                        Role = role,
                    };
        
        query = query.Skip(skipCount).Take(maxResultCount);
        var result = query.ToList();


        return result;
        

    }
}