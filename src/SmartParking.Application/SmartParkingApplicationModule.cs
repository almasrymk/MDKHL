using Microsoft.Extensions.DependencyInjection;
using SmartParking.Utilities;
using Volo.Abp.Account;
using Volo.Abp.AspNetCore.ExceptionHandling;
using Volo.Abp.AutoMapper;
using Volo.Abp.FeatureManagement;
using Volo.Abp.FluentValidation;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.TenantManagement;

namespace SmartParking;

[DependsOn(
    typeof(SmartParkingDomainModule),
    typeof(AbpAccountApplicationModule),
    typeof(SmartParkingApplicationContractsModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpTenantManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpSettingManagementApplicationModule),
    typeof(AbpFluentValidationModule) //Add the FluentValidation module

	)]
public class SmartParkingApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddTransient<IInvoiceUtility, InvoiceUtility>();
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<SmartParkingApplicationModule>();
        });
        Configure<AbpExceptionHandlingOptions>(options =>
        {
            options.SendExceptionsDetailsToClients = true;
            options.SendStackTraceToClients = false;
        });

    }

}
