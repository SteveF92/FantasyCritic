namespace FantasyCritic.Lib.Utilities;

public static class SnapshotArchiveNames
{
    private const string TemporaryInstancePrefix = "archive-";
    private const int MaxInstanceIdentifierLength = 63;

    private static readonly IReadOnlySet<string> SystemSchemas = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "information_schema",
        "mysql",
        "performance_schema",
        "sys",
    };

    public static string BuildTemporaryInstanceIdentifier(string snapshotIdentifier)
    {
        var identifier = TemporaryInstancePrefix + snapshotIdentifier;
        if (identifier.Length > MaxInstanceIdentifierLength)
        {
            identifier = identifier[..MaxInstanceIdentifierLength];
        }

        return identifier.TrimEnd('-');
    }

    public static string BuildFileName(string snapshotIdentifier, string schema) => $"{snapshotIdentifier}-{schema}.sql.gz";

    public static string BuildKey(string snapshotIdentifier, string schema) =>
        $"{BuildFolder(snapshotIdentifier)}/{BuildFileName(snapshotIdentifier, schema)}";

    //Uploaded after every dump, so its presence means the snapshot is fully archived.
    public static string BuildManifestKey(string snapshotIdentifier) => $"{BuildFolder(snapshotIdentifier)}/manifest.json";

    private static string BuildFolder(string snapshotIdentifier) => $"rds-snapshots/{snapshotIdentifier}";

    public static bool IsSystemSchema(string schema) => SystemSchemas.Contains(schema);
}
