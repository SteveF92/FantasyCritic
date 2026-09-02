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

    public static string WithDefaultCommandTimeout(string originalConnectionString, Duration commandTimeout)
    {
        var builder = new MySqlConnectionStringBuilder(originalConnectionString)
        {
            DefaultCommandTimeout = (uint)commandTimeout.TotalSeconds
        };
        return builder.ConnectionString;
    }
}
