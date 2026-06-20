# Dual Local MySQL Databases — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Consolidate local MySQL into one Docker Compose stack with two databases — `fantasycritic` (seeded, for integration tests) and `fantasycritic-fromsnapshot` (import-only, for manual dev) — and harden `RdsSnapshotManager` so import/clean can never touch the seeded database.

**Architecture:** Init SQL creates both databases on first container start. `database-updater` and `local-database-tool` continue targeting `fantasycritic` only. `RdsSnapshotManager` derives a snapshot connection string (same host/port/credentials, hardcoded `Database=fantasycritic-fromsnapshot`) and validates it via an extended guard before import or scrub. Remove the redundant `docker-compose-mysql-snapshot.yaml`.

**Tech Stack:** Docker Compose, MySQL 8.4, C# / .NET 10, MySqlConnector, NUnit, CSharpFunctionalExtensions (`Result`).

**Spec:** `docs/superpowers/specs/2026-06-20-dual-local-database-design.md`

---

## File Map

| File | Action |
|------|--------|
| `src/FantasyCritic.RdsSnapshotManager/Configuration/LocalSnapshotDatabaseNames.cs` | **Create** — hardcoded database name constants |
| `src/FantasyCritic.RdsSnapshotManager/Infrastructure/LocalSnapshotConnectionString.cs` | **Create** — build effective snapshot connection string |
| `src/FantasyCritic.RdsSnapshotManager/Infrastructure/LocalDatabaseConnectionGuard.cs` | **Modify** — reject seeded database name |
| `src/FantasyCritic.RdsSnapshotManager/Services/LocalImportService.cs` | **Modify** — use snapshot connection string + guard |
| `src/FantasyCritic.RdsSnapshotManager/Services/LocalDatabaseCleanService.cs` | **Modify** — use snapshot connection string + guard |
| `src/FantasyCritic.RdsSnapshotManager/Program.cs` | **Modify** — wire snapshot connection string for local repos |
| `src/FantasyCritic.RdsSnapshotManager/Configuration/RdsSnapshotManagerOptions.cs` | **Modify** — remove unused `ComposeFile` |
| `src/FantasyCritic.RdsSnapshotManager/appsettings.json` | **Modify** — snapshot DB in connection string; remove `composeFile` |
| `src/FantasyCritic.Test/LocalDatabaseConnectionGuardTests.cs` | **Modify** — snapshot DB strings + rejection tests |
| `src/FantasyCritic.Test/LocalSnapshotConnectionStringTests.cs` | **Create** — connection string builder tests |
| `infrastructure/mysql-init-multiple-users.sql` | **Modify** — create snapshot DB; mirror grants |
| `infrastructure/docker-compose-mysql-snapshot.yaml` | **Delete** |
| `src/FantasyCritic.RdsSnapshotManager/README.md` | **Modify** — unified compose + two-DB docs |
| `README.md` | **Modify** — note second database exists for snapshot dev |
| `docs/superpowers/specs/2026-06-20-dual-local-database-design.md` | **Modify** — set status to Approved |

---

## Task 1: Snapshot database constants and connection string builder

**Files:**
- Create: `src/FantasyCritic.RdsSnapshotManager/Configuration/LocalSnapshotDatabaseNames.cs`
- Create: `src/FantasyCritic.RdsSnapshotManager/Infrastructure/LocalSnapshotConnectionString.cs`
- Create: `src/FantasyCritic.Test/LocalSnapshotConnectionStringTests.cs`

- [ ] **Step 1: Write the failing tests**

Create `src/FantasyCritic.Test/LocalSnapshotConnectionStringTests.cs`:

