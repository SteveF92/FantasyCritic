namespace FantasyCritic.MySQL;

public static class ConnectionStringUtilities
{
    public static string GetLongTimeoutConnectionString(string originalConnectionString, Duration timeoutDuration)
    {
        var builder = new MySqlConnectionStringBuilder(originalConnectionString)
        {
            ConnectionTimeout = (uint)timeoutDuration.TotalSeconds
        };
        return builder.ConnectionString;
    }
}
