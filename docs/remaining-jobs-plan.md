# The remaining jobs own their logic

## Context

The third round after [job-handler-logic-plan.md](job-handler-logic-plan.md) (the Royale quarter jobs) and [data-refresh-jobs-plan.md](data-refresh-jobs-plan.md) (the data refresh jobs). This round covers every job left except the two weekly action processing jobs, PrepareForActionProcessing and ProcessActions, which come last in a round of their own. Same order as before: lift and shift, call repos directly, cancellation, then status and logging.

Same workflow as the data refresh round: at Step 1, Claude scaffolds and Steve moves the logic by hand, then Claude reviews the move against the AdminService code.

## Scope

### Jobs whose logic is still in AdminService

| Job | AdminService code | Goes to |
|---|---|---|
| ExpireTrades | `ExpireTrades` | the handler's `Run` |
| GrantSuperDrops | `GrantSuperDrops` | the handler's `Run` |
| MakeSlotsConsistent | `MakePublisherSlotsConsistent` | the handler's `Run` |
| ProcessSpecialAuctions | `AnyUnprocessedSpecialAuctions`, `ProcessSpecialAuctions`, `ProcessSpecialAuctionsForYear`, `GetSpecialAuctionResults` | `ShouldSchedule`, `Run`, and two private methods |
| RecalculateLastSeasonWinners | `RecalculateWinners` | the handler's `Run` |
| RecomputeRulesBasedRoyaleGroups | `RecomputeRulesBasedRoyaleGroups`, `ComputeRulesBasedMembers`, `ComputePreviousWinners` | `Run` and two private methods |
| RefreshPatreonInfo | `UpdatePatreonRoles` | the handler's `Run` |
| UpdateDailyPublisherStatistics | `UpdateDailyStats` | the handler's `Run` |

No unit here is shared between jobs, so unlike the data refresh round there are no new classes in `Jobs/Utilities`. Every one goes into its own handler.

### Jobs that already own their logic

PushGameReleaseMessages, SendReleasingThisWeekUpdate, SendAllPublicBiddingMessages, SendPublicBiddingDiscordMessages, SendPublicBiddingEmails and SnapshotDatabase. Nothing moves in Step 1. They join at Step 2 onward.

### Deferred to the action processing round: UpdateTopBidsAndDrops

`UpdateTopBidsAndDropsForMostRecentWeek` is shared: the UpdateTopBidsAndDrops job calls it, and so does `AdminService.ProcessActions`, as its last step. It can't leave AdminService while ProcessActions is still there:
- AdminService can't call a class in `Jobs/Utilities`: those are registered only in the worker, and Web resolves AdminService too, so Web's ValidateOnBuild would fail.
- Having ProcessActionsJobHandler call it after `ProcessActions` means editing a handler this round leaves alone.

**Proposal:** UpdateTopBidsAndDrops moves with ProcessActions in the last round, where it becomes a shared unit like `FantasyPointsUpdater`.
- Alternative: a `TopBidsAndDropsUpdater` class now, with ProcessActionsJobHandler calling it after `_adminService.ProcessActions` and ProcessActions no longer calling it itself. That is a small change in one of the two jobs we meant to leave.

### One touch to PrepareForActionProcessing, in Step 2

`DatabaseSnapshotJobUtilities` takes AdminService only for `StartDatabaseSnapshot` and `GetDatabaseSnapshot`, both one-line passthroughs to `IRDSManager`. SnapshotDatabase and PrepareForActionProcessing both call the utility. Step 2 switches it to `IRDSManager`, so PrepareForActionProcessing injects `IRDSManager` in place of AdminService. That changes only which object it passes, as it did in the data refresh round.
- Alternative: leave the snapshot passthroughs for the last round too.

## Steps

Each step: build, test, commit alone, then stop for review.

### Step 0: Commit this plan

### Step 1: Lift and shift, with no logic change

