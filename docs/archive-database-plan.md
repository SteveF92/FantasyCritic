# ArchiveDatabase job plan

## Goal

A weekly copy of the production database, kept somewhere other than AWS, made by the job system.

The weekly pre-bids RDS snapshot stays as it is. It is the quick restore when bids go wrong. This archive is different: a
portable `mysqldump` file, kept forever, in a place that a compromised or lost AWS account cannot reach.

## Decisions

- **Job:** `ArchiveDatabase`. A cron handler with RunType `ManualOrCron`, so the admin console can also start it.
- **Schedule:** every Sunday at 02:00 Eastern, so Saturday night into Sunday, six hours after bids. It is a fixed time and
  is not tied to the action processing constants. It is unconditional: it runs whether or not bids processed.
- **Production only.** The beta cleaner turns it off (see step 3). Beta's own migration run needs a decision; see the
  open questions.
- **What is archived:** `mysqldump --single-transaction --routines --triggers`, gzipped. That is about 300 MB and under 3
  minutes, so other jobs are allowed to wait behind it.
- **Where it goes:** Google Cloud Storage and S3. Both go through one interface, `IDatabaseArchiveLocation`.
- **Who dumps:** a dedicated read-only MySQL user, `fantasycritic-backup`. Steve creates it by hand.
- **GCP sign-in:** Workload Identity Federation. GCP trusts the AWS role the host already runs as, so no key file exists.
- **GCP permissions:** create objects only, with no read, list, overwrite or delete.
- **Retention:** never expire. The bucket's default storage class is a cold one.
- **No encryption on our side.** GCS and S3 encrypt at rest.
- **The dump file:** goes to a local temporary file, which is deleted when the job ends.
- **The snapshot manager keeps its local-directory location** and its menu. It switches to the shared pieces.

## Where the code goes

| Piece | Home |
|---|---|
| `IDatabaseArchiveLocation` | Lib `Interfaces/` |
| `IDatabaseDumper` (dump the configured database to a `.sql.gz` file) | Lib `Interfaces/` |
| `ArchiveDatabaseJobHandler` | Lib `Jobs/Handlers/` |
| `DatabaseArchiveOptions` section | Lib `Configuration/` |
| `MysqldumpRunner` (moved whole: dump and import) and the `IDatabaseDumper` implementation | FantasyCritic.MySQL |
| `S3DatabaseArchiveLocation` | FantasyCritic.AWS |
| `GoogleCloudStorageDatabaseArchiveLocation` | **new** FantasyCritic.GCP, in the ExternalServices solution folder, owning `Google.Cloud.Storage.V1` |
| `AddFantasyCriticDatabaseArchive` registration | FantasyCritic.Hosting (references GCP) |
| `LocalDirectoryDatabaseArchiveLocation` | stays in RdsSnapshotManager |

`MysqldumpRunner` currently writes progress with `Console.WriteLine`. It changes to an `ILogger`, so the progress shows up
in the snapshot manager's console and in the worker's logs. The dump file name and the remote key stay as they are today:
`{RdsInstanceName}-{yyyy-MM-dd-HHmmss}.sql.gz` under `{prefix}{RdsInstanceName}/{yyyy-MM-dd}/`, using
`BackupRemoteKeyBuilder`. The worker already has `Aws:RdsInstanceName`.

Interface shape: `string Name { get; }` and `Task Upload(string localFilePath, string remoteKey, CancellationToken)`. The
existing `UploadAsync` loses its `Async` suffix. The handler uploads to every location even if one of them fails, then
returns a failure naming each location that failed. That way a GCS outage doesn't cost us the S3 copy, and the job still
ends in Error.

## Steps

Each step is built, tested and committed alone, then stops for review.

### 1. Shared archive pieces, with no behavior change

- Add `IDatabaseArchiveLocation` and `IDatabaseDumper` to Lib.
- Create the FantasyCritic.GCP project and move the GCS destination into it.
- Move the S3 destination to FantasyCritic.AWS, and move `MysqldumpRunner` to FantasyCritic.MySQL (Console output becomes
  logging).
- Point RdsSnapshotManager at the moved pieces. `BackupDestinationRegistration` and the factory keep working with the new
  interface. Update its README.
- Build the solution and run the existing `RdsSnapshot|BackupRemote|DatabaseEmpty` tests.

### 2. Configuration and registration

- Add a `DatabaseArchive` section to Lib/Configuration, required in Production:
  - `S3:Bucket`, `S3:Prefix`
  - `GoogleCloud:Bucket`, `GoogleCloud:Prefix`, `GoogleCloud:CredentialConfiguration`. The last one is the WIF credential
    configuration JSON. It holds no key material, but it lives in Secrets Manager with everything else that's per
    environment. When it's empty in Development, the location falls back to Application Default Credentials
    (`gcloud auth application-default login`).
- Add `ConnectionStrings:ArchiveConnection` (the `fantasycritic-backup` user) through a worker-specific connection strings
  record, so Web doesn't have to carry it.
- Add `AddFantasyCriticDatabaseArchive` in Hosting. It registers both locations as `IDatabaseArchiveLocation`, plus the
  dumper. Only the worker calls it.
- Update `WorkerOptions` and the worker's `appsettings.json`. `HostOptionsTests` covers the new keys.
- Have the dumper pass `--ssl-mode=REQUIRED` and `--no-tablespaces`. The second one stops a dump from needing PROCESS.
  Move the password off the command line into a `--defaults-extra-file` temporary file.

