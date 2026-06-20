using FantasyCritic.Lib.Interfaces;
using FantasyCritic.RdsSnapshotManager.Configuration;
using FantasyCritic.RdsSnapshotManager.Services;
using Serilog;

namespace FantasyCritic.RdsSnapshotManager.Console;

public sealed class MainMenu
{
    private readonly SnapshotCreateService _snapshotCreateService;
    private readonly IRDSManager _productionRdsManager;
    private readonly BetaSyncService _betaSyncService;
    private readonly DumpAndPublishService _dumpAndPublishService;
    private readonly LocalImportService _localImportService;
    private readonly LocalDatabaseCleanService _localDatabaseCleanService;
    private readonly RdsSnapshotManagerOptions _options;

    public MainMenu(
        SnapshotCreateService snapshotCreateService,
        IRDSManager productionRdsManager,
        BetaSyncService betaSyncService,
        DumpAndPublishService dumpAndPublishService,
        LocalImportService localImportService,
        LocalDatabaseCleanService localDatabaseCleanService,
        RdsSnapshotManagerOptions options)
    {
        _snapshotCreateService = snapshotCreateService;
        _productionRdsManager = productionRdsManager;
        _betaSyncService = betaSyncService;
        _dumpAndPublishService = dumpAndPublishService;
        _localImportService = localImportService;
        _localDatabaseCleanService = localDatabaseCleanService;
        _options = options;
    }

    public async Task Run(CancellationToken cancellationToken)
    {
        while (true)
        {
            System.Console.WriteLine();
            System.Console.WriteLine("RDS Snapshot Manager");
            System.Console.WriteLine("1. Create production snapshot");
            System.Console.WriteLine("2. Beta sync from snapshot");
            System.Console.WriteLine("3. Dump and publish from instance");
            System.Console.WriteLine("4. Import local dump to Docker MySQL");
            System.Console.WriteLine("5. Clean local Docker database (scrub sensitive data)");
            System.Console.WriteLine("0. Exit");
            System.Console.Write("Select option: ");

            var choice = System.Console.ReadLine();
            switch (choice)
            {
                case "1":
                    await CreateSnapshot(cancellationToken);
                    break;
                case "2":
                    await BetaSync(cancellationToken);
                    break;
                case "3":
                    await DumpAndPublish(cancellationToken);
                    break;
                case "4":
                    await ImportLocalDump(cancellationToken);
                    break;
                case "5":
                    await CleanLocalDatabase(cancellationToken);
                    break;
                case "0":
                    return;
                default:
                    System.Console.WriteLine("Invalid option.");
                    break;
            }
        }
    }

