using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartParking.Payments.PaymentIntegrations;
using Org.BouncyCastle.Asn1.X509.Qualified;
using SmartParking.Application.Utilities;
using SmartParking.BarrierAppService;
using SmartParking.CustomerPackages.CustomerPackageTransaction;
using SmartParking.CustomerPackages.CustomerPackageTransactions;
using SmartParking.DispenserEndPoint;
using SmartParking.Events;
using SmartParking.GarageTransactionEndPoints.Factory;
using SmartParking.GarageTransactions;
using SmartParking.GarageTransactions.Factory;
using SmartParking.Invoices;
using SmartParking.Invoices.Factory;
using SmartParking.KeeperShifts;
using SmartParking.PDF;
using SmartParking.PDF.Contracts;
using SmartParking.Utilities;
using SmartParking.Watchers;
using SmartParking.WebSockets;
using System;
using System.Net.Http;
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
    typeof(AbpFluentValidationModule)
    )]
public class SmartParkingApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        // ── Active-provider IPaymentGatewayIntegrationService ─────────────────
        // Resolves the configured active gateway so Blazor pages can inject
        // IPaymentGatewayIntegrationService directly for session initiation.
        // Named HttpClients (AddHttpClient) and the Func<GatewayKey,…> factory
        // are registered in SmartParkingBlazorModule where Microsoft.Extensions.Http
        // is available via the AspNetCore shared framework.
        var activeProviderStr = configuration["Payment:ActiveProvider"] ?? "Geidea";
        if (Enum.TryParse<GatewayKey>(activeProviderStr, true, out var activeProvider))
        {
            context.Services.AddTransient<IPaymentGatewayIntegrationService>(sp => activeProvider switch
            {
                GatewayKey.Oppwa  => CreateGateway<OppwaGatewayIntegrationService>(sp),
                GatewayKey.Geidea => CreateGateway<GeideaGatewayIntegrationService>(sp),
                _                 => throw new NotSupportedException(
                    $"No IPaymentGatewayIntegrationService registered for GatewayKey.{activeProvider}")
            });
        }
        else
        {
            context.Services.AddTransient<IPaymentGatewayIntegrationService>(
                sp => CreateGateway<GeideaGatewayIntegrationService>(sp));
        }

        // ─────────────────────────────────────────────────────────────────────

        context.Services.AddTransient<IInvoiceUtilityAppService, InvoiceUtilityAppService>();

        // ── ZATCA service registration based on configuration ─────────────────
        // Register IZatcaTaxInvoiceAppService using a factory that checks if ZATCA
        // is enabled in GeideaOptions. If enabled, use the real implementation;
        // otherwise use the NullZatcaTaxInvoiceAppService (Null Object pattern).
        context.Services.AddTransient<IZatcaTaxInvoiceAppService>(sp =>
        {
            var optionsMonitor = sp.GetRequiredService<IOptionsMonitor<GeideaOptions>>();
            var geideaOptions = optionsMonitor.CurrentValue;

            return geideaOptions.Zatca.Enabled
                ? sp.GetRequiredService<ZatcaTaxInvoiceAppService>()
                : (IZatcaTaxInvoiceAppService)new NullZatcaTaxInvoiceAppService();
        });

        context.Services.AddTransient<ZatcaTaxInvoiceAppService>();

        // ─────────────────────────────────────────────────────────────────────
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
        context.Services.AddScoped<ICustomerPackageHandlerAppService, CustomerPackageHandlerAppService>();
        context.Services.AddScoped<ICustomerPackageTransactionActionsAppService, CameraTransactionActionsAppService>();
        context.Services.AddScoped<ICustomerPackageTransactionActionsAppService, DispenserTransactionActionsAppService>();



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

    // ── Private helper ────────────────────────────────────────────────────────

    /// <summary>
    /// Constructs a gateway service using <see cref="ActivatorUtilities"/> so that
    /// ABP-registered dependencies (IOptionsMonitor, repositories, etc.) are resolved
    /// from DI while a fresh <see cref="HttpClient"/> is injected directly.
    ///
    /// Named HttpClient registration (AddHttpClient) lives in SmartParkingBlazorModule
    /// which has Microsoft.Extensions.Http available. For contexts where the Blazor
    /// module runs (i.e. the web host), IHttpClientFactory is available and will be
    /// used automatically by ActivatorUtilities if the service has a matching ctor.
    /// </summary>
    private static TService CreateGateway<TService>(IServiceProvider sp)
        where TService : class
    {
        // Try to resolve via the full DI graph first (covers the case where Blazor module
        // has already registered IHttpClientFactory and named clients).
        var existing = sp.GetService<TService>();
        if (existing is not null)
            return existing;

        // Fallback: create with a plain HttpClient (test / non-ASP.NET host scenarios).
        return ActivatorUtilities.CreateInstance<TService>(sp, new HttpClient());
    }
}