### 3. The job

- Add `FantasyCriticJobType.ArchiveDatabase`, plus `ArchiveDatabaseJobHandler : IFantasyCriticCronJobHandler` with
  priority `Independent` and `Weekly(IsoDayOfWeek.Sunday, new LocalTime(2, 0))`. Register it with `ForCron`.
- Handler order: dump to `Path.GetTempPath()`, upload to each location with a job status line for each, and delete the
  file in a `finally` block.
- Add a migration, `Sequential/2026-10-03_000_archiveDatabase.sql`, inserting
  `('ArchiveDatabase', 'Archive Database', 'Database', 'Warning', 'ManualOrCron')`.
- Make `MySQLBetaCleaner` set its RunType to `Disabled`, next to the Patreon and automated-processing ones.
- Add schedule tests. Both US DST changes happen at 02:00 on a Sunday, so the tests pin what Cronos does on those dates:
  - In March, 02:00 doesn't exist. Expected: the job runs once, at 03:00.
  - In November, 02:00 exists twice. Expected: the job runs once.

### 4. Admin console trigger

- Add `ActionRunnerController.ArchiveDatabase()` → `EnqueueJob(FantasyCriticJobType.ArchiveDatabase)`, next to
  `SnapshotDatabase`.
- Add an "Archive Database" button next to "Snapshot Database" in `adminConsole.vue`, and add the type to the filter list
  in `recentJobsTable.vue`.
- Rebuild Web and run `scripts/Regenerate-ApiClient.ps1`.

### 5. Worker image

- Install a MySQL client in the worker Dockerfile, in the cached layer with curl. First check which distro
  `aspnet:10.0` is:
  - **Ubuntu 24.04 (the .NET 10 default):** use its `mysql-client-8.0` package. That's Oracle's MySQL, not MariaDB.
  - **Debian:** use Oracle's APT repository, because Debian's `default-mysql-client` is MariaDB's, which rejects
    `--set-gtid-purged`.
- Add `COPY` lines for the GCP csproj to the worker Dockerfile.
- Run the job against Docker MySQL from inside the built image, to confirm `mysqldump` exists and runs there.

### 6. Local run, then production

- Run the job locally against Docker MySQL, with a dev bucket on each side. Starting it from the admin console tests the
  manual path.
- Deploy. Run it once by hand in production, then check that both objects exist and that the GCS one has the cold storage
  class.
- Download the GCS copy and import it into `fantasycritic-fromsnapshot` with the snapshot manager (option 4). A backup
  that was never restored hasn't been tested.

## Steve's manual setup

### MySQL backup user (production RDS, as the admin user)

```sql
CREATE USER 'fantasycritic-backup'@'%' IDENTIFIED BY '<password>' REQUIRE SSL;
GRANT SELECT, SHOW VIEW, TRIGGER ON `fantasycritic`.* TO 'fantasycritic-backup'@'%';
GRANT SHOW_ROUTINE ON *.* TO 'fantasycritic-backup'@'%';
```

`SHOW_ROUTINE` lets `--routines` read procedure bodies without global SELECT. If RDS refuses to grant it, let me know and
we'll find another way.

### AWS

- Choose the S3 bucket and prefix. The snapshot manager currently uses `fantasy-critic-beta` / `db-dumps/`.
- Grant `s3:PutObject` on that prefix to the role the production host runs as.
- Find that role's name, because the GCP side needs it.

### GCP (Workload Identity Federation)

I'll go through this with you step by step. Outline:

1. Create a project and a bucket. Set the default storage class to Archive, which is the cheapest at-rest class and fits
   "never read, never delete". Turn on uniform bucket-level access.
2. Create a service account, `fantasycritic-archiver`. On the bucket only, grant it `roles/storage.objectCreator`, which
   allows create but no read, list, overwrite or delete.
3. Create a workload identity pool, then an AWS provider in that pool for the AWS account ID.
4. Let the pool impersonate the service account, but only from the production host's role:
   `roles/iam.workloadIdentityUser` for
   `principalSet://iam.googleapis.com/projects/<number>/locations/global/workloadIdentityPools/<pool>/attribute.aws_role/arn:aws:sts::<account>:assumed-role/<role>`.
5. Generate the credential configuration with `--enable-imdsv2`:

   ```
   gcloud iam workload-identity-pools create-cred-config ... --aws --enable-imdsv2
   ```

   Put the JSON in Secrets Manager as `DatabaseArchive:GoogleCloud:CredentialConfiguration`.
6. Optional: a bucket retention policy, so that not even Steve can delete an archive early.

The worker container already reaches the instance metadata service for its AWS calls, so the Google library can sign in
the same way.

## Open questions

- **Beta before its next sync:** the migration runs on beta too, which would give beta's existing database a
  `ManualOrCron` row. Beta's worker would then try the job on Sunday and fail, because the archive config is required
  only in Production. Two options:
  - The migration inserts `Disabled`, and production is switched to `ManualOrCron` by hand once, after deploy.
  - The migration inserts `ManualOrCron`, and beta is re-synced or updated by hand once.

  The first fails safe.
- **S3 storage class:** Standard, or Glacier Deep Archive to match the never-read intent?
- **Email on failure:** a failed run ends in Error in the admin console and in Grafana. Should it also email Steve, the
  way `FullAutomatedActionsProcess` does?
