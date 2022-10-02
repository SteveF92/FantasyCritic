using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Dapper;
using Dapper.NodaTime;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.OpenCritic;
using FantasyCritic.Lib.Services;
using FantasyCritic.MySQL;
using NodaTime;
using FantasyCritic.Lib.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Serilog;
using FantasyCritic.MySQL.SyncingRepos;

namespace FantasyCritic.BetaSync;

public static class Program
{
    private static string _betaConnectionString = null!;
    private static string _productionRDSName = null!;
    private static string _betaRDSName = null!;

    private static readonly IClock _clock = SystemClock.Instance;

    static async Task Main()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
            .Build();

        _betaConnectionString = configuration["betaConnectionString"];
        _productionRDSName = configuration["productionRDSName"];
        _betaRDSName = configuration["betaRDSName"];

        DapperNodaTimeSetup.Register();

        await RefreshAndCleanDatabase();
    }

    private static async Task RefreshAndCleanDatabase()
    {
        RDSRefresher rdsRefresher = new RDSRefresher(_productionRDSName, _betaRDSName);
        await rdsRefresher.CopySourceToDestination();
        RepositoryConfiguration betaRepoConfig = new RepositoryConfiguration(_betaConnectionString, _clock);
        MySQLFantasyCriticUserStore betaUserStore = new MySQLFantasyCriticUserStore(betaRepoConfig);
        MySQLBetaCleaner cleaner = new MySQLBetaCleaner(_betaConnectionString);

        Log.Information("Cleaning emails/passwords for non-beta users");
        var allUsers = await betaUserStore.GetAllUsers();
        var betaUsers = await betaUserStore.GetUsersInRoleAsync("BetaTester", CancellationToken.None);
        await cleaner.CleanEmailsAndPasswords(allUsers, betaUsers);
    }
}
