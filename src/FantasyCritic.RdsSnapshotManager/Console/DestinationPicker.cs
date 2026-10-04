using FantasyCritic.Lib.Interfaces;

namespace FantasyCritic.RdsSnapshotManager.Console;

public static class DestinationPicker
{
    public static string? PickDestinationName(IReadOnlyList<IDatabaseArchiveLocation> destinations)
    {
        if (destinations.Count == 0)
        {
            System.Console.WriteLine("No destinations are enabled in configuration.");
            return null;
        }

        for (var index = 0; index < destinations.Count; index++)
        {
            System.Console.WriteLine($"{index}: {destinations[index].Name}");
        }

        System.Console.Write("Select destination index: ");
        if (!int.TryParse(System.Console.ReadLine(), out var selected) || selected < 0 || selected >= destinations.Count)
        {
            System.Console.WriteLine("Invalid selection.");
            return null;
        }

        return destinations[selected].Name;
    }
}
