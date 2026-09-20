# Worker/bot health, worker off/on, and drain-before-deploy

## Context

The job system is live on beta. Two gaps remain: the worker and Discord bot give no sign of
whether they are alive (neither listens on a port; `deploy.sh` infers worker health from
`RestartCount`), and a deploy's `compose down` kills whatever job is running. This adds health
endpoints + an admin monitor, a DB flag that stops the worker pulling jobs, and a drain step in
the deploy.

Decisions made with Steve:
- **Off/on is a DB flag the worker honours, not a container stop.** The web container is
  non-root with no Docker socket, so it cannot run `docker compose`. The worker container stays
  up and idles, which also lets the monitor tell "Off" from "Down".
- The flag is a new bit column **`WorkerShouldPullNewJobs` on `tbl_meta_systemwidesettings`**.
  No "who set it" tracking. It stays out of `BidTimesViewModel` and every non-admin view model.
- **No off/on for the Discord bot.** Bot gets a health check and a monitor row only.
- Scheduler **keeps queuing** cron rows while pulling is off; the backlog runs on resume.
- Drain timeout (30 min default) **aborts the deploy** before any downtime. `skip_drain` input is
  the escape hatch.
- After a deploy, **restore prior state**: `deploy.sh` remembers whether pulling was on before
  the drain and only turns it back on if it was.
- **Compose `healthcheck:` blocks using curl**, installed in the worker and bot images. The
  conventional approach, chosen over a bash `/dev/tcp` trick for being obvious to read.

With the idle approach, items 3, 4 and 5 are **one control**. "Turn off worker" = clear the flag;
the worker finishes its job and then sits idle. The monitor shows the derived state
`Running → Draining → Off`. There is no separate toggle and button.

**Item 6 answer:** GitHub Actions never calls the API. `deploy.yml` already runs
`deploy/deploy.sh` as root on the host via SSM, so the drain lives in that script. The host has
no DB access, so a dedicated `FantasyCritic.CommandLine` image supplies the commands:
`docker compose run --rm -T command-line worker-wait-idle`.

**Status: done.** Steps 1–9 are built, and the whole thing deployed to beta successfully on
2026-09-20, which was the first time `deploy.sh` ran end to end with the drain in it. Still
ahead: the first production deploy, which needs `skip_drain` (see Step 8).

## How we work through this

**Each step below ends the same way: build + relevant tests green, commit that step alone, then
STOP and review with Steve before starting the next step.** Do not run ahead. Feedback from a
review may change later steps; update this plan when it does.

## Step 0 — Commit the plan

Commit this plan as `docs/worker-health-and-drain-plan.md`. → review.

## Step 1 — The flag

- Migration `src/FantasyCritic.DatabaseUpdater/Scripts/Sequential/<next date>_000_workerShouldPullNewJobs.sql`:
  `ALTER TABLE tbl_meta_systemwidesettings ADD COLUMN WorkerShouldPullNewJobs …` matching the
  column type of `ActionProcessingMode`, `NOT NULL DEFAULT 1`.
- `Lib/Domain/SystemWideSettings.cs` gains the third bool; update
  `MySQL/Entities/SystemWideSettingsEntity.cs` and the manual construction in
  `MySQLCombinedDataRepo.cs:42`. `sp_getbasicdata` is `select *`, no change. No view model changes.
- `SetWorkerShouldPullNewJobs(bool)` mirroring `SetActionProcessingMode` end to end:
  `IFantasyCriticRepo.cs:139`, `MySQLFantasyCriticRepo.cs:3244`, `InterLeagueService.cs:177`.
  Reads go through the existing uncached `GetSystemWideSettings()`.
- `MySQLJobRepo.StartJob` (`:214`): add
  `AND EXISTS (SELECT 1 FROM tbl_meta_systemwidesettings WHERE WorkerShouldPullNewJobs = 1)` so
  "flag cleared" and "job claimed" cannot race.
- `Worker.GetNextQueuedJob` (`Worker.cs:95`): read the flag first; if off, return null (avoids a
  refused claim every 5s). `Scheduler.cs` untouched — it keeps queuing.
