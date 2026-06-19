using System.Globalization;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Utilities;
using FantasyCritic.RdsSnapshotManager.Configuration;
using FantasyCritic.RdsSnapshotManager.Destinations;
using FantasyCritic.RdsSnapshotManager.Infrastructure;
using NodaTime;

namespace FantasyCritic.RdsSnapshotManager.Services;

public sealed class DumpAndPublishService
{
    private readonly RdsSnapshotManagerOptions _options;
    private readonly MysqldumpRunner _mysqldumpRunner;
    private readonly IReadOnlyList<BackupDestinationRegistration> _destinations;
    private readonly IClock _clock;

    public DumpAndPublishService(
        RdsSnapshotManagerOptions options,
        MysqldumpRunner mysqldumpRunner,
        IReadOnlyList<BackupDestinationRegistration> destinations,
        IClock clock)
    {
        _options = options;
        _mysqldumpRunner = mysqldumpRunner;
        _destinations = destinations;
        _clock = clock;
    }

    public async Task<Result<string>> DumpAndPublish(string instanceName, CancellationToken cancellationToken)
    {
        var timestamp = _clock.GetCurrentInstant();
        var zonedTimestamp = timestamp.InZone(TimeExtensions.EasternTimeZone);
        var fileName = $"{instanceName}-{zonedTimestamp.LocalDateTime.ToString("yyyy-MM-dd-HHmmss", CultureInfo.InvariantCulture)}.sql.gz";
        var stagingPath = Path.Combine(_options.LocalStagingDirectory, fileName);

        var dumpResult = await _mysqldumpRunner.DumpToGzipFile(_options.DumpConnectionString, stagingPath, cancellationToken);
        if (dumpResult.IsFailure)
        {
            return Result.Failure<string>(dumpResult.Error);
        }

        foreach (var destination in _destinations)
        {
            var remoteKey = BackupRemoteKeyBuilder.Build(destination.Prefix, instanceName, timestamp, fileName);
            await destination.Destination.UploadAsync(stagingPath, remoteKey, cancellationToken);
        }

        return Result.Success(stagingPath);
    }
}
