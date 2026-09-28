# Data refresh jobs own their logic

## Context

The second round of [job-handler-logic-plan.md](job-handler-logic-plan.md), which moved AdvanceRoyaleQuarters and RecalculateRoyaleWinners out of `AdminService`. This round covers the data refresh jobs: FullDataRefresh, RefreshCriticScores, RefreshGGInfo and RefreshCaches. As before, a straight lift and shift comes first, then cancellation, then status and logging.

## Scope: two more jobs are pulled in

FullDataRefresh runs `RefreshCriticInfo`, `RefreshGGInfo(false)`, `RefreshCaches` and `UpdateFantasyPoints` in that order. Other jobs call those units too:

| Unit | Called by |
|---|---|
| `RefreshCriticInfo` | RefreshCriticScores, FullDataRefresh, EndOfYearRollover |
| `RefreshGGInfo` | RefreshGGInfo (deep), FullDataRefresh (shallow) |
| `RefreshCaches` | RefreshCaches, FullDataRefresh, EndOfYearRollover, **LocalDatabaseTool** |
| `UpdateFantasyPoints` | UpdateFantasyPoints, FullDataRefresh, EndOfYearRollover |
| `FullDataRefresh` | FullDataRefresh, PrepareForActionProcessing |

So FullDataRefresh can't move unless `UpdateFantasyPoints` moves with it, and once the units leave `AdminService`, `RunEndOfYearRollover` can't stay there either. This round therefore also moves **UpdateFantasyPoints** and **EndOfYearRollover** into the job code. PrepareForActionProcessing only changes which object it calls.

## Where the shared units go

The earlier rule was that a reusable unit goes in `Jobs/Utilities` as a static helper, which is what `RoyaleJobUtilities` is. That doesn't scale here. `RefreshCaches` alone needs seven dependencies. Static helpers would mean the FullDataRefresh, EndOfYearRollover and PrepareForActionProcessing handlers each inject the union, about twelve services, which rebuilds AdminService's constructor three times.

**Proposal: one injectable class per unit, in `Jobs/Utilities`, registered scoped in `AddFantasyCriticJobHandlers`.**

| Class | Holds | Dependencies |
|---|---|---|
| `CriticScoreRefresher` | `RefreshCriticInfo` | InterLeagueService, IOpenCriticService, DiscordPushService, IClock |
| `GGInfoRefresher` | `RefreshGGInfo` | InterLeagueService, IGGService |
| `CacheRefresher` | `RefreshCaches` and its private helpers (`UpdateCodeBasedTags`, system-wide values, `UpdateGameStats`, `CalculateStatsForGames`, `FixDouble`) | IClock, IMasterGameRepo, InterLeagueService, IFantasyCriticRepo, IHypeFactorService, DiscordPushService, RoyaleService |
| `FantasyPointsUpdater` | `UpdateFantasyPoints` and `PushDiscordScoreChangeMessages` | InterLeagueService, IFantasyCriticRepo, FantasyCriticService, RoyaleService, IDiscordRepo, DiscordPushService, IClock |
| `FullDataRefresher` | the four-step sequence | the four classes above |

- Handlers hold orchestration: EndOfYearRollover's sequence goes in its handler. A class holds a unit that several jobs run.
- All steps of one job share a scope, so they share the scoped `MySQLMasterGameRepo` and its master game cache. That is what makes `ClearMasterGameCache` after the critic refresh matter: RefreshCaches, later in the same run, sees the new scores. The same holds after the move.
- The classes are `internal`, except `CacheRefresher`, which is `public` because LocalDatabaseTool constructs it by hand. That replaces the tool's `GetAdminService`, which builds a whole AdminService with four nulls to call one method.
- Alternative: static helpers like `RoyaleJobUtilities`, with the fat handler constructors described above. Or one `DataRefreshJobSteps` class holding all four units, which is just a smaller AdminService.

## Steps

Each step: build, test, commit alone, then stop for review.

### Step 0: Commit this plan

### Step 1: Lift and shift, with no logic change
The diff should read as a pure move (`git diff --color-moved`).
- New classes from the table above. Each method body is copied verbatim and keeps its name: `RefreshCriticInfo()`, `RefreshGGInfo(bool deepRefresh)`, `RefreshCaches()`, `UpdateFantasyPoints()`, `FullDataRefresh()`.
- Each class keeps a static `Log.ForContext<T>()` logger, so every log line keeps its wording and level; only SourceContext changes. The worker already enriches static loggers with JobID and Flow.
- Handlers:
  - RefreshCriticScores, RefreshGGInfo, RefreshCaches and UpdateFantasyPoints inject their class instead of AdminService.
  - FullDataRefresh and PrepareForActionProcessing inject `FullDataRefresher`. PrepareForActionProcessing keeps AdminService for the snapshot.
  - EndOfYearRollover's `Run` holds the body of `RunEndOfYearRollover` verbatim. It injects `InterLeagueService`, `IFantasyCriticRepo`, `DiscordPushService`, `IClock`, `CriticScoreRefresher`, `CacheRefresher` and `FantasyPointsUpdater`.
- `AdminService`: delete the moved methods and helpers, and drop the dependencies nothing uses any more: `IOpenCriticService`, `IGGService`, `IHypeFactorService` and `IDiscordRepo`.
- LocalDatabaseTool: build a `CacheRefresher` instead of an `AdminService`.
- `JobServiceCollectionExtensions`: register the five classes scoped.
- The cancellation token stays unused, as it is today.

