using FantasyCritic.Hosting;
using FantasyCritic.Lib.Configuration;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Jobs;
using FantasyCritic.MySQL.DapperTypeMaps;
using Serilog;
using Serilog.Events;

namespace FantasyCritic.Worker;

public static class Program
{
    public static async Task<int> Main()
    {
        var loggingPaths = LoggingPaths.Worker;

        //A web application only so that it can answer GET /health. It serves nothing else.
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = FantasyCriticEnvironments.NameFromEnvironmentVariables(),
            ContentRootPath = AppContext.BaseDirectory
        });

        FantasyCriticLogging.UseBootstrapLogger(CreateLoggerConfiguration(loggingPaths, builder.Environment, grafana: null));

        try
        {
            DapperNodaTimeSetup.SetupDapperNodaTimeMappings();
            Log.Information("Starting Worker in {EnvironmentName} mode.", builder.Environment.EnvironmentName);

            var configuration = await FantasyCriticConfigurationLoader.Load(builder.Environment);
            var boundOptions = configuration.Get<WorkerOptions>();

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

            builder.Host.UseDefaultServiceProvider(providerOptions =>
            {
                providerOptions.ValidateOnBuild = true;
                providerOptions.ValidateScopes = true;
            });

            builder.Configuration.AddConfiguration(configuration);
            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(Log.Logger);

            builder.Services.AddFantasyCriticCore(options.ConnectionStrings, options.Discord, options.BaseAddress, builder.Environment);
            builder.Services.AddFantasyCriticIdentityCore();
            builder.Services.AddFantasyCriticAdminServices(options.Aws, options.OpenCritic, options.Authentication.Patreon);
            builder.Services.AddFantasyCriticEmail(options.Postmark);
            builder.Services.AddFantasyCriticJobHandlers();
            builder.Services.AddSingleton<WorkerStatus>();
            builder.Services.AddHealthChecks().AddCheck<WorkerHealthCheck>("worker");
            builder.Services.AddHostedService<Worker>();
            builder.Services.AddHostedService<Scheduler>();

            var app = builder.Build();
            app.MapFantasyCriticHealth();
            await app.RunAsync();
            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Worker terminated unexpectedly");
            return 1;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    private static LoggerConfiguration CreateLoggerConfiguration(LoggingPaths loggingPaths, IHostEnvironment environment, GrafanaOptions? grafana)
    {
        var loggerConfiguration = FantasyCriticLogging
            .CreateConfiguration(loggingPaths, LogEventLevel.Information)
            //The only requests are health probes, every few seconds. Each would otherwise log four lines.
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .WriteToApplicationLogFile(loggingPaths)
            .WriteToFlowLogFiles(loggingPaths, WorkerLogging.Flows);

        if (grafana is not null && !environment.IsDevelopment())
        {
            loggerConfiguration = loggerConfiguration.WriteToGrafanaLoki(environment.EnvironmentName, grafana);
        }

        return loggerConfiguration;
    }
}
