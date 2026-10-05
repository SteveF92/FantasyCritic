# Archive manual RDS snapshots to S3 as restorable dumps

## Context

The account has 76 manual RDS snapshots in us-east-1, close to the 100-snapshot quota. `predeploy-*` (from every deploy) and `adminsnap-*` snapshots keep adding to that count. Steve wants all 76 archived to S3 so he can delete them, and the archives must be **restorable**. AWS's native "Export to S3" writes Parquet files that can't be restored, so it's out. Each snapshot is restored to a temporary instance, dumped with mysqldump to `.sql.gz` (the same format the `ArchiveDatabase` job uses), uploaded, and the instance is deleted.

What `manual-snapshots.json` shows:
- 71 MySQL snapshots (5.7.21 → 8.4.8; 66 of them from `fantasy-critic-rds`), all unencrypted, ~20 GB allocated each.
- **5 PostgreSQL 9.x snapshots** (`chrippgsql` ×2, `trump-counter`, and two 2018 `fantasy-critic*` ones from before the MySQL move). These can't be mysqldumped, and RDS no longer restores Postgres 9.x as-is. The tool skips them and lists them, and Steve decides about them separately (upgrade the snapshot step by step and run pg_dump, or delete them).

This is the "Phase 2: bulk archive of old snapshots" that the RdsSnapshotManager README already anticipates, so it becomes a new menu option there. It is not a new project.

## Design

**New menu option 7, "Archive manual snapshots to S3".** It runs from Steve's machine with his AWS credentials, like options 1 to 3.

For each manual MySQL snapshot (all instances, paginated `DescribeDBSnapshots`, `SnapshotType=manual`, `Engine=mysql`), with **3 snapshots in flight at a time**:
1. **Skip if already archived.** Check with S3 `HeadObject` on the snapshot's `manifest.json` (see step 6). This makes the run resumable: a rerun picks up where it stopped. The schema names aren't known until the snapshot is restored, so the check can't use the dump keys.
2. **Restore** to a temporary instance `archive-<snapshotId>` (≤63 chars, trailing hyphens trimmed). It uses class `db.t3.micro` (supports 5.7 through 8.4) and gp3, is single-AZ, and is tagged `fc-purpose=snapshot-archive`. It takes the subnet group, security groups and `PubliclyAccessible` from the current default snapshot source instance (`fantasy-critic-rds`), which Steve's machine can already reach for option 3.
3. **Reset the master password** to a random one generated per run (`ModifyDBInstance`, `ApplyImmediately`), and wait for the instance to be `available`. Old snapshots carry whatever root password was in use at the time.
4. **List the schemas** (`SHOW DATABASES`, excluding `information_schema`, `mysql`, `performance_schema` and `sys`), and dump each one to the local staging directory with the existing `MysqldumpRunner`, adding `--column-statistics=0`. An 8.x mysqldump client fails against 5.7 servers without it.
5. **Check the dump is complete**: the gzip stream must end with mysqldump's `-- Dump completed` line.
6. **Upload** to the existing S3 destination under `db-dumps/rds-snapshots/<snapshotId>/<snapshotId>-<schema>.sql.gz`. Because that sits under the existing `db-dumps/` prefix, the Standard-IA upload class and the Glacier lifecycle rule already apply. Then delete the local file. After every dump is uploaded, upload `rds-snapshots/<snapshotId>/manifest.json`, which records the source instance, engine version, creation time, and each file's key and size. Its presence marks the snapshot as done.
7. **In a `finally` block**, delete the temporary instance with `SkipFinalSnapshot` and `DeleteAutomatedBackups` set.

At the end, the tool prints a table of snapshot → result (archived / skipped / failed + error) → S3 keys and sizes. Before the menu option starts, it lists any leftover instances tagged `fc-purpose=snapshot-archive` (for example after a Ctrl+C) and offers to delete them.

**The tool never deletes snapshots.** Steve deletes them himself after checking the summary.

## Steps (one commit each, stop for review between)

All of this work happens in a new git worktree on a new branch, `snapshot-archive`, created from `main`. The uncommitted royale changes in the main checkout stay where they are. `manual-snapshots.json` stays in the main checkout as reference and isn't committed.

1. **Plan doc.** Commit this plan as `docs/snapshot-archive-plan.md`.
2. **Building blocks** (shared projects):
   - `src/FantasyCritic.MySQL/MysqldumpRunner.cs`: an overload of `DumpToGzipFile` that takes extra mysqldump arguments. The existing signature is unchanged, so the worker's behavior doesn't change.
   - `src/FantasyCritic.AWS/S3DatabaseArchiveLocation.cs`: `Exists(key)` through `GetObjectMetadata`. It goes on the concrete class only; `IDatabaseArchiveLocation` is unchanged.
   - New `src/FantasyCritic.AWS/RdsTemporaryRestoreService.cs`: restores a snapshot to a temporary instance, resets the master password, waits for `available`, deletes the instance, and lists the tagged leftovers. It reuses the wait pattern from `RdsRestoreService`, which stays as it is.
   - Pure helpers for the temporary instance ID and the archive key, with unit tests in `src/FantasyCritic.Test/`.
3. **Snapshot manager option**: new `src/FantasyCritic.RdsSnapshotManager/Services/SnapshotArchiveService.cs` (returns `Result`, no `Async` suffix), menu option 7 in `Console/MainMenu.cs`, wiring in `Program.cs`, and a README update that replaces the "Phase 2" note.
4. **Trial run, then the full run** (Steve runs it; see Verification).

## Verification

- `dotnet build src/FantasyCritic.slnx` (zero warnings) and `dotnet test src/FantasyCritic.Test/FantasyCritic.Test.csproj`.
- Trial run with a temporary `--only <snapshotId>` prompt, a single snapshot at a time:
  - the oldest MySQL snapshot (`bids-2019-05-06`, 5.7.21) proves that RDS still restores old 5.7 minors and that the column-statistics flag works;
  - a recent one (`predeploy-20261003-021407-4e21fa3`, 8.4.8).
- Import the recent archive into local Docker with option 4 and check that the app runs against `fantasycritic-fromsnapshot`.
- Confirm in the console that no `archive-*` instances are left behind.
- Full run. 71 snapshots at about 15 to 20 minutes each, 3 at a time, should take around 6 to 8 hours. Rough cost: a few dollars of instance time, plus 5.7 Extended Support charges for the 5.7 restores and data transfer out for the dumps.
- Steve reviews the summary, then deletes the archived snapshots himself.

## Follow-up (not in this change)

- The count will climb back toward 100 unless `predeploy-*` snapshots get pruned, for example by having the deploy workflow keep only the last N.
- The 5 Postgres snapshots need Steve's decision.