### Step 2: Call the repos directly, with no change in behavior
These `InterLeagueService` methods are one-line passthroughs, and only these jobs call them: `UpdateCriticStats` (both overloads), `UpdateGGStats`, `ClearMasterGameCache`, `ClearMasterGameYearCache` and `FinishYear`.
- The classes and the EndOfYearRollover handler call `IMasterGameRepo` / `IFantasyCriticRepo` instead. The same goes for `GetSupportedYears` and `GetMasterGames`, which stay on InterLeagueService for the rest of the app.
- Delete the six passthroughs.
- `GetSystemWideSettings` stays on InterLeagueService. It isn't a passthrough: it overlays the bid lock window.

### Step 3: Check for cancellation before each write
Same rule as last time: `ThrowIfCancellationRequested` before each write and at the top of each loop over many items. Each class's entry method takes the token.
- **CriticScoreRefresher / GGInfoRefresher:** at the top of the per-game loop. Each iteration is fetch then write, so a stop lands between games, and the next run just fetches that game again. The every-100-games `Task.Delay` takes the token. The OpenCritic and GG HTTP calls don't take a token yet: cancelling waits at most one round trip, which is fine.
- **CacheRefresher:** before tags, release date estimates, each year's system-wide values, the aggregate, each year's game stats, and the Discord batch send. Anything left in the Discord queue goes out with the next RefreshCaches in that worker process.
- **FantasyPointsUpdater:** before each active year, each finished year's winners, and each Royale quarter. **Not** between an active year's stats write and its Discord score-change messages: the messages are a diff against the old stats, so a stop there would lose them for good.
- **EndOfYearRollover:** check before `FinishYear` only. Once a year is finished, the next run skips it, so a stop after `FinishYear` would leave fantasy points un-finalized and the final standings never sent. The steps after `FinishYear` run with `CancellationToken.None`, and a comment says why.
- A cancelled critic refresh never reaches `ClearMasterGameCache`. That's harmless: the cache belongs to the job's scope, which ends with the job.

### Step 4a: Move the accumulating status into FantasyCriticJobContext
The last plan said to lift the `statusParts` list into the context once two or three more handlers repeated it. This round adds three (FullDataRefresh, PrepareForActionProcessing, EndOfYearRollover), plus `DatabaseSnapshotJobUtilities`'s `statusPrefix`, which is the same idea. So:
- `FantasyCriticJobContext` gets `CompleteStatusPart(string)`, which appends a finished step's clause and writes the whole status, and `UpdateStatusProgress(string)`, which writes the finished clauses plus one in-progress clause that the next call replaces.
- Convert AdvanceRoyaleQuarters and `DatabaseSnapshotJobUtilities`, dropping the `statusPrefix` parameter. No wording changes.

### Step 4b: Add status and structured logs to the data refresh jobs
- Each class's entry method takes the context and returns what it did as a small record. A `Describe()` method turns that record into its status clause. The one-off job then writes a single clause, while a composite job writes one clause per finished step.
  - Critic scores: `Critic scores: checked 1,250 games; 7 changed; 2 newly scored (Game A, Game B); 1 failed to fetch.` The loop reports progress every 100 games (`checked 300 of 1,250`).
  - GG: `GG (shallow): checked 40 games; 1 failed to fetch.`
  - Caches: one clause per sub-step as it finishes: tags, release estimates, system-wide values for the years it recalculated, game stats per year.
  - Fantasy points: the years updated, the finished years whose winners were updated, and the Royale quarters updated.
  - A refresh skipped because `RefreshOpenCritic` is off says so.
- `FullDataRefresher` and EndOfYearRollover call `CompleteStatusPart` after each step, so a cancelled or failed row shows how far it got.
- Logs:
  - Switch to injected `ILogger<T>`. LocalDatabaseTool passes a Serilog-backed logger for `CacheRefresher`.
  - Make the interpolated lines structured (`{GameName}`, `{OpenCriticID}`, `{OldScore}`, `{NewScore}`, `{Year}`, `{YearQuarter}`), and fix "recieved" and the stray `)`.
  - State changes log at Information; per-item "nothing to do" at Debug.

## Found along the way, not changing
- `RefreshGGInfo` is gated on the `RefreshOpenCritic` flag, not a GG flag of its own.
- A standalone RefreshCriticScores run queues its Discord score messages but doesn't send them. They go out with the next RefreshCaches in the same worker process, normally the two-hourly FullDataRefresh. A worker restart in between drops them.

## Verification
- Every step: `dotnet build src/FantasyCritic.slnx` with zero warnings, `dotnet test src/FantasyCritic.Test/FantasyCritic.Test.csproj`, and `scripts/Format.ps1 -Check`.
- Step 1: the integration tests, since AdminService's constructor changes and the test host (Development, so ValidateOnBuild) registers the handlers. Start the worker locally once for its own ValidateOnBuild. Diff review is the main check. Run RefreshCaches, RefreshGGInfo and FullDataRefresh once locally from the admin console and confirm they complete as before. RefreshCriticScores and FullDataRefresh call the real OpenCritic API. LocalDatabaseTool: build only; running it is Steve's call.
- Step 2: review, and one FullDataRefresh run locally.
- Step 3: review. These jobs run long enough to cancel partway, so cancel one FullDataRefresh from the admin console mid-critic-refresh, and confirm it records CancelledInProgress.
- Step 4a: run AdvanceRoyaleQuarters locally and confirm its status reads as before.
- Step 4b: run each job locally and confirm the new DetailedStatus, and cancel one to see a partial status.
- Local data is never edited by hand to set up a test.
