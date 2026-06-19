using FantasyCritic.RdsSnapshotManager.Configuration;

namespace FantasyCritic.RdsSnapshotManager.Console;

public static class InstancePicker
{
    public static string? PickInstance(RdsSnapshotManagerOptions options)
    {
        System.Console.WriteLine($"0: {options.ProductionRdsInstance} (production)");
        System.Console.WriteLine($"1: {options.BetaRdsInstance} (beta)");
        System.Console.Write("Select instance index: ");
        var input = System.Console.ReadLine();
        if (!int.TryParse(input, out var selected))
        {
            System.Console.WriteLine("Invalid selection.");
            return null;
        }

        return selected switch
        {
            0 => options.ProductionRdsInstance,
            1 => options.BetaRdsInstance,
            _ => GetInvalidSelection()
        };
    }

    private static string? GetInvalidSelection()
    {
        System.Console.WriteLine("Invalid selection.");
        return null;
    }
}
