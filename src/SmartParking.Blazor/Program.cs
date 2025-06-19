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
using Microsoft.AspNetCore.HttpLogging;
using System.Text;



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
            builder.Services.AddHttpLogging(logging =>
            {
                logging.LoggingFields = HttpLoggingFields.All;
                    logging.RequestBodyLogLimit = 4096;
                    logging.ResponseBodyLogLimit = 4096;
                    logging.CombineLogs = true;


            });


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
            app.UseSerilogRequestLogging();
            app.UseHttpLogging();
            app.Use(async (context, next) =>
            {
                context.Request.EnableBuffering();

                var request = context.Request;
                var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
                var body = await reader.ReadToEndAsync();
                request.Body.Position = 0;

                Log.Information("HTTP Request: {Method} {Path} - Body: {Body}", request.Method, request.Path, body);

                await next();

                Log.Information("HTTP Response: {StatusCode}", context.Response.StatusCode);
            });

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
