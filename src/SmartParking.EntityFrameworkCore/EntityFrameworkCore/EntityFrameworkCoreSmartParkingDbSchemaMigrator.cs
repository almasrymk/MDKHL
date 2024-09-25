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

        await _serviceProvider
            .GetRequiredService<SmartParkingDbContext>()
            .Database
            .MigrateAsync();
    }
}
