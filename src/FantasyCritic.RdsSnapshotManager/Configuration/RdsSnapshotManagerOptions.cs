namespace FantasyCritic.RdsSnapshotManager.Configuration;

public sealed class RdsSnapshotManagerOptions
{
    public string ProductionRdsInstance { get; set; } = null!;
    public string BetaRdsInstance { get; set; } = null!;
    public string BetaConnectionString { get; set; } = null!;
    public string DumpConnectionString { get; set; } = null!;
    public string LocalStagingDirectory { get; set; } = null!;
    public LocalDockerOptions LocalDocker { get; set; } = new();
    public DestinationOptions Destinations { get; set; } = new();
}

public sealed class LocalDockerOptions
{
    public string ConnectionString { get; set; } = null!;
    public string ContainerName { get; set; } = "fantasycritic-mysql";
}

public sealed class DestinationOptions
{
    public LocalDirectoryDestinationOptions LocalDirectory { get; set; } = new();
    public S3DestinationOptions S3 { get; set; } = new();
    public GoogleCloudDestinationOptions GoogleCloud { get; set; } = new();
}

public sealed class LocalDirectoryDestinationOptions
{
    public bool Enabled { get; set; }
    public string Path { get; set; } = null!;
}

public sealed class S3DestinationOptions
{
    public bool Enabled { get; set; }
    public string Bucket { get; set; } = null!;
    public string Prefix { get; set; } = "db-dumps/";
}

public sealed class GoogleCloudDestinationOptions
{
    public bool Enabled { get; set; }
    public string Bucket { get; set; } = null!;
    public string Prefix { get; set; } = "db-dumps/";
    public string? CredentialsPath { get; set; }
}
