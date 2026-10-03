# ArchiveDatabase job plan

## Goal

A weekly copy of the production database, kept somewhere other than AWS, made by the job system.

The weekly pre-bids RDS snapshot stays as it is. It is the quick restore when bids go wrong. This archive is different: a
portable `mysqldump` file, kept forever, in a place that a compromised or lost AWS account cannot reach.

## Status (2026-10-03)

Steps 1 to 5 are done, each in its own commit on `archive-database`. Step 6 is partly done: everything that can be
checked without the buckets has been checked locally (see step 6). What's left is Steve's manual setup below, then the
production run.

## Decisions

- **Job:** `ArchiveDatabase`. A cron handler with RunType `ManualOrCron`, so the admin console can also start it.
- **Schedule:** every Sunday at 03:00 Eastern, so Saturday night into Sunday, seven hours after bids. It is 03:00, not
  02:00, because both US daylight saving changes happen at 02:00 on a Sunday. It is a fixed time and is not tied to the
  action processing constants. It is unconditional: it runs whether or not bids processed.
- **Production only.** The migration inserts it as `ManualOrCron`, and the beta cleaner turns it off, the same way it
  does for Patreon and automated action processing. Beta always starts from a production backup.
- **On failure:** email Steve through `EmailSendingService.SendAdminNotification`, as `FullAutomatedActionsProcess` does.
  Nobody watches a 3 AM run.
- **What is archived:** `mysqldump --single-transaction --routines --triggers`, gzipped. That is about 300 MB and under 3
  minutes, so other jobs are allowed to wait behind it.
- **Where it goes:** Google Cloud Storage and S3. Both go through one interface, `IDatabaseArchiveLocation`.
- **Who dumps:** a dedicated read-only MySQL user, `fantasycritic-backup`. Steve creates it by hand.
- **GCP sign-in:** Workload Identity Federation. GCP trusts the AWS role the host already runs as, so no key file exists.
- **GCP permissions:** create objects only, with no read, list, overwrite or delete.
- **Retention:** never expire. Every object starts in an infrequent-access class, a lifecycle rule moves it to cold
  storage after 90 days, and it stays there permanently:
  - **GCS:** Nearline, then Coldline. Nearline is the bucket's default class.
  - **S3:** Standard-IA, set on upload, then Glacier Flexible Retrieval. An S3 lifecycle rule can't move an object to IA
    before it's 30 days old, so the upload sets IA itself.

  The lifecycle rules are bucket configuration that Steve sets up, not code.
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
- Have `S3DatabaseArchiveLocation` upload with storage class `STANDARD_IA`. The snapshot manager's S3 uploads get it
  too, which suits them as well.
- Add the `fantasycritic-backup` user's connection string. *As built:* it is `DatabaseArchive:ConnectionString`, not
  `ConnectionStrings:ArchiveConnection`. That keeps the section self-contained, and the shared `ConnectionStringsOptions`
  that `AddFantasyCriticCore` takes stays unchanged.
- Add `AddFantasyCriticDatabaseArchive` in Hosting. It registers both locations as `IDatabaseArchiveLocation`, plus the
  dumper. Only the worker calls it.
- Update `WorkerOptions` and the worker's `appsettings.json`. `HostOptionsTests` covers the new keys.
- Have the dumper pass `--ssl-mode=REQUIRED` and `--no-tablespaces`. The second one stops a dump from needing PROCESS.
  Move the password off the command line into a `--defaults-extra-file` temporary file. *As built:* the client tools also
  get `--ssl-mode` from the connection string's `SslMode`, not a fixed `REQUIRED`. An option file has no escape for a
  quote inside a quoted value, so the password is wrapped in whichever quote it doesn't contain. A password containing
  both kinds of quote is refused.
- *As built:* Hosting now references FantasyCritic.GCP, so all five Dockerfiles that restore Hosting copy the GCP csproj.
  That's every Dockerfile except the worker's, which was planned for step 5.
- *As built:* `IDatabaseDumper` has `InstanceName` (from `Aws:RdsInstanceName`), which names the dump files the way the
  snapshot manager names them. `BackupRemoteKeyBuilder.BuildFileName` builds that name for both.

### 3. The job

- Add `FantasyCriticJobType.ArchiveDatabase`, plus `ArchiveDatabaseJobHandler : IFantasyCriticCronJobHandler` with
  priority `Independent` and `Weekly(IsoDayOfWeek.Sunday, new LocalTime(3, 0))`. Register it with `ForCron`.
- Handler order: dump to `Path.GetTempPath()`, upload to each location with a job status line for each, and delete the
  file in a `finally` block.
