using SmartParking.Localization;
using Volo.Abp.AuditLogging;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.TenantManagement;
using Volo.Abp.Validation.Localization;
using Volo.Abp.VirtualFileSystem;

namespace SmartParking;

[DependsOn(
    typeof(AbpAuditLoggingDomainSharedModule),
    typeof(AbpBackgroundJobsDomainSharedModule),
    typeof(AbpFeatureManagementDomainSharedModule),
    typeof(AbpIdentityDomainSharedModule),
    typeof(AbpOpenIddictDomainSharedModule),
    typeof(AbpPermissionManagementDomainSharedModule),
    typeof(AbpSettingManagementDomainSharedModule),
    typeof(AbpTenantManagementDomainSharedModule)    
    )]
public class SmartParkingDomainSharedModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        SmartParkingGlobalFeatureConfigurator.Configure();
        SmartParkingModuleExtensionConfigurator.Configure();
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SmartParkingDomainSharedModule>();
        });
        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<SmartParkingResource>("ar")
                .AddBaseTypes(typeof(AbpValidationResource))
                .AddVirtualJson("/Localization/SmartParking");

            options.DefaultResourceType = typeof(SmartParkingResource);
            
        });
      


        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("SmartParking", typeof(SmartParkingResource));
        });
    }
}
