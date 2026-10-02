# Phase 4b review: findings

A review of everything between `6c5b48a09` (the start of
[Phase 4b](deployment-phase-4b-plan.md)) and `96269624d` on `main`: 239 commits, about 300
files. Written 1 October 2026, so that each finding can be picked up in its own session.

Line numbers are as of `96269624d` and will drift. Each finding names the code it is about, so
search for that if a line has moved.

**Verdict:** Phase 4b is code-complete and the design held up. The move of job logic out of
`AdminService` is clean: no step was dropped, reordered or re-conditioned by accident. The
claim, cancel and enqueue SQL is race-safe, and the deploy drain fails safe on every path
traced. What is left is below.

## How this was checked

- Read in full: `Worker.cs`, `Scheduler.cs`, the worker's `Program.cs`, `MySQLJobRepo.cs`, the
  job registry, schedule and domain types, the health check, the job controllers, the job
  schema, `deploy.sh`, `deploy.yml`, `ci.yml`, the production compose file and `operations.md`.
- All 25 handlers and 10 utilities under `Lib/Jobs` were compared against `AdminService.cs` and
  the nine deleted `Scheduling/*Task.cs` files as they stood at `6c5b48a09`.
- Unit tests pass: 2,431.
- **Not done:** the integration tests were not run. Nothing was run against a database.
- **Not known:** whether Phase 4b has been deployed to production. The `production` branch
  still points at 3 September, which may only be a stale pointer now that deploys pick a ref.

Each finding says whether it is **confirmed** (the code path was read end to end) or
**plausible** (the mechanism is confirmed, the trigger is not).

## The plan's "left to build" list, as built

| Plan item | State |
|---|---|
| 1 `FantasyCritic.Worker` | Done. A `WebApplication`, for `/health` only. |
| 2 `JobRunType` / `JobStatus` enums | Done, as `FantasyCriticJobRunType` / `FantasyCriticJobStatus` in `Lib/Jobs`. |
| 3 The job registry | Done. Also holds the skip-when-due map, which the plan did not have. |
| 4 Scheduler | Done, but seeded from 30 minutes before startup rather than from `MAX(ScheduledFor)`. |
| 5 Runner | Done, as a loop inside `Worker`. **The heartbeat and sweep are not built** (F1). |
| 6 Canceller | Done, as the second loop inside `Worker`. |
| 7 `JobService.Enqueue` | Done as `IJobRepo.EnqueueJob`. It also refuses a second open job of the same type. |
| 8 Controller action per job | Done, through `BaseJobQueuingController.EnqueueJob`. |
| 9 Console renders from `tbl_job_type` | Built by hand instead (L7). Refresh is a button, not a poll, by choice. |
| 10 Startup reconciliation | **Partial** (F5). |
| 11 Delete the old scheduler and vendored cron | Done. No references remain. |
| 12 Remove the nginx 300s overrides | **Not done** (R3). |
| Prune job ("worth adding") | Not built (L9). |

---

## Findings to act on

### F1. A `Running` row can be stuck forever, and one of its two causes is undocumented

**Confirmed.** Engine. Not a regression: the plan's addendum already says nothing sweeps these
rows. The second cause below is new information.

- **Cause 1, documented:** the worker is killed mid-job (host reboot, OOM, `skip_drain`).
- **Cause 2, not documented:** the job finishes or fails, and the write of its final status
  fails. In `Worker.RunJob` ([Worker.cs:194-212](../src/FantasyCritic.Worker/Worker.cs)) the
  calls to `CancelInProgressJob`, `ErrorJob` and `CompleteJob` are not retried. If the database
  is unreachable at that moment the exception goes to the loop's catch, which logs and carries
  on. The worker stays alive and healthy, and the row stays `Running`.
- Cause 2 is likeliest exactly when a job fails *because* the database went away.

