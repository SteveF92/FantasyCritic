using System.Globalization;
using FantasyCritic.Lib.Extensions;

namespace FantasyCritic.Lib.Utilities;

public static class BackupRemoteKeyBuilder
{
    //The snapshot manager's DumpFileNameParser reads this pattern back.
    public static string BuildFileName(string instanceName, Instant timestamp)
    {
        var easternTime = timestamp.InZone(TimeExtensions.EasternTimeZone).LocalDateTime;
        return $"{instanceName}-{easternTime.ToString("yyyy-MM-dd-HHmmss", CultureInfo.InvariantCulture)}.sql.gz";
    }

    public static string Build(string instanceName, Instant timestamp, string fileName) =>
        Build(instanceName, timestamp.InZone(TimeExtensions.EasternTimeZone).Date, fileName);

    public static string Build(string instanceName, LocalDate date, string fileName)
    {
        var dateString = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        return $"{instanceName}/{dateString}/{fileName}";
    }

    public static string WithPrefix(string prefix, string key)
    {
        var normalizedPrefix = string.IsNullOrEmpty(prefix) ? string.Empty : prefix.EndsWith('/') ? prefix : prefix + "/";
        return normalizedPrefix + key;
    }
}
