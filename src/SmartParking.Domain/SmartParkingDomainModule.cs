using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SmartParking.MultiTenancy;
using SmartParking.Services.GarageTransactions;
using System.Threading.Tasks;
using Volo.Abp.AuditLogging;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Emailing;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.OpenIddict;
using Volo.Abp.PermissionManagement.Identity;
using Volo.Abp.PermissionManagement.OpenIddict;
using Volo.Abp.SettingManagement;
using Volo.Abp.TenantManagement;

namespace SmartParking;

[DependsOn(
    typeof(SmartParkingDomainSharedModule),
    typeof(AbpAuditLoggingDomainModule),
    typeof(AbpBackgroundJobsDomainModule),
    typeof(AbpFeatureManagementDomainModule),
    typeof(AbpIdentityDomainModule),
    typeof(AbpOpenIddictDomainModule),
    typeof(AbpPermissionManagementDomainOpenIddictModule),
    typeof(AbpPermissionManagementDomainIdentityModule),
    typeof(AbpSettingManagementDomainModule),
    typeof(AbpTenantManagementDomainModule),
    typeof(AbpEmailingModule)
)]
public class SmartParkingDomainModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        
        Configure<AbpLocalizationOptions>(options =>
        {
            options.Languages.Add(new LanguageInfo("ar", "ar", "ar"));
            options.Languages.Add(new LanguageInfo("en", "en", "en"));

        });


        Configure<AbpMultiTenancyOptions>(options =>
        {
            options.IsEnabled = MultiTenancyConsts.IsEnabled;
        });

#if DEBUG
        context.Services.Replace(ServiceDescriptor.Singleton<IEmailSender, NullEmailSender>());
#endif
    }
    public override Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        //  context.Services.AddScoped<GarageTransactionsBackgroundWorker>();

        return base.ConfigureServicesAsync(context);
    }
    public override void OnApplicationInitialization(Volo.Abp.ApplicationInitializationContext context)
    {
        //var backgroundWorkerManager = context.ServiceProvider.GetRequiredService<IBackgroundWorkerManager>();

        //var worker = context.ServiceProvider.GetRequiredService<GarageTransactionsBackgroundWorker>();
        //backgroundWorkerManager.AddAsync(worker);
        base.OnApplicationInitialization(context);
    }
}
