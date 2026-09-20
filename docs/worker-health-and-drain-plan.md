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
no DB access, so the worker image gets a CLI mode: `docker compose run --rm worker drain`.

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

## Step 7 — Worker CLI: `drain` / `resume`

`Main(string[] args)`. With a verb it builds the same container minus hosted services and
Kestrel, runs the command and returns an exit code. New `src/FantasyCritic.Worker/DeployCommands.cs`:
- `drain [--timeout-minutes 30]`: read the flag and remember it; set it off; wait one poll
  interval (covers a claim already in flight); poll `GetIncompleteJobs()` every 5s until nothing
  is Running/Cancelling. Exit **0** = drained and pulling was on before; **10** = drained and it
  was already off. On timeout: put the flag back as found, exit **1**.
- `resume`: set the flag on, exit 0.
- Try it locally: queue the database snapshot job, run
  `dotnet run --project src/FantasyCritic.Worker -- drain`, confirm it blocks until the job ends.

Commit → review.

## Step 8 — Deploy

- `deploy/deploy.sh`:
  - New **Drain** section after the image pull, before `"$MAINTENANCE" install` (`:294`), so a
    long wait costs no downtime. `compose run --rm worker drain`; exit 0 → `RESUME_WORKER=true`,
    10 → `false`, anything else → `exit 1` with the site untouched. Skipped entirely when
    `FC_SKIP_DRAIN=true`. An `EXIT` trap resumes the worker if the script dies between a
    successful drain and `compose down`.
  - After migrations, before `compose up -d` (`:331`): if `RESUME_WORKER=true`,
    `compose run --rm worker resume`.
  - Replace the `sleep 15` + `RestartCount` block (`:379-403`) with polling
    `docker inspect -f '{{.State.Health.Status}}'` on the worker until `healthy`, keeping the same
    WARNING block and `exit 1`. Bot: same probe, warning only.
  - `run` uses the *new* image against the *old* schema. If a future migration changes what drain
    reads it fails loudly before downtime, and `skip_drain` gets past it.
- `.github/workflows/deploy.yml`: `skip_drain` boolean input → `FC_SKIP_DRAIN`, threaded exactly
  like `skip_migrations` (`:25, :234, :255, :269, :350`). Widen the SSM poll window (240×10s =
  40 min) to cover a 30-min drain plus the deploy; `timeout-minutes: 90` already fits.
- **The first deploy of this feature must set `skip_drain: true`**: the column doesn't exist
  until that deploy's migration runs, and the old worker ignores the flag anyway.
- `bash -n deploy/deploy.sh`.

Commit → review.

## Step 9 — Docs

`docs/operations.md`: Health section (three services, `docker ps` health), worker off/on and what
`Draining` means, drain / `skip_drain` / first-deploy note, and that a stuck `Running` row now
also blocks a drain (troubleshooting row `:226`). `docs/deployment-phase-4b-plan.md`: note the
flag. Compose header comments.

Commit → review.

## Final verification (beta)

- `dotnet build src/FantasyCritic.slnx` zero warnings; unit tests; integration tests
  `-c Release`; `scripts/Format.ps1 -Check`.
- Deploy to beta once with `skip_drain: true`. Then deploy again while a long job is running:
  the run waits, the job ends `Complete` (not `CancelledInProgress`), the worker comes back
  `Running` in the admin monitor. Turn the worker off by hand, deploy, confirm it is still off.
