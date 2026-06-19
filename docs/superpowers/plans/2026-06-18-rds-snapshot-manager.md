# RDS Snapshot Manager — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Rename `FantasyCritic.BetaSync` to `FantasyCritic.RdsSnapshotManager` and deliver a guided console tool for creating RDS snapshots, beta sync with scrub-on-load, mysqldump publish (local + S3 + GCS), and local Docker import with scrub-on-load.

**Architecture:** Console classes (`Console/`) handle all stdin/stdout; services (`Services/`) orchestrate AWS RDS, mysqldump, and MySQL scrubbing with no console I/O; destinations (`Destinations/`) upload staged `.sql.gz` files via `IBackupDestination`. Restore logic moves from `RDSRefresher` into `FantasyCritic.AWS.RdsRestoreService`; snapshot creation extends existing `RDSManager`.

**Tech Stack:** C# / .NET 10, AWSSDK.RDS + AWSSDK.S3, Google.Cloud.Storage.V1, Serilog, NodaTime, CSharpFunctionalExtensions (`Result`), NUnit, external `mysqldump`/`mysql`/`gzip` on PATH, Docker Compose.

**Out of scope (Phase 2):** Bulk migration worker, WinForms/WPF UI, Parquet export, RDS snapshot deletion after upload.

---

## Ground rules (agents)

This tool connects to **production AWS resources** (RDS snapshots, S3, etc.) and can mutate live infrastructure. The project owner runs **all** runtime and integration testing manually.

**Do NOT:**

- Run `FantasyCritic.RdsSnapshotManager` (`dotnet run` on this project)
- Execute any step that connects to AWS (RDS, S3, GCS), a live MySQL/RDS instance, or Docker for dump/import
- Run `docker compose up`, `mysqldump`, or `mysql` against real databases as part of implementation
- Perform the manual test plan at the bottom of this document — that checklist is for the owner only

**DO:**

- Write code task-by-task following this plan
- Run `dotnet build` (solution or affected projects) to verify compilation after each task
- Write unit tests as specified; the owner runs `dotnet test` when they choose
- Stop after each task (or logical checkpoint) so the owner can review before continuing

When a task step says "run" or "verify" in a way that would hit live infrastructure, **skip execution** and note in the commit message or handoff that the owner should verify manually.

---

## File Map

| File | Action |
|------|--------|
| `src/FantasyCritic.Lib/Utilities/RdsSnapshotIdentifierValidator.cs` | **Create** — RDS snapshot name validation |
| `src/FantasyCritic.Lib/Utilities/BackupRemoteKeyBuilder.cs` | **Create** — S3/GCS key path builder |
| `src/FantasyCritic.Lib/Interfaces/IRDSManager.cs` | **Modify** — `SnapshotRDS` returns `Task<string>`, optional custom name |
| `src/FantasyCritic.AWS/RDSManager.cs` | **Modify** — custom name + return identifier |
| `src/FantasyCritic.AWS/RdsRestoreService.cs` | **Create** — restore/rename/delete/wait (from `RDSRefresher`) |
| `src/FantasyCritic.Lib/Services/AdminService.cs` | **Modify** — adapt to new `SnapshotRDS` signature |
| `src/FantasyCritic.BetaSync/` | **Rename** → `src/FantasyCritic.RdsSnapshotManager/` |
| `src/FantasyCritic.RdsSnapshotManager/Configuration/RdsSnapshotManagerOptions.cs` | **Create** — strongly typed config |
| `src/FantasyCritic.RdsSnapshotManager/Destinations/IBackupDestination.cs` | **Create** |
| `src/FantasyCritic.RdsSnapshotManager/Destinations/LocalDirectoryDestination.cs` | **Create** |
| `src/FantasyCritic.RdsSnapshotManager/Destinations/S3BackupDestination.cs` | **Create** |
| `src/FantasyCritic.RdsSnapshotManager/Destinations/GoogleCloudStorageDestination.cs` | **Create** |
| `src/FantasyCritic.RdsSnapshotManager/Infrastructure/MysqldumpRunner.cs` | **Create** |
| `src/FantasyCritic.RdsSnapshotManager/Infrastructure/DatabaseEmptyChecker.cs` | **Create** |
| `src/FantasyCritic.RdsSnapshotManager/Infrastructure/DockerMySqlHealthChecker.cs` | **Create** |
| `src/FantasyCritic.RdsSnapshotManager/Services/SnapshotCreateService.cs` | **Create** |
| `src/FantasyCritic.RdsSnapshotManager/Services/BetaSyncService.cs` | **Create** |
| `src/FantasyCritic.RdsSnapshotManager/Services/DumpAndPublishService.cs` | **Create** |
| `src/FantasyCritic.RdsSnapshotManager/Services/LocalImportService.cs` | **Create** |
| `src/FantasyCritic.RdsSnapshotManager/Console/MainMenu.cs` | **Create** |
| `src/FantasyCritic.RdsSnapshotManager/Console/SnapshotPicker.cs` | **Create** |
| `src/FantasyCritic.RdsSnapshotManager/Console/InstancePicker.cs` | **Create** |
| `src/FantasyCritic.RdsSnapshotManager/Program.cs` | **Modify** — DI wiring + menu loop |
| `src/FantasyCritic.RdsSnapshotManager/appsettings.json` | **Modify** — expanded config schema |
| `src/FantasyCritic.RdsSnapshotManager/README.md` | **Create** — prerequisites + manual test steps |
| `infrastructure/docker-compose-mysql-snapshot.yaml` | **Create** — MySQL-only compose |
| `src/FantasyCritic.Test/RdsSnapshotIdentifierValidatorTests.cs` | **Create** |
| `src/FantasyCritic.Test/BackupRemoteKeyBuilderTests.cs` | **Create** |
| `src/FantasyCritic.Test/DatabaseEmptyCheckerTests.cs` | **Create** |
| `src/FantasyCritic.slnx` | **Modify** — project rename |

