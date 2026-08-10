using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Utilities;
using NodaTime;

namespace FantasyCritic.RdsSnapshotManager.Infrastructure;

public static partial class DumpFileNameParser
{
    [GeneratedRegex(@"^(?<instanceName>.+)-(?<year>\d{4})-(?<month>\d{2})-(?<day>\d{2})-\d{6}\.sql\.gz$")]
    private static partial Regex DumpFileNamePattern();

    public static Result<string> TryBuildRemoteKey(string prefix, string fileName)
    {
        var match = DumpFileNamePattern().Match(fileName);
        if (!match.Success)
        {
            return Result.Failure<string>(
                $"File name '{fileName}' does not match the expected '<instance>-yyyy-MM-dd-HHmmss.sql.gz' dump naming pattern.");
        }

        var date = new LocalDate(
            int.Parse(match.Groups["year"].Value),
            int.Parse(match.Groups["month"].Value),
            int.Parse(match.Groups["day"].Value));
        var instanceName = match.Groups["instanceName"].Value;

        return Result.Success(BackupRemoteKeyBuilder.Build(prefix, instanceName, date, fileName));
    }
}