```csharp
using FantasyCritic.RdsSnapshotManager.Configuration;
using FantasyCritic.RdsSnapshotManager.Infrastructure;
using NUnit.Framework;

namespace FantasyCritic.Test;

[TestFixture]
public class LocalSnapshotConnectionStringTests
{
    private const string LocalDockerConnectionString =
        "Server=localhost;Port=3307;Database=fantasycritic;Uid=fantasycritic-admin;Pwd=secret;SslMode=Required;charset=utf8;";

    [Test]
    public void BuildSnapshotConnectionString_OverridesDatabaseName()
    {
        string result = LocalSnapshotConnectionString.BuildSnapshotConnectionString(LocalDockerConnectionString);

        Assert.That(result, Does.Contain($"Database={LocalSnapshotDatabaseNames.SnapshotDatabase}"));
        Assert.That(result, Does.Not.Contain("Database=fantasycritic;"));
    }

    [Test]
    public void BuildSnapshotConnectionString_PreservesHostPortAndCredentials()
    {
        string result = LocalSnapshotConnectionString.BuildSnapshotConnectionString(LocalDockerConnectionString);

        Assert.That(result, Does.Contain("Server=localhost"));
        Assert.That(result, Does.Contain("Port=3307"));
        Assert.That(result, Does.Contain("Uid=fantasycritic-admin"));
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run:

```powershell
dotnet test src/FantasyCritic.Test/FantasyCritic.Test.csproj --filter "LocalSnapshotConnectionStringTests" --no-restore
```

Expected: FAIL — types `LocalSnapshotConnectionString` / `LocalSnapshotDatabaseNames` not found.

- [ ] **Step 3: Write minimal implementation**

Create `src/FantasyCritic.RdsSnapshotManager/Configuration/LocalSnapshotDatabaseNames.cs`:

```csharp
namespace FantasyCritic.RdsSnapshotManager.Configuration;

public static class LocalSnapshotDatabaseNames
{
    public const string SeededDatabase = "fantasycritic";
    public const string SnapshotDatabase = "fantasycritic-fromsnapshot";
}
```

Create `src/FantasyCritic.RdsSnapshotManager/Infrastructure/LocalSnapshotConnectionString.cs`:

```csharp
using FantasyCritic.RdsSnapshotManager.Configuration;
using MySqlConnector;

namespace FantasyCritic.RdsSnapshotManager.Infrastructure;

public static class LocalSnapshotConnectionString
{
    public static string BuildSnapshotConnectionString(string localDockerConnectionString)
    {
        var builder = new MySqlConnectionStringBuilder(localDockerConnectionString)
        {
            Database = LocalSnapshotDatabaseNames.SnapshotDatabase
        };

        return builder.ConnectionString;
    }
}
```

- [ ] **Step 4: Run test to verify it passes**

Run:

```powershell
dotnet test src/FantasyCritic.Test/FantasyCritic.Test.csproj --filter "LocalSnapshotConnectionStringTests"
```

Expected: PASS (2 tests).

- [ ] **Step 5: Commit**

```powershell
git add src/FantasyCritic.RdsSnapshotManager/Configuration/LocalSnapshotDatabaseNames.cs src/FantasyCritic.RdsSnapshotManager/Infrastructure/LocalSnapshotConnectionString.cs src/FantasyCritic.Test/LocalSnapshotConnectionStringTests.cs
git commit -m "Add snapshot database connection string builder."
```

---

## Task 2: Extend LocalDatabaseConnectionGuard

**Files:**
- Modify: `src/FantasyCritic.RdsSnapshotManager/Infrastructure/LocalDatabaseConnectionGuard.cs`
- Modify: `src/FantasyCritic.Test/LocalDatabaseConnectionGuardTests.cs`

- [ ] **Step 1: Update tests first**

Replace `LocalConnectionString` in `src/FantasyCritic.Test/LocalDatabaseConnectionGuardTests.cs`:

```csharp
private const string LocalConnectionString =
    "Server=localhost;Port=3307;Database=fantasycritic-fromsnapshot;Uid=fantasycritic-admin;Pwd=secret;SslMode=Required;charset=utf8;";
