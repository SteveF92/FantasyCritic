using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.RdsSnapshotManager.Infrastructure;

namespace FantasyCritic.RdsSnapshotManager.Services;

public sealed class ManualUploadService
{
    public ManualUploadService(IReadOnlyList<IDatabaseArchiveLocation> destinations)
    {
        Destinations = destinations;
    }

    public IReadOnlyList<IDatabaseArchiveLocation> Destinations { get; }

    public async Task<Result> Upload(string localFilePath, string destinationName, CancellationToken cancellationToken)
    {
        var destination = Destinations.FirstOrDefault(d =>
            string.Equals(d.Name, destinationName, StringComparison.OrdinalIgnoreCase));
        if (destination is null)
        {
            return Result.Failure($"No enabled destination named '{destinationName}'.");
        }

        var fileName = Path.GetFileName(localFilePath);
        var keyResult = DumpFileNameParser.TryBuildKey(fileName);
        if (keyResult.IsFailure)
        {
            return Result.Failure(keyResult.Error);
        }

        await destination.Upload(localFilePath, keyResult.Value, cancellationToken);
        return Result.Success();
    }
}
