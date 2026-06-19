using Amazon.S3;
using Amazon.S3.Transfer;

namespace FantasyCritic.RdsSnapshotManager.Destinations;

public sealed class S3BackupDestination : IBackupDestination
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucket;

    public S3BackupDestination(IAmazonS3 s3Client, string bucket)
    {
        _s3Client = s3Client;
        _bucket = bucket;
    }

    public string Name => "S3";

    public async Task UploadAsync(string localFilePath, string remoteKey, CancellationToken cancellationToken)
    {
        var transferUtility = new TransferUtility(_s3Client);
        await transferUtility.UploadAsync(localFilePath, _bucket, remoteKey, cancellationToken);
    }
}
