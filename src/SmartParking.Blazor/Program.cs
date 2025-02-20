using System;
using System.Threading.Tasks;
using Blazorise;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Microsoft.JSInterop;
using Blazorise.Bootstrap5;
using MudBlazor.Services;
using Blazored.LocalStorage;
using SmartParking.Blazor.Hubs;
using SmartParking.Blazor.Events;



namespace SmartParking.Blazor;

public class Program
{
    public async static Task<int> Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
#if DEBUG
            .MinimumLevel.Debug()
#else
            .MinimumLevel.Information()
#endif
           
            .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Error)
            .Enrich.FromLogContext()
            .WriteTo.Async(c => c.File($"Logs/logs_{DateTime.Now.ToString("dd-MM-yyyy")}.txt"))
            .WriteTo.Async(c => c.Console())
            .CreateLogger();

        try
        {
            Log.Information("Starting web host.");
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.AddAppSettingsSecretsJson()
                .UseAutofac()
                .UseSerilog();

            builder.Services.AddMudServices();
            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddSignalR();
            builder.Services.AddTransient<FileExportEventHandler>();
            await builder.AddApplicationAsync<SmartParkingBlazorModule>();
            //builder.Services.AddBlazorise()
            //                .AddBootstrapProviders()
            //                .AddFontAwesomeIcons();
            //builder.Services.AddBlazoriseRichTextEdit();


            var app = builder.Build();
            await app.InitializeApplicationAsync();
            app.MapHub<NotificationHub>("/notification");
            await app.RunAsync();
            return 0;
        }
        catch (Exception ex)
        {
            if (ex is HostAbortedException)
            {
                throw;
            }

            Log.Fatal(ex, "Host terminated unexpectedly!");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