- Every time the runner declines because of the flag (each 5s poll while it is off), it logs at
  Information: "Intentionally not running new jobs because WorkerShouldPullNewJobs is FALSE."
  No transition tracking, no extra state. Lands in the `JobRunner` flow log (and Loki); that is
  ~720 lines an hour while off, which is the point — the log says plainly why nothing is running.
- Integration test `Tests/JobManager/WorkerPullFlagTests.cs`: flag off → `IJobRepo.StartJob`
  refuses a queued job; flag on → `JobTestHelpers.RunQueuedJobAsync` succeeds. Teardown always
  sets the flag back on.

Commit → review.

## Step 2 — Worker health endpoint

- `FantasyCritic.Worker.csproj`: `Microsoft.NET.Sdk.Worker` → `Microsoft.NET.Sdk.Web`.
  `Program.cs`: `Host.CreateApplicationBuilder` → `WebApplication.CreateBuilder(new
  WebApplicationOptions { EnvironmentName, ContentRootPath })`; the double logger,
  `ValidateOnBuild`, registrations and hosted services are unchanged.
- Shared, in `FantasyCritic.Hosting` (add `<FrameworkReference Include="Microsoft.AspNetCore.App" />`):
  `MapFantasyCriticHealth(this WebApplication)` → `GET /health` with a `ResponseWriter` emitting
  `ServiceHealthReport { Status, Description, Data }`. DTO in `Lib/SharedSerialization` (written by
  Hosting, read by Web).
- Singleton `WorkerStatus` (last successful runner poll `Instant`, last observed flag, in-flight
  job IDs), updated from `Worker.cs`. `WorkerHealthCheck : IHealthCheck`: Healthy when a job is in
  flight or the last poll is < 60s old, else Unhealthy — the runner loop catches and continues on
  DB errors, so a stale poll is the real signal. Data: pulling on/off, in-flight job, last poll.
- `launchSettings.json`: `http://localhost:5081`.
- Unit tests for the staleness logic with a fake clock.

Commit → review.

## Step 3 — Discord bot health endpoint

Same Sdk/`WebApplication` change to `FantasyCritic.DiscordBot`. `DiscordBotHealthCheck` injects
the existing `DiscordSocketClient` singleton (`Hosting/ServiceCollectionExtensions.cs:174`):
Healthy = `Connected`, Degraded = `Connecting`, else Unhealthy. Data: `ConnectionState`,
`Latency`. Local URL `http://localhost:5082`.

Commit → review.

## Step 4 — Compose healthchecks

- `src/FantasyCritic.Worker/Dockerfile` and `src/FantasyCritic.DiscordBot/Dockerfile`: install
  curl in the runtime image (`apt-get update && apt-get install -y --no-install-recommends curl
  && rm -rf /var/lib/apt/lists/*`), placed in the existing brief root section that creates the
  log directory, before dropping back to `$APP_UID`.
- In `infrastructure/docker-compose-production.yaml` and `docker-compose-complete.yaml`, on
  `worker` and `discord-bot`: `ASPNETCORE_URLS: http://+:8080` and

```yaml
healthcheck:
  test: ["CMD", "curl", "-fsS", "--max-time", "5", "http://localhost:8080/health"]
  interval: 15s
  timeout: 10s
  start_period: 30s
  retries: 3
```

ECS container health checks are the same in-container command mechanism (only the ALB probes a
URL, and only `web` will sit behind one), so this carries straight into the Phase 6 task
definitions, where an unhealthy worker task actually gets replaced. **No ports published** for
either; the web app reaches them by service name and `deploy.sh` reads health from
`docker inspect`. Verify with
`docker compose -f infrastructure/docker-compose-complete.yaml --profile worker up`: `docker ps`
shows `(healthy)`; stop MySQL and watch the worker go `(unhealthy)`.

Commit → review.

## Step 5 — Admin API (`AdminController`, `[Authorize("Admin")]`)

- `GET GetServiceMonitor` → `ServiceMonitorViewModel` with `[ProducesResponseType<T>]`:
  `CheckedAt`, `WorkerShouldPullNewJobs`, `WorkerState`, and a `ServiceHealthViewModel`
  (`Name, Status, Description, Details`) each for worker and bot. `WorkerState` is derived
  server-side so the Vue stays dumb: `Unreachable` / `Unhealthy` / `Running` / `Draining` (flag
  off and `GetIncompleteJobs()` has a Running/Cancelling row) / `Off`.
