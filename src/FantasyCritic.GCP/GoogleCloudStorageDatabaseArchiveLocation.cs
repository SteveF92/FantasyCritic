using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Utilities;
using Google.Cloud.Storage.V1;

namespace FantasyCritic.GCP;

public sealed class GoogleCloudStorageDatabaseArchiveLocation : IDatabaseArchiveLocation
{
    private readonly StorageClient _storageClient;
    private readonly string _bucket;
    private readonly string _prefix;

    public GoogleCloudStorageDatabaseArchiveLocation(StorageClient storageClient, string bucket, string prefix)
    {
        _storageClient = storageClient;
        _bucket = bucket;
        _prefix = prefix;
    }

    public string Name => "GoogleCloud";

    public async Task Upload(string localFilePath, string key, CancellationToken cancellationToken)
    {
        await using var fileStream = File.OpenRead(localFilePath);
        await _storageClient.UploadObjectAsync(_bucket, BackupRemoteKeyBuilder.WithPrefix(_prefix, key), "application/gzip", fileStream,
            cancellationToken: cancellationToken);
    }
}