---

## Task 1: Snapshot identifier validation

**Files:**
- Create: `src/FantasyCritic.Lib/Utilities/RdsSnapshotIdentifierValidator.cs`
- Create: `src/FantasyCritic.Test/RdsSnapshotIdentifierValidatorTests.cs`

- [ ] **Step 1: Write the failing tests**

Create `src/FantasyCritic.Test/RdsSnapshotIdentifierValidatorTests.cs`:

```csharp
using FantasyCritic.Lib.Utilities;
using NUnit.Framework;

namespace FantasyCritic.Test;

[TestFixture]
public class RdsSnapshotIdentifierValidatorTests
{
    [TestCase("adminsnap-2026-06-18-a")]
    [TestCase("manual-backup-1")]
    public void Validate_AcceptsValidIdentifiers(string identifier)
    {
        var result = RdsSnapshotIdentifierValidator.Validate(identifier);
        Assert.That(result.IsSuccess, Is.True);
    }

    [TestCase("")]
    [TestCase("   ")]
    [TestCase("UPPERCASE-NOT-ALLOWED")]
    [TestCase("has spaces")]
    [TestCase("has_underscores")]
    public void Validate_RejectsInvalidIdentifiers(string identifier)
    {
        var result = RdsSnapshotIdentifierValidator.Validate(identifier);
        Assert.That(result.IsFailure, Is.True);
    }

    [Test]
    public void Validate_RejectsTooLongIdentifier()
    {
        var identifier = new string('a', 256);
        var result = RdsSnapshotIdentifierValidator.Validate(identifier);
        Assert.That(result.IsFailure, Is.True);
    }
}
```

- [ ] **Step 2: Verify tests (owner only — agents skip)**

Owner runs when ready:

```powershell
dotnet test src/FantasyCritic.Test/FantasyCritic.Test.csproj --filter "RdsSnapshotIdentifierValidatorTests"
```

Expected before Step 3: FAIL with `RdsSnapshotIdentifierValidator` not found.

- [ ] **Step 3: Implement validator**

Create `src/FantasyCritic.Lib/Utilities/RdsSnapshotIdentifierValidator.cs`:

```csharp
using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;

namespace FantasyCritic.Lib.Utilities;

public static partial class RdsSnapshotIdentifierValidator
{
    private const int MaxLength = 255;

    [GeneratedRegex("^[a-z][a-z0-9-]*$", RegexOptions.CultureInvariant)]
    private static partial Regex ValidIdentifierPattern();

    public static Result Validate(string snapshotIdentifier)
    {
        if (string.IsNullOrWhiteSpace(snapshotIdentifier))
        {
            return Result.Failure("Snapshot identifier is required.");
        }

        if (snapshotIdentifier.Length > MaxLength)
        {
            return Result.Failure($"Snapshot identifier must be at most {MaxLength} characters.");
        }

        if (!ValidIdentifierPattern().IsMatch(snapshotIdentifier))
        {
            return Result.Failure("Snapshot identifier must start with a letter and contain only lowercase letters, digits, and hyphens.");
        }

        return Result.Success();
    }
}
```

- [ ] **Step 4: Verify tests (owner only — agents skip)**

Owner runs when ready:

```powershell
dotnet test src/FantasyCritic.Test/FantasyCritic.Test.csproj --filter "RdsSnapshotIdentifierValidatorTests"
```

Expected: all tests PASS.

- [ ] **Step 5: Commit**

```powershell
git add src/FantasyCritic.Lib/Utilities/RdsSnapshotIdentifierValidator.cs src/FantasyCritic.Test/RdsSnapshotIdentifierValidatorTests.cs
git commit -m "Add RDS snapshot identifier validation."
```

---

## Task 2: Extend `IRDSManager.SnapshotRDS`

**Files:**
- Modify: `src/FantasyCritic.Lib/Interfaces/IRDSManager.cs`
- Modify: `src/FantasyCritic.AWS/RDSManager.cs`
- Modify: `src/FantasyCritic.Lib/Services/AdminService.cs`

- [ ] **Step 1: Update interface**

In `src/FantasyCritic.Lib/Interfaces/IRDSManager.cs`, change:

```csharp
Task SnapshotRDS(Instant snapshotTime);
```

to:

```csharp
Task<string> SnapshotRDS(Instant snapshotTime, string? snapshotIdentifier = null);
```

- [ ] **Step 2: Update `RDSManager`**

Replace `SnapshotRDS` in `src/FantasyCritic.AWS/RDSManager.cs`:

```csharp
public async Task<string> SnapshotRDS(Instant snapshotTime, string? snapshotIdentifier = null)
{
    using AmazonRDSClient rdsClient = new AmazonRDSClient();

    string snapName;
    if (snapshotIdentifier is null)
    {
        var date = snapshotTime.InZone(TimeExtensions.EasternTimeZone).LocalDateTime.Date;
        var dateString = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var random = Guid.NewGuid().ToString()[..1];
        snapName = "adminsnap-" + dateString + "-" + random;
    }
    else
    {
        var validation = RdsSnapshotIdentifierValidator.Validate(snapshotIdentifier);
        if (validation.IsFailure)
        {
            throw new InvalidOperationException(validation.Error);
        }

        snapName = snapshotIdentifier;
    }

    CreateDBSnapshotRequest request = new CreateDBSnapshotRequest(snapName, _instanceName);
    await rdsClient.CreateDBSnapshotAsync(request, CancellationToken.None);
    return snapName;
}
```

