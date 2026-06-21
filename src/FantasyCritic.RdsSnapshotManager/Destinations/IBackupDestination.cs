namespace FantasyCritic.RdsSnapshotManager.Destinations;

public interface IBackupDestination
{
    string Name { get; }
    Task UploadAsync(string localFilePath, string remoteKey, CancellationToken cancellationToken);
}
