using System.Diagnostics;
using System.IO;
using NodaTime.Text;

namespace FantasyCritic.Web.Utilities;

/// <summary>
/// Reads the RELEASE file written by the deploy pipeline (see .github/workflows/deploy.yml and
/// deploy/deploy.sh). This is deliberately best-effort: a missing or malformed file means the admin
/// console shows "local build" rather than the site failing to start.
/// </summary>
public static class BuildInfoReader
{
    private const string ReleaseFileName = "RELEASE";

    public static BuildInfo Read(string contentRootPath)
    {
        var processStartedAt = Instant.FromDateTimeUtc(Process.GetCurrentProcess().StartTime.ToUniversalTime());

        var values = ReadReleaseFile(contentRootPath);
        if (values is null)
        {
            return new BuildInfo(null, null, null, null, null, null, null, null, null, processStartedAt);
        }

        return new BuildInfo(
            ReleaseID: GetValue(values, "release_id"),
            DeployedEnvironment: GetValue(values, "environment"),
            CommitHash: GetValue(values, "git_sha"),
            CommitUrl: GetValue(values, "commit_url"),
            GitRef: GetValue(values, "git_ref"),
            CommitDate: GetInstant(values, "commit_date"),
            BuiltAt: GetInstant(values, "built_at"),
            DeployedAt: GetInstant(values, "deployed_at"),
            BuildRunUrl: GetValue(values, "run_url"),
            ProcessStartedAt: processStartedAt);
    }

    private static IReadOnlyDictionary<string, string>? ReadReleaseFile(string contentRootPath)
    {
        // The bundle puts RELEASE alongside the web/ and dbup/ directories, so it sits one level above
        // the content root once the app is running. Checking the content root itself as well keeps this
        // working if the file ever travels inside the published output.
        string[] candidatePaths =
        [
            Path.Combine(contentRootPath, ReleaseFileName),
            Path.Combine(contentRootPath, "..", ReleaseFileName)
        ];

        foreach (var path in candidatePaths)
        {
            try
            {
                if (!File.Exists(path))
                {
                    continue;
                }

                return ParseReleaseFile(File.ReadAllLines(path));
            }
            catch (IOException)
            {
                return null;
            }
            catch (UnauthorizedAccessException)
            {
                return null;
            }
        }

        return null;
    }

    private static IReadOnlyDictionary<string, string> ParseReleaseFile(IEnumerable<string> lines)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed.StartsWith('#'))
            {
                continue;
            }

            var separatorIndex = trimmed.IndexOf('=');
            if (separatorIndex <= 0)
            {
                continue;
            }

            var key = trimmed[..separatorIndex].Trim();
            var value = trimmed[(separatorIndex + 1)..].Trim();
            if (value.Length > 0)
            {
                //Later lines win: deploy.sh appends deployed_at to the file the build wrote.
                values[key] = value;
            }
        }

        return values;
    }

    private static string? GetValue(IReadOnlyDictionary<string, string> values, string key)
        => values.TryGetValue(key, out var value) ? value : null;

    private static Instant? GetInstant(IReadOnlyDictionary<string, string> values, string key)
    {
        var value = GetValue(values, key);
        if (value is null)
        {
            return null;
        }

        //Handles both the trailing-Z stamps the workflow writes and the numeric offset that git emits.
        var parseResult = OffsetDateTimePattern.ExtendedIso.Parse(value);
        return parseResult.Success ? parseResult.Value.ToInstant() : null;
    }
}
