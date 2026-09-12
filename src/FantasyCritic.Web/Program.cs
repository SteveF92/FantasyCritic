using System.Runtime.InteropServices;
using System.Security.Claims;
using FantasyCritic.Hosting;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.MySQL.DapperTypeMaps;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace FantasyCritic.Web;

public class Program
{
    public static async Task Main(string[] args)
    {
        var loggingPaths = LoggingPaths.WebApplication;
        ConfigureLogging(loggingPaths);

        try
        {
            DapperNodaTimeSetup.SetupDapperNodaTimeMappings();
            Log.Information("Starting web host");
            var builder = WebApplication.CreateBuilder(args);
            Log.Information($"Running in {builder.Environment.EnvironmentName} mode.");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                builder.WebHost.UseIIS();
            }

            builder.Host.UseSerilog();

            var configuration = await FantasyCriticConfigurationLoader.Load(builder.Environment);

            var app = builder
                .ConfigureServices(configuration)
                .ConfigurePipeline();

            if (!app.Environment.IsDevelopment())
            {
                ConfigureGrafanaLogging(loggingPaths, app.Environment, configuration);
            }

            await app.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static void ConfigureLogging(LoggingPaths loggingPaths)
    {
        Log.Logger = FantasyCriticLogging
            .CreateConfiguration(loggingPaths, LogEventLevel.Warning)
            .WriteToApplicationLogFile(loggingPaths)
            .CreateLogger();
    }

    private static void ConfigureGrafanaLogging(LoggingPaths loggingPaths, IWebHostEnvironment env, IConfiguration configuration)
    {
        //The HTTP enrichers only make sense here: they read the current request, so the bot and the
        //migrator have nothing to give them.
        Log.Logger = FantasyCriticLogging
            .CreateConfiguration(loggingPaths, LogEventLevel.Warning)
            .Enrich.WithClientIp()
            .Enrich.WithCorrelationId()
            .Enrich.WithUserClaims(ClaimTypes.NameIdentifier, ClaimTypes.Email)
            .WriteToGrafanaLoki(env, configuration)
            .CreateLogger();
    }
}
