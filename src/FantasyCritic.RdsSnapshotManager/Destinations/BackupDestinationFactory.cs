using Amazon.S3;
using FantasyCritic.AWS;
using FantasyCritic.GCP;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.RdsSnapshotManager.Configuration;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;

namespace FantasyCritic.RdsSnapshotManager.Destinations;

public static class BackupDestinationFactory
{
    public static IReadOnlyList<IDatabaseArchiveLocation> CreateAll(RdsSnapshotManagerOptions options)
    {
        List<IDatabaseArchiveLocation> destinations = [];

        if (options.Destinations.LocalDirectory.Enabled)
        {
            destinations.Add(new LocalDirectoryDatabaseArchiveLocation(options.Destinations.LocalDirectory.Path));
        }

        if (options.Destinations.S3.Enabled)
        {
            destinations.Add(new S3DatabaseArchiveLocation(new AmazonS3Client(), options.Destinations.S3.Bucket, options.Destinations.S3.Prefix));
        }

        if (options.Destinations.GoogleCloud.Enabled)
        {
            StorageClient storageClient;
            if (!string.IsNullOrWhiteSpace(options.Destinations.GoogleCloud.CredentialsPath))
            {
                GoogleCredential credential = GoogleCredential.FromFile(options.Destinations.GoogleCloud.CredentialsPath);
                storageClient = StorageClient.Create(credential);
            }
            else
            {
                storageClient = StorageClient.Create();
            }

            destinations.Add(new GoogleCloudStorageDatabaseArchiveLocation(storageClient, options.Destinations.GoogleCloud.Bucket,
                options.Destinations.GoogleCloud.Prefix));
        }

        return destinations;
    }
}
