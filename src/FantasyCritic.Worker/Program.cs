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

        var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
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

            builder.ConfigureContainer(new DefaultServiceProviderFactory(new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true
            }));

            builder.Configuration.AddConfiguration(configuration);
            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(Log.Logger);

            builder.Services.AddFantasyCriticCore(configuration, builder.Environment);
            builder.Services.AddFantasyCriticIdentityCore();
            builder.Services.AddFantasyCriticJobHandlers();
            builder.Services.AddHostedService<Worker>();
            builder.Services.AddHostedService<Scheduler>();

            await builder.Build().RunAsync();
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
            .WriteToApplicationLogFile(loggingPaths);

        if (configuration is not null && !environment.IsDevelopment())
        {
            loggerConfiguration = loggerConfiguration.WriteToGrafanaLoki(environment, configuration);
        }

        return loggerConfiguration.CreateLogger();
    }
}
