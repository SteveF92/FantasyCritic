using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Transfer;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Utilities;

namespace FantasyCritic.AWS;

public sealed class S3DatabaseArchiveLocation : IDatabaseArchiveLocation
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucket;
    private readonly string _prefix;

    public S3DatabaseArchiveLocation(IAmazonS3 s3Client, string bucket, string prefix)
    {
        _s3Client = s3Client;
        _bucket = bucket;
        _prefix = prefix;
    }

    public string Name => "S3";

    public async Task Upload(string localFilePath, string key, CancellationToken cancellationToken)
    {
        //A lifecycle rule can't move an object to infrequent access until it is 30 days old.
        var request = new TransferUtilityUploadRequest
        {
            FilePath = localFilePath,
            BucketName = _bucket,
            Key = BackupRemoteKeyBuilder.WithPrefix(_prefix, key),
            StorageClass = S3StorageClass.StandardInfrequentAccess,
        };

        var transferUtility = new TransferUtility(_s3Client);
        await transferUtility.UploadAsync(request, cancellationToken);
    }
}