- Email Steve whenever the job stops short: the dump fails, any upload fails, it throws, or it is cancelled. The email
  names the job ID and what failed. If the email itself fails, throw an `AggregateException` holding both errors, the
  same pattern as `FullAutomatedActionsProcessJobHandler.SendStoppedShortEmail`.
- Add a migration, `Sequential/2026-10-03_000_archiveDatabase.sql`, inserting
  `('ArchiveDatabase', 'Archive Database', 'Database', 'Warning', 'ManualOrCron')`.
- Add `DisableDatabaseArchive` to `MySQLBetaCleaner`, setting the job's RunType to `Disabled`. Put it next to
  `DisablePatreon` and `DisableAutomatedActionProcessing`, with a doc comment: a copy of production must not archive
  itself into production's archive locations, and beta doesn't have the configuration anyway.
- Add a schedule test for the next occurrence after a Saturday evening.

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
- *As built:* the image is Ubuntu 24.04, and its `mysql-client-core-8.0` (8.0.46) is all the job needs. Inside the
  image, as the app user, it dumped the local MySQL 8.4 server with the job's flags. If production RDS is on 8.4, an
  8.0 `mysqldump` still dumps it.

### 6. Local run, then production

- Run the job locally against Docker MySQL, with a dev bucket on each side. Starting it from the admin console tests the
  manual path.
  - *Done without buckets:* a throwaway MySQL 8.4 container held a copy of the local database. The real handler ran
    against it as a backup user made with exactly the grants below, with a password containing `"`, `#` and `\`. The
    locations were one that copies to a folder and one that throws. Results:
    - The migration applied cleanly with the real DatabaseUpdater.
    - The grants were enough for all 18 procedures.
    - The copying location got the archive under `fantasy-critic-rds/2026-10-03/…`, even though the other location
      failed.
    - The job returned a failure naming the throwing location, its status listed each step, and the failure email had
      the right subject and lines.
    - No temp files were left.
    - The archive restored into a fresh database with the same table, routine and user counts.
    - The beta cleaner set the job to `Disabled`.
  - *Still to do:* real S3 and GCS uploads. They need the setup below.
  - Running this branch's worker against the local Docker database needs the new migration applied first
    (`docker compose -f infrastructure/docker-compose-mysql.yaml up`). The worker refuses to start if a job type has no
    `tbl_job_type` row.
- Deploy. Run it once by hand in production, then check that both objects exist with the expected classes: Nearline in
  GCS, Standard-IA in S3.
- Re-sync beta after this deploy. Beta's current database gets the migration's `ManualOrCron` row, and only a sync runs
  the cleaner. Without one, beta's worker would try the job on Sunday, fail, and email Steve.
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
we'll find another way. These exact grants were verified locally on MySQL 8.4. The password may contain one kind of quote
character, but not both.

### Secrets Manager (production blob)

| Key | Value |
|---|---|
| `DatabaseArchive:ConnectionString` | The backup user, e.g. `Server=<rds host>;Database=fantasycritic;Uid=fantasycritic-backup;Pwd=<password>;SslMode=Required;` |
| `DatabaseArchive:S3:Bucket` | The S3 bucket |
| `DatabaseArchive:GoogleCloud:Bucket` | The GCS bucket |
| `DatabaseArchive:GoogleCloud:CredentialConfiguration` | The JSON from `create-cred-config` |

Both prefixes ship empty in the worker's `appsettings.json`, so archives land in `fantasy-critic-rds/<date>/` at the
bucket root, beside the snapshot manager's manual dumps. The S3 permission covers only that folder. The credential
configuration goes in the blob as an escaped JSON string, not a nested object. Production refuses to start without the
four keys, so they must be in Secrets Manager before this deploys.

### AWS

- Choose the S3 bucket and prefix. The snapshot manager currently uses `fantasy-critic-beta` / `db-dumps/`.
- Grant `s3:PutObject` on that prefix to the role the production host runs as, and nothing else: no get, list or delete.
- Add a lifecycle rule on the prefix: transition to Glacier Flexible Retrieval at 90 days, with no expiration.
- Turn on bucket versioning. PutObject alone can still overwrite an existing key, and versioning keeps the old copy if
  that ever happens.
- Find that role's name, because the GCP side needs it.

### GCP (Workload Identity Federation)

I'll go through this with you step by step. Outline:

1. Create a project and a bucket. Set the default storage class to Nearline and turn on uniform bucket-level access. Add
   a lifecycle rule: SetStorageClass to Coldline at age 90 days, with no delete rule.
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

