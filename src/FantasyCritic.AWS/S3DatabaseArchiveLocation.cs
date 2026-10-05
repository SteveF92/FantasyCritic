using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
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

    public async Task<bool> Exists(string key, CancellationToken cancellationToken)
    {
        try
        {
            await _s3Client.GetObjectMetadataAsync(_bucket, BackupRemoteKeyBuilder.WithPrefix(_prefix, key), cancellationToken);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public async Task<IReadOnlyList<ArchivedObject>> List(string keyPrefix, CancellationToken cancellationToken)
    {
        var rootLength = BackupRemoteKeyBuilder.WithPrefix(_prefix, string.Empty).Length;
        var request = new ListObjectsV2Request
        {
            BucketName = _bucket,
            Prefix = BackupRemoteKeyBuilder.WithPrefix(_prefix, keyPrefix),
            OptionalObjectAttributes = [OptionalObjectAttributes.RestoreStatus],
        };

        List<ArchivedObject> objects = [];
        ListObjectsV2Response response;
        do
        {
            response = await _s3Client.ListObjectsV2Async(request, cancellationToken);
            foreach (var s3Object in response.S3Objects ?? [])
            {
                objects.Add(new ArchivedObject(s3Object.Key[rootLength..], s3Object.Size ?? 0, GetAvailability(s3Object)));
            }

            request.ContinuationToken = response.NextContinuationToken;
        } while (response.IsTruncated == true);

        return objects;
    }

    public async Task<string> ReadText(string key, CancellationToken cancellationToken)
    {
        using var response = await _s3Client.GetObjectAsync(_bucket, BackupRemoteKeyBuilder.WithPrefix(_prefix, key), cancellationToken);
        using var reader = new StreamReader(response.ResponseStream);
        return await reader.ReadToEndAsync(cancellationToken);
    }

    public async Task Download(string key, string localFilePath, CancellationToken cancellationToken)
    {
        var request = new TransferUtilityDownloadRequest
        {
            BucketName = _bucket,
            Key = BackupRemoteKeyBuilder.WithPrefix(_prefix, key),
            FilePath = localFilePath,
        };

        var transferUtility = new TransferUtility(_s3Client);
        await transferUtility.DownloadAsync(request, cancellationToken);
    }

    public async Task RequestGlacierRestore(string key, int days, CancellationToken cancellationToken)
    {
        var request = new RestoreObjectRequest
        {
            BucketName = _bucket,
            Key = BackupRemoteKeyBuilder.WithPrefix(_prefix, key),
            Days = days,
            Tier = GlacierJobTier.Standard,
        };

        await _s3Client.RestoreObjectAsync(request, cancellationToken);
    }

    private static ArchivedObjectAvailability GetAvailability(S3Object s3Object)
    {
        if (s3Object.StorageClass != S3StorageClass.Glacier && s3Object.StorageClass != S3StorageClass.DeepArchive)
        {
            return ArchivedObjectAvailability.Available;
        }

        if (s3Object.RestoreStatus?.IsRestoreInProgress == true)
        {
            return ArchivedObjectAvailability.GlacierRestoreInProgress;
        }

        return s3Object.RestoreStatus?.RestoreExpiryDate is not null
            ? ArchivedObjectAvailability.Available
            : ArchivedObjectAvailability.InGlacier;
    }
}
