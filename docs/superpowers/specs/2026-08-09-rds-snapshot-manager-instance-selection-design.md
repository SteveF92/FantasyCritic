# RDS Snapshot Manager: generalized instance selection

## Problem

`FantasyCritic.RdsSnapshotManager` supports four conceptual admin actions against MySQL RDS instances: create a snapshot, dump-and-publish a raw backup, restore a snapshot onto another instance (with PII scrubbing), and pull a dump into local Docker (with PII scrubbing). In practice, picking "which server" barely works:

- `DumpAndPublishService` lets you pick an instance name from a menu (`InstancePicker`), but the actual `mysqldump` always connects using a single fixed `DumpConnectionString` from config, completely ignoring the selection. Picking "beta" vs. "production" never changed what got dumped.
- The restore-and-scrub flow (`BetaSyncService`) is hardcoded to always target the beta RDS instance. There's no way to point it at any other write-target without editing code.
- Configuration for RDS instances was a flat, ad hoc mix of fields (`productionRdsInstance`, `betaRdsInstance`, `betaConnectionString`, `dumpConnectionString`) with no per-instance connection string and no structural way to add a new environment.

## Goals

- A single, named list of RDS instances in configuration, each with its own AWS instance identifier and its own direct MySQL connection string.
- Every action that connects to "a server" resolves its target from that per-instance connection string — no shared/fallback connection string.
- A safety flag (`enableWriteOperations`) on each instance that gates whether it can ever be a destination for destructive operations (restore-overwrite + PII scrub), checked in two places: the instance picker (UX-level filtering) and inside the service itself (defense in depth).
- A safety flag (`defaultSnapshotSource`) identifying which single instance's snapshot history is browsed when restoring a snapshot elsewhere — validated at startup to be set on exactly one instance.
- Startup (fail-fast) validation of the RDS instance configuration so a misconfiguration is caught immediately, not mid-operation.
- Adding a third environment later requires only a config change, not new code.

## Configuration shape

`appsettings.json` (already reorganized by the user):

```json
{
  "rdsInstances": {
    "production": {
      "instanceName": "fantasy-critic-rds",
      "connectionString": "Server=...;Uid=fantasycritic-admin;...",
      "enableWriteOperations": false,
      "defaultSnapshotSource": true
    },
    "beta": {
      "instanceName": "fantasy-critic-beta-rds",
      "connectionString": "Server=...;Uid=fantasycritic-admin;...",
      "enableWriteOperations": true,
      "defaultSnapshotSource": false
    }
  },
  "localStagingDirectory": "C:/FantasyCritic/backups/staging",
  "localDocker": { "connectionString": "...", "containerName": "fantasycritic-mysql" },
  "destinations": { "localDirectory": {}, "s3": {}, "googleCloud": {} }
}
```

`RdsSnapshotManagerOptions`:

```csharp
public sealed class RdsSnapshotManagerOptions
{
    public Dictionary<string, RdsInstanceOptions> RdsInstances { get; set; } = new();
    public string LocalStagingDirectory { get; set; } = null!;
    public LocalDockerOptions LocalDocker { get; set; } = new();
    public DestinationOptions Destinations { get; set; } = new();
}

public sealed class RdsInstanceOptions
{
    public string InstanceName { get; set; } = null!;        // AWS DB instance identifier
    public string ConnectionString { get; set; } = null!;    // direct MySQL connection for this instance
    public bool EnableWriteOperations { get; set; }           // may this instance be a restore/clean target?
    public bool DefaultSnapshotSource { get; set; }            // whose snapshot history do restores browse?
}
```

The old flat fields (`ProductionRdsInstance`, `BetaRdsInstance`, `BetaConnectionString`, `DumpConnectionString`) are removed entirely. No code refers to the dictionary keys ("production", "beta") as magic strings — all behavior is driven by the `EnableWriteOperations` / `DefaultSnapshotSource` flags, so a third named instance works without code changes.

## Startup validation (fail fast)

A new `RdsSnapshotManagerOptionsValidator.Validate(RdsSnapshotManagerOptions options) : Result` runs immediately after `configuration.Bind(options)` in `Program.cs`. On failure, the error is logged and the process exits with a non-zero code before any menu is shown. Checks:

1. `RdsInstances` is non-empty.
2. Every instance has non-blank `InstanceName` and `ConnectionString`.
3. Exactly one instance has `DefaultSnapshotSource == true` (zero or multiple both fail, with a message stating the actual count and the offending keys).
4. At least one instance has `EnableWriteOperations == true` (otherwise the restore flow could never have a valid destination).

This follows the existing `Result`-returning validator pattern already used by `RdsSnapshotIdentifierValidator` and `LocalDatabaseConnectionGuard`.

## Service layer changes

### `InstancePicker` (generalized)

Replaces the current hardcoded production/beta picker. Takes the `RdsInstances` dictionary and an optional filter predicate, lists `key: instanceName`, reads a selection, and returns the selected **key** (not a raw AWS instance name):

```csharp
public static string? PickInstanceKey(
    IReadOnlyDictionary<string, RdsInstanceOptions> instances,
    Func<RdsInstanceOptions, bool>? filter = null);
```

