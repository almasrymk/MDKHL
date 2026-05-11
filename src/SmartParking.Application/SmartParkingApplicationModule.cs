using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmartParking.BarrierAppService;
using SmartParking.DispenserEndPoint;
using SmartParking.Events;
using SmartParking.GarageTransactionEndPoints.Factory;
using SmartParking.GarageTransactions;
using SmartParking.GarageTransactions.Factory;
using SmartParking.Invoices;
using SmartParking.Invoices.Factory;
using SmartParking.KeeperShifts;
using SmartParking.PDF;
using SmartParking.Utilities;
using SmartParking.Watchers;
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
        context.Services.AddTransient<IBarrierAppService, BarrierAppService.BarrierAppService>();
        context.Services.AddTransient<IDispenserEndPointsAppService, DispenserEndPointsAppService>();
        context.Services.AddSingleton<KeeperShiftSingletonService>();
        context.Services.AddSingleton<IWebSocketConnectionManager, WebSocketConnectionManager>();
        context.Services.AddSingleton<IHandlerService, HandlerService>();
        context.Services.AddScoped<ILocalizationFactoryAppService, InvoiceLocalizationAppService>();
        context.Services.AddScoped<InvoicePDFAppService>();
        context.Services.AddScoped<PackagePDFAppService>();
        context.Services.AddScoped<UnitPackagePDFAppService>();
        context.Services.AddScoped<IPdfGeneratorFactoryAppService, PdfGeneratorFactoryAppService>();
        context.Services.AddScoped<IGarageTransactionActionsAppService, GarageTransactionsCameraActionsAppService>();
        context.Services.AddScoped<IGarageTransactionActionsAppService, GarageTransactionsDispenserActionsAppService>();
        context.Services.AddScoped<IGarageTransactionsActionsFactoryAppService, GarageTransactionsActionsFactoryAppService>();
        context.Services.AddTransient<IInvoiceActionsFactoryAppService, InvoiceActionsFactoryAppService>();
        context.Services.AddTransient<IInvoiceActionsAppService, InvoiceCameraActionsAppService>();
        context.Services.AddTransient<IInvoiceActionsAppService, InvoiceDispenserActionsAppService>();
        context.Services.AddScoped<IWatcherAppService, WatcherAppService>();

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
