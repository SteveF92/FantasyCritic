using System.Globalization;
using FantasyCritic.Lib.Extensions;

namespace FantasyCritic.Lib.Utilities;

public static class BackupRemoteKeyBuilder
{
    public static string Build(string prefix, string instanceName, Instant timestamp, string fileName) =>
        Build(prefix, instanceName, timestamp.InZone(TimeExtensions.EasternTimeZone).Date, fileName);

    public static string Build(string prefix, string instanceName, LocalDate date, string fileName)
    {
        var normalizedPrefix = string.IsNullOrEmpty(prefix) ? string.Empty : prefix.EndsWith('/') ? prefix : prefix + "/";
        var dateString = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        return $"{normalizedPrefix}{instanceName}/{dateString}/{fileName}";
    }
}
