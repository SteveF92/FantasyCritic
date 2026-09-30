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
- **The flag is admin only.** Its toggle sits directly under the action processing mode toggle on the admin console, shown only to admins.
- **Admin notification emails fail loud.** `PostmarkEmailSender.SendEmailAsync` catches and logs every failure, which is right for the public bid loop (one bad address mustn't stop the rest) and wrong for an email whose job is to tell Steve something is wrong. Notifications use a new sender method that throws, so a failed email turns into a failed job.
- **Steve's address is hard-coded:** `steve.fallon@fantasycritic.games`, as on the contact page.
- **Two guards keep automated processing out of beta.** `MySQLBetaCleaner` sets the job's RunType to `Disabled` on every restore, as it does for RefreshPatreonInfo, so beta never schedules it. If someone re-enables it there, the not-production stop reason still acts as a forced `EnableAutomatedActionProcessing = false`.
- **Beta sends the stop email, clearly labelled.** With the job disabled there, it's only sent when Steve deliberately runs it, which is how the email gets tested end to end. Outside production, the subject starts with the environment name (beta's is `Staging`) and the body names the base address it came from.
- **The change request check covers only games in play.** Unanswered change requests whose master game appears in an active pickup bid, as a bid's conditional drop, or in an active drop request, across all active years.
- **Top bids and drops runs once.** `AdminService.ProcessActions` calls it at the end of each year's processing today. It skips a week it already has, so the second call does nothing. After the move it runs once, after all years. No change in result.

## Steps

Each step: build, test, commit alone, then stop for review before the next.

### Step 0: Commit this plan

### Step 1: The EnableAutomatedActionProcessing flag, controlling nothing

- Migration: `ALTER TABLE tbl_meta_systemwidesettings ADD COLUMN EnableAutomatedActionProcessing BIT(1) NOT NULL DEFAULT b'0'`.
- `SystemWideSettings`, `SystemWideSettingsEntity`, `IFantasyCriticRepo.SetEnableAutomatedActionProcessing`, the MySQL repo and the FakeRepo, `InterLeagueService`.
- AdminController (admin only): turn on, turn off, and a GET for the current value.
- Admin console: a toggle below the action processing mode toggle, `v-if="isAdmin"`, synced from the GET the way the mode switch is synced from bid times.
- Integration test: an admin turns it on and off and reads it back; a non-admin is refused.
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

### Step 4: Rename PrepareForActionProcessing to FullAutomatedActionsProcess, cron only, with no logic change

- Migration: insert the `FullAutomatedActionsProcess` row in `tbl_job_type` (display name "Full Automated Actions Process", RunType `Cron`), repoint existing `tbl_job` rows to it, delete the old row. `tbl_job.JobType` has a foreign key to `tbl_job_type.Name` without ON UPDATE CASCADE, so an in-place rename would fail.
- `FantasyCriticJobType`, the handler class, the registry and its FullDataRefresh skip-when-due entry, `recentJobsTable.vue`.
- `MySQLBetaCleaner` disables FullAutomatedActionsProcess, next to `DisablePatreon`. It's here rather than earlier so it names the job's final type.

### Step 5: The checks, the email and the happy path

- The handler collects the stop reasons after the snapshot, sends one notification listing them, and returns `Result.Failure` with the reasons.
- Otherwise it runs the `ActionProcessingRunner` (checking `CanProcessActions` first, like the manual job), then turns action processing mode off.
- The change request check: a query or a filter over `GetAllMasterGameChangeRequests` against the master games in active bids, conditional drops and drop requests for each active year.
- Tests: unit tests of the stop-reason logic; the notification's content through the capturing sender.

### Step 6: Check for cancellation before each write

The same pattern as the earlier rounds, across all three jobs and the two new utilities. Once actions are saved for a year, the job doesn't stop partway through the rest of that year.

### Step 7: Status and structured logs

The same pattern as the earlier rounds: `AppendDetailedStatus` for each finished step (per year for processing), and the stop reasons in the final status.

## Verification

- Build with zero warnings, unit tests, integration tests (with NSwag regenerated).
- Local runs: the flag toggle on the admin console; FullAutomatedActionsProcess locally (non-production, so it stops with an Error, its reasons and a labelled email); ProcessActions and UpdateTopBidsAndDrops by hand, compared against a run before the move.