- `POST TurnOffWorker` / `POST TurnOnWorker`, same shape as
  `ActionRunnerController.TurnOnActionProcessingMode` (`:123-137`).
- `ServiceHealthClient` in Web, registered with `AddHttpClient<>` in `Web/HostingExtensions.cs`
  (typed-client pattern, `ServiceCollectionExtensions.cs:111`), 3s timeout. Refused connection or
  timeout → `Unreachable`; reporting that *is* the feature, so this catch is legitimate. URLs from
  config `ServiceHealth:WorkerUrl` / `:DiscordBotUrl`: localhost defaults in `appsettings.json`,
  `http://worker:8080` / `http://discord-bot:8080` via env on `web` in both compose files.
- Rebuild Web, `scripts/Regenerate-ApiClient.ps1`.
- Integration tests: TurnOff/TurnOn round-trip through the monitor; monitor returns
  `Unreachable` for both services (test host has no worker/bot) rather than a 500.

Commit → review.

## Step 6 — Admin console (`src/FantasyCritic.ClientAppVue2/src`)

- New `components/serviceMonitor.vue`, top of the right column in `views/adminConsole.vue:158`,
  `v-if="isAdmin"`. Plain `b-table` (`thClass: 'bg-primary'`), two rows (Worker, Discord Bot):
  status badge, state, details. `Refresh` button + "last refreshed" stamp like
  `recentJobsTable.vue`; **no timer**; loads once on `created()`.
- Left column: new `Worker` heading, two explicit `b-button`s — `Turn Off Worker`
  (`$bvModal.msgBoxConfirm` first) and `Turn On Worker` — via the existing `runAction`, then
  `this.$refs.serviceMonitor.refresh()`. Watching `Draining → Off` is clicking Refresh.
- Walk through it together in the browser with web + worker running locally.

Commit → review.

## Step 7 — `FantasyCritic.CommandLine`

**Changed during the build.** The first version put `drain` / `resume` verbs on the worker
image, with exit code 10 meaning "drained, but it was already off". It worked and was tested
(branch `first-drain-attempt`), but it made the worker a two-mode program that had to be
guarded against starting a second worker, and used an exit code as a side channel. Replaced
with a project and image of its own, run as a one-off container the way the migrator is.

Four commands, each doing one thing and exiting 0 or 1. A command's answer is the only thing
on standard output; logs go to standard error, so a script can capture the answer with `$(…)`:

- `worker-should-pull` — prints `true` or `false`.
- `worker-stop-pulling` / `worker-start-pulling` — set the flag.
- `worker-wait-idle [--timeout-minutes 30]` — waits one poll interval first (covers a claim
  already in flight), then polls `GetIncompleteJobs()` every 5s until nothing is Running or
  Cancelling. Exit 1 on timeout. It does not touch the flag: what to do then is the script's call.

It is `deploy.sh` that remembers whether pulling was on beforehand. The timeout uses a
`Stopwatch`, not `IClock`: it is real waiting, and the integration test host's clock is a fake.
`FantasyCriticLogging.CreateConfiguration` gained `consoleToStandardError` for this.

Commit → review.

## Step 8 — Deploy

- `deploy/deploy.sh`:
  - New **Drain** section after the image pull, before the maintenance page, so a long wait
    costs no downtime: `worker-should-pull`, then `worker-stop-pulling` if it was on, then
    `worker-wait-idle`. Any failure exits with the site untouched. Skipped entirely when
    `FC_SKIP_DRAIN=true`.
  - If the script turned pulling off, it turns it back on before the containers start, and an
    `EXIT` trap does the same on any exit in between — including a failed migration, so that a
    worker started by hand afterwards is not silently switched off. A worker that was already
    off is left off. A restore that fails on the normal path fails the deploy, after the site
    is back up.
  - The pull names both profiles (`migrate` and `tools`), or the command-line image would
    download inside the downtime window.
  - `IMAGE_TAG` in `.env` is now written just before `compose up`, not before the pull. The
    drain made "stopped early with the site untouched" a normal outcome, and `.env` naming a
    release that never went live would point a hand-typed `docker compose up -d` at new images
    on an unmigrated database. Until then the script's own compose commands read a copy of
    `.env` with the new tag, passed with `--env-file`. Not by exporting `IMAGE_TAG`: whether
    the shell environment beats `.env` has differed between compose versions (v2.5.1 says no).
  - The `sleep 15` + `RestartCount` block became `wait_for_healthy`, polling Docker's health
    status for two minutes and giving up early on a crash loop. Same WARNING block and
    `exit 1` for the worker; the bot gets the same check as a warning only.
  - The commands run from the *new* image against the *old* schema. If a future migration
    changes what they read, it fails loudly before downtime, and `skip_drain` gets past it.
