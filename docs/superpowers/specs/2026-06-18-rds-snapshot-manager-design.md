# RDS Snapshot Manager — Design Spec

**Date:** 2026-06-18  
**Status:** Approved

---

## Overview

Evolve `FantasyCritic.BetaSync` into **`FantasyCritic.RdsSnapshotManager`**: a guided console utility for managing Fantasy Critic production database snapshots and portable backups.

The tool supports:

1. Creating RDS snapshots of production
2. Refreshing the beta RDS instance from a snapshot (existing BetaSync behavior)
3. Logical mysqldump from a target RDS instance, staged locally, then uploaded to multiple destinations
4. Importing a local dump into Docker MySQL with PII scrubbing on load

Phase 2 (later) adds a long-running bulk migration worker on EC2 to archive many old RDS snapshots to deep storage.

**Explicitly out of scope:** RDS “Export to Amazon S3” (Parquet). Portable backups use mysqldump only.

**Data scrubbing policy:** Scrubbing runs on **load into a destination server** (beta RDS, local Docker), never on export. Off-AWS backup files retain full production fidelity.

---

## Goals and constraints

| Goal | Approach |
|------|----------|
| Portable off-AWS backups | Restore snapshot → mysqldump → `.sql.gz` (no native snapshot download) |
| Guided UX | Console menu/prompts in dedicated `Console/` classes |
| Future WinForms/WPF | Services have no console I/O; UI layer calls services only |
| Keep RDS snapshots after archive | Phase 2 uploads are copies; manual deletion in AWS console |
| Local Docker import safety | Import only if DB is empty; `--force` / explicit confirm to overwrite |

**Prerequisite:** `mysqldump` (and optionally `mysql` client) available on PATH for dump/import operations.

---

## Architecture

### Project rename

Rename `FantasyCritic.BetaSync` → `FantasyCritic.RdsSnapshotManager`. Update solution references, launch settings, and any docs that mention BetaSync.

### Layering (single project, separate namespaces/folders)

| Layer | Responsibility | Examples |
|-------|----------------|----------|
| `Console/` | Menus, prompts, selection UI, formatted output | `MainMenu`, `SnapshotPicker`, `InstancePicker` |
| `Services/` | Orchestration; no `Console` I/O | `BetaSyncService`, `DumpAndPublishService`, `LocalImportService`, `SnapshotCreateService` |
| `Destinations/` | Upload after local file exists | `IBackupDestination`, S3, GCS, local directory |
| `Infrastructure/` | External processes, thin wrappers | `MysqldumpRunner`, docker compose health checks |

### AWS consolidation

Move restore/rename/delete/wait logic from `RDSRefresher` into `FantasyCritic.AWS` (e.g. `RdsRestoreService` or methods on an expanded `RDSManager`). Both beta sync and Phase 2 bulk worker use the same code.

Extend existing `RDSManager.SnapshotRDS`:

```csharp
Task<string> SnapshotRDS(Instant snapshotTime, string? snapshotIdentifier = null)
```

- `snapshotIdentifier` **null** → preserve current auto-naming: `AdminSnap-{yyyy-MM-dd}-{random}` (keeps `AdminService` weekly snapshot behavior unchanged).
- **Custom name** → use after RDS identifier validation (lowercase alphanumeric + hyphens, unique, max 255 chars).
- Return the final snapshot identifier so callers can poll until `available`.

`GetRecentSnapshots()` remains on `IRDSManager` for listing and status polling.

### Existing code reuse

- `FantasyCritic.AWS.RDSManager` — snapshot create/list (extended as above)
- `MySQLBetaCleaner` — scrub on load into beta or local Docker
- `appsettings.json` + user secrets — connection strings, buckets, paths, credentials

---

## Phase 1 operations

### 1. Create snapshot

```
MainMenu → SnapshotCreateService
  → optional custom name prompt (default: auto-generated)
  → RDSManager.SnapshotRDS(now, customName)
  → poll GetRecentSnapshots until status = available (with timeout)
  → Console displays result
```

### 2. Beta sync

Same behavior as today; restructured:

```
MainMenu → SnapshotPicker → BetaSyncService
  → RdsRestoreService.CopySnapshotToInstance(productionSnapshot, betaInstance)
  → MySQLBetaCleaner on beta connection string
```

Restore flow (unchanged semantics):

- If beta instance exists: rename to `{name}-old`, restore snapshot to original name, delete old instance.
- If beta instance absent: restore snapshot directly to beta instance name.

### 3. Dump and publish

```
MainMenu → InstancePicker (prod, beta, or configured worker)
  → DumpAndPublishService
      → MysqldumpRunner → {localStagingDir}/{instance}-{timestamp}.sql.gz
      → foreach enabled IBackupDestination: UploadAsync(localFile, remoteKey)
  → Console shows local path and per-destination results
```

**Destinations (Phase 1):**

| Destination | Implementation |
|-------------|----------------|
| Local staging / archive directory | Always write dump locally; optional second copy to archive path |
| Amazon S3 | `Amazon.S3` via existing AWS project patterns |
| Google Cloud Storage | `Google.Cloud.Storage.V1` |

