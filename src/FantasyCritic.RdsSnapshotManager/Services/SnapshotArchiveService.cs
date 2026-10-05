using System.Security.Cryptography;
using System.Text.Json;
using CSharpFunctionalExtensions;
using FantasyCritic.AWS;
using FantasyCritic.Lib.Utilities;
using FantasyCritic.MySQL;
using FantasyCritic.RdsSnapshotManager.Configuration;
using MySqlConnector;
using NodaTime;
using NodaTime.Text;
using Serilog;
using Serilog.Events;

namespace FantasyCritic.RdsSnapshotManager.Services;

//Restores each manual snapshot to a temporary instance, dumps its schemas to S3, then deletes the instance.
public sealed class SnapshotArchiveService
{
    private static readonly ILogger _logger = Log.ForContext<SnapshotArchiveService>();

    private const int MaxConcurrentSnapshots = 3;
    public const int GlacierRestoreDays = 7;
    private const int ConnectAttempts = 10;
    private static readonly TimeSpan ConnectRetryInterval = TimeSpan.FromSeconds(30);
    //An 8.x mysqldump queries a table that 5.7 servers don't have unless this is off.
    private static readonly IReadOnlyList<string> DumpArguments = ["--skip-column-statistics"];
    //Old snapshots hold these views in states that no longer compile, which stop mysqldump. The code never used them.
    private const string SkippedViewPrefix = "vw_utility_";
    private static readonly IReadOnlySet<string> SkippedViews = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "vw_discord_leaguechannel",
        "vw_cacher_mastergameyear",
        "vw_mastergame_statisticsinput",
        "vw_subquery_eligibleleaguesforgame"
    };
    private static readonly JsonSerializerOptions ManifestJsonOptions = new() { WriteIndented = true };

    private readonly RdsTemporaryRestoreService _restoreService;
    private readonly MysqldumpRunner _mysqldumpRunner;
    private readonly S3DatabaseArchiveLocation _archive;
    private readonly RdsSnapshotManagerOptions _options;
    private readonly IClock _clock;

    public SnapshotArchiveService(
        RdsTemporaryRestoreService restoreService,
        MysqldumpRunner mysqldumpRunner,
        S3DatabaseArchiveLocation archive,
        RdsSnapshotManagerOptions options,
        IClock clock)
    {
        _restoreService = restoreService;
        _mysqldumpRunner = mysqldumpRunner;
        _archive = archive;
        _options = options;
        _clock = clock;
    }

    public Task<IReadOnlyList<ManualSnapshot>> GetManualSnapshots(CancellationToken cancellationToken) =>
        _restoreService.GetManualSnapshots(cancellationToken);

    public Task<IReadOnlyList<string>> GetLeftoverInstances(CancellationToken cancellationToken) =>
        _restoreService.GetTemporaryInstanceIdentifiers(cancellationToken);

    public Task DeleteLeftoverInstance(string instanceIdentifier) =>
        _restoreService.DeleteTemporaryInstanceIfExists(instanceIdentifier);

    public async Task<IReadOnlyList<ArchivedSnapshotDump>> GetArchivedDumps(CancellationToken cancellationToken)
    {
        var objects = await _archive.List(SnapshotArchiveNames.FolderPrefix, cancellationToken);
        var objectsByKey = objects.ToDictionary(x => x.Key);
        List<Task<ArchivedSnapshotDump>> dumpTasks = [];
        foreach (var archivedObject in objects)
        {
            var snapshotIdentifier = SnapshotArchiveNames.GetSnapshotIdentifierIfApplicationSchemaKey(archivedObject.Key);
            if (snapshotIdentifier is not null)
            {
                dumpTasks.Add(BuildArchivedDump(snapshotIdentifier, archivedObject, objectsByKey, cancellationToken));
            }
        }

        return await Task.WhenAll(dumpTasks);
    }

    //A manifest the lifecycle rule has moved to Glacier can't be read, so that snapshot's date is unknown.
    private async Task<ArchivedSnapshotDump> BuildArchivedDump(string snapshotIdentifier, ArchivedObject dumpObject,
        IReadOnlyDictionary<string, ArchivedObject> objectsByKey, CancellationToken cancellationToken)
    {
        var manifestKey = SnapshotArchiveNames.BuildManifestKey(snapshotIdentifier);
        if (!objectsByKey.TryGetValue(manifestKey, out var manifestObject) || manifestObject.Availability != ArchivedObjectAvailability.Available)
        {
            return new ArchivedSnapshotDump(snapshotIdentifier, null, dumpObject);
        }

        var manifest = JsonSerializer.Deserialize<SnapshotArchiveManifest>(await _archive.ReadText(manifestKey, cancellationToken))
            ?? throw new InvalidOperationException($"{manifestKey} is empty.");
        var snapshotCreateTime = InstantPattern.General.Parse(manifest.SnapshotCreateTime).Value;
        return new ArchivedSnapshotDump(snapshotIdentifier, snapshotCreateTime, dumpObject);
    }

    public async Task<string> Download(ArchivedSnapshotDump dump, CancellationToken cancellationToken)
    {
        var localPath = Path.Combine(_options.LocalStagingDirectory, "rds-snapshots", Path.GetFileName(dump.Object.Key));
        Directory.CreateDirectory(Path.GetDirectoryName(localPath)!);
        _logger.Information("Downloading {Key} to {Path}", dump.Object.Key, localPath);
        await _archive.Download(dump.Object.Key, localPath, cancellationToken);
        return localPath;
    }

    public Task RequestGlacierRestore(ArchivedSnapshotDump dump, CancellationToken cancellationToken) =>
        _archive.RequestGlacierRestore(dump.Object.Key, GlacierRestoreDays, cancellationToken);

    public static bool CanArchive(ManualSnapshot snapshot) => snapshot.Engine.Equals("mysql", StringComparison.Ordinal);

    public async Task<IReadOnlyList<SnapshotArchiveResult>> ArchiveAll(IReadOnlyList<ManualSnapshot> snapshots, CancellationToken cancellationToken)
    {
        var results = new SnapshotArchiveResult[snapshots.Count];
        var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = MaxConcurrentSnapshots, CancellationToken = cancellationToken };
        await Parallel.ForEachAsync(Enumerable.Range(0, snapshots.Count), parallelOptions, async (index, token) =>
        {
            var result = await Archive(snapshots[index], token);
            results[index] = result;
            var level = result.Outcome == SnapshotArchiveOutcome.Failed ? LogEventLevel.Error : LogEventLevel.Information;
            _logger.Write(level, "{Snapshot}: {Outcome} {Detail}", result.SnapshotIdentifier, result.Outcome, result.Detail);
        });

        return results;
    }

    private async Task<SnapshotArchiveResult> Archive(ManualSnapshot snapshot, CancellationToken cancellationToken)
    {
        if (!CanArchive(snapshot))
        {
            return BuildResult(snapshot, SnapshotArchiveOutcome.Skipped, $"{snapshot.Engine} {snapshot.EngineVersion} can't be dumped with mysqldump.");
        }

        if (snapshot.Status != "available")
        {
            return BuildResult(snapshot, SnapshotArchiveOutcome.Failed, $"Snapshot status is {snapshot.Status}.");
        }

        if (await _archive.Exists(SnapshotArchiveNames.BuildManifestKey(snapshot.Identifier), cancellationToken))
        {
            return BuildResult(snapshot, SnapshotArchiveOutcome.AlreadyArchived, "Manifest already in S3.");
        }

        var instanceIdentifier = SnapshotArchiveNames.BuildTemporaryInstanceIdentifier(snapshot.Identifier);
        var password = RandomNumberGenerator.GetString("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 32);
        SnapshotArchiveResult result;
        try
        {
            var networkTemplate = RdsInstanceLookup.GetDefaultSnapshotSource(_options.RdsInstances).InstanceName;
            var endpoint = await _restoreService.RestoreToTemporaryInstance(snapshot.Identifier, instanceIdentifier, networkTemplate, password,
                cancellationToken);
            var filesResult = await DumpAndUpload(snapshot, endpoint, password, cancellationToken);
            result = filesResult.IsSuccess
                ? BuildResult(snapshot, SnapshotArchiveOutcome.Archived, DescribeFiles(filesResult.Value), filesResult.Value)
                : BuildResult(snapshot, SnapshotArchiveOutcome.Failed, filesResult.Error);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Archiving {Snapshot} failed.", snapshot.Identifier);
            result = BuildResult(snapshot, SnapshotArchiveOutcome.Failed, ex.Message);
        }

        try
        {
            await _restoreService.DeleteTemporaryInstanceIfExists(instanceIdentifier);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Deleting temporary instance {Instance} failed.", instanceIdentifier);
            result = result with
            {
                Outcome = SnapshotArchiveOutcome.Failed,
                Detail = $"{result.Detail} Temporary instance {instanceIdentifier} was left behind: {ex.Message}",
            };
        }

        return result;
    }

    private async Task<Result<IReadOnlyList<ArchivedFile>>> DumpAndUpload(ManualSnapshot snapshot, TemporaryInstanceEndpoint endpoint,
        string password, CancellationToken cancellationToken)
    {
        var serverConnectionString = new MySqlConnectionStringBuilder
        {
            Server = endpoint.Address,
            Port = (uint)endpoint.Port,
            UserID = snapshot.MasterUsername,
            Password = password,
            SslMode = MySqlSslMode.Required,
        }.ConnectionString;

        var schemas = await GetUserSchemas(serverConnectionString, cancellationToken);
        if (schemas.Count == 0)
        {
            return Result.Failure<IReadOnlyList<ArchivedFile>>("The snapshot has no user schemas.");
        }

        var stagingDirectory = Path.Combine(_options.LocalStagingDirectory, "rds-snapshots");
        List<ArchivedFile> files = [];
        foreach (var schema in schemas)
        {
            var schemaConnectionString = new MySqlConnectionStringBuilder(serverConnectionString) { Database = schema }.ConnectionString;
            var localPath = Path.Combine(stagingDirectory, SnapshotArchiveNames.BuildFileName(snapshot.Identifier, schema));

            var skippedViews = await GetSkippedViews(schemaConnectionString, schema, cancellationToken);
            List<string> arguments = [.. DumpArguments, .. skippedViews.Select(view => $"--ignore-table={schema}.{view}")];

            _logger.Information("Dumping {Schema} from {Snapshot}, skipping views: {SkippedViews}", schema, snapshot.Identifier,
                skippedViews);
            var dumpResult = await _mysqldumpRunner.DumpToGzipFile(schemaConnectionString, localPath, arguments, cancellationToken);
            if (dumpResult.IsFailure)
            {
                return Result.Failure<IReadOnlyList<ArchivedFile>>(dumpResult.Error);
            }

            if (!await MysqldumpRunner.GzipDumpIsComplete(localPath, cancellationToken))
            {
                return Result.Failure<IReadOnlyList<ArchivedFile>>($"The dump of {schema} lacks mysqldump's completion line; it is kept at {localPath}.");
            }

            var key = SnapshotArchiveNames.BuildKey(snapshot.Identifier, schema);
            var bytes = new FileInfo(localPath).Length;
            await _archive.Upload(localPath, key, cancellationToken);
            File.Delete(localPath);
            files.Add(new ArchivedFile(key, bytes));
        }

        await UploadManifest(snapshot, files, stagingDirectory, cancellationToken);
        return Result.Success<IReadOnlyList<ArchivedFile>>(files);
    }

    private async Task UploadManifest(ManualSnapshot snapshot, IReadOnlyList<ArchivedFile> files, string stagingDirectory,
        CancellationToken cancellationToken)
    {
        var manifest = new SnapshotArchiveManifest(
            snapshot.Identifier,
            snapshot.SourceInstanceIdentifier,
            snapshot.Engine,
            snapshot.EngineVersion,
            snapshot.CreateTime.ToString(),
            _clock.GetCurrentInstant().ToString(),
            files);

        var localPath = Path.Combine(stagingDirectory, $"{snapshot.Identifier}-manifest.json");
        await File.WriteAllTextAsync(localPath, JsonSerializer.Serialize(manifest, ManifestJsonOptions), cancellationToken);
        await _archive.Upload(localPath, SnapshotArchiveNames.BuildManifestKey(snapshot.Identifier), cancellationToken);
        File.Delete(localPath);
    }

    //The instance can refuse logins for a short while after its master password is reset.
    private static async Task<IReadOnlyList<string>> GetUserSchemas(string serverConnectionString, CancellationToken cancellationToken)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await using var connection = new MySqlConnection(serverConnectionString);
                await connection.OpenAsync(cancellationToken);
                await using var command = new MySqlCommand("SHOW DATABASES;", connection);
                await using var reader = await command.ExecuteReaderAsync(cancellationToken);

                List<string> schemas = [];
                while (await reader.ReadAsync(cancellationToken))
                {
                    var schema = reader.GetString(0);
                    if (!SnapshotArchiveNames.IsSystemSchema(schema))
                    {
                        schemas.Add(schema);
                    }
                }

                return schemas;
            }
            catch (MySqlException ex) when (attempt < ConnectAttempts)
            {
                _logger.Warning("Connecting to {Server} failed (attempt {Attempt} of {Attempts}): {Error}",
                    new MySqlConnectionStringBuilder(serverConnectionString).Server, attempt, ConnectAttempts, ex.Message);
                await Task.Delay(ConnectRetryInterval, cancellationToken);
            }
        }
    }

    private static async Task<IReadOnlyList<string>> GetSkippedViews(string schemaConnectionString, string schema,
        CancellationToken cancellationToken)
    {
        await using var connection = new MySqlConnection(schemaConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "SELECT TABLE_NAME FROM information_schema.TABLES WHERE TABLE_SCHEMA = @schema AND TABLE_TYPE = 'VIEW';", connection);
        command.Parameters.AddWithValue("@schema", schema);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        List<string> views = [];
        while (await reader.ReadAsync(cancellationToken))
        {
            var view = reader.GetString(0);
            if (view.StartsWith(SkippedViewPrefix, StringComparison.OrdinalIgnoreCase) || SkippedViews.Contains(view))
            {
                views.Add(view);
            }
        }

        return views;
    }

    private static string DescribeFiles(IReadOnlyList<ArchivedFile> files) =>
        string.Join(", ", files.Select(x => $"{x.Key} ({x.Bytes / (1024.0 * 1024.0):F1} MB)"));

    private static SnapshotArchiveResult BuildResult(ManualSnapshot snapshot, SnapshotArchiveOutcome outcome, string detail,
        IReadOnlyList<ArchivedFile>? files = null) =>
        new(snapshot.Identifier, outcome, detail, files ?? []);
}
