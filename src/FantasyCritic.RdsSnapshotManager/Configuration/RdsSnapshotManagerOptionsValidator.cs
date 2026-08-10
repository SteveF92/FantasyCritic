using CSharpFunctionalExtensions;

namespace FantasyCritic.RdsSnapshotManager.Configuration;

public static class RdsSnapshotManagerOptionsValidator
{
    public static Result Validate(RdsSnapshotManagerOptions options)
    {
        if (options.RdsInstances.Count == 0)
        {
            return Result.Failure("No RDS instances are configured. At least one entry is required under 'rdsInstances'.");
        }

        foreach (var (key, instance) in options.RdsInstances)
        {
            if (string.IsNullOrWhiteSpace(instance.InstanceName))
            {
                return Result.Failure($"RDS instance '{key}' is missing an instanceName.");
            }

            if (string.IsNullOrWhiteSpace(instance.ConnectionString))
            {
                return Result.Failure($"RDS instance '{key}' is missing a connectionString.");
            }
        }

        var defaultSnapshotSources = options.RdsInstances
            .Where(kv => kv.Value.DefaultSnapshotSource)
            .Select(kv => kv.Key)
            .ToList();

        if (defaultSnapshotSources.Count != 1)
        {
            var suffix = defaultSnapshotSources.Count == 0 ? "." : $": {string.Join(", ", defaultSnapshotSources)}.";
            return Result.Failure($"Expected exactly one RDS instance with defaultSnapshotSource=true, found {defaultSnapshotSources.Count}{suffix}");
        }

        var writeEnabledInstances = options.RdsInstances
            .Where(kv => kv.Value.EnableWriteOperations)
            .Select(kv => kv.Key)
            .ToList();

        if (writeEnabledInstances.Count == 0)
        {
            return Result.Failure("No RDS instance has enableWriteOperations=true. At least one write-enabled instance is required as a restore destination.");
        }

        return Result.Success();
    }
}