Used:
- Unfiltered for "Create Snapshot" and "Dump & Publish" sources (both are read-only against the source).
- Filtered to `EnableWriteOperations == true` for the restore destination picker.

### `SnapshotCreateService`

Changes from a ctor-injected, production-only `IRDSManager` to a factory:

```csharp
public sealed class SnapshotCreateService
{
    private readonly Func<string, IRDSManager> _rdsManagerFactory;
    private readonly IClock _clock;

    public async Task<Result<string>> CreateSnapshot(string instanceName, string? customName, CancellationToken cancellationToken);
}
```

`Program.cs` registers `Func<string, IRDSManager> rdsManagerFactory = name => new RDSManager(name);` and `MainMenu` resolves the chosen instance's `InstanceName` via the picker before calling this.

### `DumpAndPublishService` (bug fix)

Takes an instance **key** instead of a raw name, resolves both `InstanceName` and `ConnectionString` from `RdsInstances[key]`, and dumps using that instance's own connection string — fixing the core bug where the selection was cosmetic. File/remote-key naming continues to use `InstanceName` (the AWS identifier) to preserve the existing backup naming scheme in S3/GCS/local-directory destinations. Behavior otherwise unchanged: it never scrubs data (backups are exact copies for disaster recovery, which is intentional and unaffected by this change).

### `BetaSyncService` → `RestoreSnapshotService` (generalized)

Renamed to reflect that it restores onto *any* write-enabled instance, not just beta:

```csharp
public sealed class RestoreSnapshotService
{
    public async Task<Result> Restore(string snapshotIdentifier, string destinationInstanceKey, CancellationToken cancellationToken);
}
```

Behavior:
1. Look up `destinationInstanceKey` in `RdsInstances`; fail if unknown.
2. **Re-check `EnableWriteOperations == true`** even though the picker already filtered to write-enabled instances — defense in depth, matching the existing `LocalDatabaseConnectionGuard` philosophy of not trusting UI-level filtering alone.
3. `RdsRestoreService.CopySnapshotToInstance(snapshotIdentifier, destination.InstanceName)`.
4. Build `MySQLBetaCleaner` / `MySQLFantasyCriticUserStore` from `destination.ConnectionString` (previously always beta's connection string) and run the existing scrub (`CleanEmailsAndPasswords`).

Snapshot **browsing** for this flow does not go through the generic instance picker. At startup, `Program.cs` resolves the single instance with `DefaultSnapshotSource == true` (guaranteed to exist by validation) and constructs one `IRDSManager` for it; `MainMenu` uses that manager to call `GetRecentSnapshots()` exactly as it does today for production.

### `LocalImportService`, `LocalDatabaseCleanService`, `LocalDatabaseConnectionGuard`

No behavioral change to the local-only flows themselves. `LocalDatabaseConnectionGuard.ValidateForClean` changes signature from two named connection-string parameters to a general collection:

```csharp
public static Result ValidateForClean(string localDockerConnectionString, IEnumerable<string> remoteConnectionStrings);
```

It rejects the local connection string if it matches *any* configured remote instance's connection string, so adding a third named instance automatically extends this safety check without code changes. `LocalDatabaseConnectionGuardTests` is updated accordingly.

## Menu (`MainMenu`)

```
RDS Snapshot Manager
1. Create snapshot                           (pick any instance as source)
2. Restore snapshot to instance (sanitized)  (browse default-source snapshots, pick a write-enabled destination, confirm, restore + scrub)
3. Dump & publish raw backup (unsanitized)   (pick any instance as source; uploads as-is, never scrubbed)
4. Import local dump to Docker (sanitized)   (pick a staged .sql.gz, import, scrub)
5. Clean local Docker database               (re-scrub without reimporting)
0. Exit
```

For action 2, before calling `RestoreSnapshotService.Restore`, `MainMenu` shows a confirmation naming the destination:

```
Restore snapshot 'adminsnap-2026-08-09-a' onto 'beta' (fantasy-critic-beta-rds)?
This will overwrite the instance and scrub PII. (y/N):
```

Anything other than `y`/`Y` cancels without side effects.

## Error handling

- Unknown/invalid instance keys from a picker return a `Result.Failure` with a clear message; no exceptions for expected control flow, consistent with existing code.
- The `EnableWriteOperations` re-check inside `RestoreSnapshotService` returns a `Result.Failure` (not a thrown exception) if somehow bypassed, consistent with the `Result`-based style used throughout this project.
- Startup validation failures are fatal: logged via Serilog at `Fatal` level and the process exits before the menu loop starts.

## Testing

- `LocalDatabaseConnectionGuardTests`: update call sites for the new `IEnumerable<string>` signature; add a case with 3+ remote connection strings to confirm the "any match" behavior.
- New `RdsSnapshotManagerOptionsValidatorTests`: cover zero default sources, multiple default sources, zero write-enabled instances, missing instance name/connection string, and the happy path.
- No integration test changes needed — this project has no NSwag-generated API surface and isn't covered by `FantasyCritic.IntegrationTests`.

## Out of scope

- No changes to `RDSManager`, `RdsRestoreService`, or `MySQLBetaCleaner` internals beyond how they're constructed/called.
- No changes to the `DatabaseUpdater` schema pipeline — this is a config/console-tool refactor only.
- No new destination types or changes to `IBackupDestination` implementations.