Add `using FantasyCritic.Lib.Utilities;`.

Note: auto-generated names now use lowercase `adminsnap-` to match RDS rules and existing snapshot naming in the console picker.

- [ ] **Step 3: Verify `AdminService` compiles**

`AdminService.SnapshotDatabase()` and the action-processing call already invoke `SnapshotRDS(now)` — no signature change needed at call sites beyond return type (both ignore return value).

- [ ] **Step 4: Build solution**

```powershell
dotnet build src/FantasyCritic.slnx
```

Expected: build succeeds with no warnings.

- [ ] **Step 5: Commit**

```powershell
git add src/FantasyCritic.Lib/Interfaces/IRDSManager.cs src/FantasyCritic.AWS/RDSManager.cs
git commit -m "Allow custom RDS snapshot names and return identifier."
```

---

## Task 3: Move restore logic to `RdsRestoreService`

**Files:**
- Create: `src/FantasyCritic.AWS/RdsRestoreService.cs`
- Delete (later in Task 4): `src/FantasyCritic.BetaSync/RDSRefresher.cs`

- [ ] **Step 1: Create `RdsRestoreService`**

Create `src/FantasyCritic.AWS/RdsRestoreService.cs` by moving the body of `RDSRefresher` with these changes:

- Class name: `RdsRestoreService`
- Namespace: `FantasyCritic.AWS`
- Replace interactive `SelectDBSnapshot` with a public method that accepts a snapshot identifier:

```csharp
public async Task CopySnapshotToInstance(string snapshotIdentifier, string destinationInstanceIdentifier)
```

- Keep private helpers: `RenameOldInstance`, `RestoreFromSnapshot`, `DeleteInstance`, `AssertDBInstanceByIdentifier`, `GetDBInstanceByIdentifier`, `WaitForDBToHaveName`, `WaitForDBToBeAvailable`
- Use `Serilog` for logging instead of `Console.WriteLine`
- Inject or construct `AmazonRDSClient` in the constructor (same as today)

