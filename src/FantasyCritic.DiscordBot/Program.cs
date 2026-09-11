using FantasyCritic.Hosting;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.MySQL.DapperTypeMaps;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace FantasyCritic.DiscordBot;

public static class Program
{
    public static async Task<int> Main()
    {
        var loggingPaths = LoggingPaths.DiscordBot;
        var builder = Host.CreateApplicationBuilder();

        Log.Logger = CreateLogger(loggingPaths, builder.Environment, configuration: null);

        try
        {
            DapperNodaTimeSetup.SetupDapperNodaTimeMappings();
            Log.Information("Starting Discord bot in {EnvironmentName} mode.", builder.Environment.EnvironmentName);

            var configuration = await FantasyCriticConfigurationLoader.Load(builder.Environment);

            //Loki credentials live in the secret store, so the real logger cannot be built until now.
            Log.Logger = CreateLogger(loggingPaths, builder.Environment, configuration);

            var botToken = configuration["BotToken"];
            if (string.IsNullOrWhiteSpace(botToken) || botToken == "secret")
            {
                Log.Fatal("No Discord bot token is configured.");
                return 1;
            }

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
            builder.Services.AddFantasyCriticDiscordBot(configuration);

            await builder.Build().RunAsync();
            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Discord bot terminated unexpectedly");
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
