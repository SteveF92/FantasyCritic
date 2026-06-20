using System.Text.RegularExpressions;
using FantasyCritic.RdsSnapshotManager.Configuration;
using MySqlConnector;

namespace FantasyCritic.RdsSnapshotManager.Infrastructure;

public static class LocalSnapshotConnectionString
{
    public static string BuildSnapshotConnectionString(string localDockerConnectionString)
    {
        var builder = new MySqlConnectionStringBuilder(localDockerConnectionString)
        {
            Database = LocalSnapshotDatabaseNames.SnapshotDatabase
        };

        return Regex.Replace(
            localDockerConnectionString,
            "(?i)Database=[^;]*",
            $"Database={builder.Database}");
    }
}