Core public method logic (same semantics as today's `CopySourceToDestination`, but snapshot passed in):

```csharp
public async Task CopySnapshotToInstance(string snapshotIdentifier, string destinationInstanceIdentifier)
{
    Log.Information("Starting restore of {Snapshot} to {Destination}", snapshotIdentifier, destinationInstanceIdentifier);

    DBSnapshot snapshot = await AssertDBSnapshotByIdentifier(snapshotIdentifier);
    DBInstance? destinationDB = await GetDBInstanceByIdentifier(destinationInstanceIdentifier);

    if (destinationDB is not null)
    {
        string newNameForOldServer = await RenameOldInstance(destinationDB);
        await RestoreFromSnapshot(destinationDB, snapshot);
        DBInstance oldServer = await AssertDBInstanceByIdentifier(newNameForOldServer);
        await DeleteInstance(oldServer);
    }
    else
    {
        // Destination instance does not exist yet — restore using source instance metadata.
        DBInstance productionDB = await AssertDBInstanceByIdentifier(snapshot.DBInstanceIdentifier);
        await RestoreFromSnapshot(productionDB, snapshot, destinationInstanceIdentifier);
    }

    Log.Information("Restore complete.");
}
```

Add `AssertDBSnapshotByIdentifier`:

```csharp
private async Task<DBSnapshot> AssertDBSnapshotByIdentifier(string snapshotIdentifier)
{
    var response = await _rdsClient.DescribeDBSnapshotsAsync(new DescribeDBSnapshotsRequest
    {
        DBSnapshotIdentifier = snapshotIdentifier
    });

    DBSnapshot? snapshot = response.DBSnapshots.SingleOrDefault();
    if (snapshot is null)
    {
        throw new InvalidOperationException($"RDS snapshot not found: {snapshotIdentifier}");
    }

    return snapshot;
}
```

Copy the `else` branch from existing `RDSRefresher.CopySourceToDestination` verbatim — it uses `AssertDBInstanceByIdentifier(_sourceRdsName)` via `productionDB`.

- [ ] **Step 2: Build AWS project**

```powershell
dotnet build src/FantasyCritic.AWS/FantasyCritic.AWS.csproj
```

Expected: PASS.

- [ ] **Step 3: Commit**

```powershell
git add src/FantasyCritic.AWS/RdsRestoreService.cs
git commit -m "Extract RDS restore logic into RdsRestoreService."
```

---

## Task 4: Rename project to `FantasyCritic.RdsSnapshotManager`

**Files:**
- Rename: `src/FantasyCritic.BetaSync/` → `src/FantasyCritic.RdsSnapshotManager/`
- Modify: `src/FantasyCritic.slnx`
- Modify: `src/FantasyCritic.RdsSnapshotManager/FantasyCritic.RdsSnapshotManager.csproj`
- Modify: `src/FantasyCritic.RdsSnapshotManager/Program.cs` (namespace only for now)
- Delete: `src/FantasyCritic.RdsSnapshotManager/RDSRefresher.cs`

- [ ] **Step 1: Git rename**

```powershell
git mv src/FantasyCritic.BetaSync src/FantasyCritic.RdsSnapshotManager
git mv src/FantasyCritic.RdsSnapshotManager/FantasyCritic.BetaSync.csproj src/FantasyCritic.RdsSnapshotManager/FantasyCritic.RdsSnapshotManager.csproj
```

- [ ] **Step 2: Update csproj**

In `FantasyCritic.RdsSnapshotManager.csproj`, add package references:

```xml
<PackageReference Include="Google.Cloud.Storage.V1" Version="4.13.0" />
<PackageReference Include="MySqlConnector" Version="2.5.0" />
```

(`AWSSDK.RDS`/`S3` come transitively via `FantasyCritic.AWS`.)

- [ ] **Step 3: Update namespaces**

Replace `FantasyCritic.BetaSync` → `FantasyCritic.RdsSnapshotManager` in all files under the project.

- [ ] **Step 4: Update solution**

In `src/FantasyCritic.slnx`, change:

```xml
<Project Path="FantasyCritic.BetaSync/FantasyCritic.BetaSync.csproj" />
```

to:

```xml
<Project Path="FantasyCritic.RdsSnapshotManager/FantasyCritic.RdsSnapshotManager.csproj" />
```

- [ ] **Step 5: Delete `RDSRefresher.cs`**

```powershell
git rm src/FantasyCritic.RdsSnapshotManager/RDSRefresher.cs
```

- [ ] **Step 6: Build**

```powershell
dotnet build src/FantasyCritic.RdsSnapshotManager/FantasyCritic.RdsSnapshotManager.csproj
```

Expected: build error on `Program.cs` still referencing `RDSRefresher` — fixed in Task 10.

- [ ] **Step 7: Commit**

```powershell
git add -A
git commit -m "Rename BetaSync project to RdsSnapshotManager."
```

---

## Task 5: Configuration model

**Files:**
- Create: `src/FantasyCritic.RdsSnapshotManager/Configuration/RdsSnapshotManagerOptions.cs`
- Modify: `src/FantasyCritic.RdsSnapshotManager/appsettings.json`

- [ ] **Step 1: Create options classes**

Create `src/FantasyCritic.RdsSnapshotManager/Configuration/RdsSnapshotManagerOptions.cs`:

```csharp
namespace FantasyCritic.RdsSnapshotManager.Configuration;

public sealed class RdsSnapshotManagerOptions
{
    public string ProductionRdsInstance { get; set; } = null!;
    public string BetaRdsInstance { get; set; } = null!;
    public string BetaConnectionString { get; set; } = null!;
    public string DumpConnectionString { get; set; } = null!;
    public string LocalStagingDirectory { get; set; } = null!;
    public LocalDockerOptions LocalDocker { get; set; } = new();
    public DestinationOptions Destinations { get; set; } = new();
}

public sealed class LocalDockerOptions
{
    public string ComposeFile { get; set; } = null!;
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

- [ ] **Step 2: Update appsettings.json**

Replace contents of `appsettings.json` with structure matching the design spec (keep existing instance names; use placeholder paths):

```json
{
  "productionRdsInstance": "fantasy-critic-rds",
  "betaRdsInstance": "fantasy-critic-beta-rds",
  "betaConnectionString": "Server=localhost;Database=fantasycritic;Uid=fantasycritic;Pwd=secret;SslMode=Required;charset=utf8;",
  "dumpConnectionString": "Server=localhost;Database=fantasycritic;Uid=fantasycritic-admin;Pwd=secret;SslMode=Required;charset=utf8;",
  "localStagingDirectory": "C:/FantasyCritic/backups/staging",
  "localDocker": {
    "composeFile": "infrastructure/docker-compose-mysql-snapshot.yaml",
    "connectionString": "Server=localhost;Port=3307;Database=fantasycritic;Uid=fantasycritic-admin;Pwd=anotherfantasticpassword;SslMode=Required;charset=utf8;",
    "containerName": "fantasycritic-mysql"
  },
  "destinations": {
    "localDirectory": {
      "enabled": true,
      "path": "C:/FantasyCritic/backups/archive"
    },
    "s3": {
      "enabled": true,
      "bucket": "fantasy-critic-beta",
      "prefix": "db-dumps/"
    },
    "googleCloud": {
      "enabled": false,
      "bucket": "",
      "prefix": "db-dumps/",
      "credentialsPath": null
    }
  }
}
```

- [ ] **Step 3: Commit**

```powershell
git add src/FantasyCritic.RdsSnapshotManager/Configuration/RdsSnapshotManagerOptions.cs src/FantasyCritic.RdsSnapshotManager/appsettings.json
git commit -m "Add RdsSnapshotManager configuration model."
```

---

## Task 6: Backup remote key builder

**Files:**
- Create: `src/FantasyCritic.Lib/Utilities/BackupRemoteKeyBuilder.cs`
- Create: `src/FantasyCritic.Test/BackupRemoteKeyBuilderTests.cs`

- [ ] **Step 1: Write failing tests**

```csharp
using FantasyCritic.Lib.Utilities;
using NodaTime;
using NUnit.Framework;

namespace FantasyCritic.Test;

[TestFixture]
public class BackupRemoteKeyBuilderTests
{
    [Test]
    public void Build_IncludesPrefixInstanceDateAndFileName()
    {
        var instant = Instant.FromUtc(2026, 6, 18, 15, 30, 0);
        var key = BackupRemoteKeyBuilder.Build("db-dumps/", "fantasy-critic-beta-rds", instant, "fantasy-critic-beta-rds-2026-06-18.sql.gz");
        Assert.That(key, Is.EqualTo("db-dumps/fantasy-critic-beta-rds/2026-06-18/fantasy-critic-beta-rds-2026-06-18.sql.gz"));
    }

    [Test]
    public void Build_NormalizesMissingTrailingSlashOnPrefix()
    {
        var instant = Instant.FromUtc(2026, 6, 18, 15, 30, 0);
        var key = BackupRemoteKeyBuilder.Build("db-dumps", "prod", instant, "prod.sql.gz");
        Assert.That(key, Is.EqualTo("db-dumps/prod/2026-06-18/prod.sql.gz"));
    }
}
```

- [ ] **Step 2: Verify tests (owner only — agents skip)**

Owner runs when ready:

```powershell
dotnet test src/FantasyCritic.Test/FantasyCritic.Test.csproj --filter "BackupRemoteKeyBuilderTests"
```

Expected before Step 3: FAIL.

- [ ] **Step 3: Implement**

Create `src/FantasyCritic.Lib/Utilities/BackupRemoteKeyBuilder.cs`:

```csharp
using System.Globalization;
using FantasyCritic.Lib.Extensions;
using NodaTime;

namespace FantasyCritic.Lib.Utilities;

public static class BackupRemoteKeyBuilder
{
    public static string Build(string prefix, string instanceName, Instant timestamp, string fileName)
    {
        var normalizedPrefix = string.IsNullOrEmpty(prefix) ? string.Empty : prefix.EndsWith('/') ? prefix : prefix + "/";
        var date = timestamp.InZone(TimeExtensions.EasternTimeZone).Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        return $"{normalizedPrefix}{instanceName}/{date}/{fileName}";
    }
}
```

- [ ] **Step 4: Verify tests (owner only — agents skip)**

Owner runs when ready — expected: PASS.

- [ ] **Step 5: Commit**

```powershell
git add src/FantasyCritic.Lib/Utilities/BackupRemoteKeyBuilder.cs src/FantasyCritic.Test/BackupRemoteKeyBuilderTests.cs
git commit -m "Add backup remote key builder."
```

---

## Task 7: Backup destinations

**Files:**
- Create: `src/FantasyCritic.RdsSnapshotManager/Destinations/IBackupDestination.cs`
- Create: `src/FantasyCritic.RdsSnapshotManager/Destinations/LocalDirectoryDestination.cs`
- Create: `src/FantasyCritic.RdsSnapshotManager/Destinations/S3BackupDestination.cs`
- Create: `src/FantasyCritic.RdsSnapshotManager/Destinations/GoogleCloudStorageDestination.cs`
- Create: `src/FantasyCritic.RdsSnapshotManager/Destinations/BackupDestinationFactory.cs`

- [ ] **Step 1: Create interface**

```csharp
namespace FantasyCritic.RdsSnapshotManager.Destinations;

public interface IBackupDestination
{
    string Name { get; }
    Task UploadAsync(string localFilePath, string remoteKey, CancellationToken cancellationToken);
}
```

- [ ] **Step 2: Implement `LocalDirectoryDestination`**

```csharp
public sealed class LocalDirectoryDestination : IBackupDestination
{
    private readonly string _directoryPath;

    public LocalDirectoryDestination(string directoryPath)
    {
        _directoryPath = directoryPath;
    }

    public string Name => "LocalDirectory";

    public async Task UploadAsync(string localFilePath, string remoteKey, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_directoryPath);
        var fileName = Path.GetFileName(remoteKey);
        var destinationPath = Path.Combine(_directoryPath, fileName);
        await using var source = File.OpenRead(localFilePath);
        await using var destination = File.Create(destinationPath);
        await source.CopyToAsync(destination, cancellationToken);
    }
}
```

- [ ] **Step 3: Implement `S3BackupDestination`**

```csharp
using Amazon.S3;
using Amazon.S3.Transfer;

