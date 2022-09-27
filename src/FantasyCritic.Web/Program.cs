using System.Reflection;
using System.Runtime.InteropServices;
using Amazon;
using Amazon.CloudWatchLogs;
using Microsoft.AspNetCore.Hosting;
using Dapper.NodaTime;
using FantasyCritic.AWS;
using FantasyCritic.Lib.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.AwsCloudWatch;
using System.IO;

namespace FantasyCritic.Web;

public class Program
{
    private const string outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] ({SourceContext}.{Method}) {Message}{NewLine}{Exception}";

    public static async Task Main(string[] args)
    {
        var loggingPaths = new LoggingPaths();
        ConfigureLogging(loggingPaths);

        try
        {
            DapperNodaTimeSetup.Register();
            Log.Information("Starting web host");
            var builder = WebApplication.CreateBuilder(args);
            Log.Information($"Running in {builder.Environment.EnvironmentName} mode.");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                builder.WebHost.UseIIS();
            }

            builder.Host.UseSerilog();

            var awsRegion = builder.Configuration["AWS:region"];
            var configuration = await GetConfiguration(builder.Environment, awsRegion);
            var app = builder
                .ConfigureServices(configuration)
                .ConfigurePipeline();

            if (!app.Environment.IsDevelopment())
            {
                ConfigureCloudLogging(loggingPaths, app.Environment, awsRegion);
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

    private static async Task<IConfigurationRoot> GetConfiguration(IWebHostEnvironment environment, string awsRegion)
    {
        var nativeConfig = new ConfigurationBuilder()
            .SetBasePath(environment.ContentRootPath)
            .AddJsonFile("appsettings.json")
            .AddUserSecrets(Assembly.GetExecutingAssembly(), true);

        if (!environment.IsDevelopment())
        {
            string fixedEnvironmentName = environment.EnvironmentName;
            if (environment.IsStaging())
            {
                fixedEnvironmentName = "beta";
            }
            var awsStore = new SecretsManagerConfigurationStore(awsRegion, "fantasyCritic", fixedEnvironmentName);
            var awsString = await awsStore.GetConfiguration();

            nativeConfig.AddJsonStream(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(awsString)));
        }

        return nativeConfig.Build();
    }

    private static void ConfigureLogging(LoggingPaths loggingPaths)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(loggingPaths.AllLogPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 3, outputTemplate: outputTemplate)
            .WriteTo.File(loggingPaths.WarnLogPath, rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: LogEventLevel.Warning, retainedFileCountLimit: 10, outputTemplate: outputTemplate)
            .WriteTo.Logger(config =>
            {
                config.Filter
                    .ByIncludingOnly(logEvent =>
                    {
                        var sourceContext = logEvent.Properties.GetValueOrDefault("SourceContext");
                        var sourceContextString = sourceContext?.ToString();
                        return sourceContextString != null && sourceContextString.StartsWith("\"FantasyCritic");
                    })
                    .WriteTo.File(loggingPaths.MyLogPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 5, outputTemplate: outputTemplate);
            })
            .CreateLogger();
    }

    private static void ConfigureCloudLogging(LoggingPaths loggingPaths, IWebHostEnvironment env, string awsRegion)
    {
        var region = RegionEndpoint.GetBySystemName(awsRegion);
        var client = new AmazonCloudWatchLogsClient(region);

        string environmentName = "dev";
        if (env.IsStaging())
        {
            environmentName = "beta";
        }

        if (env.IsProduction())
        {
            environmentName = "production";
        }

        var logGroupName = "/fantasyCritic/" + environmentName;
        Log.Information($"Using cloudwatch: {logGroupName}");

        // options for the sink defaults in https://github.com/Cimpress-MCP/serilog-sinks-awscloudwatch/blob/master/src/Serilog.Sinks.AwsCloudWatch/CloudWatchSinkOptions.cs
        var options = new CloudWatchSinkOptions
        {
            // the name of the CloudWatch Log group for logging
            LogGroupName = logGroupName,

            // the main formatter of the log event
            TextFormatter = new JsonFormatter(),

            // other defaults defaults
            MinimumLogEventLevel = LogEventLevel.Information,
            BatchSizeLimit = 100,
            QueueSizeLimit = 10000,
            Period = TimeSpan.FromSeconds(10),
            CreateLogGroup = true,
            LogStreamNameProvider = new DefaultLogStreamProvider(),
            RetryAttempts = 5
        };

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(loggingPaths.AllLogPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 3, outputTemplate: outputTemplate)
            .WriteTo.File(loggingPaths.WarnLogPath, rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: LogEventLevel.Warning, retainedFileCountLimit: 10, outputTemplate: outputTemplate)
            .WriteTo.Logger(config =>
            {
                config.Filter
                    .ByIncludingOnly(logEvent =>
                    {
                        var sourceContext = logEvent.Properties.GetValueOrDefault("SourceContext");
                        var sourceContextString = sourceContext?.ToString();
                        return sourceContextString != null && sourceContextString.StartsWith("\"FantasyCritic");
                    })
                    .WriteTo.File(loggingPaths.MyLogPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 5, outputTemplate: outputTemplate);
            })
            .WriteTo.AmazonCloudWatch(options, client)
            .CreateLogger();
    }
}
