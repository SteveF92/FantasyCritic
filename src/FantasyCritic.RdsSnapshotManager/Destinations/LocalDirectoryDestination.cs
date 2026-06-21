namespace FantasyCritic.RdsSnapshotManager.Destinations;

public sealed class LocalDirectoryDestination : IBackupDestination
{
    private readonly string _directoryPath;

    public LocalDirectoryDestination(string directoryPath)
    {
        _directoryPath = directoryPath;
    }

    public string Name => "LocalDirectory";

    public async Task UploadAsync(string localFilePath, string remoteKey, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_directoryPath);
        var fileName = Path.GetFileName(remoteKey);
        var destinationPath = Path.Combine(_directoryPath, fileName);
        await using var source = File.OpenRead(localFilePath);
        await using var destination = File.Create(destinationPath);
        await source.CopyToAsync(destination, cancellationToken);
    }
}
