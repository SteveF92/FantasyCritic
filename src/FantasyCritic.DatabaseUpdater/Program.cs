using DbUp;
using Microsoft.Extensions.Configuration;
using System.Reflection;

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

        var upgrader =
            DeployChanges.To
                .MySqlDatabase(_connectionString)
                .JournalToMySqlTable("fantasycritic", "_schemaversion")
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
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
}
