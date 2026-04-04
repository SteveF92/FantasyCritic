using DbUp;
using DbUp.Engine;
using DbUp.Support;
using FantasyCritic.AWS;
using FantasyCritic.Lib.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.Grafana.Loki;
using System.Reflection;
using System.Text;

namespace FantasyCritic.DatabaseUpdater;

public class Program
{
    private const string outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] ({SourceContext}.{Method}) {Message}{NewLine}{Exception}";
    private static string _connectionString = null!;

    public static async Task<int> Main()
    {
        var loggingPaths = LoggingPaths.DatabaseUpdater;
        (ILoggerFactory loggerFactory, Logger logger) = ConfigureLogging(loggingPaths);

        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                              ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                              ?? "Production";

        var preliminaryConfig = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var awsRegion = preliminaryConfig["AWS:region"] ?? "";
        var configuration = await GetConfiguration(environmentName, awsRegion);
        _connectionString = configuration.GetConnectionString("AdminConnection")!;

        if (environmentName != "Development")
        {
            (loggerFactory, logger) = ConfigureGrafanaLogging(loggingPaths, environmentName, configuration);
        }

        try
        {
            EnsureDatabase.For.MySqlDatabase(_connectionString);

            var scriptsRoot = GetScriptsRoot();
            var sequentialScriptsPath = Path.Combine(scriptsRoot, "Sequential");
            var idempotentScriptsPath = Path.Combine(scriptsRoot, "Idempotent");

            var upgrader =
                DeployChanges.To
                    .MySqlDatabase(_connectionString)
                    .JournalToMySqlTable("fantasycritic", "_schemaversion")
                    // Run-once, journaled scripts
                    .WithScriptsFromFileSystem(sequentialScriptsPath)
                    // Run-always scripts (e.g., views / stored procedures)
                    .WithScripts(GetRunAlwaysScripts(idempotentScriptsPath))
                    .LogTo(loggerFactory)
                    .Build();

            var result = upgrader.PerformUpgrade();

            if (!result.Successful)
            {
                logger.Error($"Database update could not be completed.", result.Error);
                return -1;
            }

            logger.Information("Database update was completed.");
            return 0;
        }
        finally
        {
            logger.Dispose();
            loggerFactory.Dispose();
        }
        
    }

    private static async Task<IConfigurationRoot> GetConfiguration(string environmentName, string awsRegion)
    {
        var isDevelopment = environmentName.Equals("Development", StringComparison.OrdinalIgnoreCase);
        var isStaging = environmentName.Equals("Staging", StringComparison.OrdinalIgnoreCase);

        var nativeConfig = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .AddUserSecrets(Assembly.GetExecutingAssembly(), true);

        if (!isDevelopment)
        {
            string fixedEnvironmentName = environmentName;
            if (isStaging)
            {
                fixedEnvironmentName = "beta";
            }
            var awsStore = new SecretsManagerConfigurationStore(awsRegion, "fantasyCritic", fixedEnvironmentName);
            var awsString = await awsStore.GetConfiguration();

            nativeConfig.AddJsonStream(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(awsString)));
        }

        // Override JSON / AWS secrets (last wins) — required for Docker Compose and hosting env vars.
        nativeConfig.AddEnvironmentVariables();

        return nativeConfig.Build();
    }

    private static string GetScriptsRoot()
    {
        // Prefer scripts located alongside the compiled executable (published container, etc.)
        // Fall back to the working directory (useful when running from the repo).
        var baseDir = AppContext.BaseDirectory;
        var scriptsInBaseDir = Path.Combine(baseDir, "Scripts");
        if (Directory.Exists(scriptsInBaseDir))
        {
            return scriptsInBaseDir;
        }

        var scriptsInCwd = Path.Combine(Directory.GetCurrentDirectory(), "Scripts");
        if (Directory.Exists(scriptsInCwd))
        {
            return scriptsInCwd;
        }

        // Last resort: keep existing behavior (embedded scripts) by failing fast with a clear message.
        throw new DirectoryNotFoundException(
            $"Could not find 'Scripts' directory in '{baseDir}' or '{Directory.GetCurrentDirectory()}'. " +
            "Ensure scripts are copied to the output directory.");
    }

    private static IEnumerable<SqlScript> GetRunAlwaysScripts(string rootFolder)
    {
        if (!Directory.Exists(rootFolder))
        {
            return [];
        }

        // DbUp's built-in file system provider journals scripts as RunOnce.
        // For views / procs we want to *always* run them, so we provide the scripts explicitly as RunAlways.
        var sqlFiles = Directory
            .EnumerateFiles(rootFolder, "*.sql", SearchOption.AllDirectories)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return sqlFiles.Select(sqlFile =>
        {
            var contents = File.ReadAllText(sqlFile, Encoding.UTF8);
            var scriptName = Path.GetRelativePath(rootFolder, sqlFile).Replace('\\', '/');
            return new SqlScript(scriptName, contents, new SqlScriptOptions { ScriptType = ScriptType.RunAlways });
        });
    }

    private static (ILoggerFactory factory, Logger logger) ConfigureLogging(LoggingPaths loggingPaths)
    {
        var loggerConfig = new LoggerConfiguration()
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
            });

        var logger = loggerConfig.CreateLogger();
        var loggerFactory = new LoggerFactory().AddSerilog(logger);
        return (loggerFactory, logger);
    }


    private static (ILoggerFactory factory, Logger logger) ConfigureGrafanaLogging(LoggingPaths loggingPaths, string environmentName, IConfiguration configuration)
    {
        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(loggingPaths.AllLogPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 3, outputTemplate: outputTemplate)
            .WriteTo.File(loggingPaths.WarnLogPath, rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: LogEventLevel.Warning, retainedFileCountLimit: 10, outputTemplate: outputTemplate);

        var lokiUri = configuration["Grafana:Loki:Uri"];
        var lokiUserId = configuration["Grafana:Loki:UserId"];
        var lokiApiToken = configuration["Grafana:Loki:ApiToken"];
        if (!string.IsNullOrWhiteSpace(lokiUri) &&
            !string.IsNullOrWhiteSpace(lokiUserId) &&
            !string.IsNullOrWhiteSpace(lokiApiToken))
        {
            var labelSection = configuration.GetSection("Grafana:Loki:Labels");
            var lokiLabels = labelSection.GetChildren()
                .Where(c => !string.IsNullOrWhiteSpace(c.Key) && !string.IsNullOrWhiteSpace(c.Value))
                .Where(c => !string.Equals(c.Key, "env", StringComparison.OrdinalIgnoreCase))
                .Select(c => new LokiLabel { Key = c.Key, Value = c.Value! })
                .Append(new LokiLabel { Key = "env", Value = environmentName })
                .ToArray();

            loggerConfig = loggerConfig.WriteTo.GrafanaLoki(
                uri: lokiUri,
                credentials: new LokiCredentials
                {
                    Login = lokiUserId,
                    Password = lokiApiToken
                },
                labels: lokiLabels,
                propertiesAsLabels: new[] { "SourceContext" });
        }

        var logger = loggerConfig.CreateLogger();
        var loggerFactory = new LoggerFactory().AddSerilog(logger);
        return (loggerFactory, logger);
    }
}
