using Amazon.S3;
using FantasyCritic.RdsSnapshotManager.Configuration;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;

namespace FantasyCritic.RdsSnapshotManager.Destinations;

public static class BackupDestinationFactory
{
    public static IReadOnlyList<BackupDestinationRegistration> CreateRegistrations(RdsSnapshotManagerOptions options)
    {
        List<BackupDestinationRegistration> destinations = new();

        if (options.Destinations.LocalDirectory.Enabled)
        {
            destinations.Add(new BackupDestinationRegistration(
                new LocalDirectoryDestination(options.Destinations.LocalDirectory.Path),
                "db-dumps/"));
        }

        if (options.Destinations.S3.Enabled)
        {
            destinations.Add(new BackupDestinationRegistration(
                new S3BackupDestination(new AmazonS3Client(), options.Destinations.S3.Bucket),
                options.Destinations.S3.Prefix));
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

            destinations.Add(new BackupDestinationRegistration(
                new GoogleCloudStorageDestination(storageClient, options.Destinations.GoogleCloud.Bucket),
                options.Destinations.GoogleCloud.Prefix));
        }

        return destinations;
    }

    public static IReadOnlyList<IBackupDestination> CreateAll(RdsSnapshotManagerOptions options) =>
        CreateRegistrations(options).Select(x => x.Destination).ToList();
}