**Claude scaffolds:**
- Each handler in the first table gets the dependencies its moved code needs, as fields and constructor parameters. It keeps AdminService and its current `Run` body until Steve moves the code.

  | Handler | Injects |
  |---|---|
  | ExpireTrades | InterLeagueService, IFantasyCriticRepo, IClock |
  | GrantSuperDrops | InterLeagueService, IFantasyCriticRepo, DiscordPushService, IClock |
  | MakeSlotsConsistent | InterLeagueService, IFantasyCriticRepo |
  | ProcessSpecialAuctions | InterLeagueService, IFantasyCriticRepo, IMasterGameRepo, DiscordPushService, IClock |
  | RecalculateLastSeasonWinners | InterLeagueService, IFantasyCriticRepo, FantasyCriticService |
  | RecomputeRulesBasedRoyaleGroups | RoyaleService, ILogger\<T\> |
  | RefreshPatreonInfo | FantasyCriticUserManager, PatreonService |
  | UpdateDailyPublisherStatistics | InterLeagueService, RoyaleService, IDailyStatsRepo, IClock |

- Empty private method signatures for ProcessSpecialAuctions (`ProcessSpecialAuctionsForYear`, `GetSpecialAuctionResults`) and RecomputeRulesBasedRoyaleGroups (`ComputeRulesBasedMembers`, `ComputePreviousWinners`).
- Hosting comments: `AddFantasyCriticAdminServices` and `AddFantasyCriticPatreon` say AdminService is why a host needs Patreon. After the move, Web needs it for `ExternalLogins` and the worker for RefreshPatreonInfo.

**Steve moves:**
- Each body verbatim into its handler, keeping the method names for the private helpers.
- Two substitutions, since the moved code can no longer reach AdminService:
  - `GetLeagueYears(year)` becomes `_fantasyCriticRepo.GetLeagueYears(year)`. It's a one-line passthrough, used by GrantSuperDrops and ProcessSpecialAuctionsForYear. AdminService keeps it for ProcessActions and `ActionRunnerController`.
  - The static `_logger.Information(...)` lines become the handler's `ILogger<T>` (`LogInformation`), wording unchanged, still interpolated where they were. Step 4 makes them structured.
- Delete the moved methods from AdminService, and drop the dependencies nothing uses any more: `FantasyCriticService`, `FantasyCriticUserManager`, `PatreonService`, `RoyaleService` and `IDailyStatsRepo`. AdminService has no log lines left, so its static logger goes too.
- Remove AdminService from each handler that no longer needs it.
- The cancellation token stays as it is today.

**Claude reviews** against AdminService at `c4ff1960f`: dropped lines, changed conditions, missed callers, and leftover dependencies and usings.

### Step 2: Call the repos directly, with no change in behavior

Same rule as before: a job may call a repo directly, but SQL never moves into the job layer.

- `InterLeagueService.GetSystemWideValues`, `GetSupportedYears` and `GetMasterGameYears` are one-line passthroughs. The handlers and `PublicBiddingJobUtilities` call `IFantasyCriticRepo` / `IMasterGameRepo` instead. The three stay on InterLeagueService for the rest of the app. After this, no handler in this round injects InterLeagueService.
- `RoyaleService.GetYearQuarters` and `GetAllRoyaleGroupsByType`: call `IRoyaleRepo`. Both stay on RoyaleService, which the app still uses.
- Job-only passthroughs, called directly and then deleted:
  - `RoyaleService.SetRoyaleGroupMembers` → `IRoyaleRepo`
  - `FantasyCriticUserManager.GetAllPatreonUsers` and `UpdatePatronInfo` → `IFantasyCriticUserStore`
  - `AdminService.StartDatabaseSnapshot` and `GetDatabaseSnapshot` → `IRDSManager`. `DatabaseSnapshotJobUtilities` takes `IRDSManager`. SnapshotDatabase and PrepareForActionProcessing inject it in place of AdminService.
- `GetSystemWideSettings` stays on InterLeagueService, as before: it overlays the bid lock window.

### Step 3: Check for cancellation before each write

Same rule: `ThrowIfCancellationRequested` before each write and at the top of each loop over many items. Nothing is checked between a write and the messages that describe it, since the next run won't send them.

- **ExpireTrades, MakeSlotsConsistent, RecalculateLastSeasonWinners, UpdateDailyPublisherStatistics:** one write each; check just before it. ExpireTrades' existing check at the top moves down to the write.
- **GrantSuperDrops:** check before the repo write, not between it and `SendSuperDropMessages`. The next run skips publishers already granted, so their messages would be lost.
- **ProcessSpecialAuctions:** check before each year's `SaveProcessedActionResults`, after the reads and the processing, so a cancel during the slow `GetLeagueYears` still stops before that year writes. Not between the save and `SendActionProcessingSummary`.
- **RecomputeRulesBasedRoyaleGroups:** check at the top of each group.
- **RefreshPatreonInfo:** check before `UpdatePatronInfo`, after the Patreon call. The Patreon HTTP call doesn't take a token; a cancel waits at most one round trip.
- **PushGameReleaseMessages, SendReleasingThisWeekUpdate, SendPublicBiddingDiscordMessages, SendPublicBiddingEmails:** check before the send.
- **SendAllPublicBiddingMessages, SnapshotDatabase:** already checked.

