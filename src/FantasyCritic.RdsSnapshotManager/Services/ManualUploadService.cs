using CSharpFunctionalExtensions;
using FantasyCritic.RdsSnapshotManager.Destinations;
using FantasyCritic.RdsSnapshotManager.Infrastructure;

namespace FantasyCritic.RdsSnapshotManager.Services;

public sealed class ManualUploadService
{
    public ManualUploadService(IReadOnlyList<BackupDestinationRegistration> destinations)
    {
        Destinations = destinations;
    }

    public IReadOnlyList<BackupDestinationRegistration> Destinations { get; }

    public async Task<Result> Upload(string localFilePath, string destinationName, CancellationToken cancellationToken)
    {
        var destination = Destinations.FirstOrDefault(d =>
            string.Equals(d.Destination.Name, destinationName, StringComparison.OrdinalIgnoreCase));
        if (destination is null)
        {
            return Result.Failure($"No enabled destination named '{destinationName}'.");
        }

        var fileName = Path.GetFileName(localFilePath);
        var remoteKeyResult = DumpFileNameParser.TryBuildRemoteKey(destination.Prefix, fileName);
        if (remoteKeyResult.IsFailure)
        {
            return Result.Failure(remoteKeyResult.Error);
        }

        await destination.Destination.UploadAsync(localFilePath, remoteKeyResult.Value, cancellationToken);
        return Result.Success();
    }
}
