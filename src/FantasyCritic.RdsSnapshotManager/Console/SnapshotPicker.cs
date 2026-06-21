using FantasyCritic.Lib.Utilities;

namespace FantasyCritic.RdsSnapshotManager.Console;

public static class SnapshotPicker
{
    public static string? PickSnapshot(IReadOnlyList<DatabaseSnapshotInfo> snapshots)
    {
        if (snapshots.Count == 0)
        {
            System.Console.WriteLine("No snapshots found.");
            return null;
        }

        for (var index = 0; index < snapshots.Count; index++)
        {
            var snap = snapshots[index];
            System.Console.WriteLine($"{index}: {snap.SnapshotName} | {snap.CreationTime} | {snap.Status}");
        }

        System.Console.Write("Select snapshot index: ");
        var input = System.Console.ReadLine();
        if (!int.TryParse(input, out var selected) || selected < 0 || selected >= snapshots.Count)
        {
            System.Console.WriteLine("Invalid selection.");
            return null;
        }

        return snapshots[selected].SnapshotName;
    }
}
