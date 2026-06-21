# Dual local MySQL databases design

**Date:** 2026-06-20  
**Status:** Approved

## Problem

Local development currently uses two Docker Compose files that target the same MySQL container, port, volume, and database name (`fantasycritic`):

| Compose file | Services | Purpose |
|--------------|----------|---------|
| `docker-compose-mysql.yaml` | MySQL + DatabaseUpdater + LocalDatabaseTool | Seeded DB for integration tests and default local dev |
| `docker-compose-mysql-snapshot.yaml` | MySQL only | Import production/beta dumps without re-seeding |

Because both compose files share one volume and one database name, you cannot keep a stable seeded test database and a full snapshot database at the same time. Switching modes is destructive and awkward.

## Goal

One Docker Compose stack, one MySQL instance, two databases:

| Database | Purpose | Populated by |
|----------|---------|--------------|
| `fantasycritic` | Integration tests and default local dev | `database-updater` + `local-database-tool` (unchanged) |
| `fantasycritic-fromsnapshot` | Full production-like data for manual dev | `RdsSnapshotManager` import only |

Developers switch the Web app to the snapshot database via user secrets when desired. Integration tests remain pinned to `fantasycritic`.

## Non-goals

- Migration tooling for existing Docker volumes. Seeded data is disposable; snapshot data can be re-imported. Developers with stale volumes can recreate the volume and start fresh.
- A third local database or configurable snapshot database name.
- Running DbUp migrations against the snapshot database (dumps are schema-complete).

## Architecture

### Docker Compose

**Keep:** `infrastructure/docker-compose-mysql.yaml` (unchanged service layout).

**Remove:** `infrastructure/docker-compose-mysql-snapshot.yaml`.

The unified compose file continues to:

1. Start MySQL 8.4 on port `3307` (`fantasycritic-mysql`).
2. Run `database-updater` against `Database=fantasycritic`.
3. Run `local-database-tool` against `Database=fantasycritic`.

The snapshot database is never touched by updater or seeder services.

### Init SQL

Update `infrastructure/mysql-init-multiple-users.sql`:

```sql
CREATE DATABASE IF NOT EXISTS fantasycritic;
CREATE DATABASE IF NOT EXISTS `fantasycritic-fromsnapshot`;

-- Grant fantasycritic (existing)
GRANT ... ON fantasycritic.* TO 'fantasycritic'@'%';
GRANT ... ON fantasycritic.* TO 'fantasycritic-admin'@'%' WITH GRANT OPTION;

-- Mirror grants for snapshot database
GRANT ... ON `fantasycritic-fromsnapshot`.* TO 'fantasycritic'@'%';
GRANT ... ON `fantasycritic-fromsnapshot`.* TO 'fantasycritic-admin'@'%' WITH GRANT OPTION;
```

Backtick the hyphenated database name in DDL/grant statements.

Init SQL runs only on first container initialization (empty volume). No supplemental migration scripts.

### Connection strings

| Consumer | Database | Notes |
|----------|----------|-------|
| Integration tests (`FantasyCriticWebApplicationFactory`) | `fantasycritic` | Hardcoded — no change |
| Web app default (`appsettings.json`) | `fantasycritic` | No change |
| Web app snapshot dev (user secrets) | `fantasycritic-fromsnapshot` | Developer opt-in |
| `RdsSnapshotManager` import/clean | `fantasycritic-fromsnapshot` | Hardcoded constant — not config-driven |

Example Web user secret for snapshot dev:

```json
{
  "ConnectionStrings:DefaultConnection": "Server=localhost;Port=3307;Database=fantasycritic-fromsnapshot;Uid=fantasycritic;Pwd=afantasticpassword;SslMode=required;charset=utf8;"
}
```

## RdsSnapshotManager changes

### Hardcoded snapshot database name

Add a constant (e.g. `LocalSnapshotDatabaseNames.SnapshotDatabase = "fantasycritic-fromsnapshot"`) used by import and clean flows.

Import and clean **always** target this database. They derive the effective connection string by overriding `Database=` on the configured `localDocker.connectionString` (host, port, credentials still come from config).

### Safety guard

Extend `LocalDatabaseConnectionGuard` (used by import and clean):

- After parsing the effective connection string, **refuse** if `Database` equals `fantasycritic` (the seeded test database).
- Continue existing checks: localhost only, port `3307`, no remote markers, no match with beta/dump connection strings.

