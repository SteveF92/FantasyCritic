using CSharpFunctionalExtensions;

namespace FantasyCritic.RdsSnapshotManager.Configuration;

public static class RdsInstanceLookup
{
    public static Result<RdsInstanceOptions> TryResolve(IReadOnlyDictionary<string, RdsInstanceOptions> instances, string instanceKey)
    {
        if (instances.TryGetValue(instanceKey, out var instance))
        {
            return Result.Success(instance);
        }

        var knownKeys = string.Join(", ", instances.Keys.OrderBy(x => x, StringComparer.OrdinalIgnoreCase));
        return Result.Failure<RdsInstanceOptions>($"Unknown RDS instance '{instanceKey}'. Configured instances: {knownKeys}.");
    }

    public static Result<RdsInstanceOptions> TryResolveWriteEnabled(IReadOnlyDictionary<string, RdsInstanceOptions> instances, string instanceKey)
    {
        var resolved = TryResolve(instances, instanceKey);
        if (resolved.IsFailure)
        {
            return resolved;
        }

        if (!resolved.Value.EnableWriteOperations)
        {
            return Result.Failure<RdsInstanceOptions>($"Refusing to write to '{instanceKey}': write operations are disabled for this instance.");
        }

        return resolved;
    }

    public static RdsInstanceOptions GetDefaultSnapshotSource(IReadOnlyDictionary<string, RdsInstanceOptions> instances)
    {
        return instances.Single(kv => kv.Value.DefaultSnapshotSource).Value;
    }
}
