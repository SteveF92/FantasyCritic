using Google.Cloud.Storage.V1;

namespace FantasyCritic.RdsSnapshotManager.Destinations;

public sealed class GoogleCloudStorageDestination : IBackupDestination
{
    private readonly StorageClient _storageClient;
    private readonly string _bucket;

    public GoogleCloudStorageDestination(StorageClient storageClient, string bucket)
    {
        _storageClient = storageClient;
        _bucket = bucket;
    }

    public string Name => "GoogleCloud";

    public async Task UploadAsync(string localFilePath, string remoteKey, CancellationToken cancellationToken)
    {
        await using var fileStream = File.OpenRead(localFilePath);
        await _storageClient.UploadObjectAsync(_bucket, remoteKey, "application/gzip", fileStream, cancellationToken: cancellationToken);
    }
}
