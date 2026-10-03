using System.Globalization;
using FantasyCritic.Lib.Extensions;

namespace FantasyCritic.Lib.Utilities;

public static class BackupRemoteKeyBuilder
{
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