public sealed class S3BackupDestination : IBackupDestination
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucket;

    public S3BackupDestination(IAmazonS3 s3Client, string bucket)
    {
        _s3Client = s3Client;
        _bucket = bucket;
    }

    public string Name => "S3";

    public async Task UploadAsync(string localFilePath, string remoteKey, CancellationToken cancellationToken)
    {
        var transferUtility = new TransferUtility(_s3Client);
        await transferUtility.UploadAsync(localFilePath, _bucket, remoteKey, cancellationToken);
    }
}
```

- [ ] **Step 4: Implement `GoogleCloudStorageDestination`**

```csharp
using Google.Cloud.Storage.V1;

public sealed class GoogleCloudStorageDestination : IBackupDestination
{
    private readonly StorageClient _storageClient;
    private readonly string _bucket;

    public GoogleCloudStorageDestination(StorageClient storageClient, string bucket)
    {
        _storageClient = storageClient;
        _bucket = bucket;
    }

    public string Name => "GoogleCloud";

    public async Task UploadAsync(string localFilePath, string remoteKey, CancellationToken cancellationToken)
    {
        await using var fileStream = File.OpenRead(localFilePath);
        await _storageClient.UploadObjectAsync(_bucket, remoteKey, "application/gzip", fileStream, cancellationToken: cancellationToken);
    }
}
```

- [ ] **Step 5: Create factory**

Create `src/FantasyCritic.RdsSnapshotManager/Destinations/BackupDestinationFactory.cs`:

```csharp
using Amazon.S3;
using FantasyCritic.RdsSnapshotManager.Configuration;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;

namespace FantasyCritic.RdsSnapshotManager.Destinations;