What a stuck row costs, from the troubleshooting table in [operations.md](operations.md): the
console button for that job type refuses, a turned-off worker reads as Draining forever, the
next deploy waits its full 30 minutes and then stops, and the fix is editing `tbl_job` by hand.

**Suggested fix, no heartbeat column needed.** The runner is single-threaded. When it is in
`GetNextQueuedJob` it holds no job, so any row with `StartedAt` set and a status of `Running`
or `Cancelling` belongs to nobody. That method already loads the incomplete jobs; it can settle
those rows as `Error` with a message saying the worker lost them. This covers both causes.

**Constraint to record with the fix:** it is only correct while exactly one worker exists. Two
workers overlapping (an ECS rolling deploy, R4) would have the new one settle the old one's
live job. The plan already says a second runner needs an owning-runner column.

### F2. The scheduler loop has no exception handling; both worker loops do

**Confirmed.** Engine. Needs a decision before a fix.

- [Scheduler.cs:35-129](../src/FantasyCritic.Worker/Scheduler.cs) has no `try`. It touches the
  database at every wake: `GetJobTypeRunTypes` (line 46), `ShouldSchedule` for
  ProcessSpecialAuctions (line 82), `CreateJob` (line 109). It wakes at least six times an hour.
- The worker's `Program.cs` does not set `BackgroundServiceExceptionBehavior`, so the default
  applies: an exception out of `ExecuteAsync` stops the host.
- So a database error at a wake stops the whole worker, which cancels whatever job the runner
  has. Docker restarts the container. If the database is still down the worker crash-loops
  until it is back, and the cancelled job's final status write fails, which is F1's cause 2.
- `Worker.JobRunnerLoop` and `Worker.JobCancellationLoop` both catch, log, wait and retry.
- `operations.md` says of an unhealthy worker "it recovers by itself when the database does".
  That is true of the runner and not of the scheduler.

**The decision:** catch, log and retry like the runner, or keep the crash as fail-loud.
A middle path: retry on a failed database call, and keep the "Job Type not found in database"
throw at line 55 fatal, since that one is a deploy mistake and a crash loop is how `deploy.sh`
notices it.

### F3. Year-end rollover: three problems, with a 1 January deadline

The first is older than Phase 4b but is first exercised by the new handler on 1 January 2027.

**F3a. League winners are never written at rollover. Confirmed.**

- [FantasyPointsUpdater.cs:57](../src/FantasyCritic.Lib/Jobs/Utilities/FantasyPointsUpdater.cs):
  `if (today.PlusDays(30) > lastDayOfFinishedYear) continue;` is inverted. A year is only
  finished after its 31 December, so the condition is true for every finished year and
  `UpdateLeagueWinners` at line 67 is unreachable.
