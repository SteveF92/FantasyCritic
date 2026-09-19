# Phase 4b plan: the job system

The plan for
[Phase 4b](deployment-modernization-roadmap.md#phase-4b-hangfire-worker), which the roadmap
sketched as "Hangfire worker". It is not Hangfire. This document records what replaced it and
why, the decisions already made, and what is left to build.

Written September 2026. The schema is committed; everything else is ahead of us.

---

## The short version

| Roadmap said | Plan is |
|---|---|
| Hangfire server in a new `FantasyCritic.Worker` | Cronos for cron parsing, our own runner, in a new `FantasyCritic.Worker` |
| Hangfire dashboard in Web, behind admin auth | The existing admin console, reading `tbl_job` |
| Hangfire recurring jobs replace `IScheduledTask` | Job types in a code registry, scheduled by Cronos in America/New_York |
| Quartz.NET as the fallback | Quartz considered and rejected; Cronos is a better cron than Quartz's cron |
| Budget time for master game cache invalidation | Not a real problem. See below. |

Two processes become three: **Web**, **DiscordBot**, **Worker**. The worker hosts three hosted
services — a scheduler, a runner and a canceller — coordinating through `tbl_job`.

---

## Why not Hangfire

The roadmap set a gate: *"Hangfire's MySQL storage provider is community maintained. Check its
release activity against the current Hangfire core."* It fails.

| | Latest commit | Latest package |
|---|---|---|
| Hangfire core | 2026-08-28 | v1.8.25, Aug 2026 |
| `Hangfire.MySqlStorage` (5.3M downloads, the default pick) | 2020-07-13 | 2.0.3 |
| `Hangfire.Storage.MySql` (the recommended fork) | 2021-08-18 | 2.1.0-**beta**, Nov 2020 |
| `Hangfire.MySql.Core` | — | deprecated |

Nothing newer exists on NuGet. The recommended fork's own README notes the author ended up
writing his own scheduler because of problems hosting Hangfire on MySQL.

Forking it was considered seriously and is mechanically feasible — it is ~24 files and ~105 KB
of C# depending on exactly our stack (Hangfire.Core, Dapper, MySqlConnector), and Hangfire 1.8
made new storage methods optional behind feature flags, so a 1.7-era adapter still runs. It was
rejected on judgement, not mechanics: that code exists to coordinate *many* workers safely, and
the worker runs at desired count 1 forever. Forking an orphaned distributed-coordination layer
to serialize a single consumer is the wrong trade, and storage correctness bugs present as lost
or double-run jobs rather than as exceptions.

## Why not Quartz.NET

Quartz is genuinely healthy — commit 2026-09-11, v4.0.1, and MySQL is first-party and CI-tested
in-repo (`tables_mysql_innodb.sql`, a `pr-integration-mysql.yml` workflow, a clustered
exactly-once MySQL test). It was the roadmap's fallback and it was close.

It lost on the thing this phase is actually about. Cronos's own compatibility table says of
Quartz: *"Cronos uses different, but more intuitive Daylight saving time handling logic."*
Quartz also wants 7-field expressions with different day-of-week numbering, so all nine existing
crons would need translating; it works in `DateTimeOffset` rather than NodaTime; it adds eleven
`QRTZ_` tables; and it has **no run history** — `qrtz_fired_triggers` is in-flight only — so the
`tbl_job` table and the admin console get built either way.

Given that we write the history and the UI regardless, the only question left was whose cron
loop runs. Cronos is the better cron, and the loop is about a hundred lines.

---

## The real problem this phase solves

The servers run UTC. The rules of Fantasy Critic are always America/New_York. The current
scheduler is not timezone-aware, so every time-of-day task is scheduled **hourly** and then
throws away 23 of 24 runs:

```csharp
public string Schedule => "0 */1 * * *";        // heartbeat, not a schedule
...
if (!IsTimeToNotify(now)) { return; }           // the real schedule, in NodaTime
```

The NodaTime logic is correct. The workaround around it is not, and it carries two live bugs —
neither about DST.

**Every task fires on every deploy.** `SchedulerHostedService` seeds `NextRunTime` from process
start, and `ShouldRun` is `NextRunTime < currentTime && LastRunTime != NextRunTime`. At startup
that is `startTime < now` (true) and `default(DateTime) != startTime` (true), and
`ExecuteOnceAsync` runs before the first delay. All nine tasks execute immediately on boot.

**A deploy during a task's slot silently skips it.** Same cause, other direction. Restart at
20:00:30 and `GetNextOccurrence(startTime)` returns 21:00 — the 20:00 occurrence is gone, the
±1 minute window is long past, and the weekly public-bidding post simply does not happen. No
error, because the task "succeeded" by returning early.

Both dissolve once the next occurrence is computed from the last enqueued slot in the database
instead of from process start.

### On DST specifically

Worth being honest about scope: every important time is 20:00 Eastern
(`PublicBiddingRevealTime`, `ActionProcessingTime`, `ReleasingThisWeekNewsTime`) or Eastern
midnight, and DST transitions happen at 02:00 local. Nothing important goes near the ambiguous
or skipped hour. What is needed is plain, trustworthy "20:00 in America/New_York, and the
library works out whether that is 00:00 or 01:00 UTC" — enough to delete every guard.

[Cronos](https://github.com/HangfireIO/Cronos) provides exactly that, and more than is needed:
time zones are first-class, local `DateTime` is **rejected with an exception** because it is
ambiguous during transitions, there are 1000+ tests aimed at this, and it is maintained by
HangfireIO (v0.13.0, commit 2026-09-10). Critically it is **NCrontab-compatible**, and the
vendored `Scheduling/Lib/Cron` folder *is* NCrontab — so existing expressions port unchanged and
845 lines of copied parser get deleted.

---

## Process topology

```
Web          existing site + one controller action per manually-runnable job
DiscordBot   command gateway (Phase 4a)
Worker       JobSchedulerHostedService  — Cronos decides what is due, INSERTs
             JobRunnerHostedService     — polls for Queued rows, executes them
             JobCancellerHostedService  — polls for Cancelling rows, resolves them
```

A separate `JobScheduler` container was considered and rejected. Both halves would sit at
desired count 1, so the split buys no independent scaling — the only reason the pattern exists
— while doubling the silent-failure surface in a system whose alerting is still a backlog item,
and doubling the per-deploy service ceremony.

The scheduler and the runner are nonetheless written as if they were separate processes:
**they communicate only through `tbl_job`, never in-process.** Splitting them later is moving one
`AddHostedService` line into a new `Program.cs`. The trigger for doing so would be wanting jobs to
run concurrently, and even that is better served by N worker loops inside the runner.

The canceller is the deliberate exception, for reasons below.

### Cancellation

`JobCancellerHostedService` owns the `Cancelling` status. It polls for rows in that state every
few seconds and resolves each one of two ways:

- **Never started** — set `Cancelled`, done.
- **Genuinely in progress** — trip the `CancellationTokenSource` for that job and stop there. The
  running code observes the token, unwinds, and the runner writes `CancelledInProgress`. The
  canceller never writes a terminal status for a job that started.

It is separate from the runner because the runner is *busy* — it may be minutes into
`ProcessActions` and in no position to poll for cancel requests. Watching from outside is what
makes cancellation prompt.

**The API always writes `Cancelling`, never `Cancelled` directly.** Between reading "this job is
still queued" and writing "cancelled", the runner can pick it up and start it. Routing every
request through `Cancelling` and letting one component resolve it makes that a single-writer
decision instead of a race.

The same race then exists inside the canceller, and conditional updates close it. Both
transitions are conditional updates on the same row, so exactly one wins and nothing needs
locking:

```sql
-- canceller, the "never started" path
UPDATE tbl_job SET Status = 'Cancelled', FinishedAt = NOW(6)
WHERE JobID = ? AND Status = 'Cancelling' AND StartedAt IS NULL;

-- runner, claiming a job
UPDATE tbl_job SET Status = 'Running', StartedAt = NOW(6)
WHERE JobID = ? AND Status = 'Queued';
```

Zero rows affected on the canceller's update means the job started underneath it — fall through
to the token path on the next tick. Zero rows on the runner's means it was cancelled before it
began, and the runner drops it.

**This is the one place the worker's halves are coupled in-process.** A `CancellationTokenSource`
cannot cross a process boundary, so the canceller needs the runner's registry of in-flight
tokens. The row is the signal; the token is a local effect. Two consequences:

- The canceller travels with the runner. If the scheduler is ever split into its own container,
  the canceller does not go with it.
- With more than one runner, each needs its own canceller and `tbl_job` needs an owning-runner
  column, so a canceller only trips tokens for jobs it holds. At desired count 1 that column is
  unnecessary.

**Known limitation:** a job that ignores its cancellation token sits in `Cancelling` indefinitely.
That is visible in the console rather than hidden, and the stale-heartbeat sweep still catches a
runner that dies outright. No hard timeout is planned.

---

## Schema

Committed as
[`2026-09-12_000_jobSystem.sql`](../src/FantasyCritic.DatabaseUpdater/Scripts/Sequential/2026-09-12_000_jobSystem.sql).
Four tables: `tbl_job`, plus `tbl_job_status`, `tbl_job_runtype` and `tbl_job_type`.

**`RunType` mirrors Windows service startup types** — `Manual`, `Cron`, `ManualOrCron`,
`Disabled` — and separates "the scheduler may enqueue this" from "an admin may press the button"
in one column rather than two booleans. Turning off a cron means setting `Manual`; a job that
should never run at all means `Disabled`. Current distribution is 13 `ManualOrCron`, 8 `Manual`,
2 `Cron`, 0 `Disabled`. The two `Cron` entries are `SetTimeFlags` and `PushGameReleaseMessages`,
the only job types with no controller action.

`ProcessActions` is `Manual`. Bid processing stays a button press until that is comfortable to
automate; flipping it is then one `UPDATE`, no deploy, reversible in one statement.

**`ScheduledFor` plus `UNIQUE (JobType, ScheduledFor)` is what makes the scheduler idempotent.**
MySQL permits multiple NULLs in a unique index, so scheduled slots are deduplicated while manual
runs stay unconstrained — no partial index needed. A scheduler that restarts, overlaps itself,
or runs twice simply gets a duplicate-key error on the second insert. It also makes the startup
catch-up query possible, and cheap: `SELECT MAX(ScheduledFor) WHERE JobType = ?` explains as
"Select tables optimized away", answered from the index with no rows read.

Other decisions worth remembering:

- `timestamp(6)`, not `timestamp`. Whole seconds would report every fast job as 0s and make
  queue ordering non-deterministic within a second.
- `CreatedAt` relies on the server's `explicit_defaults_for_timestamp = 1`. Without it MySQL
  would attach `ON UPDATE CURRENT_TIMESTAMP` to the first timestamp column and silently rewrite
  it on every status change.
- `IX_tbl_job_pollqueue (Status, CreatedAt)` covers the runner's polling query — verified as a
  covering index with no filesort.
- `CancelledInProgress` distinguishes a job killed mid-run from one cancelled before it started.
  The runner updates statuses on shutdown; that is enough resiliency here.
- Lookup tables are created before `tbl_job`, so the script no longer depends on the HeidiSQL
  dump's `FOREIGN_KEY_CHECKS=0` to resolve its foreign keys.

### No dependency graph

`DependsOnJobID` was drafted and removed. Composition lives in `AdminService`, where it already
does: `FullDataRefresh()` is four job types called in sequence, and has worked that way for
years with no orchestration infrastructure.

A dependency column would have meant defining and implementing: what the runner does when a
dependency is still `Queued`, what happens when it is `Error` (cancel, run anyway, or block
forever), cascade-cancel semantics, and cycle detection. It would also have cost the polling
index, because the queue query stops being `WHERE Status='Queued' ORDER BY CreatedAt` and
becomes a self-join.

A "combined job type" therefore needs no concept in the job system at all. It is a job whose
handler calls two service methods. Put the composition in `AdminService` so the sequence stays
unit-testable without the job system and so the button and the cron execute identical code.

The cost to accept: a wrapper is atomic in *reporting* but not in *effect*. If `RefreshCaches`
succeeds and `UpdateFantasyPoints` throws, the row says `Error` while the caches really were
refreshed. Mitigate with `DetailedStatus` written per step and `ErrorMessage` naming the step
that failed — that is what `DetailedStatus` is for.

`PrepareForActionProcessing` is the second genuine wrapper: enable action processing mode, then
snapshot. It may disappear once bid processing is automated.

---

## Cron conversion

| Task | Today | After | Guard |
|---|---|---|---|
| `ExpireTradesTask` | `*/10 * * * *` | `0 * * * *` (hourly is precise enough, and it keeps the job history readable) | none |
| `ProcessSpecialAuctionsTask` | `*/10 * * * *` | unchanged | none |
| `PatreonUpdateTask` | `0 */1 * * *` | unchanged | none |
| `TimeFlagsTask` | `0 */1 * * *` | unchanged | none |
| `GameReleaseNotificationTask` | `1 */1 * * *` + midnight±2min | `0 0 * * *` | delete |
| `PublicBiddingNotificationTask` | `0 */1 * * *` + Thu 20:00±1min | `0 20 * * THU` | delete, and split into `SendPublicBiddingEmails` + `PushPublicBiddingMessages` |
| `ReleasingThisWeekNotificationTask` | `0 */1 * * *` + Sun 20:00±1min | `0 20 * * SUN` | delete |
| `RefreshDataTask` | `0 */2 * * *` + 21:30–23:30 inner guard | split: `FullDataRefresh` at `0 */2 * * *`, `UpdateDailyPublisherStatistics` at `0 22 * * *` | delete |
| `GrantSuperDropsTask` | `*/10 * * * *` + `ShouldGrantSuperDrops()` | unchanged, **keep the guard** | keep |

All expressions are evaluated in America/New_York.

**`GrantSuperDropsTask` is the exception and must not be converted.** Its guard is
`now >= September 1 midnight Eastern`, which is true for the rest of the year, and
`AdminService.GrantSuperDrops()` excludes publishers already granted. The ten-minute poll is a
deliberate idempotent catch-up for leagues whose first draft finishes *after* September 1. An
annual cron would silently deny super drops to every league that drafts late.

`RefreshDataTask`'s apparent dependency dissolves here: daily stats only ran inside the
two-hourly refresh because there was no way to schedule them separately.

The conversion removes roughly 70 no-op executions per day, which matters now that each run
writes a row — about 494 rows/day, ~180k/year. A prune job is worth adding, and it is naturally
just another job type.

### One source of truth for the schedule

`PublicBiddingRevealDay` / `PublicBiddingRevealTime` and friends in `TimeExtensions` also drive
the user-facing "next bid time" display, so a cron string written separately could silently
disagree with what the site tells players. Derive the expression from those constants and assert
the derivation in a unit test — then drift is a build failure rather than a missed post.

This is also why the cron expression stays in **code** and not in `tbl_job_type`. The database
owns whether a schedule is active (`RunType`); the code owns what the schedule is.

---

## Corrections to the roadmap

**The master game cache is not a problem.** The roadmap flags a "hidden design item" about the
web process's in-process cache going stale once refreshes move to the worker. `IMasterGameRepo`
is registered `AddScoped` and the caches are instance fields, so it is a per-request cache.
There is nothing to go stale across processes and no invalidation to design. (It also means the
"Refresh Caches" button does less than its name suggests.)

---

## What is left to build

1. **`FantasyCritic.Worker`** — `Program.cs` modelled on the Discord bot's: generic host,
   `FantasyCriticConfigurationLoader`, Serilog with the Loki sink, `AddFantasyCriticCore`,
   `ValidateOnBuild`. Add `LoggingPaths.Worker`. Dockerfile, compose service, ECR repository and
   CI build job all mirror Phase 4a.
2. **`JobRunType` and `JobStatus` as `TypeSafeEnum<T>`** in `Lib/Enums`, with `AllowsCron` /
   `AllowsManual` as properties so no call site compares strings.
3. **The job registry** — job name to handler, plus the Cronos expression where one exists.
   Handlers stay thin adapters over `AdminService`.
4. **`JobSchedulerHostedService`** — for each type whose `RunType` allows cron, compute the next
   occurrence from `MAX(ScheduledFor)` (falling back to now on first run) and insert. Duplicate
   key means another instance got there first; that is success, not failure.
5. **`JobRunnerHostedService`** — poll for `Queued`, claim with a conditional update, run one at
   a time, write status transitions, and heartbeat while running so a killed runner's rows can be
   swept. It does not watch for cancellation; the canceller trips its token from outside. Before
   picking, it re-checks each queued job against its type's *current* `RunType`
   (`FantasyCriticJob.AllowedByRunType`, cron-ness read from `ScheduledFor`) and cancels any it no
   longer allows, recording why in `DetailedStatus`. Otherwise a job queued before its type was set
   to `Disabled` sits `Queued` and runs the moment the type is re-enabled.
6. **`JobCancellerHostedService`** — poll for `Cancelling`, settle never-started jobs directly and
   trip the token for in-flight ones. Needs the shared in-flight token registry it and the runner
   both hold.
7. **`JobService.Enqueue(jobType, user)` returning `Result<Guid>`** — the single place manual runs
   check `RunType.AllowsManual`, so twenty controller actions cannot each forget it. The scheduler
   checks `AllowsCron` the same way. Both read the same properties the runner re-checks, so the
   enqueue-time and run-time checks cannot disagree; the runner's check exists only to catch a
   `RunType` changed while a job waited.
8. **Controller actions per job.** Deliberately *not* a single `POST /api/jobs/{name}`: the three
   admin roles are not hierarchical (`Admin` alone satisfies neither `FactChecker` nor
   `ActionRunner`, which is why `FactCheckerOrAdmin` exists), so one endpoint could not carry the
   right policy. Per-action endpoints also let the console call the generated client instead of
   building URLs by string concatenation. Each needs
   `[ProducesResponseType<T>(StatusCodes.Status200OK)]` or NSwag generates `Task` and the client
   cannot return an ID to poll.
9. **Admin console changes** — render the button grid from `tbl_job_type` using `DisplayName`,
   `Category` and `Severity`, enqueue and poll rather than holding a request open, and show
   recent runs.
10. **Startup reconciliation.** A `RunType` of `Cron` with no expression in code is inert and
   deserves only a warning. A handler registered in code with **no row in `tbl_job_type`** should
   fail at startup — the FK rejects every insert, so the job can never run and nobody finds out
   until they press the button.
11. **Delete** `Scheduling/Lib/Cron` (845 lines), `SchedulerHostedService`, `SchedulerExtensions`,
    the `IsTimeToNotify` guards, and `AddScheduler` from `HostingExtensions`.
12. **Remove the 300s nginx overrides** for `/api/admin`, `/api/factchecker` and
    `/api/actionrunner` in `nginx_nocertbot.txt`, once no admin request runs long.

---

## Deferred

- **The admin site.** `admin.fantasycritic.games` with its own auth, able to answer while the
  public site is down, is attractive and is gated on Phase 7a — before Razor Identity becomes
  Vue and JSON endpoints, it means forking the Identity host. Nothing here blocks it later.
- **Automating bid processing.** `ProcessActions` stays `Manual` until the automated prep has
  been trusted for a few weeks.
- **Retries.** Deliberately none. Automatically re-running `ProcessActions` or
  `SendPublicBiddingEmails` would be actively harmful; "run again" is the correct retry here.
- **Concurrent execution.** One job at a time. If a long `ProcessActions` starts blocking hourly
  work, add worker loops inside the runner before considering another container.