public static class BackupDestinationFactory
{
    public static IReadOnlyList<IBackupDestination> CreateAll(RdsSnapshotManagerOptions options)
    {
        List<IBackupDestination> destinations = new();

        if (options.Destinations.LocalDirectory.Enabled)
        {
            destinations.Add(new LocalDirectoryDestination(options.Destinations.LocalDirectory.Path));
        }

        if (options.Destinations.S3.Enabled)
        {
            destinations.Add(new S3BackupDestination(new AmazonS3Client(), options.Destinations.S3.Bucket));
        }

        if (options.Destinations.GoogleCloud.Enabled)
        {
            StorageClient storageClient;
            if (!string.IsNullOrWhiteSpace(options.Destinations.GoogleCloud.CredentialsPath))
            {
                GoogleCredential credential = GoogleCredential.FromFile(options.Destinations.GoogleCloud.CredentialsPath);
                storageClient = StorageClient.Create(credential);
            }
            else
            {
                storageClient = StorageClient.Create();
            }

            destinations.Add(new GoogleCloudStorageDestination(storageClient, options.Destinations.GoogleCloud.Bucket));
        }

        return destinations;
    }
}
```

- [ ] **Step 6: Build project**

```powershell
dotnet build src/FantasyCritic.RdsSnapshotManager/FantasyCritic.RdsSnapshotManager.csproj
```

- [ ] **Step 7: Commit**

```powershell
git add src/FantasyCritic.RdsSnapshotManager/Destinations/
git commit -m "Add backup destination implementations."
```

---

## Task 8: Infrastructure helpers (mysqldump, empty DB, Docker health)

**Files:**
- Create: `src/FantasyCritic.RdsSnapshotManager/Infrastructure/MysqldumpRunner.cs`
- Create: `src/FantasyCritic.RdsSnapshotManager/Infrastructure/DatabaseEmptyChecker.cs`
- Create: `src/FantasyCritic.RdsSnapshotManager/Infrastructure/DockerMySqlHealthChecker.cs`
- Create: `src/FantasyCritic.Test/DatabaseEmptyCheckerTests.cs`

- [ ] **Step 1: Write failing test for empty checker SQL constant**

Create `src/FantasyCritic.Test/DatabaseEmptyCheckerTests.cs`:

```csharp
using FantasyCritic.RdsSnapshotManager.Infrastructure;
using NUnit.Framework;

namespace FantasyCritic.Test;

[TestFixture]
public class DatabaseEmptyCheckerTests
{
    [Test]
    public void EmptyDatabaseQuery_TargetsConfiguredSchema()
    {
        Assert.That(DatabaseEmptyChecker.BuildTableCountQuery("fantasycritic"), Does.Contain("fantasycritic"));
    }
}
```

Add project reference in `src/FantasyCritic.Test/FantasyCritic.Test.csproj`:

```xml
<ProjectReference Include="..\FantasyCritic.RdsSnapshotManager\FantasyCritic.RdsSnapshotManager.csproj" />
```

- [ ] **Step 2: Implement `DatabaseEmptyChecker`**

```csharp
using CSharpFunctionalExtensions;
using MySqlConnector;

namespace FantasyCritic.RdsSnapshotManager.Infrastructure;

public sealed class DatabaseEmptyChecker
{
    public static string BuildTableCountQuery(string schemaName) =>
        $"SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = '{schemaName}' AND table_type = 'BASE TABLE';";

    public async Task<Result> EnsureEmptyOrFailure(string connectionString, string schemaName, CancellationToken cancellationToken)
    {
        await using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new MySqlCommand(BuildTableCountQuery(schemaName), connection);
        var count = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
        return count == 0
            ? Result.Success()
            : Result.Failure($"Database '{schemaName}' is not empty ({count} tables).");
    }
}
```

- [ ] **Step 3: Implement `MysqldumpRunner`**

Create `src/FantasyCritic.RdsSnapshotManager/Infrastructure/MysqldumpRunner.cs`:

```csharp
using System.Diagnostics;
using CSharpFunctionalExtensions;
using MySqlConnector;

namespace FantasyCritic.RdsSnapshotManager.Infrastructure;

public sealed class MysqldumpRunner
{
    public async Task<Result<string>> DumpToGzipFile(string connectionString, string outputFilePath, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath)!);
        var builder = new MySqlConnectionStringBuilder(connectionString);

        var dumpArgs =
            $"--single-transaction --routines --triggers --set-gtid-purged=OFF " +
            $"-h {builder.Server} -P {builder.Port} -u {builder.UserID} " +
            $"--password={builder.Password} {builder.Database}";

        var command = $"mysqldump {dumpArgs} | gzip > \"{outputFilePath}\"";
        var exitCode = await RunShellCommandAsync(command, cancellationToken);
        return exitCode == 0
            ? Result.Success(outputFilePath)
            : Result.Failure<string>($"mysqldump failed with exit code {exitCode}.");
    }

    public async Task<Result> ImportGzipFile(string connectionString, string inputFilePath, CancellationToken cancellationToken)
    {
        var builder = new MySqlConnectionStringBuilder(connectionString);
        var importArgs = $"-h {builder.Server} -P {builder.Port} -u {builder.UserID} --password={builder.Password} {builder.Database}";
        var command = $"gzip -dc \"{inputFilePath}\" | mysql {importArgs}";
        var exitCode = await RunShellCommandAsync(command, cancellationToken);
        return exitCode == 0
            ? Result.Success()
            : Result.Failure($"mysql import failed with exit code {exitCode}.");
    }

    private static async Task<int> RunShellCommandAsync(string command, CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/c {command}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start shell process.");

        await process.WaitForExitAsync(cancellationToken);
        return process.ExitCode;
    }
}
```

Document in README that `mysqldump`, `mysql`, and `gzip` must be on PATH (Git for Windows ships all three).

- [ ] **Step 4: Implement `DockerMySqlHealthChecker`**

```csharp
using System.Diagnostics;
using CSharpFunctionalExtensions;