This makes it impossible to wipe seeded integration-test data via RdsSnapshotManager, even with a misconfigured connection string.

### LocalImportService

1. Build effective connection string → `fantasycritic-fromsnapshot`.
2. Run guard validation.
3. Check container health (unchanged).
4. Empty check uses the snapshot database name (not hardcoded `"fantasycritic"`).
5. Import via `mysql` client with snapshot database as target.
6. Scrub via `MySQLBetaCleaner` against snapshot connection string.

Dump compatibility: `MysqldumpRunner.DumpToGzipFile` dumps a single database without `--databases`, producing unqualified table DDL/DML. Piping into `mysql fantasycritic-fromsnapshot` loads into the correct schema.

### LocalDatabaseCleanService

Same guard + effective connection string pattern. Scrub runs only against `fantasycritic-fromsnapshot`.

### Configuration cleanup

Remove unused `localDocker.composeFile` from `LocalDockerOptions` and `appsettings.json` (the property is defined but never read; health checks use `containerName` only).

Update default `localDocker.connectionString` in `appsettings.json` to use `Database=fantasycritic-fromsnapshot` for clarity, though import/clean override the database name regardless.

### Documentation

Update `src/FantasyCritic.RdsSnapshotManager/README.md`:

- Replace snapshot compose instructions with `docker compose -f infrastructure/docker-compose-mysql.yaml up -d`.
- Document the two-database layout and Web user-secrets switch.
- Note that import/clean only touch `fantasycritic-fromsnapshot`.

Update root `README.md` if it references the snapshot compose file.

## Data flow

```
docker compose -f infrastructure/docker-compose-mysql.yaml up
  → mysql-init creates fantasycritic + fantasycritic-fromsnapshot
  → database-updater migrates fantasycritic
  → local-database-tool seeds fantasycritic

Integration tests / default Web dev
  → localhost:3307 / fantasycritic

RdsSnapshotManager → Import local dump
  → guard (must not be fantasycritic)
  → empty check on fantasycritic-fromsnapshot
  → mysql import → fantasycritic-fromsnapshot
  → scrub credentials

Web dev (user secrets)
  → localhost:3307 / fantasycritic-fromsnapshot
```

## Error handling

| Scenario | Behavior |
|----------|----------|
| Import into non-empty snapshot DB | Refuse unless `--force` (unchanged) |
| Connection string targets `fantasycritic` | Guard refuses before any mutation |
| Snapshot DB missing (stale volume) | Import fails at connection/empty check; developer recreates volume or creates DB manually |
| Container not healthy | Fail with existing health-check message |

## Testing

Update `LocalDatabaseConnectionGuardTests`:

- Local connection strings use `Database=fantasycritic-fromsnapshot`.
- Add test: guard rejects `Database=fantasycritic`.
- Add test: guard rejects seeded database even when other fields are valid.

Optional: unit test that import/clean build the effective connection string with the snapshot database name.

No integration test changes — tests continue using `fantasycritic`.

## Files to change

| File | Change |
|------|--------|
| `infrastructure/mysql-init-multiple-users.sql` | Create snapshot DB; mirror grants |
| `infrastructure/docker-compose-mysql-snapshot.yaml` | **Delete** |
| `src/FantasyCritic.RdsSnapshotManager/Infrastructure/LocalDatabaseConnectionGuard.cs` | Reject seeded DB name |
| `src/FantasyCritic.RdsSnapshotManager/Services/LocalImportService.cs` | Use snapshot DB constant |
| `src/FantasyCritic.RdsSnapshotManager/Services/LocalDatabaseCleanService.cs` | Use snapshot DB constant |
| `src/FantasyCritic.RdsSnapshotManager/Configuration/RdsSnapshotManagerOptions.cs` | Remove `ComposeFile`; add snapshot DB constant |
| `src/FantasyCritic.RdsSnapshotManager/appsettings.json` | Update connection string; remove composeFile |
| `src/FantasyCritic.RdsSnapshotManager/README.md` | Unified compose; two-DB docs |
| `src/FantasyCritic.Test/LocalDatabaseConnectionGuardTests.cs` | Update + new rejection tests |
| `README.md` | Remove snapshot compose reference if present |

Historical specs/plans referencing `docker-compose-mysql-snapshot.yaml` are left as-is (they document what was built at the time).
