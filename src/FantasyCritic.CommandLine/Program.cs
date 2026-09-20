using FantasyCritic.Hosting;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.MySQL.DapperTypeMaps;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace FantasyCritic.CommandLine;

/// <summary>
/// Commands for scripts that need something from the database, which the host they run on cannot reach itself.
/// deploy.sh is the first caller:
///   docker compose run --rm -T command-line worker-should-pull
/// A command's answer, if it has one, is the only thing written to standard output. Logs go to standard error.
/// Exit code 0 means it did what was asked; anything else means it did not.
/// </summary>
public static class Program
{
    private const int SucceededExitCode = 0;
    private const int FailedExitCode = 1;

    private const string Usage =
        """
        Expected one of:
          worker-should-pull                          Prints true or false: whether WorkerShouldPullNewJobs is on.
          worker-stop-pulling                         Turns WorkerShouldPullNewJobs off.
          worker-start-pulling                        Turns WorkerShouldPullNewJobs on.
          worker-wait-idle [--timeout-minutes <n>]    Waits until no job is running. Fails if one still is after the timeout (default 30).
        """;

    public static async Task<int> Main(string[] args)
    {
        var loggingPaths = LoggingPaths.CommandLine;

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
            Log.Information("Running '{Command}' in {EnvironmentName} mode.", string.Join(' ', args), builder.Environment.EnvironmentName);

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
            builder.Services.AddScoped<WorkerCommands>();

            //Built for its container only. It is never started: there is nothing here to host.
            using var host = builder.Build();
            await using var scope = host.Services.CreateAsyncScope();
            return await RunCommand(scope.ServiceProvider, args);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Command '{Command}' terminated unexpectedly", string.Join(' ', args));
            return FailedExitCode;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    private static async Task<int> RunCommand(IServiceProvider services, string[] args)
    {
        var workerCommands = services.GetRequiredService<WorkerCommands>();

        switch (args)
        {
            case ["worker-should-pull"]:
                var shouldPull = await workerCommands.GetShouldPull();
                Console.Out.WriteLine(shouldPull ? "true" : "false");
                return SucceededExitCode;

            case ["worker-stop-pulling"]:
                await workerCommands.StopPulling();
                return SucceededExitCode;

            case ["worker-start-pulling"]:
                await workerCommands.StartPulling();
                return SucceededExitCode;

            case ["worker-wait-idle"]:
                return await WaitUntilIdle(workerCommands, WorkerCommands.DefaultIdleTimeout);

            case ["worker-wait-idle", "--timeout-minutes", var minutesText] when int.TryParse(minutesText, out var minutes) && minutes > 0:
                return await WaitUntilIdle(workerCommands, TimeSpan.FromMinutes(minutes));

            default:
                Log.Error("Unrecognised arguments: '{Arguments}'. {Usage}", string.Join(' ', args), Usage);
                return FailedExitCode;
        }
    }

    private static async Task<int> WaitUntilIdle(WorkerCommands workerCommands, TimeSpan timeout)
    {
        var idle = await workerCommands.WaitUntilIdle(timeout, WorkerCommands.DefaultPollInterval);
        return idle ? SucceededExitCode : FailedExitCode;
    }

    private static Serilog.Core.Logger CreateLogger(LoggingPaths loggingPaths, IHostEnvironment environment, IConfiguration? configuration)
    {
        var loggerConfiguration = FantasyCriticLogging
            .CreateConfiguration(loggingPaths, LogEventLevel.Warning, consoleToStandardError: true)
            .WriteToApplicationLogFile(loggingPaths);

        if (configuration is not null && !environment.IsDevelopment())
        {
            loggerConfiguration = loggerConfiguration.WriteToGrafanaLoki(environment, configuration);
        }

        return loggerConfiguration.CreateLogger();
    }
}
