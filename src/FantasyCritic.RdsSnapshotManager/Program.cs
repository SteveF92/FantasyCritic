using System.Reflection;
using FantasyCritic.AWS;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.MySQL;
using FantasyCritic.MySQL.DapperTypeMaps;
using FantasyCritic.MySQL.SyncingRepos;
using FantasyCritic.RdsSnapshotManager.Configuration;
using FantasyCritic.RdsSnapshotManager.Destinations;
using FantasyCritic.RdsSnapshotManager.Infrastructure;
using FantasyCritic.RdsSnapshotManager.Services;
using Microsoft.Extensions.Configuration;
using NodaTime;
using Serilog;

namespace FantasyCritic.RdsSnapshotManager;

public static class Program
{
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

        var options = new RdsSnapshotManagerOptions();
        configuration.Bind(options);

        DapperNodaTimeSetup.SetupDapperNodaTimeMappings();

        IClock clock = SystemClock.Instance;

        IRDSManager productionRdsManager = new RDSManager(options.ProductionRdsInstance);
        var restoreService = new RdsRestoreService();
        var mysqldumpRunner = new MysqldumpRunner();
        var dockerHealthChecker = new DockerMySqlHealthChecker();
        var emptyChecker = new DatabaseEmptyChecker();
        var destinations = BackupDestinationFactory.CreateRegistrations(options);

        RepositoryConfiguration betaRepoConfig = new RepositoryConfiguration(options.BetaConnectionString, clock);
        MySQLFantasyCriticUserStore betaUserStore = new MySQLFantasyCriticUserStore(betaRepoConfig);
        MySQLBetaCleaner betaCleaner = new MySQLBetaCleaner(options.BetaConnectionString);

        RepositoryConfiguration localRepoConfig = new RepositoryConfiguration(options.LocalDocker.ConnectionString, clock);
        MySQLFantasyCriticUserStore localUserStore = new MySQLFantasyCriticUserStore(localRepoConfig);
        MySQLBetaCleaner localCleaner = new MySQLBetaCleaner(options.LocalDocker.ConnectionString);

        SnapshotCreateService snapshotCreateService = new SnapshotCreateService(productionRdsManager, clock);
        BetaSyncService betaSyncService = new BetaSyncService(restoreService, options, betaCleaner, betaUserStore);
        DumpAndPublishService dumpAndPublishService = new DumpAndPublishService(options, mysqldumpRunner, destinations, clock);
        LocalImportService localImportService = new LocalImportService(
            options,
            dockerHealthChecker,
            emptyChecker,
            mysqldumpRunner,
            localCleaner,
            localUserStore);

        Console.MainMenu mainMenu = new Console.MainMenu(
            snapshotCreateService,
            productionRdsManager,
            betaSyncService,
            dumpAndPublishService,
            localImportService,
            options);

        await mainMenu.Run(CancellationToken.None);
    }
}
