# RDS Snapshot Manager: Generalized Instance Selection Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the flat, buggy production/beta configuration in `FantasyCritic.RdsSnapshotManager` with a generic, named `rdsInstances` dictionary so every action (create snapshot, restore+scrub, dump-and-publish) correctly targets the instance you actually pick, with `enableWriteOperations` and `defaultSnapshotSource` safety flags enforced by fail-fast startup validation and defense-in-depth checks.

**Architecture:** `RdsSnapshotManagerOptions.RdsInstances` becomes a `Dictionary<string, RdsInstanceOptions>` (instance name + connection string + two safety flags) bound from the already-updated `appsettings.json`. Two new pure helper classes (`RdsSnapshotManagerOptionsValidator`, `RdsInstanceLookup`) centralize validation and safe lookup, both unit-tested. `InstancePicker` becomes generic over the dictionary. `DumpAndPublishService` is fixed to use each instance's own connection string. `BetaSyncService` is renamed to `RestoreSnapshotService` and generalized to restore onto any write-enabled instance. `SnapshotCreateService` takes an `IRDSManager` factory instead of a fixed production manager.

**Tech Stack:** .NET 10, C#, `CSharpFunctionalExtensions` (`Result`/`Result<T>`), NodaTime, NUnit, Microsoft.Extensions.Configuration binding.

## Global Constraints

- `TreatWarningsAsErrors` is on for this project and `FantasyCritic.Test` — keep both warning-clean.
- No `Async` suffix on app/service methods (existing methods like `CreateSnapshot`, `GetAllUsers` already follow this — keep it).
- Use `Result` / `Result<T>` for expected failures, never exceptions for control flow (matches existing `LocalDatabaseConnectionGuard`, `RdsSnapshotIdentifierValidator`).
- No `out`/`ref` parameters.
- Read-only collection surfaces (`IReadOnlyList<T>`, `IReadOnlyDictionary<K,V>`) on public method parameters/returns where the codebase already does this; `Dictionary<K,V>` is acceptable on options/config classes bound directly by `Microsoft.Extensions.Configuration`, consistent with the existing options classes in this file.
- No schema/DB migration work — this is a config + console-tool refactor only, no `FantasyCritic.DatabaseUpdater` changes.

**Important manual step (not code, cannot be scripted here):** This project loads real credentials from local user secrets (`dotnet user-secrets`, `UserSecretsId` = `321dc8ab-26fb-47ba-bb34-94d992fbfe64`) which currently still use the *old* flat key names (`productionRdsInstance`, `betaRdsInstance`, `betaConnectionString`, `dumpConnectionString`). After this plan is implemented, update your local secrets to the new nested shape, e.g.:

```json
{
  "rdsInstances": {
    "production": { "connectionString": "Server=...;Uid=fantasycritic-admin;Pwd=...;..." },
    "beta": { "connectionString": "Server=...;Uid=fantasycritic-admin;Pwd=...;..." }
  }
}
```

This only matters for actually *running* the tool against real databases — it has no effect on building or testing.

---

## Task 1: Add the new `RdsInstances` config shape (additive)

**Files:**
- Modify: `src/FantasyCritic.RdsSnapshotManager/Configuration/RdsSnapshotManagerOptions.cs`

**Interfaces:**
- Produces: `RdsInstanceOptions` (`InstanceName`, `ConnectionString`, `EnableWriteOperations`, `DefaultSnapshotSource` — all with public getters/setters for config binding) and `RdsSnapshotManagerOptions.RdsInstances : Dictionary<string, RdsInstanceOptions>`, used by every later task.

This task is additive only: the old flat fields (`ProductionRdsInstance`, `BetaRdsInstance`, `BetaConnectionString`, `DumpConnectionString`) stay in place for now so the rest of the project keeps compiling. They're deleted in Task 9 once nothing references them. `appsettings.json` has already been hand-edited by the user to the new shape (currently uncommitted) — this task's commit includes that file too.

- [ ] **Step 1: Update the options file**

Replace the full contents of `src/FantasyCritic.RdsSnapshotManager/Configuration/RdsSnapshotManagerOptions.cs` with:

```csharp
namespace FantasyCritic.RdsSnapshotManager.Configuration;

public sealed class RdsSnapshotManagerOptions
{
    public string ProductionRdsInstance { get; set; } = null!;
    public string BetaRdsInstance { get; set; } = null!;
    public string BetaConnectionString { get; set; } = null!;
    public string DumpConnectionString { get; set; } = null!;
    public Dictionary<string, RdsInstanceOptions> RdsInstances { get; set; } = new();
    public string LocalStagingDirectory { get; set; } = null!;
    public LocalDockerOptions LocalDocker { get; set; } = new();
    public DestinationOptions Destinations { get; set; } = new();
}

public sealed class RdsInstanceOptions
{
    public string InstanceName { get; set; } = null!;
    public string ConnectionString { get; set; } = null!;
    public bool EnableWriteOperations { get; set; }
    public bool DefaultSnapshotSource { get; set; }
}

public sealed class LocalDockerOptions
{
    public string ConnectionString { get; set; } = null!;
    public string ContainerName { get; set; } = "fantasycritic-mysql";
}

public sealed class DestinationOptions
{
    public LocalDirectoryDestinationOptions LocalDirectory { get; set; } = new();
    public S3DestinationOptions S3 { get; set; } = new();
    public GoogleCloudDestinationOptions GoogleCloud { get; set; } = new();
}

public sealed class LocalDirectoryDestinationOptions
{
    public bool Enabled { get; set; }
    public string Path { get; set; } = null!;
}

public sealed class S3DestinationOptions
{
    public bool Enabled { get; set; }
    public string Bucket { get; set; } = null!;
    public string Prefix { get; set; } = "db-dumps/";
}

public sealed class GoogleCloudDestinationOptions
{
    public bool Enabled { get; set; }
    public string Bucket { get; set; } = null!;
    public string Prefix { get; set; } = "db-dumps/";
    public string? CredentialsPath { get; set; }
}
```

- [ ] **Step 2: Build to verify nothing broke**

Run: `dotnet build src/FantasyCritic.RdsSnapshotManager/FantasyCritic.RdsSnapshotManager.csproj`
Expected: `0 Error(s)` — this is purely additive so every existing call site still compiles.

- [ ] **Step 3: Commit**

```bash
git add src/FantasyCritic.RdsSnapshotManager/appsettings.json src/FantasyCritic.RdsSnapshotManager/Configuration/RdsSnapshotManagerOptions.cs
git commit -m "Add generic rdsInstances config shape alongside existing fields"
```

---

## Task 2: `RdsSnapshotManagerOptionsValidator` (TDD)

**Files:**
- Create: `src/FantasyCritic.RdsSnapshotManager/Configuration/RdsSnapshotManagerOptionsValidator.cs`
- Test: `src/FantasyCritic.Test/RdsSnapshotManagerOptionsValidatorTests.cs`

**Interfaces:**
- Consumes: `RdsSnapshotManagerOptions`, `RdsInstanceOptions` (from Task 1).
- Produces: `RdsSnapshotManagerOptionsValidator.Validate(RdsSnapshotManagerOptions options) : Result`, used by `Program.cs` in Task 9.

- [ ] **Step 1: Write the failing tests**

Create `src/FantasyCritic.Test/RdsSnapshotManagerOptionsValidatorTests.cs`:

```csharp
using System.Collections.Generic;
using FantasyCritic.RdsSnapshotManager.Configuration;
using NUnit.Framework;

namespace FantasyCritic.Test;

[TestFixture]
public class RdsSnapshotManagerOptionsValidatorTests
{
    private static RdsInstanceOptions MakeInstance(bool enableWriteOperations = false, bool defaultSnapshotSource = false) => new()
    {
        InstanceName = "some-instance",
        ConnectionString = "Server=localhost;Database=fantasycritic;Uid=fantasycritic;Pwd=secret;",
        EnableWriteOperations = enableWriteOperations,
        DefaultSnapshotSource = defaultSnapshotSource
    };

    private static RdsSnapshotManagerOptions MakeOptions(Dictionary<string, RdsInstanceOptions> instances) => new()
    {
        RdsInstances = instances
    };

    [Test]
    public void Validate_AcceptsWellFormedConfiguration()
    {
        var options = MakeOptions(new Dictionary<string, RdsInstanceOptions>
        {
            ["production"] = MakeInstance(enableWriteOperations: false, defaultSnapshotSource: true),
            ["beta"] = MakeInstance(enableWriteOperations: true, defaultSnapshotSource: false)
        });

        var result = RdsSnapshotManagerOptionsValidator.Validate(options);

        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public void Validate_RejectsEmptyInstanceDictionary()
    {
        var options = MakeOptions(new Dictionary<string, RdsInstanceOptions>());

        var result = RdsSnapshotManagerOptionsValidator.Validate(options);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Does.Contain("No RDS instances"));
        }
    }

    [Test]
    public void Validate_RejectsMissingInstanceName()
    {
        var instance = MakeInstance(defaultSnapshotSource: true, enableWriteOperations: true);
        instance.InstanceName = "";
        var options = MakeOptions(new Dictionary<string, RdsInstanceOptions> { ["production"] = instance });

        var result = RdsSnapshotManagerOptionsValidator.Validate(options);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Does.Contain("instanceName"));
        }
    }

    [Test]
    public void Validate_RejectsMissingConnectionString()
    {
        var instance = MakeInstance(defaultSnapshotSource: true, enableWriteOperations: true);
        instance.ConnectionString = "   ";
        var options = MakeOptions(new Dictionary<string, RdsInstanceOptions> { ["production"] = instance });

        var result = RdsSnapshotManagerOptionsValidator.Validate(options);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Does.Contain("connectionString"));
        }
    }

    [Test]
    public void Validate_RejectsZeroDefaultSnapshotSources()
    {
        var options = MakeOptions(new Dictionary<string, RdsInstanceOptions>
        {
            ["production"] = MakeInstance(enableWriteOperations: true, defaultSnapshotSource: false)
        });

        var result = RdsSnapshotManagerOptionsValidator.Validate(options);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Does.Contain("found 0"));
        }
    }

    [Test]
    public void Validate_RejectsMultipleDefaultSnapshotSources()
    {
        var options = MakeOptions(new Dictionary<string, RdsInstanceOptions>
        {
            ["production"] = MakeInstance(enableWriteOperations: true, defaultSnapshotSource: true),
            ["beta"] = MakeInstance(enableWriteOperations: true, defaultSnapshotSource: true)
        });

        var result = RdsSnapshotManagerOptionsValidator.Validate(options);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Does.Contain("found 2"));
        }
    }

    [Test]
    public void Validate_RejectsZeroWriteEnabledInstances()
    {
        var options = MakeOptions(new Dictionary<string, RdsInstanceOptions>
        {
            ["production"] = MakeInstance(enableWriteOperations: false, defaultSnapshotSource: true)
        });

        var result = RdsSnapshotManagerOptionsValidator.Validate(options);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Does.Contain("enableWriteOperations"));
        }
    }
}
```

- [ ] **Step 2: Run tests to verify they fail to compile**

Run: `dotnet test src/FantasyCritic.Test/FantasyCritic.Test.csproj --filter "FullyQualifiedName~RdsSnapshotManagerOptionsValidatorTests"`
Expected: build FAILS — `RdsSnapshotManagerOptionsValidator` does not exist yet.

- [ ] **Step 3: Implement the validator**

Create `src/FantasyCritic.RdsSnapshotManager/Configuration/RdsSnapshotManagerOptionsValidator.cs`:

```csharp
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
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test src/FantasyCritic.Test/FantasyCritic.Test.csproj --filter "FullyQualifiedName~RdsSnapshotManagerOptionsValidatorTests"`
Expected: all 7 tests PASS.

- [ ] **Step 5: Commit**

```bash
git add src/FantasyCritic.RdsSnapshotManager/Configuration/RdsSnapshotManagerOptionsValidator.cs src/FantasyCritic.Test/RdsSnapshotManagerOptionsValidatorTests.cs
git commit -m "Add fail-fast validation for RDS instance configuration"
```

---

## Task 3: `RdsInstanceLookup` (TDD)

**Files:**
- Create: `src/FantasyCritic.RdsSnapshotManager/Configuration/RdsInstanceLookup.cs`
- Test: `src/FantasyCritic.Test/RdsInstanceLookupTests.cs`

**Interfaces:**
- Consumes: `RdsInstanceOptions` (Task 1).
- Produces: `RdsInstanceLookup.TryResolve(IReadOnlyDictionary<string, RdsInstanceOptions>, string) : Result<RdsInstanceOptions>`, `RdsInstanceLookup.TryResolveWriteEnabled(...) : Result<RdsInstanceOptions>`, `RdsInstanceLookup.GetDefaultSnapshotSource(IReadOnlyDictionary<string, RdsInstanceOptions>) : KeyValuePair<string, RdsInstanceOptions>` — all consumed by `DumpAndPublishService` (Task 6), `RestoreSnapshotService` (Task 7), and `Program.cs` (Task 7/9).

- [ ] **Step 1: Write the failing tests**

Create `src/FantasyCritic.Test/RdsInstanceLookupTests.cs`:

```csharp
using System.Collections.Generic;
using FantasyCritic.RdsSnapshotManager.Configuration;
using NUnit.Framework;

namespace FantasyCritic.Test;

[TestFixture]
public class RdsInstanceLookupTests
{
    private static Dictionary<string, RdsInstanceOptions> BuildInstances() => new()
    {
        ["production"] = new RdsInstanceOptions
        {
            InstanceName = "fantasy-critic-rds",
            ConnectionString = "Server=prod;Database=fantasycritic;Uid=fantasycritic-admin;Pwd=secret;",
            EnableWriteOperations = false,
            DefaultSnapshotSource = true
        },
        ["beta"] = new RdsInstanceOptions
        {
            InstanceName = "fantasy-critic-beta-rds",
            ConnectionString = "Server=beta;Database=fantasycritic;Uid=fantasycritic-admin;Pwd=secret;",
            EnableWriteOperations = true,
            DefaultSnapshotSource = false
        }
    };

    [Test]
    public void TryResolve_ReturnsInstanceForKnownKey()
    {
        var result = RdsInstanceLookup.TryResolve(BuildInstances(), "beta");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.InstanceName, Is.EqualTo("fantasy-critic-beta-rds"));
        }
    }

    [Test]
    public void TryResolve_FailsForUnknownKey()
    {
        var result = RdsInstanceLookup.TryResolve(BuildInstances(), "staging");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Does.Contain("staging"));
        }
    }

    [Test]
    public void TryResolveWriteEnabled_SucceedsForWriteEnabledInstance()
    {
        var result = RdsInstanceLookup.TryResolveWriteEnabled(BuildInstances(), "beta");

        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public void TryResolveWriteEnabled_FailsForWriteDisabledInstance()
    {
        var result = RdsInstanceLookup.TryResolveWriteEnabled(BuildInstances(), "production");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Does.Contain("disabled"));
        }
    }

    [Test]
    public void TryResolveWriteEnabled_FailsForUnknownKey()
    {
        var result = RdsInstanceLookup.TryResolveWriteEnabled(BuildInstances(), "staging");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Does.Contain("staging"));
        }
    }

    [Test]
    public void GetDefaultSnapshotSource_ReturnsFlaggedInstance()
    {
        var defaultSource = RdsInstanceLookup.GetDefaultSnapshotSource(BuildInstances());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(defaultSource.Key, Is.EqualTo("production"));
            Assert.That(defaultSource.Value.InstanceName, Is.EqualTo("fantasy-critic-rds"));
        }
    }
}
```