namespace FantasyCritic.RdsSnapshotManager.Infrastructure;

public sealed class DockerMySqlHealthChecker
{
    public async Task<Result> EnsureHealthy(string containerName, CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = $"inspect -f \"{{{{.State.Health.Status}}}}\" {containerName}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start docker inspect.");

        var status = (await process.StandardOutput.ReadToEndAsync(cancellationToken)).Trim();
        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            return Result.Failure($"docker inspect failed for container {containerName}.");
        }

        return status == "healthy"
            ? Result.Success()
            : Result.Failure($"Container {containerName} is not healthy (status: {status}).");
    }
}
```

- [ ] **Step 5: Build (agents); tests (owner only)**

```powershell
dotnet build src/FantasyCritic.RdsSnapshotManager/FantasyCritic.RdsSnapshotManager.csproj
```

Owner may run when ready:

```powershell
dotnet test src/FantasyCritic.Test/FantasyCritic.Test.csproj --filter "DatabaseEmptyCheckerTests"
```

- [ ] **Step 6: Commit**

```powershell
git add src/FantasyCritic.RdsSnapshotManager/Infrastructure/ src/FantasyCritic.Test/DatabaseEmptyCheckerTests.cs src/FantasyCritic.Test/FantasyCritic.Test.csproj
git commit -m "Add mysqldump, empty-DB, and Docker health helpers."
```

---

## Task 9: Services layer

**Files:**
- Create: `src/FantasyCritic.RdsSnapshotManager/Services/SnapshotCreateService.cs`
- Create: `src/FantasyCritic.RdsSnapshotManager/Services/BetaSyncService.cs`
- Create: `src/FantasyCritic.RdsSnapshotManager/Services/DumpAndPublishService.cs`
- Create: `src/FantasyCritic.RdsSnapshotManager/Services/LocalImportService.cs`

- [ ] **Step 1: `SnapshotCreateService`**

```csharp
public sealed class SnapshotCreateService
{
    private readonly IRDSManager _rdsManager;
    private readonly IClock _clock;

    public async Task<Result<string>> CreateSnapshot(string? customName, CancellationToken cancellationToken)
    {
        if (customName is not null)
        {
            var validation = RdsSnapshotIdentifierValidator.Validate(customName);
            if (validation.IsFailure) return Result.Failure<string>(validation.Error);
        }

        string snapshotId = await _rdsManager.SnapshotRDS(_clock.GetCurrentInstant(), customName);
        await WaitUntilAvailable(snapshotId, cancellationToken);
        return Result.Success(snapshotId);
    }

