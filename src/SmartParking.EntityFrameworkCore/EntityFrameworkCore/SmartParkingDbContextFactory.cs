using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartParking.Entities.Lookups;
using Volo.Abp.Data;
using Volo.Abp.MultiTenancy;

namespace SmartParking.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class SmartParkingDbContextFactory : IDesignTimeDbContextFactory<SmartParkingDbContext>
{
    public SmartParkingDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();

        var services = new ServiceCollection();
        // Manually add necessary dependencies
        services.AddDbContext<SmartParkingDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("Default"));
        });

        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<IConnectionStringResolver>(new DefaultConnectionStringResolver(configuration));
        services.AddSingleton<ICurrentTenant, CurrentTenant>();
        services.AddSingleton<ICurrentTenantAccessor, NullCurrentTenantAccessor>(); // This line is crucial to bypass multi-tenancy at design-time


        var serviceProvider = services.BuildServiceProvider();

        return serviceProvider.GetRequiredService<SmartParkingDbContext>();
        //// https://www.npgsql.org/efcore/release-notes/6.0.html#opting-out-of-the-new-timestamp-mapping-logic
        //AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        //SmartParkingEfCoreEntityExtensionMappings.Configure();

        //var configuration = BuildConfiguration();

        //var builder = new DbContextOptionsBuilder<SmartParkingDbContext>()
        //    .UseNpgsql(configuration.GetConnectionString("Default"));

        //return new SmartParkingDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../SmartParking.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false)
           .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true);
        return builder.Build();
    }
   
}