- The intent, from the comment and from commit `1692a21cb` (17 May 2026, "Don't try to update
  finished years after a certain point"), is `today > lastDayOfFinishedYear.PlusDays(30)`.
  `CacheRefresher.YearNeedsSystemWideValuesRefresh` expresses the same 30-day grace correctly.
- It was moved verbatim in `6187dd092`; the original is at `AdminService.cs:211` in `6c5b48a09`.
- On 1 January, `EndOfYearRolloverJobHandler` finishes 2026 and calls `UpdateFantasyPoints`,
  which skips 2026's winners. `WinningUserID` stays empty until someone presses Recalculate
  Last Season Winners.
- Related, and the same as the old `SetTimeFlags`: the rollover refreshes critic scores (line
  53), then finishes the year (line 56), then updates fantasy points (line 62). By then 2026 is
  no longer an active year, so that last refresh never reaches 2026's points. The comment at
  lines 60-61 assumes the call finalizes them. Decide whether points should be updated before
  `FinishYear`.

**F3b. The rollover gets one attempt a year. Confirmed.**

- Its schedule is `0 0 1 1 *`
  ([EndOfYearRolloverJobHandler.cs:12](../src/FantasyCritic.Lib/Jobs/Handlers/EndOfYearRolloverJobHandler.cs)),
  chosen deliberately in `5f6271233`. The old `TimeFlagsTask` ran hourly and so retried until
  the year was finished.
- `OpenCriticService.GetOpenCriticGame` rethrows anything but a 404, and
  `CriticScoreRefresher.cs:61` does not catch. One failed call among the midnight fetches ends
  the job in Error before `FinishYear`.
- The scheduler only looks back 30 minutes at startup, so a worker down from 00:00 to 00:30
  skips the slot.
- Either way nothing queues it again for a year. The job type is `ManualOrCron`, so the button
  is the retry, but nothing tells anyone to press it (R1).
- The handler is already safe to run repeatedly: it skips finished years and years that have
  not ended.
- **A trap for the obvious fix:** `FullDataRefresh` skips its slot whenever `EndOfYearRollover`
  is due in the same wake (`CreateSkipWhenDue` in `FantasyCriticJobRegistry`). A daily midnight
  rollover would therefore cancel the midnight refresh every day. A retry schedule has to
  account for that, and `FantasyCriticJobScheduleTests` pins both the expression and the shared
  slot.

**F3c. The status says the standings were sent when they may not have been. Confirmed.**

- `EndOfYearRolloverJobHandler.cs:65-67` logs "Sent final standings" and writes "Final
  standings for {year} sent." unconditionally. `DiscordPushService` sends return silently when
  the bot is disabled or not ready (L6).
- `remaining-jobs-plan.md` set the rule that a status says what the job found, not that a
  message went out. This handler does not follow it.

### F4. Turning action processing mode on now waits in the job queue

**Mechanics confirmed, likelihood low.** Needs a decision on how much of it to close.

- `FullAutomatedActionsProcessJobHandler.cs:74` is the only thing that turns the mode on. The
  worker runs one job at a time, oldest first. The old `TimeFlagsTask` ran in the web process,
  independent of other work.
- The bid lock only covers Saturday 20:00:00 to 20:03:59 (`IsBidLockWindow` in
  `TimeExtensions.cs`).
- **(a) A long job is running at Saturday 20:00**, such as a manual full refresh started at
  19:50. Mode-on is delayed. From 20:04 until the job starts, bids and drops are accepted, and
  they are processed that night. They also skip public bidding, since the next reveal is the
  following Thursday.
- **(b) The worker is not pulling from Saturday to Monday.** The scheduler keeps queuing. On
  resume the stale job turns the mode on, refreshes, snapshots, and then stops on the day check
  (`AdminService.GetReasonsNotToProcessActions`, lines 98-102). The site is locked on a
  weekday, and a manual Process Actions is refused by the same day check.
- **(c) The same outage ending Sunday.** The day check passes, and every bid placed between
  Saturday 20:04 and the resume is processed.
- The reasons that cannot change during the run (wrong day, not production) are evaluated at
  line 87, after the mode is already on.

**Suggested:** evaluate the wrong-day reason before turning the mode on, which closes (b) with
a few lines. (a) and (c) are the cost of one runner and may simply be accepted; alerting (R1)
is what makes (c) visible.

**Do not change:** leaving the mode on after a stop is deliberate. So is stopping with an email
when `EnableAutomatedActionProcessing` is off.

**Decided (1 October 2026):** the scheduler turns the mode on when the slot is due, through a new
`IOnScheduledCronJobHandler` hook that the scheduler calls before enqueueing. That closes (a) and
(c), since the scheduler runs whether or not the runner is busy or pulling. (b) becomes harmless:
the site is locked from Saturday 20:00, and the stale job stops on the day check. The early day
check is no longer worth adding. Widening `IsBidLockWindow` was rejected: it forces the mode on
for the whole window, so it would block next week's bids long after processing finished. What is
left is the whole Worker process being down at 20:00, the same exposure Web being down had before.

### F5. Nothing checks that every job type has a `tbl_job_type` row

**Confirmed.** Plan item 10, half built.

- The scheduler throws for a missing row
  ([Scheduler.cs:52-56](../src/FantasyCritic.Worker/Scheduler.cs)), but only for job types
  that have a schedule.
- For a manual-only type, the first sign is a 500 when the button is pressed:
  `MySQLJobRepo.EnqueueJob` throws at lines 169-174.
- `GetJobTypeRunTypes` silently skips a row whose name, run type or severity it does not
  recognise.
- The comment in `deploy.sh` ("a handler with no tbl_job_type row" stops the worker at startup)
  is only true of scheduled types.
- Today the 25 types and the 25 rows match. Nothing keeps them matched.

**Suggested:** one integration test asserting that `FantasyCriticJobType.GetAllPossibleValues()`
and the names in `tbl_job_type` are the same set. Or have the scheduler check
`registry.Definitions` rather than `registry.Schedules` on its first pass.

---

## Lower priority

**L1. Jobs queued in the same wake have no defined order. Confirmed.**
The scheduler gives every job in a wake the same `CreatedAt` (`Scheduler.cs:104`), and
`GetIncompleteJobs` orders by `CreatedAt` alone. At 22:00 `UpdateDailyPublisherStatistics` and
`FullDataRefresh` are both due; the old `RefreshDataTask` always ran stats after the refresh.
When stats win the tie, the day's row reflects the 20:00 refresh. Moving stats to 22:05, or a
tie-break in the query, fixes it.

**L2. Daily jobs take their date from the clock, not from their slot. Confirmed, needs an
outage that spans midnight.**
- `UpdateDailyPublisherStatisticsJobHandler.cs:32`: a 22:00 job that starts after midnight
  writes under the next date. `MySQLDailyStatsRepo` inserts with `insertIgnore` (lines 60 and
  139), so that day's real 22:00 run is then discarded, and the missed day has no row.
- `PushGameReleaseMessagesJobHandler.cs:29`: a backlog of N queued daily jobs each posts the
  resume day's releases, N times, and nothing for the missed days.
- Both could use `context.Job.ScheduledFor` when it is set.

**L3. The automated run processes against master game data cached before the snapshot wait.
Plausible, narrow window.**
All steps of `FullAutomatedActionsProcess` share one DI scope, and `MySQLMasterGameRepo` caches
master games in instance fields. `CacheRefresher.cs:74-75` clears them, the fantasy points
step reloads them, and then the snapshot wait runs for minutes. If a fact checker answers a
change request by editing a game during that wait, the pending-corrections check passes (it is
a fresh query) and processing uses the pre-edit game. Clearing both caches just before the
check at line 87 closes it. The manual Process Actions job starts in a fresh scope and is not
affected.

**L4. ProcessSpecialAuctions may throw in December. Pre-existing; reachability not verified.**
`GetAllActiveSpecialAuctions` returns every year's unprocessed auctions, but
`ProcessSpecialAuctionsJobHandler.cs:85-90` indexes a dictionary of one year's leagues. With
two years open, a locked auction in the other year throws `KeyNotFoundException`, every ten
minutes. `ShouldSchedule` and the special-auction reason in `GetReasonsNotToProcessActions`
would then stay true, which also stops action processing. Check whether a next-year league can
hold a special auction in December.

**L5. The stop email is wrong about how the site unlocks. Confirmed, trivial.**
`FullAutomatedActionsProcessJobHandler.cs:12` says the site stays locked "until actions are
processed by hand or the mode is turned off". `ProcessActionsJobHandler` no longer turns the
mode off, so only the second half is true.

**L6. Discord sends can do nothing while the job reports success. Mechanism confirmed; how
often is not known.**
Every `DiscordPushService.Send*` returns without sending when `StartBot()` is false: the bot is
disabled, or it was not Ready about five seconds after first use (`DiscordPushService.cs:60-95`,
`MaxAttempts = 4`). The service is a singleton, so the exposure is the first Discord-sending
job after a worker start. Only `SendPendingMasterGameUpdates` reports "bot unavailable".
Whether Ready ever takes longer than five seconds on a cold start has not been measured.

**L7. `tbl_job_type.DisplayName` and `Category` are never read. Confirmed.**
`GetJobTypeRunTypes` selects only `Name`, `RunType` and `Severity`. The console was laid out by
hand, and `recentJobsTable.vue` hard-codes the list of job types for its filter, so a new job
type needs a line there too. Either drop the two columns or accept them as documentation.

**L8. Scheduler noise. Confirmed, cosmetic.**
- A job type whose `RunType` does not allow cron logs a Warning at every slot
  (`Scheduler.cs:71-76`). On beta the cleaner disables `RefreshPatreonInfo`, so that is one
  Warning an hour, which shows up behind the service monitor's Warnings button.
- Three types are `ManualOrCron` with no schedule in code (`MakeSlotsConsistent`,
  `RecomputeRulesBasedRoyaleGroups`, `UpdateTopBidsAndDrops`). The plan said that deserves a
  startup warning; there is none. Setting them to `Manual` is the simpler answer.
- After every restart, slots from the last 30 minutes are retried and log "probably a
  deployment overlap" at Warning. Expected, but it is a Warning on every deploy.

**L9. No prune job.** The plan estimated about 494 rows a day. Since ProcessSpecialAuctions is
now only queued when there is work, the real figure is roughly 90 a day, about 33,000 a year.
Suggest dropping the idea.

**L10. GrantSuperDrops reads two different years. Pre-existing; from the handler comparison.**
`GrantSuperDropsJobHandler.cs:34` takes leagues from the earliest open year; line 38 takes
"already granted" from the calendar year. They only disagree for a manual run between January
and August, or a 31 December slot that runs after midnight.

---

## Decided already: do not re-raise

These were chosen on purpose in earlier sessions. A future session should leave them alone
unless Steve reopens them.

- Missed cron slots are not reported. The scheduler looks back 30 minutes at startup and no
  further. *Note:* this was decided before `FullAutomatedActionsProcess` and the yearly
  `EndOfYearRollover` existed. It still stands; R1 is the proposed answer to the higher stakes.
- The scheduler keeps queuing while `WorkerShouldPullNewJobs` is off, and the backlog runs on
  resume.
- Off and on is a database flag, not a container stop. There is no off and on for the bot.
- A drain that times out aborts the deploy. A worker turned off by hand stays off across a
  deploy.
- The runner logs its "Intentionally not running new jobs" line on every five-second poll.
- A job that ignores its cancellation token sits in `Cancelling`. No hard timeout.
- The action processing jobs check for cancellation up to the first save and never after.
- Automated processing never runs outside production. A stop emails every reason, ends the job
  in Error and leaves the mode on. There is no success email. A cancellation emails too.
- `GrantSuperDrops` runs hourly with a calendar guard, not every ten minutes as the plan said.
- Public bidding email and Discord run concurrently in one job.
- No retries, no dependency graph, one job at a time.
- A missing `WorkerShouldPullNewJobs` column reads as "off" rather than failing.

---

## Roadmap: what should change

Done: Phases 1, 2, 3, 4a, 4b and 7b. Left: 5, 6, 7a, 7c and 8.

**R0. Production deploy of 4b, if it has not happened.** It needs `skip_drain: true` (there is
no `tbl_job` to read before the migration runs) and the Secrets Manager key renames from
[typed-configuration-plan.md](typed-configuration-plan.md), or every host refuses to start.
F3a should go out before 1 January regardless.

**R1. Pull alerting out of the backlog and do it before Phase 5.** The roadmap lists alerting
as low priority. Phase 4b raised the stakes:
- The worker fails silently by design. The compose file says so: with it down, nothing
  scheduled runs and the site gives no sign.
- Automated action processing sends no success email, so a dead worker on a Saturday looks the
  same as a good run.
- Suggested rules, all in Grafana Cloud at no AWS cost: no worker log lines for 15 minutes (the
  scheduler logs at every wake); any job ending in Error; an external uptime check on the
  site's `/health`.

**R2. Phase 5 (Terraform and the ALB) gains two decisions.**
- Whether nginx leaves the box along with certbot. After Phase 5 its remaining jobs are
  proxying and timeouts. Removing it simplifies the launch template and carries into ECS.
- Long requests. The ALB idle timeout defaults to 60 seconds, the same question as R3.

**R3. Plan item 12, the nginx 300-second overrides.** Still in
[nginx_nocertbot.txt](../infrastructure/nginx_nocertbot.txt) for `/api/admin`,
`/api/factchecker` and `/api/actionrunner`. Before removing them, two requests still do long
work synchronously in the web process:
- `ActionRunnerController.ActionProcessingDryRun` (and `ComparableActionProcessingDryRun`).
- `AdminController.PushYearEndDiscordMessages`.

Every `/api/factchecker` action is now an enqueue or a single database operation (none was
timed), so that override looks safe to remove first. Time the other two, then either make them
jobs or keep one override. Best settled with R2.

**R4. Phase 6 (ECS) text is stale.**
- The deploy sequence must drain the worker first, and stop the worker and bot as well as web
  before migrating. The roadmap only sets `web` to 0.
- There are two one-off tasks now: the migrator and `command-line`.
- `worker-should-pull` answers on stdout, which an ECS task cannot hand back to the pipeline.
  How the deploy remembers whether the worker was pulling needs a design.
- Worker tasks must never overlap: F1's fix, the canceller and the bot all assume one. Deploy
  with a minimum of 0% and a maximum of 100%.
- The 0.5 vCPU / 1 GB Fargate guess for the worker predates it owning refreshes and action
  processing. Measure a Saturday with `docker stats` first.
- The Discord token can now have three gateway sessions (web push, worker push, bot), not two.
  The push sessions start on first send.

**R5. The sequencing decision the roadmap deferred is now due.** It said to complete the first
few infrastructure phases, then decide whether to alternate with client work. The suggestion
from this review is Phase 7a (Identity pages to Vue) next, ahead of 5 and 6:
- It depends on no infrastructure phase.
- It is the only remaining work on the Vue 2 end-of-life clock.
- It unblocks the deferred admin site.
- Phases 5 and 6 buy little that is felt now that deploys are push-button.
- The cost: the hand-configured server and certbot stay longer.

Suggested order: findings → production deploy → alerting → 7a → 5 → 6 → 7c → 8.

**R6. Bring the docs up to date.**
- [deployment-modernization-roadmap.md](deployment-modernization-roadmap.md): rewrite the Phase
  4b section as built, the way Phases 3 and 4a were. It still describes Hangfire, the dashboard
  and the master game cache problem. Phase 3's bullet says CI builds three images; it is five.
  The cost table and hard dependencies are still right.
- [deployment-phase-4b-plan.md](deployment-phase-4b-plan.md):
  - "The schema is committed; everything else is ahead of us."
  - The schema link points at `2026-09-12_000_jobSystem.sql`; the file is now
    `2026-09-19_000_jobSystem.sql`.
  - "Three hosted services" is two: `Scheduler`, and `Worker` with two loops.
  - It still names `SetTimeFlags`, `PrepareForActionProcessing`, `JobService.Enqueue` and
    `SendPublicBiddingEmails` + `PushPublicBiddingMessages`.
  - The `RunType` counts and "ProcessActions stays a button press" predate
    `FullAutomatedActionsProcess`.
  - The scheduler's 30-minute lookback replaced the `MAX(ScheduledFor)` catch-up it describes.
  - "Automating bid processing" is still under Deferred.
- The roadmap's backlog item "Remove Windows leftovers" is still open: `LoggingPaths.cs` still
  has Windows branches.

---

## Decisions for Steve

1. Has 4b been deployed to production? (R0)
2. Which findings close the phase? Suggested: F1, F3 and F5, plus the day-check half of F4.
3. F2: retry like the runner, or keep the crash?
4. F3a: fix the guard only, or also update points before `FinishYear`?
5. F3b: what retry does the rollover get, given the skip-when-due trap?
6. R5: is 7a next, or infrastructure through Phase 6?
7. L7 and L9: drop the unused columns and the prune job from the plan?

---

## Checked and found correct

So that a later session does not re-review these.

**Engine**
- `StartJob`, `CancelJob`, `CancelQueuedJob`, `RequestCancellation` and `FinishStartedJob` are
  conditional updates; in each race exactly one side wins.
- `StartJob` includes `WorkerShouldPullNewJobs` in the claim, so nothing starts once it is off.
- `EnqueueJob` checks for an open job of the same type inside the insert.
- `CreateJob` treats a duplicate scheduled slot as success, and only for scheduled jobs.
- The in-flight token is registered before the claim, so a `Running` row always has a token.
- A non-cancellation `OperationCanceledException` (an HTTP timeout) ends the job in Error, not
  Cancelled.
- Error messages are truncated to fit the column.
- The registry rejects a missing handler, a duplicate handler, a cron handler registered
  without its schedule, and skip-when-due entries that chain, self-refer or name an
  unscheduled type.
- Weekly schedules derive from the `TimeExtensions` constants, and tests pin them against the
  site's displayed times across both DST changes.
- The `GrantSuperDrops` guard turns on at the 1 September midnight Eastern slot, off at 1
  January, and re-arms each year.
- A slot whose `RunType` or condition rules it out is dropped before the skip-when-due pass, so
  nothing defers to a job that was never queued.
- A special auction must end at least an hour before bid time, so one cannot lock during the
  automated run.

**Deploy**
- The drain runs before the maintenance page; a timeout leaves the site untouched.
- The exit trap turns pulling back on after any failure, and only if the deploy turned it off.
- `.env` keeps naming the live release until just before `compose up`.
- The pull names both profiles, so the migrator and command-line images download before the
  downtime window.
- A worker that fails to become healthy fails the deploy after the maintenance page is lowered.
- CI builds all five images on pushes to `main`.

**Handlers and utilities**
- `CriticScoreRefresher`, `GGInfoRefresher`, `CacheRefresher`, `FantasyPointsUpdater`,
  `TopBidsAndDropsUpdater` and `FullDataRefresher` match the old `AdminService` methods step
  for step.
- ExpireTrades, MakeSlotsConsistent, RecalculateLastSeasonWinners,
  RecomputeRulesBasedRoyaleGroups, RefreshPatreonInfo, UpdateDailyPublisherStatistics,
  GrantSuperDrops and ProcessSpecialAuctions have bodies identical to the old methods.
- AdvanceRoyaleQuarters has the same three steps and comparisons as `SetTimeFlags` and is safe
  to re-run daily.
- The removed passthroughs on `InterLeagueService`, `RoyaleService` and
  `FantasyCriticUserManager` were one-line repo calls; no logic went with them.
- The public bidding trio builds the same sets. The combined job fails if either half throws.
- The old Process Actions controller checks and the old special-auction throw are all in
  `GetReasonsNotToProcessActions`, checked at the button and again in the job.
- `FullAutomatedActionsProcess` has no path that processes despite a reason, and none that
  processes twice.
- Top bids and drops run once after all years, which fixes the old December miss.
- No handler checks for cancellation after a partial write in a way that leaves data
  half-saved.
- `SendReleasingThisWeekUpdate` failures now end the job in Error instead of being swallowed.
- `DatabaseSnapshotJobUtilities` waits for "available", throws on an unexpected status or after
  30 minutes, and can be cancelled while it polls.
