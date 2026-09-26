# FantasyCritic.RdsSnapshotManager

Guided console utility for managing Fantasy Critic RDS snapshots and portable database backups.

## Prerequisites

- AWS credentials configured for RDS and S3 (default credential chain)
- `mysqldump` and `mysql` on PATH (MySQL client tools)
- Docker on PATH for local import health checks
- Google Cloud credentials if GCS uploads are enabled

## Configuration

Settings live in `appsettings.json`. Put secrets in user secrets (`UserSecretsId` in the csproj):

| Key | Purpose |
|-----|---------|
| `RdsInstances.<name>.InstanceName` | The RDS instance identifier, e.g. `fantasy-critic-rds` |
| `RdsInstances.<name>.ConnectionString` | MySQL connection to that instance, typically the admin user |
| `RdsInstances.<name>.EnableWriteOperations` | Whether it may ever be the destination of a restore and scrub |
| `RdsInstances.<name>.DefaultSnapshotSource` | Whose snapshots are browsed when restoring; exactly one instance |
| `LocalStagingDirectory` | Where `.sql.gz` dumps are written before upload |
| `LocalDocker.ConnectionString` | The local Docker MySQL that imports and cleans target |
| `LocalDocker.ContainerName` | The container whose health is checked first |
| `Destinations.LocalDirectory.Path` | Local archive for dumps, when `Enabled` |
| `Destinations.S3.Bucket` | S3 bucket for backup uploads, when `Enabled` |
| `Destinations.GoogleCloud.CredentialsPath` | Path to GCS service account JSON (optional) |

Connection strings and paths should be set via user secrets in local development, not committed to git.

## Running

```powershell
dotnet run --project src/FantasyCritic.RdsSnapshotManager/FantasyCritic.RdsSnapshotManager.csproj
```

## Menu operations

1. **Create production snapshot** — `CreateDBSnapshot` on production RDS; optional custom name or auto-generated `adminsnap-{date}-{n}`.
2. **Beta sync from snapshot** — Restore selected production snapshot to beta RDS, then scrub non-BetaTester users.
3. **Dump and publish from instance** — mysqldump to local staging, then upload to all enabled destinations (local archive, S3, GCS).
4. **Import local dump to Docker MySQL** — Import a staging `.sql.gz` into local Docker MySQL, then scrub. Refuses if the database already has tables unless you force.
5. **Clean local Docker database** — Run the scrub step only against the configured local Docker MySQL instance. Refuses remote hosts, non-3307 ports, or connection strings that match beta/dump settings. Requires confirmation.

**Scrubbing policy:** Exports are full-fidelity. Scrubbing runs on **load** into beta RDS or local Docker only. Option 5 is local Docker only and cannot target production or beta RDS. Scrubbing removes non-beta user credentials, external logins, most Discord config, and unprocessed pickup bids and drop requests in non-test leagues.

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

## Manual test plan (owner only)

Do not run this tool in CI or automated agents against production AWS resources.

1. Create snapshot with auto name; verify in AWS console.
2. Create snapshot with custom name `manual-test-1`; verify validation rejects bad names.
3. Beta sync from latest snapshot; confirm beta DB reachable and non-beta users scrubbed.
4. Dump from beta; confirm `.sql.gz` in staging + S3 + local archive paths.
5. Enable GCS in config; confirm object appears in GCS bucket.
6. `docker compose -f infrastructure/docker-compose-mysql.yaml up -d`
7. Import dump; confirm app can connect on port 3307 and users scrubbed.
8. Run import again without force; confirm refusal when DB has tables.
9. Run option 5 on the imported database; confirm scrub runs and refuses if `LocalDocker.ConnectionString` is pointed at a remote host.

Optional unit tests:

```powershell
dotnet test src/FantasyCritic.Test/FantasyCritic.Test.csproj --filter "RdsSnapshot|BackupRemote|DatabaseEmpty"
```

## Phase 2 (not yet implemented)

Bulk migration worker for archiving many old RDS snapshots to deep storage via EC2.
