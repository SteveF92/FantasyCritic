using FantasyCritic.Lib.Interfaces;

namespace FantasyCritic.RdsSnapshotManager.Destinations;

//A flat folder: only the key's file name is used.
public sealed class LocalDirectoryDatabaseArchiveLocation : IDatabaseArchiveLocation
{
    private readonly string _directoryPath;

    public LocalDirectoryDatabaseArchiveLocation(string directoryPath)
    {
        _directoryPath = directoryPath;
    }

    public string Name => "LocalDirectory";

    public async Task Upload(string localFilePath, string key, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_directoryPath);
        var fileName = Path.GetFileName(key);
        var destinationPath = Path.Combine(_directoryPath, fileName);
        await using var source = File.OpenRead(localFilePath);
        await using var destination = File.Create(destinationPath);
        await source.CopyToAsync(destination, cancellationToken);
    }
}