### Step 4: Add status and structured logs

Each handler reports once, with `UpdateDetailedStatus`, except the two with a loop, which use `AppendDetailedStatus` so a cancelled run shows how far it got. Wording is a first draft for review:

- **ExpireTrades:** `Expired 2 trades in 2026.` or `No trades to expire in 2026.` It runs hourly, so the no-op logs at Debug. Step 1 already replaced the handler's own "Expiring trades." Debug line with the moved Information one, so there's no duplicate left; the summary replaces that line.
- **GrantSuperDrops:** `Granted super drops to 5 publishers in 3 leagues for 2026.` or `No super drops to grant for 2026.`
- **MakeSlotsConsistent:** `Made publisher slots consistent for 2026.`
- **ProcessSpecialAuctions:** one clause per year: `2026: processed 3 special auctions in 2 leagues.` or `2026: nothing to process.`
- **RecalculateLastSeasonWinners:** `Recalculated winners for 2025: 1,234 leagues.`
- **RecomputeRulesBasedRoyaleGroups:** one clause per group: `Previous Winners: 12 members.`
- **RefreshPatreonInfo:** `Updated patron info for 40 users.`
- **UpdateDailyPublisherStatistics:** `Updated daily stats for 2026 on 2026-09-28.`
- **PushGameReleaseMessages:** `Sent release messages for 3 games: Game A, Game B, Game C.` or `No games released today.`
- **SendReleasingThisWeekUpdate:** `Sent 10 games releasing by 2026-10-05.`
- **SendPublicBiddingDiscordMessages / SendPublicBiddingEmails:** `Sent public bidding for 12 leagues.`
- **SendAllPublicBiddingMessages:** writes `Emails: succeeded; Discord: succeeded` on success too, not only on failure.

Logs:
- Structured properties (`{Year}`, `{Count}`, `{GroupName}`, `{GameName}`), replacing the interpolated lines ("Processing special auctions for {year}", PushGameReleaseMessages' three lines).
- Each state change and each run's summary at Information. "Nothing to do" at Debug, since most of these run hourly or every ten minutes.

## Found along the way, not changing

- `AdminService.UpdateTopBidsAndDropsForWeek(LocalDate)` is public and has no callers. Only a comment in `TopBidsAndDropsRecomputeMigration` mentions it. Worth deleting in the last round, when UpdateTopBidsAndDrops moves.
- ExpireTrades and MakePublisherSlotsConsistent act on the latest open year (`MaxBy`); GrantSuperDrops on the earliest (`MinBy`). All three throw a NullReferenceException through `currentYear!` if no year is open.
- `PatreonService.GetPatronInfo` logs through a static logger with interpolated strings. It isn't job code, so it stays.

## Verification

- Every step: `dotnet build src/FantasyCritic.slnx` with zero warnings, `dotnet test src/FantasyCritic.Test/FantasyCritic.Test.csproj`, and `scripts/Format.ps1 -Check`.
- Step 1: the integration tests, since AdminService's constructor changes and the test host registers the handlers. Start the worker locally once for its own ValidateOnBuild. Diff review is the main check. Run ExpireTrades, MakeSlotsConsistent, RecalculateLastSeasonWinners, RecomputeRulesBasedRoyaleGroups, UpdateDailyPublisherStatistics and ProcessSpecialAuctions once locally and confirm they complete as before.
  - GrantSuperDrops sends Discord messages and the public bidding jobs send email and Discord messages. Running them locally is your call.
  - Patreon is off in local copies of production, so RefreshPatreonInfo is build-only.
- Step 2: review, and one run each of the jobs whose dependencies changed, and one SnapshotDatabase run if RDS is reachable locally.
- Step 3: review. These jobs are too quick to cancel partway reliably, as with the Royale jobs.
- Step 4: run each job locally and confirm the new DetailedStatus.
- Local data is never edited by hand to set up a test.
