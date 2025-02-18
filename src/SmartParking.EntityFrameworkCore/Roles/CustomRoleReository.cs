using Microsoft.EntityFrameworkCore;
using SmartParking.EntityFrameworkCore;
using SmartParking.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Linq;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Roles;

public class CustomRoleReository :EfCoreRepository<SmartParkingDbContext, IdentityRole, Guid>, ICustomRoleReository
{
    public CustomRoleReository(
        IDbContextProvider<SmartParkingDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }
    public async Task<IdentityRole?> FindRoleByTenantAsync(string role, Guid? tenantId)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            var dbContext = await GetDbContextAsync();
            return await dbContext.Roles.FirstOrDefaultAsync(r => r.Name == role && r.TenantId == tenantId);
        }
    }
}
