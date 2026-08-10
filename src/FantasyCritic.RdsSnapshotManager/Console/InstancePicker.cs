using FantasyCritic.RdsSnapshotManager.Configuration;

namespace FantasyCritic.RdsSnapshotManager.Console;

public static class InstancePicker
{
    public static string? PickInstanceKey(
        IReadOnlyDictionary<string, RdsInstanceOptions> instances,
        Func<RdsInstanceOptions, bool>? filter = null)
    {
        var candidates = instances
            .Where(kv => filter is null || filter(kv.Value))
            .OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (candidates.Count == 0)
        {
            System.Console.WriteLine("No matching RDS instances are configured.");
            return null;
        }

        for (var index = 0; index < candidates.Count; index++)
        {
            var candidate = candidates[index];
            System.Console.WriteLine($"{index}: {candidate.Key} ({candidate.Value.InstanceName})");
        }

        System.Console.Write("Select instance index: ");
        var input = System.Console.ReadLine();
        if (!int.TryParse(input, out var selected) || selected < 0 || selected >= candidates.Count)
        {
            System.Console.WriteLine("Invalid selection.");
            return null;
        }

        return candidates[selected].Key;
    }
}
