using Microsoft.EntityFrameworkCore;
using SmartParking.Entities;
using SmartParking.Entities.Cameras;
using SmartParking.Entities.Customers;
using SmartParking.Entities.Lookups;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Subscriptions;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

namespace SmartParking.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ConnectionStringName("Default")]
public class SmartParkingDbContext :
    AbpDbContext<SmartParkingDbContext>,
    IIdentityDbContext,
    ITenantManagementDbContext
{


    /* Add DbSet properties for your Aggregate Roots / Entities here. */


    #region Entities from the modules

    /* Notice: We only implemented IIdentityDbContext and ITenantManagementDbContext
     * and replaced them for this DbContext. This allows you to perform JOIN
     * queries for the entities of these modules over the repositories easily. You
     * typically don't need that for other modules. But, if you need, you can
     * implement the DbContext interface of the needed module and use ReplaceDbContext
     * attribute just like IIdentityDbContext and ITenantManagementDbContext.
     *
     * More info: Replacing a DbContext of a module ensures that the related module
     * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
     */

    //Identity

    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }
    // Tenant Management
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    public DbSet<Organization> Organizations { get; set; }
    public DbSet<OrganizationCategory> OrganizationCategories { get; set; }
    public DbSet<OrganizationContactInfo> OrganizationContactInfos { get; set; }
    public DbSet<OrganizationCustomers> OrganizationCustomers { get; set; }
    public DbSet<OrganizationGarage> OrganizationGarages { get; set; }
    public DbSet<OrganizationPackage> OrganizationPackages { get; set; }
    public DbSet<OrganizationSubscriptions> OrganizationSubscriptions { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Camera> Cameras { get; set; }
    public DbSet<CustomerCategory> CustomerCategories { get; set; }
    #endregion

    #region Lookups
    public DbSet<Country> Countries { get; set; }
    public DbSet<Governorate> Governorates { get; set; }
    public DbSet<City> Cities { get; set; }
    public DbSet<Feature> Features { get; set; }
    public DbSet<Screen> Screens { get; set; }
    public DbSet<UserType> UserTypes { get; set; }
    public DbSet<PackageType> PackageTypes { get; set; }
    public DbSet<Emaraa> Emaraas { get; set; }
    public DbSet<Stats> Stats { get; set; }
    #endregion

  public SmartParkingDbContext(DbContextOptions<SmartParkingDbContext> options)
        : base(options)
    {

    }


    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Your custom configurations...
        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureFeatureManagement();
        builder.ConfigureTenantManagement();
        builder.ApplyConfigurationsFromAssembly(typeof(SmartParkingDbContext).Assembly);
        /* Configure your own tables/entities inside here */

        //builder.Entity<YourEntity>(b =>
        //{
        //    b.ToTable(SmartParkingConsts.DbTablePrefix + "YourEntities", SmartParkingConsts.DbSchema);
        //    b.ConfigureByConvention(); //auto configure for the base class props
        //    //...
        //});
    }

}
