using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Utilities;
using FantasyCritic.MySQL;
using FantasyCritic.RdsSnapshotManager.Configuration;
using NodaTime;

namespace FantasyCritic.RdsSnapshotManager.Services;

public sealed class DumpAndPublishService
{
    private readonly RdsSnapshotManagerOptions _options;
    private readonly MysqldumpRunner _mysqldumpRunner;
    private readonly IReadOnlyList<IDatabaseArchiveLocation> _destinations;
    private readonly IClock _clock;

    public DumpAndPublishService(
        RdsSnapshotManagerOptions options,
        MysqldumpRunner mysqldumpRunner,
        IReadOnlyList<IDatabaseArchiveLocation> destinations,
        IClock clock)
    {
        _options = options;
        _mysqldumpRunner = mysqldumpRunner;
        _destinations = destinations;
        _clock = clock;
    }

    public async Task<Result<string>> DumpAndPublish(string instanceKey, CancellationToken cancellationToken)
    {
        var instanceResult = RdsInstanceLookup.TryResolve(_options.RdsInstances, instanceKey);
        if (instanceResult.IsFailure)
        {
            return Result.Failure<string>(instanceResult.Error);
        }

        var instance = instanceResult.Value;
        var timestamp = _clock.GetCurrentInstant();
        var fileName = BackupRemoteKeyBuilder.BuildFileName(instance.InstanceName, timestamp);
        var stagingPath = Path.Combine(_options.LocalStagingDirectory, fileName);

        var dumpResult = await _mysqldumpRunner.DumpToGzipFile(instance.ConnectionString, stagingPath, cancellationToken);
        if (dumpResult.IsFailure)
        {
            return Result.Failure<string>(dumpResult.Error);
        }

        var key = BackupRemoteKeyBuilder.Build(instance.InstanceName, timestamp, fileName);
        foreach (var destination in _destinations)
        {
            await destination.Upload(stagingPath, key, cancellationToken);
        }

        return Result.Success(stagingPath);
    }
}