- [ ] **Step 2: Run tests to verify they fail to compile**

Run: `dotnet test src/FantasyCritic.Test/FantasyCritic.Test.csproj --filter "FullyQualifiedName~RdsInstanceLookupTests"`
Expected: build FAILS — `RdsInstanceLookup` does not exist yet.

- [ ] **Step 3: Implement the lookup helper**

Create `src/FantasyCritic.RdsSnapshotManager/Configuration/RdsInstanceLookup.cs`:

```csharp
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

    public static KeyValuePair<string, RdsInstanceOptions> GetDefaultSnapshotSource(IReadOnlyDictionary<string, RdsInstanceOptions> instances)
    {
        return instances.Single(kv => kv.Value.DefaultSnapshotSource);
    }
}
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test src/FantasyCritic.Test/FantasyCritic.Test.csproj --filter "FullyQualifiedName~RdsInstanceLookupTests"`
Expected: all 6 tests PASS.

- [ ] **Step 5: Commit**

```bash
git add src/FantasyCritic.RdsSnapshotManager/Configuration/RdsInstanceLookup.cs src/FantasyCritic.Test/RdsInstanceLookupTests.cs
git commit -m "Add safe RDS instance lookup helper"
```

---

## Task 4: Generalize `LocalDatabaseConnectionGuard` to check all configured remote instances

**Files:**
- Modify: `src/FantasyCritic.RdsSnapshotManager/Infrastructure/LocalDatabaseConnectionGuard.cs`
- Modify: `src/FantasyCritic.RdsSnapshotManager/Services/LocalImportService.cs`
- Modify: `src/FantasyCritic.RdsSnapshotManager/Services/LocalDatabaseCleanService.cs`
- Modify: `src/FantasyCritic.Test/LocalDatabaseConnectionGuardTests.cs`

**Interfaces:**
- Consumes: nothing new.
- Produces: `LocalDatabaseConnectionGuard.ValidateForClean(string localDockerConnectionString, IEnumerable<string> remoteConnectionStrings) : Result` (signature change from the current two named-parameter version), used by `LocalImportService.Import` and `LocalDatabaseCleanService.Clean`.

- [ ] **Step 1: Update the test file for the new signature (failing/red step)**

Replace the full contents of `src/FantasyCritic.Test/LocalDatabaseConnectionGuardTests.cs`:

```csharp
using System.Collections.Generic;
using FantasyCritic.RdsSnapshotManager.Infrastructure;
using NUnit.Framework;

namespace FantasyCritic.Test;

[TestFixture]
public class LocalDatabaseConnectionGuardTests
{
    private const string LocalConnectionString =
        "Server=localhost;Port=3307;Database=fantasycritic-fromsnapshot;Uid=fantasycritic-admin;Pwd=secret;SslMode=Required;charset=utf8;";

    private const string RemoteConnectionStringA =
        "Server=example-beta-db.abc123.us-east-1.rds.amazonaws.com;Database=fantasycritic;Uid=fantasycritic;Pwd=secret;SslMode=Required;charset=utf8;";

    private const string RemoteConnectionStringB =
        "Server=example-prod-db.abc123.us-east-1.rds.amazonaws.com;Database=fantasycritic;Uid=fantasycritic-admin;Pwd=secret;SslMode=Required;charset=utf8;";

    private static readonly IReadOnlyList<string> RemoteConnectionStrings = [RemoteConnectionStringA, RemoteConnectionStringB];

    [Test]
    public void ValidateForClean_AcceptsConfiguredLocalDockerConnection()
    {
        var result = LocalDatabaseConnectionGuard.ValidateForClean(LocalConnectionString, RemoteConnectionStrings);

        Assert.That(result.IsSuccess, Is.True);
    }

    [TestCase("Server=127.0.0.1;Port=3307;Database=fantasycritic-fromsnapshot;Uid=fantasycritic;Pwd=secret;")]
    [TestCase("Server=::1;Port=3307;Database=fantasycritic-fromsnapshot;Uid=fantasycritic;Pwd=secret;")]
    public void ValidateForClean_AcceptsLocalhostVariants(string connectionString)
    {
        var result = LocalDatabaseConnectionGuard.ValidateForClean(connectionString, RemoteConnectionStrings);

        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public void ValidateForClean_RejectsRemoteConnectionString()
    {
        var result = LocalDatabaseConnectionGuard.ValidateForClean(RemoteConnectionStringA, RemoteConnectionStrings);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Does.Contain("localhost").Or.Contain("remote"));
        }
    }

    [Test]
    public void ValidateForClean_RejectsNonLocalPort()
    {
        var result = LocalDatabaseConnectionGuard.ValidateForClean(
            "Server=localhost;Port=3306;Database=fantasycritic;Uid=fantasycritic;Pwd=secret;",
            RemoteConnectionStrings);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Does.Contain("3307"));
        }
    }

    [Test]
    public void ValidateForClean_RejectsWhenLocalMatchesAnyConfiguredRemoteConnectionString()
    {
        var result = LocalDatabaseConnectionGuard.ValidateForClean(LocalConnectionString, [RemoteConnectionStringB, LocalConnectionString]);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Does.Contain("remote RDS instance"));
        }
    }

    [Test]
    public void ValidateForClean_AcceptsWhenNoRemoteConnectionStringsConfigured()
    {
        var result = LocalDatabaseConnectionGuard.ValidateForClean(LocalConnectionString, []);

        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public void ValidateForClean_RejectsSeededDatabaseName()
    {
        var result = LocalDatabaseConnectionGuard.ValidateForClean(
            "Server=localhost;Port=3307;Database=fantasycritic;Uid=fantasycritic-admin;Pwd=secret;SslMode=Required;charset=utf8;",
            RemoteConnectionStrings);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Does.Contain("fantasycritic"));
        }
        Assert.That(result.Error, Does.Contain("seeded").IgnoreCase);
    }

    [Test]
    public void ValidateForClean_AcceptsSnapshotDatabaseName()
    {
        var result = LocalDatabaseConnectionGuard.ValidateForClean(LocalConnectionString, RemoteConnectionStrings);

        Assert.That(result.IsSuccess, Is.True);
    }
}
```

- [ ] **Step 2: Run tests to verify they fail to compile**

Run: `dotnet test src/FantasyCritic.Test/FantasyCritic.Test.csproj --filter "FullyQualifiedName~LocalDatabaseConnectionGuardTests"`
Expected: build FAILS — the old `ValidateForClean(string, string, string)` overload doesn't match these calls.

- [ ] **Step 3: Update the guard implementation**

Replace the full contents of `src/FantasyCritic.RdsSnapshotManager/Infrastructure/LocalDatabaseConnectionGuard.cs`:

