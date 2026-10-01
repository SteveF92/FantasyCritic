# Automated action processing

## Context

The last round of job moves, after [remaining-jobs-plan.md](remaining-jobs-plan.md). It covers the three weekly action processing jobs:

| Job | Today | After this round |
|---|---|---|
| PrepareForActionProcessing | Cron. Mode on, full data refresh, snapshot and wait. | Renamed **FullAutomatedActionsProcess**, cron only. Prepares as today, then checks, then processes actions itself. |
| ProcessActions | Manual. `CanProcessActions`, then `AdminService.ProcessActions` per active year. | Unchanged in behavior. Its logic leaves AdminService so the automated job can share it. |
| UpdateTopBidsAndDrops | Manual. `AdminService.UpdateTopBidsAndDropsForMostRecentWeek`. | Unchanged in behavior. Becomes a shared unit, called by both processing jobs. |

Same workflow as the earlier rounds: at the lift-and-shift step, Claude scaffolds and Steve moves the logic by hand, then Claude reviews the move against the AdminService code.

## The FullAutomatedActionsProcess flow

1. Action processing mode on.
2. Full data refresh.
3. Snapshot the database and wait for it to be available.
4. Checks. Each failing check is a stop reason; the job collects all of them before stopping:
   - **Not production.** Automated processing never runs outside production (`EnvironmentConfiguration.IsProduction`). Outside production, action processing is always done by hand.
   - **Automated processing is off.** The new `EnableAutomatedActionProcessing` flag in `tbl_meta_systemwidesettings`.
   - **Pending master game change requests** for games that have an active bid or drop, where a missed correction could make those actions invalid.
5. If there is any stop reason: one email to Steve that lists every reason (and, for change requests, which games and requests), then the job ends in **Error** with the reasons in its detailed status. Stopping is abnormal and should be noticed. Action processing mode stays on, so Steve can finish by hand, as today.
6. Otherwise, the happy path: process actions, update top bids and drops, action processing mode off.

With the flag off in production, the job does exactly what PrepareForActionProcessing does today, plus the email. Public bidding needs nothing: it moves to the next week on its own.

## Decisions

