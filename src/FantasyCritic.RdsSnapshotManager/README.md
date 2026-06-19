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
| `betaConnectionString` | Beta RDS MySQL connection (for scrub after beta sync) |
| `dumpConnectionString` | MySQL connection used by mysqldump (typically admin user) |
| `localStagingDirectory` | Where `.sql.gz` dumps are written before upload |
| `destinations.s3.bucket` | S3 bucket for backup uploads |
| `destinations.googleCloud.credentialsPath` | Path to GCS service account JSON (optional) |

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

**Scrubbing policy:** Exports are full-fidelity. Scrubbing runs on **load** into beta RDS or local Docker only.

## Local Docker MySQL (snapshot import)

Use the minimal compose file (MySQL only — no database-updater or local-database-tool):

```powershell
docker compose -f infrastructure/docker-compose-mysql-snapshot.yaml up -d
```

The dump is assumed schema-complete; no post-import migration step is required.

## Manual test plan (owner only)

Do not run this tool in CI or automated agents against production AWS resources.

1. Create snapshot with auto name; verify in AWS console.
2. Create snapshot with custom name `manual-test-1`; verify validation rejects bad names.
3. Beta sync from latest snapshot; confirm beta DB reachable and non-beta users scrubbed.
4. Dump from beta; confirm `.sql.gz` in staging + S3 + local archive paths.
5. Enable GCS in config; confirm object appears in GCS bucket.
6. `docker compose -f infrastructure/docker-compose-mysql-snapshot.yaml up -d`
7. Import dump; confirm app can connect on port 3307 and users scrubbed.
8. Run import again without force; confirm refusal when DB has tables.

Optional unit tests:

```powershell
dotnet test src/FantasyCritic.Test/FantasyCritic.Test.csproj --filter "RdsSnapshot|BackupRemote|DatabaseEmpty"
```

## Phase 2 (not yet implemented)

Bulk migration worker for archiving many old RDS snapshots to deep storage via EC2.
