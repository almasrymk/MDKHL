using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmartParking.DispenserAppService;
using SmartParking.Events;
using SmartParking.KeeperShifts;
using SmartParking.Utilities;
using SmartParking.WebSockets;
using System;
using Volo.Abp;
using Volo.Abp.Account;
using Volo.Abp.AspNetCore.ExceptionHandling;
using Volo.Abp.AutoMapper;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.FeatureManagement;
using Volo.Abp.FluentValidation;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.TenantManagement;
using Volo.Abp.Threading;

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
        context.Services.AddTransient<IInvoiceUtilityAppService, InvoiceUtilityAppService>();
        context.Services.AddTransient<IBarrierAppService, BarrierAppService>();
        context.Services.AddSingleton<KeeperShiftSingletonService>();
        context.Services.AddSingleton<IWebSocketConnectionManager, WebSocketConnectionManager>();
        context.Services.AddScoped<IHandlerService, HandlerService>();
        context.Services.AddTransient<WebSocketHeartbeatWorker>();
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

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {

        var backgroundWorkerManager = context.ServiceProvider.GetRequiredService<IBackgroundWorkerManager>();

        var worker = context.ServiceProvider.GetRequiredService<WebSocketHeartbeatWorker>();
        backgroundWorkerManager.AddAsync(worker);

        base.OnApplicationInitialization(context);
    }


}