- `.github/workflows/deploy.yml`: the fifth image build; `skip_drain` → `FC_SKIP_DRAIN`, threaded
  exactly like `skip_migrations`; the SSM poll window widened to an hour to match
  `executionTimeout`, and the job timeout raised to 120 minutes.
- `infrastructure/docker-compose-production.yaml`: the `command-line` service behind
  `profiles: [tools]`, so `up -d` never starts it.
- **The first deploy of the job system to an environment must set `skip_drain: true`**:
  `worker-wait-idle` reads `tbl_job`, which does not exist until that deploy's migration runs.
- Verified with a harness that sources the real Drain section and trap out of `deploy.sh` with
  the commands stubbed, across nine scenarios, plus the health helpers against real containers.
  The script as a whole first runs on a beta deploy.

Commit → review.

## Step 9 — Docs

`docs/operations.md`: Health section (three services, `docker ps` health), worker off/on and what
`Draining` means, drain / `skip_drain` / first-deploy note, and that a stuck `Running` row now
also blocks a drain (troubleshooting row `:226`). `docs/deployment-phase-4b-plan.md`: note the
flag. Compose header comments.

Commit → review.

## Smaller things that differ from the steps above

- **Step 2.** Adding the ASP.NET framework reference to `FantasyCritic.Hosting` made seven of
  its `Microsoft.Extensions.*` package references redundant (NU1510), so they were removed. Each
  health probe logged four lines, so `Microsoft.AspNetCore` is overridden to Warning in the
  worker's and the bot's logger only. The URL comes from `launchSettings.json` and
  `ASPNETCORE_URLS`, never `appsettings.json`: the config loader is added last and would
  override the compose value.
- **Step 3.** The bot had no `launchSettings.json`; it has one now, which makes `dotnet run`
  default to Development.
- **Step 4.** curl is installed in the Dockerfiles' `base` stage, not after the publish copy,
  so the layer is cached rather than rebuilt every release.
- **Step 5.** `WorkerState` and its derivation live in `Lib/Jobs` as a `TypeSafeEnum`, so they
  can be unit tested. The integration test host points the monitor at a port nothing listens
  on; otherwise its result depended on whether a developer's own worker was running.
- **Known and left alone.** Dapper maps a missing column to `false`, so new worker code against
  a database without the `WorkerShouldPullNewJobs` column reads as "turned off" rather than
  failing. A deploy always migrates first, so this only bites a local database that has not
  been migrated. Listing the columns in `GetSystemWideSettings` instead of `select *` would
  make it fail loudly.

## Final verification (beta)

- `dotnet build src/FantasyCritic.slnx` zero warnings; unit tests; integration tests
  `-c Release`; `scripts/Format.ps1 -Check`.
- Create the `fantasycritic-command-line` ECR repository and add it to the deploy role first.
- Deploy to beta. It already has `tbl_job`, so this one works without `skip_drain`; the first
  **production** deploy needs it. Check the run for "The worker is idle.", "Worker is healthy."
  and "Discord bot is healthy." — the last is the first time the bot's Connected state is seen
  for real.
- Deploy again while a long job is running: the run waits, the job ends `Complete` (not
  `CancelledInProgress`), and the worker comes back `Running` in the admin console.
- Turn the worker off by hand, deploy, and confirm it is still `Off` afterwards.
- In the admin console, turn the worker off during a long job and watch `Draining` become `Off`.
