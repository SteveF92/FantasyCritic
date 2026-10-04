namespace FantasyCritic.Lib.Utilities;

public static class SnapshotArchiveNames
{
    private const string TemporaryInstancePrefix = "archive-";
    private const int MaxInstanceIdentifierLength = 63;

    public const string FolderPrefix = "rds-snapshots/";
    //RDS snapshots also hold empty innodb and tmp schemas.
    public const string ApplicationSchema = "fantasycritic";

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

    private static string BuildFolder(string snapshotIdentifier) => $"{FolderPrefix}{snapshotIdentifier}";

    public static string? GetSnapshotIdentifierIfApplicationSchemaKey(string key)
    {
        if (!key.StartsWith(FolderPrefix, StringComparison.Ordinal))
        {
            return null;
        }

        var parts = key[FolderPrefix.Length..].Split('/');
        if (parts.Length != 2)
        {
            return null;
        }

        var snapshotIdentifier = parts[0];
        return parts[1] == BuildFileName(snapshotIdentifier, ApplicationSchema) ? snapshotIdentifier : null;
    }

    public static bool IsSystemSchema(string schema) => SystemSchemas.Contains(schema);
}