All destinations implement:

```csharp
interface IBackupDestination
{
    string Name { get; }
    Task UploadAsync(string localFilePath, string remoteKey, CancellationToken cancellationToken);
}
```

Each destination is independently enabled in config. If any **enabled** destination fails, the operation fails; the local staging file is retained for retry.

Remote key convention: `{prefix}{instance}/{yyyy-MM-dd}/{filename}.sql.gz`

### 4. Import to local Docker

```
MainMenu → pick local .sql.gz
  → LocalImportService
      → verify MySQL container healthy (docker compose against snapshot compose file)
      → check database empty (information_schema / sentinel query)
      → if not empty: abort unless user confirms force (console-only concern)
      → import (docker exec or host client → localhost:3307)
      → MySQLBetaCleaner on local connection string
```

**New compose file:** `infrastructure/docker-compose-mysql-snapshot.yaml`

- MySQL 8.4 only (no `database-updater`, no `local-database-tool`)
- Same port (`3307`) and credential pattern as `docker-compose-mysql.yaml` for consistency
- Dump is assumed schema-complete; no post-import migration step

---

## Configuration

**`appsettings.json`** (secrets in user secrets):

```json
{
  "productionRdsInstance": "fantasy-critic-rds",
  "betaRdsInstance": "fantasy-critic-beta-rds",
  "betaConnectionString": "...",
  "dumpConnectionString": "...",
  "localStagingDirectory": "...",
  "localDocker": {
    "composeFile": "infrastructure/docker-compose-mysql-snapshot.yaml",
    "connectionString": "Server=localhost;Port=3307;Database=fantasycritic;..."
  },
  "destinations": {
    "localDirectory": { "enabled": true, "path": "..." },
    "s3": { "enabled": true, "bucket": "fantasy-critic-beta", "prefix": "db-dumps/" },
    "googleCloud": { "enabled": true, "bucket": "...", "prefix": "db-dumps/" }
  }
}
```

GCS credentials via standard Google application-default / service-account env or config path (document in README; values in user secrets).

---

## Error handling

- Services prefer structured results (`Result` / `Result<T>`) over throwing for expected failures.
- Long operations log via Serilog; console shows high-level step progress.
- Dump/publish: partial upload failure leaves staging file on disk for manual retry.
- Import: if scrub fails after successful import, log prominently (DB loaded but not scrubbed).

---

## Phase 2: Bulk migration worker (future)

Non-interactive runner for EC2; same services, no console I/O.

**Config additions:** `workerRdsInstance`, snapshot age filter, manifest file path.

**`BulkMigrationService` loop:**

1. List manual snapshots older than configured age (or explicit list)
2. Restore snapshot to worker RDS instance
3. Run `DumpAndPublishService`
4. Append to manifest: `{ snapshotId, remoteKeys, timestamp, localPath }`
5. Tear down / reset worker instance for next snapshot
6. **Do not** delete RDS snapshot

Entry point: `--bulk` flag or dedicated runner class invoked from EC2.

**Phase 2 GUI:** WinForms preferred for solo Windows dev tool (buttons wired to existing services). Deferred until Phase 1 CLI is stable.

---

## Testing

| Area | Approach |
|------|----------|
| Snapshot name validation | Unit tests in `FantasyCritic.Test` |
| Empty-DB detection | Unit tests |
| Remote key path building | Unit tests |
| Manifest append | Unit tests |
| AWS / S3 / GCS | Mock `IRDSManager` and `IBackupDestination`; no live cloud in CI |

**Manual test plan (Phase 1):**

1. Create snapshot (auto name and custom name)
2. Beta sync from new snapshot; verify scrub on beta
3. Dump from beta; verify local file + S3 + GCS objects
4. Start empty Docker MySQL via snapshot compose; import dump; verify scrub
5. Confirm import refuses when DB is not empty unless forced

---

## `IRDSManager` interface change

Update `FantasyCritic.Lib.Interfaces.IRDSManager`:

```csharp
Task<string> SnapshotRDS(Instant snapshotTime, string? snapshotIdentifier = null);
Task<IReadOnlyList<DatabaseSnapshotInfo>> GetRecentSnapshots();
```

Update call sites:

- `AdminService.SnapshotDatabase()` — pass `null` for identifier (unchanged behavior)
- `AdminService` action-processing auto-snapshot — pass `null`
- `RdsSnapshotManager` — pass custom name when user provides one

---

## Decisions log

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Parquet S3 export | Rejected | User wants portable mysqldump backups, not analytics export |
| Scrub timing | On load only | Full-fidelity off-AWS archives |
| RDS snapshot after bulk upload | Keep | User deletes manually when satisfied |
| Docker import | Empty DB only (+ force) | Prevent accidental overwrite |
| Snapshot compose | Minimal MySQL only | Dump is complete; no updater/tools needed |
| UI approach | Guided console + service separation | Matches today; enables Phase 2 GUI |
| Phase 1 destinations | Local + S3 + GCS | Shared `IBackupDestination` interface |
