//using Microsoft.EntityFrameworkCore;
//using SmartParking.Entities;
//using SmartParking.EntityFrameworkCore;
//using SmartParking.Repositories;
//using System;
//using System.Threading.Tasks;
//using Volo.Abp.DependencyInjection;
//using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
//using Volo.Abp.EntityFrameworkCore;

//public class TenantConnectionStringRepository : EfCoreRepository<SmartParkingDbContext, SmartParking.Entities.TenantConnectionString, Guid>, ITenantConnectionStringRepository, ITransientDependency
//{
//    public TenantConnectionStringRepository(IDbContextProvider<SmartParkingDbContext> dbContextProvider)
//        : base(dbContextProvider)
//    {
//    }

//    public async Task<string> GetConnectionStringAsync(Guid tenantId)
//    {
//        var tenantConnectionString = await DbContext.TenantConnectionStrings
//            .FirstOrDefaultAsync(t => t.TenantId == tenantId);
//        return tenantConnectionString?.ConnectionString;
//    }
//}