```csharp
using CSharpFunctionalExtensions;
using FantasyCritic.RdsSnapshotManager.Configuration;
using MySqlConnector;

namespace FantasyCritic.RdsSnapshotManager.Infrastructure;

public static class LocalDatabaseConnectionGuard
{
    private const uint LocalDockerPort = 3307;

    public static Result ValidateForClean(string localDockerConnectionString, IEnumerable<string> remoteConnectionStrings)
    {
        if (string.IsNullOrWhiteSpace(localDockerConnectionString))
        {
            return Result.Failure("Local Docker connection string is not configured.");
        }

        if (ContainsRemoteDatabaseMarker(localDockerConnectionString))
        {
            return Result.Failure("Refusing to clean: local Docker connection string appears to target a remote database.");
        }

        foreach (var remoteConnectionString in remoteConnectionStrings)
        {
            if (string.Equals(localDockerConnectionString, remoteConnectionString, StringComparison.Ordinal))
            {
                return Result.Failure("Refusing to clean: local Docker connection string matches a configured remote RDS instance's connection string.");
            }
        }

        var builder = new MySqlConnectionStringBuilder(localDockerConnectionString);
        var server = builder.Server.Trim().ToLowerInvariant();

        if (server != "localhost" && server != "127.0.0.1" && server != "::1")
        {
            return Result.Failure($"Refusing to clean: server must be localhost, got '{builder.Server}'.");
        }

        if (builder.Port != LocalDockerPort)
        {
            return Result.Failure($"Refusing to clean: port must be {LocalDockerPort} (local Docker), got {builder.Port}.");
        }

        if (string.Equals(builder.Database, LocalSnapshotDatabaseNames.SeededDatabase, StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure(
                $"Refusing to clean: database must not be the seeded local database '{LocalSnapshotDatabaseNames.SeededDatabase}'.");
        }

        if (!string.Equals(builder.Database, LocalSnapshotDatabaseNames.SnapshotDatabase, StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure(
                $"Refusing to clean: database must be '{LocalSnapshotDatabaseNames.SnapshotDatabase}', got '{builder.Database}'.");
        }

        return Result.Success();
    }

    private static bool ContainsRemoteDatabaseMarker(string connectionString)
    {
        return connectionString.Contains("amazonaws.com", StringComparison.OrdinalIgnoreCase)
            || connectionString.Contains(".rds.", StringComparison.OrdinalIgnoreCase);
    }
}
```

- [ ] **Step 4: Update `LocalImportService` call site**

In `src/FantasyCritic.RdsSnapshotManager/Services/LocalImportService.cs`, find:

```csharp
        var guard = LocalDatabaseConnectionGuard.ValidateForClean(
            snapshotConnectionString,
            _options.BetaConnectionString,
            _options.DumpConnectionString);
```

Replace with:

```csharp
        var guard = LocalDatabaseConnectionGuard.ValidateForClean(
            snapshotConnectionString,
            _options.RdsInstances.Values.Select(i => i.ConnectionString));
```

- [ ] **Step 5: Update `LocalDatabaseCleanService` call site**

In `src/FantasyCritic.RdsSnapshotManager/Services/LocalDatabaseCleanService.cs`, find:

```csharp
        var guard = LocalDatabaseConnectionGuard.ValidateForClean(
            snapshotConnectionString,
            _options.BetaConnectionString,
            _options.DumpConnectionString);
```

Replace with:

```csharp
        var guard = LocalDatabaseConnectionGuard.ValidateForClean(
            snapshotConnectionString,
            _options.RdsInstances.Values.Select(i => i.ConnectionString));
```

- [ ] **Step 6: Run tests to verify they pass, and build the project**

Run: `dotnet test src/FantasyCritic.Test/FantasyCritic.Test.csproj --filter "FullyQualifiedName~LocalDatabaseConnectionGuardTests"`
Expected: all 8 tests PASS.

Run: `dotnet build src/FantasyCritic.RdsSnapshotManager/FantasyCritic.RdsSnapshotManager.csproj`
Expected: `0 Error(s)`.

- [ ] **Step 7: Commit**

```bash
git add src/FantasyCritic.RdsSnapshotManager/Infrastructure/LocalDatabaseConnectionGuard.cs src/FantasyCritic.RdsSnapshotManager/Services/LocalImportService.cs src/FantasyCritic.RdsSnapshotManager/Services/LocalDatabaseCleanService.cs src/FantasyCritic.Test/LocalDatabaseConnectionGuardTests.cs
git commit -m "Generalize local database connection guard to check all configured RDS instances"
```

---

## Task 5: Generalize `InstancePicker` over the `RdsInstances` dictionary

**Files:**
- Modify: `src/FantasyCritic.RdsSnapshotManager/Console/InstancePicker.cs`
- Modify: `src/FantasyCritic.RdsSnapshotManager/Console/MainMenu.cs` (only the `DumpAndPublish` method, to keep it compiling against the still-unchanged `DumpAndPublishService.DumpAndPublish(string instanceName, ...)` signature)

**Interfaces:**
- Consumes: `RdsInstanceOptions` (Task 1).
- Produces: `InstancePicker.PickInstanceKey(IReadOnlyDictionary<string, RdsInstanceOptions> instances, Func<RdsInstanceOptions, bool>? filter = null) : string?`, used by `MainMenu` in this task and Tasks 7 and 8.

No unit test for this class: it's a thin `System.Console` I/O wrapper, consistent with the existing untested `SnapshotPicker` in the same folder.

- [ ] **Step 1: Rewrite `InstancePicker`**

Replace the full contents of `src/FantasyCritic.RdsSnapshotManager/Console/InstancePicker.cs`:

```csharp
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
```

- [ ] **Step 2: Update the `DumpAndPublish` call site in `MainMenu`**

In `src/FantasyCritic.RdsSnapshotManager/Console/MainMenu.cs`, find:

```csharp
    private async Task DumpAndPublish(CancellationToken cancellationToken)
    {
        var instanceName = InstancePicker.PickInstance(_options);
        if (instanceName is null)
        {
            System.Console.WriteLine("Invalid instance selection.");
            return;
        }

        try
        {
            Log.Information("Starting dump and publish from {Instance}", instanceName);
            var result = await _dumpAndPublishService.DumpAndPublish(instanceName, cancellationToken);
```