    private async Task WaitUntilAvailable(string snapshotId, CancellationToken cancellationToken)
    {
        var timeout = Duration.FromMinutes(30);
        var deadline = _clock.GetCurrentInstant().Plus(timeout);
        while (_clock.GetCurrentInstant() < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var snaps = await _rdsManager.GetRecentSnapshots();
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

- [ ] **Step 2: `BetaSyncService`**

```csharp
public sealed class BetaSyncService
{
    public async Task<Result> Sync(string snapshotIdentifier, CancellationToken cancellationToken)
    {
        await _restoreService.CopySnapshotToInstance(snapshotIdentifier, _options.BetaRdsInstance);
        var allUsers = await _userStore.GetAllUsers();
        var betaUsers = await _userStore.GetUsersInRoleAsync("BetaTester", cancellationToken);
        await _cleaner.CleanEmailsAndPasswords(allUsers, betaUsers);
        return Result.Success();
    }
}
```

Wire `RdsRestoreService`, `MySQLBetaCleaner`, `MySQLFantasyCriticUserStore` from config.

- [ ] **Step 3: `DumpAndPublishService`**

```csharp
public sealed class DumpAndPublishService
{
    public async Task<Result<string>> DumpAndPublish(string instanceName, CancellationToken cancellationToken)
    {
        var timestamp = _clock.GetCurrentInstant();
        var fileName = $"{instanceName}-{timestamp.InZone(TimeExtensions.EasternTimeZone):yyyy-MM-dd-HHmmss}.sql.gz";
        var stagingPath = Path.Combine(_options.LocalStagingDirectory, fileName);

        var dumpResult = await _mysqldumpRunner.DumpToGzipFile(_options.DumpConnectionString, stagingPath, cancellationToken);
        if (dumpResult.IsFailure) return Result.Failure<string>(dumpResult.Error);

        var remoteKey = BackupRemoteKeyBuilder.Build("db-dumps/", instanceName, timestamp, fileName);
        foreach (var destination in _destinations)
        {
            await destination.UploadAsync(stagingPath, remoteKey, cancellationToken);
        }

        return Result.Success(stagingPath);
    }
}
```

Use each destination's configured prefix when building keys (pass prefix from options per destination).

- [ ] **Step 4: `LocalImportService`**

```csharp
public sealed class LocalImportService
{
    public async Task<Result> Import(string gzipFilePath, bool force, CancellationToken cancellationToken)
    {
        var health = await _dockerHealthChecker.EnsureHealthy(_options.LocalDocker.ContainerName, cancellationToken);
        if (health.IsFailure) return health;

        if (!force)
        {
            var empty = await _emptyChecker.EnsureEmptyOrFailure(_options.LocalDocker.ConnectionString, "fantasycritic", cancellationToken);
            if (empty.IsFailure) return empty;
        }

        var import = await _mysqldumpRunner.ImportGzipFile(_options.LocalDocker.ConnectionString, gzipFilePath, cancellationToken);
        if (import.IsFailure) return import;

        var allUsers = await _localUserStore.GetAllUsers();
        var betaUsers = await _localUserStore.GetUsersInRoleAsync("BetaTester", cancellationToken);
        await _cleaner.CleanEmailsAndPasswords(allUsers, betaUsers);
        return Result.Success();
    }
}
```

- [ ] **Step 5: Build**

```powershell
dotnet build src/FantasyCritic.RdsSnapshotManager/FantasyCritic.RdsSnapshotManager.csproj
```

- [ ] **Step 6: Commit**

```powershell
git add src/FantasyCritic.RdsSnapshotManager/Services/
git commit -m "Add RdsSnapshotManager orchestration services."
```

---

## Task 10: Console UI layer

**Files:**
- Create: `src/FantasyCritic.RdsSnapshotManager/Console/MainMenu.cs`
- Create: `src/FantasyCritic.RdsSnapshotManager/Console/SnapshotPicker.cs`
- Create: `src/FantasyCritic.RdsSnapshotManager/Console/InstancePicker.cs`
- Modify: `src/FantasyCritic.RdsSnapshotManager/Program.cs`

- [ ] **Step 1: `SnapshotPicker`**

```csharp
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
```

- [ ] **Step 2: `InstancePicker`**

Offer configured instances (production, beta) from options; return selected instance name.

- [ ] **Step 3: `MainMenu`**

Menu loop (no AWS/MySQL calls inside picker classes beyond receiving data):

```
1. Create production snapshot
2. Beta sync from snapshot
3. Dump and publish from instance
4. Import local dump to Docker MySQL
0. Exit
```

For create snapshot: prompt optional custom name (blank = auto).

For import: list `.sql.gz` files in staging directory; if DB not empty, prompt `Force import? (y/N)`.

- [ ] **Step 4: Wire `Program.cs`**

Build configuration, options, services, destinations, `MainMenu.Run()` — remove old `RDSRefresher` usage entirely.

- [ ] **Step 5: Build only (do not run the program)**

```powershell
dotnet build src/FantasyCritic.RdsSnapshotManager/FantasyCritic.RdsSnapshotManager.csproj
```

Expected: build succeeds. **Owner** smoke-tests the menu locally when ready (`dotnet run`).

- [ ] **Step 6: Commit**

```powershell
git add src/FantasyCritic.RdsSnapshotManager/Console/ src/FantasyCritic.RdsSnapshotManager/Program.cs
git commit -m "Add guided console menu for RdsSnapshotManager."
```

---

## Task 11: Minimal Docker Compose for snapshot import

**Files:**
- Create: `infrastructure/docker-compose-mysql-snapshot.yaml`

- [ ] **Step 1: Create compose file**

Copy `docker-compose-mysql.yaml` but keep **only** the `mysql` service and `mysql_data` volume (lines 2–26 and 60–61 of the original). Use the same `container_name`, port `3307`, init SQL mount, and healthcheck.

- [ ] **Step 2: Document startup in README**

```powershell
docker compose -f infrastructure/docker-compose-mysql-snapshot.yaml up -d
```

- [ ] **Step 3: Commit**

```powershell
git add infrastructure/docker-compose-mysql-snapshot.yaml
git commit -m "Add minimal MySQL compose file for snapshot import."
```

---

## Task 12: README and owner verification

**Files:**
- Create: `src/FantasyCritic.RdsSnapshotManager/README.md`

- [ ] **Step 1: Write README**

Include:

- Prerequisites: AWS credentials, `mysqldump`, `mysql`, `gzip`, `docker` on PATH
- User secrets fields (`dumpConnectionString`, GCS credentials)
- Menu operation descriptions
- **Owner-only manual test checklist** (copy from bottom of this plan)
- Note: exports are **not** scrubbed; beta sync and Docker import **are** scrubbed
- Explicit note: agents do not run this tool; owner validates all AWS/DB behavior

- [ ] **Step 2: Full solution build (agents stop here)**

```powershell
dotnet build src/FantasyCritic.slnx
```

Expected: build succeeds. Owner runs `dotnet test` and the manual test plan when ready.

- [ ] **Step 3: Commit**

```powershell
git add src/FantasyCritic.RdsSnapshotManager/README.md
git commit -m "Document RdsSnapshotManager usage and manual test plan."
```

---

## Spec Coverage Checklist

| Spec requirement | Task |
|------------------|------|
| Rename to `FantasyCritic.RdsSnapshotManager` | Task 4 |
| Console/services separation | Tasks 9–10 |
| `SnapshotRDS` custom name + return value | Tasks 1–2 |
| Beta sync + scrub on load | Task 9 (`BetaSyncService`) |
| Dump → local staging → S3 + GCS + local archive | Tasks 6–7, 9 |
| Import to Docker, empty-only + force | Tasks 8–9, 11 |
| Minimal snapshot compose | Task 11 |
| No Parquet export | — (not implemented) |
| Phase 2 bulk worker | — (deferred) |
| Unit tests for validation/key paths/empty DB | Tasks 1, 6, 8 |

---

## Manual Test Plan (owner only — agents do not run)

1. Create snapshot with auto name; verify in AWS console.
2. Create snapshot with custom name `manual-test-1`; verify validation rejects bad names.
3. Beta sync from latest snapshot; confirm beta DB reachable and non-beta users scrubbed.
4. Dump from beta; confirm `.sql.gz` in staging + S3 + local archive paths.
5. Enable GCS in config; confirm object appears in GCS bucket.
6. `docker compose -f infrastructure/docker-compose-mysql-snapshot.yaml up -d`
7. Import dump; confirm app can connect on port 3307 and users scrubbed.
8. Run import again without force; confirm refusal when DB has tables.
