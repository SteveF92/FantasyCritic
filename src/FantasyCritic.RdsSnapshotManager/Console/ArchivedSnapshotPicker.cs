using FantasyCritic.AWS;
using FantasyCritic.RdsSnapshotManager.Services;

namespace FantasyCritic.RdsSnapshotManager.Console;

public static class ArchivedSnapshotPicker
{
    public static ArchivedSnapshotDump? PickDump(IReadOnlyList<ArchivedSnapshotDump> dumps)
    {
        if (dumps.Count == 0)
        {
            System.Console.WriteLine("No archived snapshots found in S3.");
            return null;
        }

        var sortedDumps = dumps.OrderBy(x => x.SnapshotIdentifier, StringComparer.Ordinal).ToList();
        for (var index = 0; index < sortedDumps.Count; index++)
        {
            var dump = sortedDumps[index];
            var megabytes = dump.Object.Bytes / (1024.0 * 1024.0);
            System.Console.WriteLine($"{index}: {dump.SnapshotIdentifier} | {megabytes:F1} MB{DescribeAvailability(dump.Object.Availability)}");
        }

        System.Console.Write("Select archived snapshot index: ");
        if (!int.TryParse(System.Console.ReadLine(), out var selected) || selected < 0 || selected >= sortedDumps.Count)
        {
            System.Console.WriteLine("Invalid selection.");
            return null;
        }

        return sortedDumps[selected];
    }

    private static string DescribeAvailability(ArchivedObjectAvailability availability) => availability switch
    {
        ArchivedObjectAvailability.Available => string.Empty,
        ArchivedObjectAvailability.InGlacier => " | in Glacier",
        ArchivedObjectAvailability.GlacierRestoreInProgress => " | Glacier restore in progress",
        _ => throw new ArgumentOutOfRangeException(nameof(availability), availability, null),
    };
}