Replace the picker call and log line only (leave the rest of the method body unchanged for now — `DumpAndPublishService`'s signature is fixed in Task 6):

```csharp
    private async Task DumpAndPublish(CancellationToken cancellationToken)
    {
        var instanceKey = InstancePicker.PickInstanceKey(_options.RdsInstances);
        if (instanceKey is null)
        {
            System.Console.WriteLine("Invalid instance selection.");
            return;
        }

        var instanceName = _options.RdsInstances[instanceKey].InstanceName;

        try
        {
            Log.Information("Starting dump and publish from {Instance}", instanceKey);
            var result = await _dumpAndPublishService.DumpAndPublish(instanceName, cancellationToken);
```

- [ ] **Step 3: Build to verify it compiles**

Run: `dotnet build src/FantasyCritic.RdsSnapshotManager/FantasyCritic.RdsSnapshotManager.csproj`
Expected: `0 Error(s)`.

- [ ] **Step 4: Commit**

```bash
git add src/FantasyCritic.RdsSnapshotManager/Console/InstancePicker.cs src/FantasyCritic.RdsSnapshotManager/Console/MainMenu.cs
git commit -m "Generalize InstancePicker over the configured RDS instance dictionary"
```

---

## Task 6: Fix `DumpAndPublishService` to use each instance's own connection string

**Files:**
- Modify: `src/FantasyCritic.RdsSnapshotManager/Services/DumpAndPublishService.cs`
- Modify: `src/FantasyCritic.RdsSnapshotManager/Console/MainMenu.cs` (only the `DumpAndPublish` method)

**Interfaces:**
- Consumes: `RdsInstanceLookup.TryResolve` (Task 3).
- Produces: `DumpAndPublishService.DumpAndPublish(string instanceKey, CancellationToken) : Task<Result<string>>` (signature change: parameter renamed/repurposed from a raw AWS instance name to a config dictionary key), consumed by `MainMenu`.

This is the core bug fix: `DumpAndPublish` currently always dumps via `_options.DumpConnectionString` regardless of which instance you picked. After this task, it resolves the instance from `RdsInstances` and dumps via *that instance's own* `ConnectionString`.

No unit test for this service: it shells out to `mysqldump` and touches the filesystem/network, matching the existing untested precedent for this class. The instance-resolution logic it depends on (`RdsInstanceLookup.TryResolve`) is already covered by Task 3's tests.

- [ ] **Step 1: Update `DumpAndPublishService`**

Replace the full contents of `src/FantasyCritic.RdsSnapshotManager/Services/DumpAndPublishService.cs`:

```csharp
using System.Globalization;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Utilities;
using FantasyCritic.RdsSnapshotManager.Configuration;
using FantasyCritic.RdsSnapshotManager.Destinations;
using FantasyCritic.RdsSnapshotManager.Infrastructure;
using NodaTime;

namespace FantasyCritic.RdsSnapshotManager.Services;

public sealed class DumpAndPublishService
{
    private readonly RdsSnapshotManagerOptions _options;
    private readonly MysqldumpRunner _mysqldumpRunner;
    private readonly IReadOnlyList<BackupDestinationRegistration> _destinations;
    private readonly IClock _clock;

    public DumpAndPublishService(
        RdsSnapshotManagerOptions options,
        MysqldumpRunner mysqldumpRunner,
        IReadOnlyList<BackupDestinationRegistration> destinations,
        IClock clock)
    {
        _options = options;
        _mysqldumpRunner = mysqldumpRunner;
        _destinations = destinations;
        _clock = clock;
    }

    public async Task<Result<string>> DumpAndPublish(string instanceKey, CancellationToken cancellationToken)
    {
        var instanceResult = RdsInstanceLookup.TryResolve(_options.RdsInstances, instanceKey);
        if (instanceResult.IsFailure)
        {
            return Result.Failure<string>(instanceResult.Error);
        }

        var instance = instanceResult.Value;
        var timestamp = _clock.GetCurrentInstant();
        var zonedTimestamp = timestamp.InZone(TimeExtensions.EasternTimeZone);
        var fileName = $"{instance.InstanceName}-{zonedTimestamp.LocalDateTime.ToString("yyyy-MM-dd-HHmmss", CultureInfo.InvariantCulture)}.sql.gz";
        var stagingPath = Path.Combine(_options.LocalStagingDirectory, fileName);

        var dumpResult = await _mysqldumpRunner.DumpToGzipFile(instance.ConnectionString, stagingPath, cancellationToken);
        if (dumpResult.IsFailure)
        {
            return Result.Failure<string>(dumpResult.Error);
        }

        foreach (var destination in _destinations)
        {
            var remoteKey = BackupRemoteKeyBuilder.Build(destination.Prefix, instance.InstanceName, timestamp, fileName);
            await destination.Destination.UploadAsync(stagingPath, remoteKey, cancellationToken);
        }

        return Result.Success(stagingPath);
    }
}
```

- [ ] **Step 2: Update the `DumpAndPublish` call site in `MainMenu`**

In `src/FantasyCritic.RdsSnapshotManager/Console/MainMenu.cs`, find the method updated in Task 5:

```csharp
    private async Task DumpAndPublish(CancellationToken cancellationToken)
    {
        var instanceKey = InstancePicker.PickInstanceKey(_options.RdsInstances);
        if (instanceKey is null)
        {
            System.Console.WriteLine("Invalid instance selection.");
            return;
        }

        var instanceName = _options.RdsInstances[instanceKey].InstanceName;

        try
        {
            Log.Information("Starting dump and publish from {Instance}", instanceKey);
            var result = await _dumpAndPublishService.DumpAndPublish(instanceName, cancellationToken);
```

Replace with (drop the now-unnecessary `instanceName` lookup, pass the key straight through):

```csharp
    private async Task DumpAndPublish(CancellationToken cancellationToken)
    {
        var instanceKey = InstancePicker.PickInstanceKey(_options.RdsInstances);
        if (instanceKey is null)
        {
            System.Console.WriteLine("Invalid instance selection.");
            return;
        }

        try
        {
            Log.Information("Starting dump and publish from {Instance}", instanceKey);
            var result = await _dumpAndPublishService.DumpAndPublish(instanceKey, cancellationToken);
```

Also update the menu label for this option. Find:

```csharp
            System.Console.WriteLine("3. Dump and publish from instance");
```

Replace with:

```csharp
            System.Console.WriteLine("3. Dump & publish raw backup (unsanitized)");
```

- [ ] **Step 3: Build to verify it compiles**

Run: `dotnet build src/FantasyCritic.RdsSnapshotManager/FantasyCritic.RdsSnapshotManager.csproj`
Expected: `0 Error(s)`.

- [ ] **Step 4: Commit**

```bash
git add src/FantasyCritic.RdsSnapshotManager/Services/DumpAndPublishService.cs src/FantasyCritic.RdsSnapshotManager/Console/MainMenu.cs
git commit -m "Fix DumpAndPublishService to dump from the selected instance's own connection string"
```

---

## Task 7: Generalize `BetaSyncService` into `RestoreSnapshotService`

**Files:**
- Delete: `src/FantasyCritic.RdsSnapshotManager/Services/BetaSyncService.cs`
- Create: `src/FantasyCritic.RdsSnapshotManager/Services/RestoreSnapshotService.cs`
- Modify: `src/FantasyCritic.RdsSnapshotManager/Console/MainMenu.cs`
- Modify: `src/FantasyCritic.RdsSnapshotManager/Program.cs`

**Interfaces:**
- Consumes: `RdsInstanceLookup.TryResolveWriteEnabled` (Task 3), `RdsInstanceLookup.GetDefaultSnapshotSource` (Task 3), `InstancePicker.PickInstanceKey` (Task 5).
- Produces: `RestoreSnapshotService.Restore(string snapshotIdentifier, string destinationInstanceKey, CancellationToken) : Task<Result>`, consumed by `MainMenu`.

This delivers the "copy to another db, cleanse it" flow generalized to any write-enabled instance, with a y/N confirmation and a defense-in-depth re-check of `EnableWriteOperations` inside the service itself.

No unit test for this service: like `BetaSyncService` before it, it drives real AWS RDS restore calls and MySQL connections. The `EnableWriteOperations` gating logic it depends on is already covered by Task 3's `RdsInstanceLookupTests.TryResolveWriteEnabled_*` tests.

- [ ] **Step 1: Delete the old service**

```bash
git rm src/FantasyCritic.RdsSnapshotManager/Services/BetaSyncService.cs
```

- [ ] **Step 2: Create `RestoreSnapshotService`**

Create `src/FantasyCritic.RdsSnapshotManager/Services/RestoreSnapshotService.cs`:

```csharp
using CSharpFunctionalExtensions;
using FantasyCritic.AWS;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.MySQL;
using FantasyCritic.MySQL.SyncingRepos;
using FantasyCritic.RdsSnapshotManager.Configuration;
using NodaTime;

namespace FantasyCritic.RdsSnapshotManager.Services;

public sealed class RestoreSnapshotService
{
    private readonly RdsRestoreService _restoreService;
    private readonly RdsSnapshotManagerOptions _options;
    private readonly IClock _clock;

    public RestoreSnapshotService(RdsRestoreService restoreService, RdsSnapshotManagerOptions options, IClock clock)
    {
        _restoreService = restoreService;
        _options = options;
        _clock = clock;
    }

    public async Task<Result> Restore(string snapshotIdentifier, string destinationInstanceKey, CancellationToken cancellationToken)
    {
        var destinationResult = RdsInstanceLookup.TryResolveWriteEnabled(_options.RdsInstances, destinationInstanceKey);
        if (destinationResult.IsFailure)
        {
            return Result.Failure(destinationResult.Error);
        }

        var destination = destinationResult.Value;
        await _restoreService.CopySnapshotToInstance(snapshotIdentifier, destination.InstanceName);

        var repoConfig = new RepositoryConfiguration(destination.ConnectionString, _clock);
        var userStore = new MySQLFantasyCriticUserStore(repoConfig);
        var cleaner = new MySQLBetaCleaner(destination.ConnectionString);

        var allUsers = await userStore.GetAllUsers();
        var betaUsers = await userStore.GetUsersInRoleAsync("BetaTester", cancellationToken);
        await cleaner.CleanEmailsAndPasswords(allUsers, betaUsers);
        return Result.Success();
    }
}
```

- [ ] **Step 3: Update `MainMenu`**

In `src/FantasyCritic.RdsSnapshotManager/Console/MainMenu.cs`, update the field, constructor, menu label, and the beta-sync method.

Find the field declarations:

```csharp
    private readonly SnapshotCreateService _snapshotCreateService;
    private readonly IRDSManager _productionRdsManager;
    private readonly BetaSyncService _betaSyncService;
    private readonly DumpAndPublishService _dumpAndPublishService;
```

Replace with:

```csharp
    private readonly SnapshotCreateService _snapshotCreateService;
    private readonly IRDSManager _defaultSourceRdsManager;
    private readonly RestoreSnapshotService _restoreSnapshotService;
    private readonly DumpAndPublishService _dumpAndPublishService;
```

Find the constructor:

```csharp
    public MainMenu(
        SnapshotCreateService snapshotCreateService,
        IRDSManager productionRdsManager,
        BetaSyncService betaSyncService,
        DumpAndPublishService dumpAndPublishService,
        LocalImportService localImportService,
        LocalDatabaseCleanService localDatabaseCleanService,
        RdsSnapshotManagerOptions options)
    {
        _snapshotCreateService = snapshotCreateService;
        _productionRdsManager = productionRdsManager;
        _betaSyncService = betaSyncService;
        _dumpAndPublishService = dumpAndPublishService;
        _localImportService = localImportService;
        _localDatabaseCleanService = localDatabaseCleanService;
        _options = options;
    }
```

Replace with:

```csharp
    public MainMenu(
        SnapshotCreateService snapshotCreateService,
        IRDSManager defaultSourceRdsManager,
        RestoreSnapshotService restoreSnapshotService,
        DumpAndPublishService dumpAndPublishService,
        LocalImportService localImportService,
        LocalDatabaseCleanService localDatabaseCleanService,
        RdsSnapshotManagerOptions options)
    {
        _snapshotCreateService = snapshotCreateService;
        _defaultSourceRdsManager = defaultSourceRdsManager;
        _restoreSnapshotService = restoreSnapshotService;
        _dumpAndPublishService = dumpAndPublishService;
        _localImportService = localImportService;
        _localDatabaseCleanService = localDatabaseCleanService;
        _options = options;
    }
```

Find the menu label:

```csharp
            System.Console.WriteLine("2. Beta sync from snapshot");
```

Replace with:

```csharp
            System.Console.WriteLine("2. Restore snapshot to instance (sanitized)");
```

Find the switch case call:

```csharp
                case "2":
                    await BetaSync(cancellationToken);
                    break;
```

Replace with:

```csharp
                case "2":
                    await RestoreSnapshotToInstance(cancellationToken);
                    break;
```

Find the whole `BetaSync` method:

```csharp
    private async Task BetaSync(CancellationToken cancellationToken)
    {
        try
        {
            var snapshots = await _productionRdsManager.GetRecentSnapshots();
            var recentSnapshots = snapshots
                .OrderByDescending(x => x.CreationTime)
                .Take(10)
                .ToList();

            var snapshotIdentifier = SnapshotPicker.PickSnapshot(recentSnapshots);
            if (snapshotIdentifier is null)
            {
                return;
            }

            Log.Information("Starting beta sync from {Snapshot}", snapshotIdentifier);
            var result = await _betaSyncService.Sync(snapshotIdentifier, cancellationToken);
            if (result.IsSuccess)
            {
                System.Console.WriteLine("Beta sync complete.");
            }
            else
            {
                System.Console.WriteLine($"Beta sync failed: {result.Error}");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Beta sync failed.");
            System.Console.WriteLine($"Beta sync failed: {ex.Message}");
        }
    }
```

Replace with:

```csharp
    private async Task RestoreSnapshotToInstance(CancellationToken cancellationToken)
    {
        try
        {
            var snapshots = await _defaultSourceRdsManager.GetRecentSnapshots();
            var recentSnapshots = snapshots
                .OrderByDescending(x => x.CreationTime)
                .Take(10)
                .ToList();

            var snapshotIdentifier = SnapshotPicker.PickSnapshot(recentSnapshots);
            if (snapshotIdentifier is null)
            {
                return;
            }

            var destinationKey = InstancePicker.PickInstanceKey(_options.RdsInstances, i => i.EnableWriteOperations);
            if (destinationKey is null)
            {
                return;
            }

            var destinationInstanceName = _options.RdsInstances[destinationKey].InstanceName;
            System.Console.Write(
                $"Restore snapshot '{snapshotIdentifier}' onto '{destinationKey}' ({destinationInstanceName})? " +
                "This will overwrite the instance and scrub PII. (y/N): ");
            var confirmation = System.Console.ReadLine();
            if (!string.Equals(confirmation, "y", StringComparison.OrdinalIgnoreCase))
            {
                System.Console.WriteLine("Cancelled.");
                return;
            }

            Log.Information("Starting restore of {Snapshot} to {Destination}", snapshotIdentifier, destinationKey);
            var result = await _restoreSnapshotService.Restore(snapshotIdentifier, destinationKey, cancellationToken);
            if (result.IsSuccess)
            {
                System.Console.WriteLine("Restore complete.");
            }
            else
            {
                System.Console.WriteLine($"Restore failed: {result.Error}");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Restore failed.");
            System.Console.WriteLine($"Restore failed: {ex.Message}");
        }
    }
```

- [ ] **Step 4: Update `Program.cs`**

In `src/FantasyCritic.RdsSnapshotManager/Program.cs`, find:

```csharp
        IRDSManager productionRdsManager = new RDSManager(options.ProductionRdsInstance);
        var restoreService = new RdsRestoreService();
        var mysqldumpRunner = new MysqldumpRunner();
        var dockerHealthChecker = new DockerMySqlHealthChecker();
        var emptyChecker = new DatabaseEmptyChecker();
        var destinations = BackupDestinationFactory.CreateRegistrations(options);

        RepositoryConfiguration betaRepoConfig = new RepositoryConfiguration(options.BetaConnectionString, clock);
        MySQLFantasyCriticUserStore betaUserStore = new MySQLFantasyCriticUserStore(betaRepoConfig);
        MySQLBetaCleaner betaCleaner = new MySQLBetaCleaner(options.BetaConnectionString);

        string localSnapshotConnectionString = LocalSnapshotConnectionString.BuildSnapshotConnectionString(
            options.LocalDocker.ConnectionString);

        RepositoryConfiguration localRepoConfig = new RepositoryConfiguration(localSnapshotConnectionString, clock);
        MySQLFantasyCriticUserStore localUserStore = new MySQLFantasyCriticUserStore(localRepoConfig);
        MySQLBetaCleaner localCleaner = new MySQLBetaCleaner(localSnapshotConnectionString);

        SnapshotCreateService snapshotCreateService = new SnapshotCreateService(productionRdsManager, clock);
        BetaSyncService betaSyncService = new BetaSyncService(restoreService, options, betaCleaner, betaUserStore);
        DumpAndPublishService dumpAndPublishService = new DumpAndPublishService(options, mysqldumpRunner, destinations, clock);
        LocalImportService localImportService = new LocalImportService(
            options,
            dockerHealthChecker,
            emptyChecker,
            mysqldumpRunner,
            localCleaner,
            localUserStore);
        LocalDatabaseCleanService localDatabaseCleanService = new LocalDatabaseCleanService(
            options,
            dockerHealthChecker,
            localCleaner,
            localUserStore);

        Console.MainMenu mainMenu = new Console.MainMenu(
            snapshotCreateService,
            productionRdsManager,
            betaSyncService,
            dumpAndPublishService,
            localImportService,
            localDatabaseCleanService,
            options);
```

Replace with:

```csharp
        var defaultSnapshotSource = RdsInstanceLookup.GetDefaultSnapshotSource(options.RdsInstances);
        IRDSManager defaultSourceRdsManager = new RDSManager(defaultSnapshotSource.Value.InstanceName);
        var restoreService = new RdsRestoreService();
        var mysqldumpRunner = new MysqldumpRunner();
        var dockerHealthChecker = new DockerMySqlHealthChecker();
        var emptyChecker = new DatabaseEmptyChecker();
        var destinations = BackupDestinationFactory.CreateRegistrations(options);

        string localSnapshotConnectionString = LocalSnapshotConnectionString.BuildSnapshotConnectionString(
            options.LocalDocker.ConnectionString);

        RepositoryConfiguration localRepoConfig = new RepositoryConfiguration(localSnapshotConnectionString, clock);
        MySQLFantasyCriticUserStore localUserStore = new MySQLFantasyCriticUserStore(localRepoConfig);
        MySQLBetaCleaner localCleaner = new MySQLBetaCleaner(localSnapshotConnectionString);

        SnapshotCreateService snapshotCreateService = new SnapshotCreateService(defaultSourceRdsManager, clock);
        RestoreSnapshotService restoreSnapshotService = new RestoreSnapshotService(restoreService, options, clock);
        DumpAndPublishService dumpAndPublishService = new DumpAndPublishService(options, mysqldumpRunner, destinations, clock);
        LocalImportService localImportService = new LocalImportService(
            options,
            dockerHealthChecker,
            emptyChecker,
            mysqldumpRunner,
            localCleaner,
            localUserStore);
        LocalDatabaseCleanService localDatabaseCleanService = new LocalDatabaseCleanService(
            options,
            dockerHealthChecker,
            localCleaner,
            localUserStore);

        Console.MainMenu mainMenu = new Console.MainMenu(
            snapshotCreateService,
            defaultSourceRdsManager,
            restoreSnapshotService,
            dumpAndPublishService,
            localImportService,
            localDatabaseCleanService,
            options);
```

Note: `SnapshotCreateService` is still constructed with a single fixed `IRDSManager` here (`defaultSourceRdsManager`) — this is intentionally temporary and is generalized to a proper per-call factory in Task 8, which also updates this line again.

- [ ] **Step 5: Build to verify it compiles**

Run: `dotnet build src/FantasyCritic.RdsSnapshotManager/FantasyCritic.RdsSnapshotManager.csproj`
Expected: `0 Error(s)`.

- [ ] **Step 6: Commit**

```bash
git add -A src/FantasyCritic.RdsSnapshotManager
git commit -m "Generalize BetaSyncService into RestoreSnapshotService targeting any write-enabled instance"
```

---

## Task 8: Generalize `SnapshotCreateService` to accept any source instance

**Files:**
- Modify: `src/FantasyCritic.RdsSnapshotManager/Services/SnapshotCreateService.cs`
- Modify: `src/FantasyCritic.RdsSnapshotManager/Console/MainMenu.cs`
- Modify: `src/FantasyCritic.RdsSnapshotManager/Program.cs`

**Interfaces:**
- Consumes: `InstancePicker.PickInstanceKey` (Task 5).
- Produces: `SnapshotCreateService.CreateSnapshot(string instanceName, string? customName, CancellationToken) : Task<Result<string>>` (adds a required `instanceName` parameter), consumed by `MainMenu`.

No unit test for this service: it drives real AWS RDS API calls and a polling loop, matching the existing untested precedent for this class.

- [ ] **Step 1: Update `SnapshotCreateService`**

Replace the full contents of `src/FantasyCritic.RdsSnapshotManager/Services/SnapshotCreateService.cs`:

```csharp
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Utilities;
using NodaTime;

namespace FantasyCritic.RdsSnapshotManager.Services;

public sealed class SnapshotCreateService
{
    private readonly Func<string, IRDSManager> _rdsManagerFactory;
    private readonly IClock _clock;

    public SnapshotCreateService(Func<string, IRDSManager> rdsManagerFactory, IClock clock)
    {
        _rdsManagerFactory = rdsManagerFactory;
        _clock = clock;
    }

    public async Task<Result<string>> CreateSnapshot(string instanceName, string? customName, CancellationToken cancellationToken)
    {
        if (customName is not null)
        {
            var validation = RdsSnapshotIdentifierValidator.Validate(customName);
            if (validation.IsFailure)
            {
                return Result.Failure<string>(validation.Error);
            }
        }

        var rdsManager = _rdsManagerFactory(instanceName);
        string snapshotId = await rdsManager.SnapshotRDS(_clock.GetCurrentInstant(), customName);
        await WaitUntilAvailable(rdsManager, snapshotId, cancellationToken);
        return Result.Success(snapshotId);
    }

    private async Task WaitUntilAvailable(IRDSManager rdsManager, string snapshotId, CancellationToken cancellationToken)
    {
        var timeout = Duration.FromMinutes(30);
        var deadline = _clock.GetCurrentInstant().Plus(timeout);
        while (_clock.GetCurrentInstant() < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var snaps = await rdsManager.GetRecentSnapshots();
            var match = snaps.SingleOrDefault(s => s.SnapshotName == snapshotId);
            if (match is { Status: "available", Percent: 100 })
            {
                return;
            }

            await Task.Delay(TimeSpan.FromSeconds(15), cancellationToken);
        }

        throw new TimeoutException($"Snapshot {snapshotId} did not become available within {timeout}.");
    }
}
```

- [ ] **Step 2: Update `Program.cs` to build a factory instead of one fixed manager**

In `src/FantasyCritic.RdsSnapshotManager/Program.cs`, find:

```csharp
        var defaultSnapshotSource = RdsInstanceLookup.GetDefaultSnapshotSource(options.RdsInstances);
        IRDSManager defaultSourceRdsManager = new RDSManager(defaultSnapshotSource.Value.InstanceName);
```

Replace with:

```csharp
        Func<string, IRDSManager> rdsManagerFactory = instanceName => new RDSManager(instanceName);
        var defaultSnapshotSource = RdsInstanceLookup.GetDefaultSnapshotSource(options.RdsInstances);
        IRDSManager defaultSourceRdsManager = rdsManagerFactory(defaultSnapshotSource.Value.InstanceName);
```

Then find:

```csharp
        SnapshotCreateService snapshotCreateService = new SnapshotCreateService(defaultSourceRdsManager, clock);
```

Replace with:

```csharp
        SnapshotCreateService snapshotCreateService = new SnapshotCreateService(rdsManagerFactory, clock);
```

- [ ] **Step 3: Update `MainMenu.CreateSnapshot` to pick a source instance**

In `src/FantasyCritic.RdsSnapshotManager/Console/MainMenu.cs`, find:

```csharp
    private async Task CreateSnapshot(CancellationToken cancellationToken)
    {
        System.Console.Write("Custom snapshot name (leave blank for auto-generated): ");
        var customName = System.Console.ReadLine();
        if (string.IsNullOrWhiteSpace(customName))
        {
            customName = null;
        }

        try
        {
            var result = await _snapshotCreateService.CreateSnapshot(customName, cancellationToken);
```

Replace with:

```csharp
    private async Task CreateSnapshot(CancellationToken cancellationToken)
    {
        var instanceKey = InstancePicker.PickInstanceKey(_options.RdsInstances);
        if (instanceKey is null)
        {
            return;
        }

        var instanceName = _options.RdsInstances[instanceKey].InstanceName;

        System.Console.Write("Custom snapshot name (leave blank for auto-generated): ");
        var customName = System.Console.ReadLine();
        if (string.IsNullOrWhiteSpace(customName))
        {
            customName = null;
        }

        try
        {
            var result = await _snapshotCreateService.CreateSnapshot(instanceName, customName, cancellationToken);
```

Also update the menu label. Find:

```csharp
            System.Console.WriteLine("1. Create production snapshot");
```

Replace with:

```csharp
            System.Console.WriteLine("1. Create snapshot");
```

- [ ] **Step 4: Build to verify it compiles**

Run: `dotnet build src/FantasyCritic.RdsSnapshotManager/FantasyCritic.RdsSnapshotManager.csproj`
Expected: `0 Error(s)`.

- [ ] **Step 5: Commit**

```bash
git add src/FantasyCritic.RdsSnapshotManager/Services/SnapshotCreateService.cs src/FantasyCritic.RdsSnapshotManager/Console/MainMenu.cs src/FantasyCritic.RdsSnapshotManager/Program.cs
git commit -m "Generalize SnapshotCreateService to snapshot any configured instance"
```

---

## Task 9: Wire up fail-fast validation, remove dead config fields, finish menu polish

**Files:**
- Modify: `src/FantasyCritic.RdsSnapshotManager/Program.cs`
- Modify: `src/FantasyCritic.RdsSnapshotManager/Configuration/RdsSnapshotManagerOptions.cs`
- Modify: `src/FantasyCritic.RdsSnapshotManager/Console/MainMenu.cs`

**Interfaces:**
- Consumes: `RdsSnapshotManagerOptionsValidator.Validate` (Task 2).
- Produces: nothing new — this is cleanup and final wiring.

By this point, `ProductionRdsInstance`, `BetaRdsInstance`, `BetaConnectionString`, and `DumpConnectionString` are no longer referenced anywhere (confirmed by the grep in Step 1) — this task removes them and wires the fail-fast validator into startup.

- [ ] **Step 1: Confirm the old fields are unused**

Run (PowerShell):

```powershell
Select-String -Path src\FantasyCritic.RdsSnapshotManager\**\*.cs -Pattern "ProductionRdsInstance|BetaRdsInstance|BetaConnectionString|DumpConnectionString"
```

Expected: only matches inside `RdsSnapshotManagerOptions.cs` itself (the declarations) — no other file references them.

- [ ] **Step 2: Remove the dead fields**

In `src/FantasyCritic.RdsSnapshotManager/Configuration/RdsSnapshotManagerOptions.cs`, find:

```csharp
public sealed class RdsSnapshotManagerOptions
{
    public string ProductionRdsInstance { get; set; } = null!;
    public string BetaRdsInstance { get; set; } = null!;
    public string BetaConnectionString { get; set; } = null!;
    public string DumpConnectionString { get; set; } = null!;
    public Dictionary<string, RdsInstanceOptions> RdsInstances { get; set; } = new();
    public string LocalStagingDirectory { get; set; } = null!;
    public LocalDockerOptions LocalDocker { get; set; } = new();
    public DestinationOptions Destinations { get; set; } = new();
}
```

Replace with:

```csharp
public sealed class RdsSnapshotManagerOptions
{
    public Dictionary<string, RdsInstanceOptions> RdsInstances { get; set; } = new();
    public string LocalStagingDirectory { get; set; } = null!;
    public LocalDockerOptions LocalDocker { get; set; } = new();
    public DestinationOptions Destinations { get; set; } = new();
}
```

- [ ] **Step 3: Wire fail-fast validation into `Program.cs`**

In `src/FantasyCritic.RdsSnapshotManager/Program.cs`, find:

```csharp
        var options = new RdsSnapshotManagerOptions();
        configuration.Bind(options);

        DapperNodaTimeSetup.SetupDapperNodaTimeMappings();
```

Replace with:

```csharp
        var options = new RdsSnapshotManagerOptions();
        configuration.Bind(options);

        var validation = RdsSnapshotManagerOptionsValidator.Validate(options);
        if (validation.IsFailure)
        {
            Log.Fatal("Invalid RDS Snapshot Manager configuration: {Error}", validation.Error);
            Environment.Exit(1);
            return;
        }

        DapperNodaTimeSetup.SetupDapperNodaTimeMappings();
```

- [ ] **Step 4: Polish the remaining menu labels in `MainMenu`**

In `src/FantasyCritic.RdsSnapshotManager/Console/MainMenu.cs`, find:

```csharp
            System.Console.WriteLine("4. Import local dump to Docker MySQL");
```

Replace with:

```csharp
            System.Console.WriteLine("4. Import local dump to Docker (sanitized)");
```

- [ ] **Step 5: Build to verify it compiles**

Run: `dotnet build src/FantasyCritic.RdsSnapshotManager/FantasyCritic.RdsSnapshotManager.csproj`
Expected: `0 Error(s)`.

- [ ] **Step 6: Commit**

```bash
git add src/FantasyCritic.RdsSnapshotManager/Program.cs src/FantasyCritic.RdsSnapshotManager/Configuration/RdsSnapshotManagerOptions.cs src/FantasyCritic.RdsSnapshotManager/Console/MainMenu.cs
git commit -m "Wire fail-fast config validation and remove dead flat RDS instance fields"
```

---

## Task 10: Full solution verification

**Files:** none (verification only).

- [ ] **Step 1: Build the whole solution**

Run: `dotnet build src/FantasyCritic.slnx`
Expected: `0 Error(s)`, `0 Warning(s)` (warnings are errors in this solution).

- [ ] **Step 2: Run the full unit test suite**

Run: `dotnet test src/FantasyCritic.Test/FantasyCritic.Test.csproj`
Expected: all tests PASS, including the new `RdsSnapshotManagerOptionsValidatorTests`, `RdsInstanceLookupTests`, and the updated `LocalDatabaseConnectionGuardTests`.

- [ ] **Step 3: Check formatting**

Run: `scripts/Format.ps1 -Check`
Expected: no formatting violations. If it reports issues, run `scripts/Format.ps1` (without `-Check`) to apply fixes, then re-run Step 1 and 2.

- [ ] **Step 4: Manual smoke check of the menu (optional, no automated test)**

If you have Docker MySQL running (`docker compose -f infrastructure/docker-compose-mysql.yaml up`) and valid local user secrets configured per the note at the top of this plan, run:

```powershell
dotnet run --project src/FantasyCritic.RdsSnapshotManager/FantasyCritic.RdsSnapshotManager.csproj
```

Expected: the menu prints as:

```
RDS Snapshot Manager
1. Create snapshot
2. Restore snapshot to instance (sanitized)
3. Dump & publish raw backup (unsanitized)
4. Import local dump to Docker (sanitized)
5. Clean local Docker database
0. Exit
```

Selecting `1` or `3` should show an instance picker listing both `production` and `beta`; selecting `2` should show only `beta` as a destination candidate (since `production` has `enableWriteOperations: false`). Press `0` to exit without running any destructive action.

- [ ] **Step 5: Final commit (only if Step 3 made formatting changes)**

```bash
git add -A
git commit -m "Apply formatting fixes"
```
