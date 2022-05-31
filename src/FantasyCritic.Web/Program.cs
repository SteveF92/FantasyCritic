using Microsoft.AspNetCore.Hosting;
using Dapper.NodaTime;
using FantasyCritic.AWS;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Web.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace FantasyCritic.Web;

public class Program
{
    public static async Task Main(string[] args)
    {
        ConfigureLogging();

        try
        {
            DapperNodaTimeSetup.Register();
            Log.Information("Starting web host");
            var builder = WebApplication.CreateBuilder(args);
            builder.WebHost.UseIIS();
            builder.Host.UseSerilog();

            var configurationStore = await GetConfigurationStore(builder.Configuration);
            var app = builder
                .ConfigureServices(configurationStore)
                .ConfigurePipeline();

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

    private static void ConfigureLogging()
    {
        string allLogPath = @"C:\FantasyCritic\Logs\log-all";
        string myLogPath = @"C:\FantasyCritic\Logs\log-my";
        string warnLogPath = @"C:\FantasyCritic\Logs\log-warning";
        string outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] ({SourceContext}.{Method}) {Message}{NewLine}{Exception}";
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.File(allLogPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 3, outputTemplate: outputTemplate)
            .WriteTo.File(warnLogPath, rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: LogEventLevel.Warning, retainedFileCountLimit: 10, outputTemplate: outputTemplate)
            .WriteTo.Logger(config =>
            {
                config.Filter
                    .ByIncludingOnly(logEvent =>
                    {
                        if (logEvent.Properties.TryGetValue("SourceContext", out var sourceContext))
                        {
                            var sourceContextString = sourceContext?.ToString();
                            if (sourceContextString is not null && sourceContextString.StartsWith("\"FantasyCritic"))
                            {
                                return true;
                            }
                        }

                        return false;
                    })
                    .WriteTo.File(myLogPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 5, outputTemplate: outputTemplate);
            })
            .CreateLogger();
    }

    private static async Task<IConfigurationStore> GetConfigurationStore(ConfigurationManager configurationManager)
    {
        IConfigurationStore configurationStore;
        var fileConfigurationStore = new ConfigurationFileStore(configurationManager);
        var secretsManagerPrefix = Environment.GetEnvironmentVariable("secretsManagerPrefix");
        if (!string.IsNullOrWhiteSpace(secretsManagerPrefix))
        {
            var awsRegion = fileConfigurationStore.GetAWSRegion();
            var awsStore = new SecretsManagerConfigurationStore(awsRegion, secretsManagerPrefix);
            await awsStore.PopulateAllValues();
            configurationStore = new ConfigurationStoreSet(fileConfigurationStore, awsStore);
        }
        else
        {
            configurationStore = fileConfigurationStore;
        }

        return configurationStore;
    }
}
