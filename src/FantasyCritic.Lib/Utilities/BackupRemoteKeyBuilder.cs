using System.Globalization;
using FantasyCritic.Lib.Extensions;
using NodaTime;

namespace FantasyCritic.Lib.Utilities;

public static class BackupRemoteKeyBuilder
{
    public static string Build(string prefix, string instanceName, Instant timestamp, string fileName)
    {
        var normalizedPrefix = string.IsNullOrEmpty(prefix) ? string.Empty : prefix.EndsWith('/') ? prefix : prefix + "/";
        var date = timestamp.InZone(TimeExtensions.EasternTimeZone).Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        return $"{normalizedPrefix}{instanceName}/{date}/{fileName}";
    }
}
