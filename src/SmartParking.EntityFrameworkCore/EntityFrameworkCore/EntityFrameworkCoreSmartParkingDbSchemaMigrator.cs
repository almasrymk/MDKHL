using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartParking.Data;
using Volo.Abp.DependencyInjection;

namespace SmartParking.EntityFrameworkCore;

public class EntityFrameworkCoreSmartParkingDbSchemaMigrator
    : ISmartParkingDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreSmartParkingDbSchemaMigrator(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolve the SmartParkingDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<SmartParkingDbContext>();

            // Ensure the database connection string is properly set for the current tenant
            await dbContext.Database.MigrateAsync();
        }
        //await _serviceProvider
        //    .GetRequiredService<SmartParkingDbContext>()
        //    .Database
        //    .MigrateAsync();
    }
}
