using System.Text;
using DbUp;
using DbUp.Engine;
using DbUp.Support;
using FantasyCritic.DatabaseUpdater.CodeMigrations;
using FantasyCritic.Hosting;
using FantasyCritic.Lib.Configuration;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.MySQL.DapperTypeMaps;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using NodaTime;
using Serilog;
using Serilog.Events;

namespace FantasyCritic.DatabaseUpdater;

public class Program
{
    private const int SucceededExitCode = 0;
    private const int FailedExitCode = 1;

    private static string _connectionString = null!;

    public static async Task<int> Main()
    {
        var loggingPaths = LoggingPaths.DatabaseUpdater;
        var environmentName = FantasyCriticEnvironments.NameFromEnvironmentVariables();

        FantasyCriticLogging.UseBootstrapLogger(CreateLoggerConfiguration(loggingPaths, environmentName, grafana: null));

        try
        {
            Log.Information("Starting database update in {EnvironmentName} mode.", environmentName);

            var configuration = await FantasyCriticConfigurationLoader.Load(environmentName, AppContext.BaseDirectory);
            var boundOptions = configuration.Get<DatabaseUpdaterOptions>();

            //Loki credentials live in the secret store, so the real logger cannot be built until now. It is built before the
            //options are validated, so that a host refusing to start says why in Loki too.
            FantasyCriticLogging.ReplaceBootstrapLogger(() => CreateLoggerConfiguration(loggingPaths, environmentName, boundOptions?.Grafana));

            var validOptions = boundOptions.ToValidOptions(FantasyCriticEnvironments.FromName(environmentName));
            if (validOptions.IsFailure)
            {
                Log.Fatal("Invalid configuration: {Error}", validOptions.Error);
                return FailedExitCode;
            }

            var options = validOptions.Value;

            using var loggerFactory = new LoggerFactory().AddSerilog(Log.Logger);

            _connectionString = options.ConnectionStrings.AdminConnection;
            MySqlConnectionStringBuilder mySqlConnectionStringBuilder = new MySqlConnectionStringBuilder(_connectionString);
            DapperNodaTimeSetup.SetupDapperNodaTimeMappings();

            EnsureDatabase.For.MySqlDatabase(_connectionString);

            var scriptsRoot = GetScriptsRoot();
            var sequentialScriptsPath = Path.Combine(scriptsRoot, "Sequential");
            var idempotentScriptsPath = Path.Combine(scriptsRoot, "Idempotent");

            // Code-based migrations (see CodeMigrations/): for data fixes too complex for plain SQL.
            // Each one is journaled (run-once) by the name it's registered under below, same as the
            // file-based scripts.
            var repositoryConfiguration = new RepositoryConfiguration(_connectionString, SystemClock.Instance);

            var upgrader =
                DeployChanges.To
                    .MySqlDatabase(_connectionString)
                    .JournalToMySqlTable(mySqlConnectionStringBuilder.Database, "_schemaversion")
                    // Run-once, journaled scripts
                    .WithScriptsFromFileSystem(sequentialScriptsPath)
                    // Run-always scripts (e.g., views / stored procedures)
                    .WithScripts(GetRunAlwaysScripts(idempotentScriptsPath))
                    // Run-once, journaled code migrations
                    .WithScript("2026-08-09_002_processSetCleanup.cs", new ProcessSetCleanupMigration(repositoryConfiguration, Log.ForContext<ProcessSetCleanupMigration>()))
                    .WithScript("2026-08-31_001_topBidsAndDropsBackfill.cs", new TopBidsAndDropsBackfillMigration(repositoryConfiguration, Log.ForContext<TopBidsAndDropsBackfillMigration>()))
                    .WithScript("2026-09-02_001_topBidsAndDropsRecompute.cs", new TopBidsAndDropsRecomputeMigration(repositoryConfiguration, Log.ForContext<TopBidsAndDropsRecomputeMigration>()))
                    .WithExecutionTimeout(TimeSpan.FromMinutes(30))
                    .LogTo(loggerFactory)
                    .Build();

            var result = upgrader.PerformUpgrade();

            if (!result.Successful)
            {
                Log.Error(result.Error, "Database update could not be completed.");
                return FailedExitCode;
            }

            Log.Information("Database update was completed.");
            return SucceededExitCode;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Database update terminated unexpectedly");
            return FailedExitCode;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
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

    private static LoggerConfiguration CreateLoggerConfiguration(LoggingPaths loggingPaths, string environmentName, GrafanaOptions? grafana)
    {
        var loggerConfiguration = FantasyCriticLogging
            .CreateConfiguration(loggingPaths, LogEventLevel.Information)
            .WriteToApplicationLogFile(loggingPaths);

        if (grafana is not null && !IsDevelopment(environmentName))
        {
            loggerConfiguration = loggerConfiguration.WriteToGrafanaLoki(environmentName, grafana);
        }

        return loggerConfiguration;
    }

    private static bool IsDevelopment(string environmentName) => FantasyCriticEnvironments.FromName(environmentName) == FantasyCriticEnvironment.Development;
}