    private async Task CreateSnapshot(CancellationToken cancellationToken)
    {
        System.Console.Write("Custom snapshot name (leave blank for auto-generated): ");
        var customName = System.Console.ReadLine();
        if (string.IsNullOrWhiteSpace(customName))
        {
            customName = null;
        }

        try
        {
            var result = await _snapshotCreateService.CreateSnapshot(customName, cancellationToken);
            if (result.IsSuccess)
            {
                System.Console.WriteLine($"Snapshot created: {result.Value}");
            }
            else
            {
                System.Console.WriteLine($"Failed to create snapshot: {result.Error}");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create snapshot.");
            System.Console.WriteLine($"Failed to create snapshot: {ex.Message}");
        }
    }

    private async Task BetaSync(CancellationToken cancellationToken)
    {
        try
        {
            var snapshots = await _productionRdsManager.GetRecentSnapshots();
            var recentSnapshots = snapshots
                .OrderByDescending(x => x.CreationTime)
                .Take(10)
                .ToList();

            var snapshotIdentifier = SnapshotPicker.PickSnapshot(recentSnapshots);
            if (snapshotIdentifier is null)
            {
                return;
            }

            Log.Information("Starting beta sync from {Snapshot}", snapshotIdentifier);
            var result = await _betaSyncService.Sync(snapshotIdentifier, cancellationToken);
            if (result.IsSuccess)
            {
                System.Console.WriteLine("Beta sync complete.");
            }
            else
            {
                System.Console.WriteLine($"Beta sync failed: {result.Error}");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Beta sync failed.");
            System.Console.WriteLine($"Beta sync failed: {ex.Message}");
        }
    }

    private async Task DumpAndPublish(CancellationToken cancellationToken)
    {
        var instanceName = InstancePicker.PickInstance(_options);
        if (instanceName is null)
        {
            System.Console.WriteLine("Invalid instance selection.");
            return;
        }

        try
        {
            Log.Information("Starting dump and publish from {Instance}", instanceName);
            var result = await _dumpAndPublishService.DumpAndPublish(instanceName, cancellationToken);
            if (result.IsSuccess)
            {
                System.Console.WriteLine($"Dump complete: {result.Value}");
            }
            else
            {
                System.Console.WriteLine($"Dump and publish failed: {result.Error}");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Dump and publish failed.");
            System.Console.WriteLine($"Dump and publish failed: {ex.Message}");
        }
    }

    private async Task ImportLocalDump(CancellationToken cancellationToken)
    {
        if (!Directory.Exists(_options.LocalStagingDirectory))
        {
            System.Console.WriteLine($"Staging directory not found: {_options.LocalStagingDirectory}");
            return;
        }

        var dumpFiles = Directory.GetFiles(_options.LocalStagingDirectory, "*.sql.gz")
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .ToList();

        if (dumpFiles.Count == 0)
        {
            System.Console.WriteLine("No .sql.gz files found in staging directory.");
            return;
        }

        for (var index = 0; index < dumpFiles.Count; index++)
        {
            System.Console.WriteLine($"{index}: {Path.GetFileName(dumpFiles[index])}");
        }

        System.Console.Write("Select dump file index: ");
        if (!int.TryParse(System.Console.ReadLine(), out var selected) || selected < 0 || selected >= dumpFiles.Count)
        {
            System.Console.WriteLine("Invalid selection.");
            return;
        }

        var dumpPath = dumpFiles[selected];

        try
        {
            var result = await _localImportService.Import(dumpPath, force: false, cancellationToken);
            if (result.IsFailure && result.Error.Contains("not empty", StringComparison.OrdinalIgnoreCase))
            {
                System.Console.Write("Database is not empty. Force import? (y/N): ");
                var forceResponse = System.Console.ReadLine();
                if (string.Equals(forceResponse, "y", StringComparison.OrdinalIgnoreCase))
                {
                    result = await _localImportService.Import(dumpPath, force: true, cancellationToken);
                }
            }

            if (result.IsSuccess)
            {
                System.Console.WriteLine("Import complete.");
            }
            else
            {
                System.Console.WriteLine($"Import failed: {result.Error}");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Import failed.");
            System.Console.WriteLine($"Import failed: {ex.Message}");
        }
    }

    private async Task CleanLocalDatabase(CancellationToken cancellationToken)
    {
        System.Console.WriteLine();
        System.Console.WriteLine("This scrubs emails, passwords, external logins, Discord data, and unprocessed bids/drops in non-test leagues.");
        System.Console.WriteLine("It only runs against the configured local Docker MySQL instance (localhost:3307).");
        System.Console.Write("Continue? (y/N): ");
        var confirmation = System.Console.ReadLine();
        if (!string.Equals(confirmation, "y", StringComparison.OrdinalIgnoreCase))
        {
            System.Console.WriteLine("Cancelled.");
            return;
        }

        try
        {
            Log.Information("Starting local database clean.");
            var result = await _localDatabaseCleanService.Clean(cancellationToken);
            if (result.IsSuccess)
            {
                System.Console.WriteLine("Local database clean complete.");
            }
            else
            {
                System.Console.WriteLine($"Local database clean failed: {result.Error}");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Local database clean failed.");
            System.Console.WriteLine($"Local database clean failed: {ex.Message}");
        }
    }
}
