using DbUp;
using DbUp.Engine;
using DbUp.Support;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using System.Text;

namespace FantasyCritic.DatabaseUpdater;

public class Program
{
    private static string _connectionString = null!;

    public static int Main()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
            .Build();

        _connectionString = configuration["ConnectionString"]!;

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
                .LogToConsole()
                .Build();

        var result = upgrader.PerformUpgrade();

        if (!result.Successful)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(result.Error);
            Console.ResetColor();
#if DEBUG
            Console.ReadLine();
#endif
            return -1;
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Success!");
        Console.ResetColor();
        return 0;
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
}
