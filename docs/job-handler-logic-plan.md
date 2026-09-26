# Job handlers own their logic — starting with AdvanceRoyaleQuarters

## Context

Job handlers are mostly one-line wrappers around `AdminService` methods. That leaves the handler unable to report progress (`context.UpdateDetailedStatus`), unable to check the cancellation token between steps, and logging through `AdminService`'s logger instead of its own. The idea: move job-only logic out of `AdminService` and into the handler, using `AdvanceRoyaleQuarters` as the first, easy case. If it works, the same pattern goes to every handler.

## My take

I agree, with one refinement for the later jobs.

**Why it's right:**
- `AdvanceRoyaleQuarters` is job policy: finish past-due quarters, calculate winners, open the next quarter 15 days early. Only the job calls it, so it belongs with the job.
- Once the handler runs the steps itself, it can check the token between steps and write DetailedStatus after each one. When a job is cancelled, the row already says what finished. `CancelInProgressJob` keeps DetailedStatus, so that text becomes the cancellation report with no runner changes.
- Handlers depend on narrow services (`RoyaleService`, `IClock`) instead of AdminService's 16 dependencies. Every handler builds a whole AdminService today.
- AdminService shrinks to what the Web controllers use (dry run, linking, merge, snapshots list), which is a coherent "admin UI" service.

**Something I found that strengthens the case:** AdminService logs through a static `Log.ForContext<AdminService>()`. The worker's `BeginJobScope` and `BeginFlowScope` are MEL scopes. `SerilogLoggerProvider` adds them only to events written through an MEL `ILogger<T>`. So AdminService's lines very likely carry no `JobID`, `JobType` or `Flow`: they miss `JobRunner.log` and the Loki `Flow` label. The comment in `WorkerLogging.cs` says services pick up the scope; that's only true for injected loggers. `RoyaleService`, `DiscordPushService`, `DraftService` and others use static loggers too, and moving logic into handlers won't fix those. Step 1 fixes this for every logger.

**The refinement, for later handlers, not this one:** some AdminService operations are shared by several jobs:
- `RefreshCriticInfo`: RefreshCriticScores, FullDataRefresh, EndOfYearRollover, PrepareForActionProcessing
- `RefreshCaches`
- `UpdateFantasyPoints`

Handlers can't call each other, so those can't live in any one handler. The rule should be: **the handler owns orchestration, status and cancellation; a reusable unit of work goes in `Jobs/Utilities` as a helper that takes the context and token.** `DatabaseSnapshotJobUtilities` already works this way, statusPrefix and all. We'll hit this on the composite jobs; AdvanceRoyaleQuarters only needs it for the small winners loop.

## Steps

Each step: build, test, commit alone, then stop for review.

### Step 0: Commit this plan
Commit this plan as `docs/job-handler-logic-plan.md`.

### Step 1: Put job and flow properties on every log line
- In `src/FantasyCritic.Worker/WorkerLogging.cs`, have `BeginJobScope` and `BeginFlowScope` push `JobID`, `JobType` and `Flow` with `Serilog.Context.LogContext.PushProperty` instead of `ILogger.BeginScope`.
  - `FantasyCriticLogging.CreateConfiguration` already has `.Enrich.FromLogContext()`, so static Serilog loggers and MEL-path events both get the properties.
  - Return a combined disposable. Drop the now-unused `ILogger` receiver and update the call sites in `Worker.cs` and `Scheduler.cs`.
- Fix the class comment so it describes how it really works.
- Check first: confirm the gap is real before changing anything. Use a quick scratch check: a Serilog logger with a collecting sink, `AddSerilog(logger)`, a scope, then one static and one `ILogger<T>` write.

### Step 2a: Lift and shift, with no logic change
The diff should read as a pure move.
- **New `src/FantasyCritic.Lib/Jobs/Utilities/RoyaleJobUtilities.cs`** (static): `RecalculateRoyaleWinners(RoyaleService)`, the body of `AdminService.RecalculateRoyaleWinners` copied unchanged.
- **`AdvanceRoyaleQuartersJobHandler`** injects `RoyaleService`, `IClock` and `ILogger<AdvanceRoyaleQuartersJobHandler>` in place of `AdminService`.
  - `Run` holds the body of `AdminService.AdvanceRoyaleQuarters` verbatim, calling the utility for the winners step.
  - The one log line keeps its wording. Only the logger call changes (`LogInformation` in place of `Information`). It stays interpolated; step 2b makes it structured.
- **`RecalculateRoyaleWinnersJobHandler`** calls the utility instead of `AdminService`.
- **`AdminService`**: delete `AdvanceRoyaleQuarters` and `RecalculateRoyaleWinners`. Grep confirms no other callers.
- The cancellation token stays unused, as it is today.

### Step 2b: Add status, cancellation and structured logs
- **`RoyaleJobUtilities`**: rename the method to `CalculateMissingWinners(RoyaleService, ILogger, CancellationToken)`.
  - Loops finished quarters with no `WinningUser`: token check, `CalculateRoyaleWinnerForQuarter`, structured log.
  - Returns the quarters it calculated, so each caller can report them.
- **`AdvanceRoyaleQuartersJobHandler`** runs three steps:
  1. Finish quarters past their end date.
  2. `RoyaleJobUtilities.CalculateMissingWinners`.
  3. Start the next quarter if within 15 days.
  - `cancellationToken.ThrowIfCancellationRequested()` before each write. Each step reconciles from DB state, so stopping between steps is safe: the next run picks up where this one stopped.
  - DetailedStatus accumulates one clause per step, written after each step. A no-op run still says so, e.g. "No quarters to finish. No winners to calculate. 2026 Q4 opens after 2026-09-16." The table then shows what the nightly run did, and a cancelled row shows how far it got.
  - Logs are structured (`{YearQuarter}`, `{EasternDate}`) instead of today's interpolated strings, so Loki can query them. Each state change is logged at Information; "nothing to do" at Debug.
- **`RecalculateRoyaleWinnersJobHandler`** reports which quarters it calculated, in its DetailedStatus.
- The accumulating status stays a local `List<string>` in each handler for now. If two or three more handlers repeat it, lift it into `FantasyCriticJobContext` then, not before.

## Verification
- `dotnet build src/FantasyCritic.slnx` with zero warnings, `dotnet test src/FantasyCritic.Test/FantasyCritic.Test.csproj`, and `scripts/Format.ps1 -Check`.
- Step 2a: diff review is the main check. Also run both jobs once locally and confirm they complete as before.
- Step 2b: locally, with Docker MySQL and the worker's connection overridden to :3307 per memory:
  - Enqueue AdvanceRoyaleQuarters and RecalculateRoyaleWinners from the admin console. Confirm the rows show the new DetailedStatus.
  - Confirm `JobRunner.log` now contains the handler's lines and the RoyaleService lines.
  - Cancel a run mid-way, using a temporary local delay that is not committed, and confirm the row reads `CancelledInProgress` with the completed steps in DetailedStatus.
- Step 1: after the beta deploy, check in Grafana that `{Flow="JobRunner"} | json | JobID != ""` returns lines whose SourceContext is a static-logger service.