```

Update the two `TestCase` strings in `ValidateForClean_AcceptsLocalhostVariants` to use `Database=fantasycritic-fromsnapshot`.

Add these tests at the end of the fixture:

```csharp
[Test]
public void ValidateForClean_RejectsSeededDatabaseName()
{
    var result = LocalDatabaseConnectionGuard.ValidateForClean(
        "Server=localhost;Port=3307;Database=fantasycritic;Uid=fantasycritic-admin;Pwd=secret;SslMode=Required;charset=utf8;",
        BetaConnectionString,
        DumpConnectionString);

    Assert.That(result.IsFailure, Is.True);
    Assert.That(result.Error, Does.Contain("fantasycritic"));
    Assert.That(result.Error, Does.Contain("seeded").IgnoreCase);
}

[Test]
public void ValidateForClean_AcceptsSnapshotDatabaseName()
{
    var result = LocalDatabaseConnectionGuard.ValidateForClean(
        LocalConnectionString,
        BetaConnectionString,
        DumpConnectionString);

    Assert.That(result.IsSuccess, Is.True);
}
```

- [ ] **Step 2: Run tests to verify new rejection test fails**

Run:

```powershell
dotnet test src/FantasyCritic.Test/FantasyCritic.Test.csproj --filter "LocalDatabaseConnectionGuardTests"
```

Expected: `ValidateForClean_RejectsSeededDatabaseName` FAIL (guard currently accepts `fantasycritic`).

- [ ] **Step 3: Implement guard rejection**

In `src/FantasyCritic.RdsSnapshotManager/Infrastructure/LocalDatabaseConnectionGuard.cs`, add at top:

```csharp
using FantasyCritic.RdsSnapshotManager.Configuration;
```

After parsing `builder` (after the port check block, before `return Result.Success()`), add:

```csharp
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
```

Rename the guard method to reflect it is used for import and clean — optional; keep `ValidateForClean` name to minimize churn (same method used by import after this task).

- [ ] **Step 4: Run tests to verify they pass**

Run:

```powershell
dotnet test src/FantasyCritic.Test/FantasyCritic.Test.csproj --filter "LocalDatabaseConnectionGuardTests"
```

Expected: PASS (all tests in fixture).

- [ ] **Step 5: Commit**

```powershell
git add src/FantasyCritic.RdsSnapshotManager/Infrastructure/LocalDatabaseConnectionGuard.cs src/FantasyCritic.Test/LocalDatabaseConnectionGuardTests.cs
git commit -m "Guard local import/clean against seeded database."
```

---

## Task 3: Wire snapshot connection string into import and clean services

**Files:**
- Modify: `src/FantasyCritic.RdsSnapshotManager/Services/LocalImportService.cs`
- Modify: `src/FantasyCritic.RdsSnapshotManager/Services/LocalDatabaseCleanService.cs`
- Modify: `src/FantasyCritic.RdsSnapshotManager/Program.cs`

- [ ] **Step 1: Update LocalImportService**

Replace the body of `Import` in `src/FantasyCritic.RdsSnapshotManager/Services/LocalImportService.cs`:

```csharp
    public async Task<Result> Import(string gzipFilePath, bool force, CancellationToken cancellationToken)
    {
        string snapshotConnectionString = LocalSnapshotConnectionString.BuildSnapshotConnectionString(
            _options.LocalDocker.ConnectionString);

        var guard = LocalDatabaseConnectionGuard.ValidateForClean(
            snapshotConnectionString,
            _options.BetaConnectionString,
            _options.DumpConnectionString);
        if (guard.IsFailure)
        {
            return guard;
        }

        var health = await _dockerHealthChecker.EnsureHealthy(_options.LocalDocker.ContainerName, cancellationToken);
        if (health.IsFailure)
        {
            return health;
        }

        if (!force)
        {
            var empty = await _emptyChecker.EnsureEmptyOrFailure(
                snapshotConnectionString,
                LocalSnapshotDatabaseNames.SnapshotDatabase,
                cancellationToken);
            if (empty.IsFailure)
            {
                return empty;
            }
        }

        var import = await _mysqldumpRunner.ImportGzipFile(snapshotConnectionString, gzipFilePath, cancellationToken);
        if (import.IsFailure)
        {
            return import;
        }

        var allUsers = await _localUserStore.GetAllUsers();
        var betaUsers = await _localUserStore.GetUsersInRoleAsync("BetaTester", cancellationToken);
        await _cleaner.CleanEmailsAndPasswords(allUsers, betaUsers);
        return Result.Success();
    }
