using System.Runtime.InteropServices;
using System.Security.Claims;
using FantasyCritic.Hosting;
using FantasyCritic.Lib.Configuration;
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
    public static async Task<int> Main(string[] args)
    {
        var loggingPaths = LoggingPaths.WebApplication;
        FantasyCriticLogging.UseBootstrapLogger(CreateLoggerConfiguration(loggingPaths, environment: null, grafana: null));

        try
        {
            DapperNodaTimeSetup.SetupDapperNodaTimeMappings();
            Log.Information("Starting web host");
            //The application name locates the static assets manifest. Under NSwag's document generation it defaults to the
            //assembly's full name, which finds no manifest, so pin it to the simple name.
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                Args = args,
                ApplicationName = typeof(Program).Assembly.GetName().Name
            });
            Log.Information($"Running in {builder.Environment.EnvironmentName} mode.");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                builder.WebHost.UseIIS();
            }

            builder.Host.UseSerilog();

            var configuration = await FantasyCriticConfigurationLoader.Load(builder.Environment);
            var boundOptions = configuration.Get<WebOptions>();

            //Loki credentials live in the secret store, so the real logger cannot be built until now. It is built before the
            //options are validated, so that a host refusing to start says why in Loki too.
            FantasyCriticLogging.ReplaceBootstrapLogger(() => CreateLoggerConfiguration(loggingPaths, builder.Environment, boundOptions?.Grafana));

            var validOptions = boundOptions.ToValidOptions(builder.Environment.GetFantasyCriticEnvironment());
            if (validOptions.IsFailure)
            {
                Log.Fatal("Invalid configuration: {Error}", validOptions.Error);
                return 1;
            }

            var options = validOptions.Value;

            var app = builder
                .ConfigureServices(options)
                .ConfigurePipeline();

            await app.RunAsync();
            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    /// <param name="environment">Unknown while the bootstrap logger is built, before the builder exists.</param>
    private static LoggerConfiguration CreateLoggerConfiguration(LoggingPaths loggingPaths, IHostEnvironment? environment, GrafanaOptions? grafana)
    {
        var loggerConfiguration = FantasyCriticLogging
            .CreateConfiguration(loggingPaths, LogEventLevel.Warning)
            .WriteToApplicationLogFile(loggingPaths);

        if (environment is null || grafana is null || environment.IsDevelopment())
        {
            return loggerConfiguration;
        }

        //The HTTP enrichers only make sense here: they read the current request, so the bot and the
        //migrator have nothing to give them.
        return loggerConfiguration
            .Enrich.WithClientIp()
            .Enrich.WithCorrelationId()
            .Enrich.WithUserClaims(ClaimTypes.NameIdentifier, ClaimTypes.Email)
            .WriteToGrafanaLoki(environment.EnvironmentName, grafana);
    }
}
