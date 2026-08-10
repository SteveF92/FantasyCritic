using FantasyCritic.RdsSnapshotManager.Configuration;

namespace FantasyCritic.RdsSnapshotManager.Console;

public static class DumpFilePicker
{
    public static string? PickDumpFile(RdsSnapshotManagerOptions options)
    {
        if (!Directory.Exists(options.LocalStagingDirectory))
        {
            System.Console.WriteLine($"Staging directory not found: {options.LocalStagingDirectory}");
            return null;
        }

        var dumpFiles = Directory.GetFiles(options.LocalStagingDirectory, "*.sql.gz")
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .ToList();

        if (dumpFiles.Count == 0)
        {
            System.Console.WriteLine("No .sql.gz files found in staging directory.");
            return null;
        }

        for (var index = 0; index < dumpFiles.Count; index++)
        {
            System.Console.WriteLine($"{index}: {Path.GetFileName(dumpFiles[index])}");
        }

        System.Console.Write("Select dump file index: ");
        if (!int.TryParse(System.Console.ReadLine(), out var selected) || selected < 0 || selected >= dumpFiles.Count)
        {
            System.Console.WriteLine("Invalid selection.");
            return null;
        }

        return dumpFiles[selected];
    }
}
