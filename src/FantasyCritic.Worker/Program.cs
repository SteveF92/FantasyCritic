using FantasyCritic.Hosting;
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
            EnvironmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                              ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
            ContentRootPath = AppContext.BaseDirectory
        });

        Log.Logger = CreateLogger(loggingPaths, builder.Environment, configuration: null);

        try
        {
            DapperNodaTimeSetup.SetupDapperNodaTimeMappings();
            Log.Information("Starting Worker in {EnvironmentName} mode.", builder.Environment.EnvironmentName);

            var configuration = await FantasyCriticConfigurationLoader.Load(builder.Environment);

            //Loki credentials live in the secret store, so the real logger cannot be built until now.
            Log.Logger = CreateLogger(loggingPaths, builder.Environment, configuration);

            builder.Host.UseDefaultServiceProvider(options =>
            {
                options.ValidateOnBuild = true;
                options.ValidateScopes = true;
            });

            builder.Configuration.AddConfiguration(configuration);
            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(Log.Logger);

            builder.Services.AddFantasyCriticCore(configuration, builder.Environment);
            builder.Services.AddFantasyCriticIdentityCore();
            builder.Services.AddFantasyCriticAdminServices(configuration);
            builder.Services.AddFantasyCriticEmail(configuration);
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

    private static Serilog.Core.Logger CreateLogger(LoggingPaths loggingPaths, IHostEnvironment environment, IConfiguration? configuration)
    {
        var loggerConfiguration = FantasyCriticLogging
            .CreateConfiguration(loggingPaths, LogEventLevel.Information)
            //The only requests are health probes, every few seconds. Each would otherwise log four lines.
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .WriteToApplicationLogFile(loggingPaths)
            .WriteToFlowLogFiles(loggingPaths, WorkerLogging.Flows);

        if (configuration is not null && !environment.IsDevelopment())
        {
            loggerConfiguration = loggerConfiguration.WriteToGrafanaLoki(environment, configuration);
        }

        return loggerConfiguration.CreateLogger();
    }
}
