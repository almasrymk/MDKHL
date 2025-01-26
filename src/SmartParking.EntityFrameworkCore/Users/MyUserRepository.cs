using Microsoft.EntityFrameworkCore;
using SmartParking.Entities;
using SmartParking.EntityFrameworkCore;
using SmartParking.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Users;

public class MyUserRepository
    : EfCoreRepository<SmartParkingDbContext, IdentityUser, Guid>,
      IMyUserRepository
{
    private readonly IDataFilter _dataFilter;

    public MyUserRepository(
        IDbContextProvider<SmartParkingDbContext> dbContextProvider, IDataFilter dataFilter)
        : base(dbContextProvider)
    {
        this._dataFilter = dataFilter;
    }

    public async Task<List<IdentityUser>> FindByOrganizationAsync(int organizationId)
    {
        var dbContext = await GetDbContextAsync();
        return await dbContext.Users.Where(user => EF.Property<int>(user, "OrganizationId") == organizationId).ToListAsync();
    }
    public async Task<IdentityUser?> FindUserByOrganizationAsync(int organizationId, Guid userId)
    {
        var dbContext = await GetDbContextAsync();
        return await dbContext.Users.Where(user => EF.Property<int>(user, "OrganizationId") == organizationId && user.Id == userId).FirstOrDefaultAsync();
    }

    public async Task<IdentityUser?> FindByOrganizationByTenantAsync(Guid? tenantId)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            var dbContext = await GetDbContextAsync();
            return await dbContext.Users.Where(user => user.TenantId == tenantId && EF.Property<int>(user, "UserTypeId") == 1).FirstOrDefaultAsync();
        }
    }

    public async Task<PagedResult<UserWithRole>> FindUserWithRoleAsync(int skipCount, int maxResultCount, string Sorting, string searchTerm)
    {
        var dbContext = await GetDbContextAsync();
        var query = from user in dbContext.Users
                    from roleId in user.Roles.Select(r => r.RoleId)
                    join role in dbContext.Roles
                        on roleId equals role.Id
                    where user.Name.ToLower().Contains(searchTerm.ToLower())
                    || role.Name.ToLower().Contains(searchTerm.ToLower())
                    || user.PhoneNumber.ToLower().Contains(searchTerm.ToLower())
                    || user.UserName.ToLower().Contains(searchTerm.ToLower())
                    || user.Email.ToLower().Contains(searchTerm.ToLower())
                    select new UserWithRole
                    {
                        User = user,
                        Role = role

                    };
        var total = query.Count();
        query = query.Skip(skipCount).Take(maxResultCount);
        query = ApplySort(Sorting, query);
        var result = query.ToList();

        return new PagedResult<UserWithRole>
        {
            PageSize = total,
            Queryable = query,
        };


    }

    private static IQueryable<UserWithRole> ApplySort(string Sorting, IQueryable<UserWithRole> query)
    {
        if (!string.IsNullOrEmpty(Sorting))
        {
            query = query.OrderBy(Sorting);
        }
        return query;
    }

    public async Task<PagedResult<UserWithRole>> FindUserWithRoleMdkhlAsync(int skipCount, int maxResultCount, string Sorting,string searchTerm,Guid? tenantId)
    {
        var dbContext = await GetDbContextAsync();
        var query = from user in dbContext.Users
                    from roleId in user.Roles.Select(r => (Guid?)r.RoleId).DefaultIfEmpty() // Left join for roles
                    join role in dbContext.Roles
                        on roleId equals role.Id into roles
                    from r in roles.DefaultIfEmpty() // Preserve users with no roles
                    join organization in dbContext.Organizations
                        on EF.Property<int>(user, "OrganizationId") equals organization.Id into org
                    from o in org.DefaultIfEmpty()
                    where user.Name.ToLower().Contains(searchTerm.ToLower())
                   || r.Name.ToLower().Contains(searchTerm.ToLower())
                   || user.PhoneNumber.ToLower().Contains(searchTerm.ToLower())
                   || user.UserName.ToLower().Contains(searchTerm.ToLower())
                   || user.Email.ToLower().Contains(searchTerm.ToLower())
                   || tenantId.HasValue? user.TenantId == tenantId : true
                    select new UserWithRole
                    {
                        User = user,
                        Role = r,
                        OrganizationEnglishName = o.EnglishName,
                        OrganizationArabicName = o.ArabicName

                    };
        var total = query.Count();
        query = query.Skip(skipCount).Take(maxResultCount);
        query = ApplySort(Sorting, query);
        var result = query.ToList();

        return new PagedResult<UserWithRole>
        {
            PageSize = total,
            Queryable = query,
        };


    }
    public async Task<PagedResult<UserWithRole>> SearchUserWithRoleAsync(int skipCount, int maxResultCount, string term, string Sorting = "User.Id")
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
        var total = query.Count();
        query = query.Skip(skipCount).Take(maxResultCount);
        query = ApplySort(Sorting, query);
        var result = query.ToList();


        return new PagedResult<UserWithRole>
        {
            PageSize = total,
            Queryable = query,
        };


    }
    public async Task<PagedResult<UserWithRole>> SearchUserWithRoleMdkhlAsync(int skipCount, int maxResultCount, string term, string Sorting = "User.Id")
    {
        var dbContext = await GetDbContextAsync();
        var query = from user in dbContext.Users
                    from roleId in user.Roles.Select(r => (Guid?)r.RoleId).DefaultIfEmpty() // Get RoleIds for the user
                    join role in dbContext.Roles // Join with Roles table
                        on roleId equals role.Id into rolegroup
                    from r in rolegroup.DefaultIfEmpty()// Match RoleId
                    join organization in dbContext.Organizations
                    on EF.Property<int>(user, "OrganizationId") equals organization.Id into org
                    from o in org.DefaultIfEmpty()
                    where user.Name.ToLower().Contains(term.ToLower())
                    || r.Name.ToLower().Contains(term.ToLower())
                    || user.PhoneNumber.ToLower().Contains(term.ToLower())
                    || user.UserName.ToLower().Contains(term.ToLower())
                    || user.Email.ToLower().Contains(term.ToLower())
                    || o.ArabicName.ToLower().Contains(term.ToLower())
                    || o.EnglishName.ToLower().Contains(term.ToLower())
                    select new UserWithRole
                    {
                        User = user,
                        Role = r,
                        OrganizationEnglishName = o.EnglishName,
                        OrganizationArabicName = o.ArabicName
                    };
        var total = query.Count();
        query = query.Skip(skipCount).Take(maxResultCount);
        query = ApplySort(Sorting, query);
        var result = query.ToList();


        return new PagedResult<UserWithRole>
        {
            PageSize = total,
            Queryable = query,
        };


    }

    public async Task<List<UserRolesForMdkhl>> GetUserWithRolesForMdkhlAsync(Guid userId)
    {
        var dbContext = await GetDbContextAsync();

        var query = from user in dbContext.Users
                    where user.Id == userId
                    from roleId in user.Roles.Select(r => r.RoleId) // Get RoleIds for the user
                    join role in dbContext.Roles // Join with Roles table
                    on roleId equals role.Id // Match RoleId
                    join organization in dbContext.Organizations
                 on EF.Property<int>(user, "OrganizationId") equals organization.Id into org
                    from o in org.DefaultIfEmpty()

                    select new UserRolesForMdkhl
                    {
                        OrganizationName = o.EnglishName,
                        RoleId = role.Id,
                        RoleName = role.Name,
                        TenantId = o.TenantId
                    };

        var result = query.ToList();


        return result;


    }



   
}