```

Add usings:

```csharp
using FantasyCritic.RdsSnapshotManager.Configuration;
```

(`LocalSnapshotConnectionString` is in `Infrastructure`, already imported.)

- [ ] **Step 2: Update LocalDatabaseCleanService**

Replace the start of `Clean` in `src/FantasyCritic.RdsSnapshotManager/Services/LocalDatabaseCleanService.cs`:

```csharp
    public async Task<Result> Clean(CancellationToken cancellationToken)
    {
        string snapshotConnectionString = LocalSnapshotConnectionString.BuildSnapshotConnectionString(
            _options.LocalDocker.ConnectionString);

        var guard = LocalDatabaseConnectionGuard.ValidateForClean(
            snapshotConnectionString,
            _options.BetaConnectionString,
            _options.DumpConnectionString);
        if (guard.IsFailure)
        {
            return guard;
        }

        var health = await _dockerHealthChecker.EnsureHealthy(_options.LocalDocker.ContainerName, cancellationToken);
        ...
```

- [ ] **Step 3: Update Program.cs local wiring**

In `src/FantasyCritic.RdsSnapshotManager/Program.cs`, replace local repo setup:

```csharp
        string localSnapshotConnectionString = LocalSnapshotConnectionString.BuildSnapshotConnectionString(
            options.LocalDocker.ConnectionString);

        RepositoryConfiguration localRepoConfig = new RepositoryConfiguration(localSnapshotConnectionString, clock);
        MySQLFantasyCriticUserStore localUserStore = new MySQLFantasyCriticUserStore(localRepoConfig);
        MySQLBetaCleaner localCleaner = new MySQLBetaCleaner(localSnapshotConnectionString);
```

Add using:

```csharp
using FantasyCritic.RdsSnapshotManager.Infrastructure;
```

(if not already present via other imports).

- [ ] **Step 4: Build to verify compilation**

Run:

```powershell
dotnet build src/FantasyCritic.RdsSnapshotManager/FantasyCritic.RdsSnapshotManager.csproj
```

Expected: Build succeeded.

- [ ] **Step 5: Run unit tests**

Run:

```powershell
dotnet test src/FantasyCritic.Test/FantasyCritic.Test.csproj --filter "LocalDatabaseConnectionGuardTests|LocalSnapshotConnectionStringTests"
```

Expected: PASS.

- [ ] **Step 6: Commit**

```powershell
git add src/FantasyCritic.RdsSnapshotManager/Services/LocalImportService.cs src/FantasyCritic.RdsSnapshotManager/Services/LocalDatabaseCleanService.cs src/FantasyCritic.RdsSnapshotManager/Program.cs
git commit -m "Route RdsSnapshotManager import/clean to snapshot database."
```

---

## Task 4: Configuration cleanup

**Files:**
- Modify: `src/FantasyCritic.RdsSnapshotManager/Configuration/RdsSnapshotManagerOptions.cs`
- Modify: `src/FantasyCritic.RdsSnapshotManager/appsettings.json`

- [ ] **Step 1: Remove unused ComposeFile property**

In `src/FantasyCritic.RdsSnapshotManager/Configuration/RdsSnapshotManagerOptions.cs`, delete `ComposeFile` from `LocalDockerOptions`:

```csharp
public sealed class LocalDockerOptions
{
    public string ConnectionString { get; set; } = null!;
    public string ContainerName { get; set; } = "fantasycritic-mysql";
}
```

- [ ] **Step 2: Update appsettings.json**

Replace the `localDocker` section in `src/FantasyCritic.RdsSnapshotManager/appsettings.json`:

```json
  "localDocker": {
    "connectionString": "Server=localhost;Port=3307;Database=fantasycritic-fromsnapshot;Uid=fantasycritic-admin;Pwd=anotherfantasticpassword;SslMode=Required;charset=utf8;",
    "containerName": "fantasycritic-mysql"
  },
```

- [ ] **Step 3: Build**

Run:

```powershell
dotnet build src/FantasyCritic.RdsSnapshotManager/FantasyCritic.RdsSnapshotManager.csproj
```

Expected: Build succeeded.

- [ ] **Step 4: Commit**

```powershell
git add src/FantasyCritic.RdsSnapshotManager/Configuration/RdsSnapshotManagerOptions.cs src/FantasyCritic.RdsSnapshotManager/appsettings.json
git commit -m "Remove unused composeFile config from RdsSnapshotManager."
```

---

## Task 5: MySQL init SQL — second database

**Files:**
- Modify: `infrastructure/mysql-init-multiple-users.sql`

- [ ] **Step 1: Add snapshot database and grants**

Replace `infrastructure/mysql-init-multiple-users.sql` with:

```sql
-- Runs automatically on first container initialization.
-- Creates MySQL users required by the app and grants required privileges.

CREATE DATABASE IF NOT EXISTS fantasycritic;
CREATE DATABASE IF NOT EXISTS `fantasycritic-fromsnapshot`;

-- App user
CREATE USER IF NOT EXISTS 'fantasycritic'@'%' IDENTIFIED BY 'afantasticpassword';
ALTER USER 'fantasycritic'@'%' IDENTIFIED BY 'afantasticpassword';
-- Ensure privileges match the app's required capabilities (not "all").
REVOKE ALL PRIVILEGES, GRANT OPTION FROM 'fantasycritic'@'%';

-- App admin
CREATE USER IF NOT EXISTS 'fantasycritic-admin'@'%' IDENTIFIED BY 'anotherfantasticpassword';
ALTER USER 'fantasycritic-admin'@'%' IDENTIFIED BY 'anotherfantasticpassword';

-- fantasycritic: data access required by the app
GRANT EXECUTE, SELECT, SHOW VIEW, DELETE, INSERT, UPDATE
  ON fantasycritic.* TO 'fantasycritic'@'%';

GRANT EXECUTE, SELECT, SHOW VIEW, DELETE, INSERT, UPDATE
  ON `fantasycritic-fromsnapshot`.* TO 'fantasycritic'@'%';

-- fantasycritic-admin: full access on both local databases + minimal global read for tooling
-- WITH GRANT OPTION lets fantasycritic-admin run GRANT statements (e.g. in DbUp migrations).
GRANT ALL PRIVILEGES ON fantasycritic.* TO 'fantasycritic-admin'@'%' WITH GRANT OPTION;
GRANT ALL PRIVILEGES ON `fantasycritic-fromsnapshot`.* TO 'fantasycritic-admin'@'%' WITH GRANT OPTION;
GRANT SELECT, SHOW DATABASES, SHOW VIEW ON *.* TO 'fantasycritic-admin'@'%';

FLUSH PRIVILEGES;
```

- [ ] **Step 2: Commit**

```powershell
git add infrastructure/mysql-init-multiple-users.sql
git commit -m "Create fantasycritic-fromsnapshot database in MySQL init SQL."
```

---

## Task 6: Remove snapshot compose file

**Files:**
- Delete: `infrastructure/docker-compose-mysql-snapshot.yaml`

- [ ] **Step 1: Delete the file**

```powershell
Remove-Item infrastructure/docker-compose-mysql-snapshot.yaml
```

- [ ] **Step 2: Verify no code references remain**

Run:

```powershell
Select-String -Path . -Pattern "docker-compose-mysql-snapshot" -Recurse -Exclude @("*.md")
```

Expected: no matches outside `docs/` markdown history files.

- [ ] **Step 3: Commit**

```powershell
git add -A infrastructure/docker-compose-mysql-snapshot.yaml
git commit -m "Remove redundant docker-compose-mysql-snapshot.yaml."
```

---

## Task 7: Documentation

**Files:**
- Modify: `src/FantasyCritic.RdsSnapshotManager/README.md`
- Modify: `README.md`
- Modify: `docs/superpowers/specs/2026-06-20-dual-local-database-design.md`

- [ ] **Step 1: Update RdsSnapshotManager README**

In `src/FantasyCritic.RdsSnapshotManager/README.md`, replace the **Local Docker MySQL (snapshot import)** section with:

```markdown
## Local Docker MySQL (two databases)

One MySQL instance (`docker compose -f infrastructure/docker-compose-mysql.yaml up -d`) hosts two databases:

| Database | Purpose |
|----------|---------|
| `fantasycritic` | Seeded by DatabaseUpdater + LocalDatabaseTool — used by integration tests and default Web dev |
| `fantasycritic-fromsnapshot` | Import target for menu options 4 and 5 — never touched by migrations or seeding |

Import and clean **always** target `fantasycritic-fromsnapshot` and refuse the seeded `fantasycritic` database.

To develop against imported snapshot data, set Web user secrets:

```json
{
  "ConnectionStrings:DefaultConnection": "Server=localhost;Port=3307;Database=fantasycritic-fromsnapshot;Uid=fantasycritic;Pwd=afantasticpassword;SslMode=required;charset=utf8;"
}
```

The dump is assumed schema-complete; no post-import migration step is required.
```

Update manual test plan steps 6–7 to use `docker-compose-mysql.yaml` instead of `docker-compose-mysql-snapshot.yaml`.

- [ ] **Step 2: Update root README**

After the bullet list under **1. Start the database** in `README.md`, add:

```markdown
- Create both `fantasycritic` (seeded) and `fantasycritic-fromsnapshot` (empty until you import a dump) databases.
```

Add a short subsection after step 1:

```markdown
#### Optional: develop against a production snapshot

Import a dump with `FantasyCritic.RdsSnapshotManager` (menu option 4), then point `FantasyCritic.Web` user secrets at `Database=fantasycritic-fromsnapshot`. Integration tests always use the seeded `fantasycritic` database and are unaffected.
```

- [ ] **Step 3: Mark spec approved**

In `docs/superpowers/specs/2026-06-20-dual-local-database-design.md`, change:

```markdown
**Status:** Approved
```

- [ ] **Step 4: Commit**

```powershell
git add src/FantasyCritic.RdsSnapshotManager/README.md README.md docs/superpowers/specs/2026-06-20-dual-local-database-design.md
git commit -m "Document dual local MySQL database workflow."
```

---

## Task 8: Final verification

- [ ] **Step 1: Run full unit test filter**

```powershell
dotnet test src/FantasyCritic.Test/FantasyCritic.Test.csproj --filter "LocalDatabaseConnectionGuardTests|LocalSnapshotConnectionStringTests|DatabaseEmpty"
```

Expected: PASS.

- [ ] **Step 2: Build solution**

```powershell
dotnet build src/FantasyCritic.slnx
```

Expected: Build succeeded.

- [ ] **Step 3: Manual smoke test (owner)**

1. Recreate Docker volume if needed: `docker compose -f infrastructure/docker-compose-mysql.yaml down -v` then `up -d`.
2. Confirm both databases exist: `docker exec fantasycritic-mysql mysql -uroot -prootpassword --ssl-mode=REQUIRED -e "SHOW DATABASES;"`.
3. Run integration tests — should pass against seeded `fantasycritic`.
4. Import a dump via RdsSnapshotManager option 4 — should land in `fantasycritic-fromsnapshot`.
5. Switch Web user secrets to snapshot DB; confirm app loads real data.
6. Switch back to default connection string; confirm seeded dev DB still intact.

---

## Spec coverage checklist

| Spec requirement | Task |
|------------------|------|
| Two databases on one compose | Task 5, 6 |
| Remove snapshot compose | Task 6 |
| Hardcoded snapshot DB name | Task 1, 3 |
| Guard rejects seeded DB | Task 2 |
| Import/clean use snapshot DB | Task 3 |
| Remove composeFile config | Task 4 |
| Update RdsSnapshotManager README | Task 7 |
| Update root README | Task 7 |
| Unit test updates | Task 1, 2 |
| No integration test changes | (none required) |
| No migration scripts for existing volumes | (by design — non-goal) |
