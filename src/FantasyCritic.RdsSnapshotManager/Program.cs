using System.Reflection;
using Amazon.RDS;
using Amazon.S3;
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
    private static async Task Main()
    {
        //Debug, for the MySQL tools' progress and output.
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
            .Build();

        var options = new RdsSnapshotManagerOptions();
        configuration.Bind(options);

        var validation = RdsSnapshotManagerOptionsValidator.Validate(options);
        if (validation.IsFailure)
        {
            Log.Fatal("Invalid RDS Snapshot Manager configuration: {Error}", validation.Error);
            Environment.Exit(1);
            return;
        }

        DapperNodaTimeSetup.SetupDapperNodaTimeMappings();

        IClock clock = SystemClock.Instance;

        var defaultSnapshotSource = RdsInstanceLookup.GetDefaultSnapshotSource(options.RdsInstances);
        IRDSManager defaultSourceRdsManager = new RDSManager(defaultSnapshotSource.InstanceName);
        var restoreService = new RdsRestoreService();
        var mysqldumpRunner = new MysqldumpRunner();
        var dockerHealthChecker = new DockerMySqlHealthChecker();
        var emptyChecker = new DatabaseEmptyChecker();
        var destinations = BackupDestinationFactory.CreateAll(options);

        string localSnapshotConnectionString = LocalSnapshotConnectionString.BuildSnapshotConnectionString(
            options.LocalDocker.ConnectionString);

        RepositoryConfiguration localRepoConfig = new RepositoryConfiguration(localSnapshotConnectionString, clock);
        MySQLFantasyCriticUserStore localUserStore = new MySQLFantasyCriticUserStore(localRepoConfig);
        MySQLBetaCleaner localCleaner = new MySQLBetaCleaner(localSnapshotConnectionString);

        SnapshotCreateService snapshotCreateService = new SnapshotCreateService(clock);
        RestoreSnapshotService restoreSnapshotService = new RestoreSnapshotService(restoreService, options, clock);
        DumpAndPublishService dumpAndPublishService = new DumpAndPublishService(options, mysqldumpRunner, destinations, clock);
        LocalImportService localImportService = new LocalImportService(
            options,
            dockerHealthChecker,
            emptyChecker,
            mysqldumpRunner,
            localCleaner,
            localUserStore);
        LocalDatabaseCleanService localDatabaseCleanService = new LocalDatabaseCleanService(
            options,
            dockerHealthChecker,
            localCleaner,
            localUserStore);
        ManualUploadService manualUploadService = new ManualUploadService(destinations);
        SnapshotArchiveService? snapshotArchiveService = options.Destinations.S3.Enabled
            ? new SnapshotArchiveService(
                new RdsTemporaryRestoreService(new AmazonRDSClient()),
                mysqldumpRunner,
                new S3DatabaseArchiveLocation(new AmazonS3Client(), options.Destinations.S3.Bucket, options.Destinations.S3.Prefix),
                options,
                clock)
            : null;

        Console.MainMenu mainMenu = new Console.MainMenu(
            snapshotCreateService,
            defaultSourceRdsManager,
            restoreSnapshotService,
            dumpAndPublishService,
            localImportService,
            localDatabaseCleanService,
            manualUploadService,
            snapshotArchiveService,
            options);

        await mainMenu.Run(CancellationToken.None);
    }
}