- **The flag defaults off.** The migration adds it as `b'0'`, so nothing is automated until Steve turns it on.
- **The flag is on the ActionRunner permission**, where it belongs semantically, like action processing mode. Its toggle sits directly under the action processing mode toggle on the admin console. (Steve is the only action runner outside special occasions, and the permission may go away once automated processing is trusted.)
- **Admin notification emails fail loud.** `PostmarkEmailSender.SendEmailAsync` catches and logs every failure, which is right for the public bid loop (one bad address mustn't stop the rest) and wrong for an email whose job is to tell Steve something is wrong. Notifications use a new sender method that throws, so a failed email turns into a failed job.
- **Steve's address is hard-coded:** `steve.fallon@fantasycritic.games`, as on the contact page.
- **Two guards keep automated processing out of beta.** `MySQLBetaCleaner` sets the job's RunType to `Disabled` on every restore, as it does for RefreshPatreonInfo, so beta never schedules it. If someone re-enables it there, the not-production stop reason still acts as a forced `EnableAutomatedActionProcessing = false`.
- **Beta sends the stop email, clearly labelled.** With the job disabled there, it's only sent when Steve deliberately runs it, which is how the email gets tested end to end. Outside production, the subject starts with the environment name (beta's is `Staging`) and the body names the base address it came from.
- **The change request check covers only games in play.** Unanswered change requests whose master game appears in an active pickup bid, as a bid's conditional drop, or in an active drop request, across all active years.
- **Top bids and drops runs once, after every year is processed.** `AdminService.ProcessActions` called it at the end of each year's processing. All processing sets on one Eastern date make one week, and a week already written is skipped, so in December the first year's call wrote the week from its own set and the second year's bids were never counted. One call after the loop counts both.
- **The pending corrections check applies to manual processing too** (Steve's change, `6618c4fbf`). It lives in `AdminService.CanProcessActions`, so the Process Actions button, the ProcessActions job and the automated job all refuse while a game with an unprocessed bid, conditional drop or drop has an unanswered change request. It covers the bids and drops processing would pick up: leagues not deleted, years open for play and not finished. Test leagues count, since their bids are processed too.
- **The change request integration tests answer their own requests.** The check covers every league, and other tests leave unprocessed drops on the game those tests use, so an unanswered request there blocked every later processing test.

## Steps

Each step: build, test, commit alone, then stop for review before the next.

Steve took Step 5 ahead of the end of Step 4 (`6618c4fbf`), so Steps 3–5 landed in a different order than written. Where each piece stands is under each step.

### Step 0: Commit this plan

### Step 1: The EnableAutomatedActionProcessing flag, controlling nothing

- Migration: `ALTER TABLE tbl_meta_systemwidesettings ADD COLUMN EnableAutomatedActionProcessing BIT(1) NOT NULL DEFAULT b'0'`.
- `SystemWideSettings`, `SystemWideSettingsEntity`, `IFantasyCriticRepo.SetEnableAutomatedActionProcessing`, the MySQL repo and the FakeRepo, `InterLeagueService`.
- ActionRunnerController: turn on, turn off, and a GET for the current value.
- Admin console: a toggle below the action processing mode toggle, in the action runner section, loaded from the GET.
- Integration test: an action runner turns it on and off and reads it back; anyone else is refused.
- Regenerate the NSwag clients.

### Step 2: Admin notification emails

- `IEmailSender.SendEmailOrThrow(email, subject, htmlMessage)`: Postmark sends without the catch and throws when the result isn't Success. `NullEmailSender` and `CapturingEmailSender` implement it.
- `EmailSendingService.SendAdminNotification(subject, lines)`: builds a plain HTML body, one paragraph per line, no Razor template, and sends it to the hard-coded address with `SendEmailOrThrow`.
- `EnvironmentConfiguration` gains `EnvironmentName`, from `IHostEnvironment.EnvironmentName`. Outside production the subject is prefixed `[<EnvironmentName>]` and the body's first line says which environment and base address sent it.
- Test: the capturing sender receives the email at Steve's address with the subject and lines, prefixed outside production.
- Nothing calls it yet.

### Step 3: Lift and shift ProcessActions and UpdateTopBidsAndDrops, with no logic change

Claude scaffolds; Steve moves the logic.

- New `Jobs/Utilities/TopBidsAndDropsUpdater`: `UpdateTopBidsAndDropsForMostRecentWeek` and the private per-week method, from AdminService.
- New `Jobs/Utilities/ActionProcessingRunner`: the year loop from ProcessActionsJobHandler and the body of `AdminService.ProcessActions`, then `TopBidsAndDropsUpdater` once after the loop.
- ProcessActionsJobHandler: `CanProcessActions` (stays in AdminService, since the controller also checks it when the button is pressed), then the runner.
- UpdateTopBidsAndDropsJobHandler: calls `TopBidsAndDropsUpdater`.
- AdminService loses `ProcessActions` and the three `UpdateTopBidsAndDrops*` methods, including the `LocalDate` overload that has no callers. `GetActionProcessingDryRun` stays, since the dry run controller uses it, and the runner calls it.
- Register both utilities with the other job utilities. Update the comment in `TopBidsAndDropsRecomputeMigration`.

**Done:** scaffold `84dab9b00`, Steve's move `6ac3aaf59` (reviewed: nothing dropped). Top bids and drops moved after the loop and AdminService's unused `DiscordPushService` removed in `7cf6a1fd7`.

### Step 4: Rename PrepareForActionProcessing to FullAutomatedActionsProcess, cron only, with no logic change

- Migration: insert the `FullAutomatedActionsProcess` row in `tbl_job_type` (display name "Full Automated Actions Process", RunType `Cron`), repoint existing `tbl_job` rows to it, delete the old row. `tbl_job.JobType` has a foreign key to `tbl_job_type.Name` without ON UPDATE CASCADE, so an in-place rename would fail.
- `FantasyCriticJobType`, the handler class, the registry and its FullDataRefresh skip-when-due entry, `recentJobsTable.vue`.
- `MySQLBetaCleaner` disables FullAutomatedActionsProcess, next to `DisablePatreon`. It's here rather than earlier so it names the job's final type.

**Done:** the handler class rename is Steve's (`6618c4fbf`); the rest is `38d21a11e`. The new row's Severity is `Danger`, like ProcessActions, since the job now processes actions. The snapshot keeps its `pre-action-processing` name.

### Step 5: The checks, the email and the happy path

- The handler collects the stop reasons after the snapshot, sends one notification listing them, and returns `Result.Failure` with the reasons.
- Otherwise it runs the `ActionProcessingRunner` (checking `CanProcessActions` first, like the manual job), then turns action processing mode off.
- The change request check: a query against the master games in active bids, conditional drops and drop requests for each active year.
- Tests: unit tests of the stop-reason logic; the notification's content through the capturing sender.

**Done:** the handler checks `CanProcessActions` and the flag after the snapshot, then runs the runner (Steve, `6618c4fbf`). `IMasterGameRepo.GetGamesWithPendingBidsOrDropsThatHavePendingCorrections`, its query, and an integration test that the button is refused (`7cf6a1fd7`).

**Not done yet**, in this order:

#### Step 5a: One function for every check, and a pre-check email button

`AdminService.GetReasonsNotToProcessActions(bool isAutomatedRun, Instant processingTime)` returns the reasons as `IReadOnlyList<string>`, empty meaning go. It replaces `CanProcessActions`; the Process Actions button and the ProcessActions job join the reasons into their `BadRequest`/`Result.Failure`.

| Check | Manual run | Automated run / pre-check |
|---|---|---|
| Action processing mode is off | ✓ | — (the job turns it on itself) |
| Wrong day (production only) | ✓ | ✓ |
| Pending corrections on games with bids/drops | ✓ | ✓ |
| Not production | — | ✓ |
| EnableAutomatedActionProcessing is off | — | ✓ |

- The day check uses `processingTime`, not today: manual runs and the job pass now; the pre-check passes `clock.GetNextBidTime()`, so pressing it midweek doesn't report the weekday. The day check still matters to the job: a worker turned off over a weekend could pick the queued job up on Monday.
- AdminService only returns reasons and never sends email; AdminService is registered by hosts without email, so the callers send it.
- `ActionRunnerController.SendActionProcessingPreCheckEmail`, run in the request: emails "Action processing pre-check" with the reasons, or a line saying nothing would stop the next automated run.
- Admin console: a "Send Action Processing Pre-Check Email" button in the Action Processing section.
- Integration test: the button emails Steve, labelled `[Development]`, with the not-production reason.

#### Step 5b: The job uses it

- **The production guard and the flag** come from `GetReasonsNotToProcessActions(isAutomatedRun: true, now)`; the job stops once with every reason.
- **The email**: "Automated action processing stopped", the reasons, and a line saying action processing mode is still on.
- **Unexpected failures email too**: if the job throws, it emails Steve and rethrows, so it still ends in Error. No email on success (Steve's call).
- **Action processing mode off** after a successful run.

**Done:** 5a is `4f72e1e0e`: the function, the button and its integration tests; the job got the production guard from it. 5b is the commit after it. A cancellation sends an email too (Steve: it's an odd enough case to want one), then rethrows, so the job still ends cancelled. If the failure email also fails, both exceptions go on the job as an `AggregateException`.

Not covered by an automated test: the job itself. The test factory registers the real `RDSManager`, so running the job there would take a real snapshot. The stop email's reasons are the pre-check's, which is tested. The job's own path gets its first real run on beta, with the job's RunType set back to `Cron` there for one run.

### Step 6: Check for cancellation before each write

The same pattern as the earlier rounds, across all three jobs and the two new utilities. Once actions are saved for a year, the job doesn't stop partway through the rest of that year.

### Step 7: Status and structured logs

The same pattern as the earlier rounds: `AppendDetailedStatus` for each finished step (per year for processing), and the stop reasons in the final status.

Known now: the runner's per-year `UpdateDetailedStatus` and the handler's final "Processed actions for all active years." replace the appended mode, refresh and snapshot clauses.

## Verification

- Build with zero warnings, unit tests, integration tests (with NSwag regenerated).
- Local runs: the flag toggle on the admin console; FullAutomatedActionsProcess locally (non-production, so it stops with an Error, its reasons and a labelled email); ProcessActions and UpdateTopBidsAndDrops by hand, compared against a run before the move.
