# Multi-Draft Leagues — Final Code Review

**Date:** 2026-06-28
**Branch:** `multi-draft-leagues` vs `main` (merge-base `20bccd7`)
**Scope reviewed:** 194 files, ~21.7k insertions / ~1.5k deletions — DB schema + repos, domain (`FantasyCritic.Lib`), Web API/viewmodels, and the ClientApp Vue surface.
**Method:** Four parallel slice-focused reviews (DB, domain, Web, ClientApp) checked against every relevant design spec, plus independent verification/calibration of the highest-severity findings.

> **Overall verdict:** This is in strong shape. The schema, stored-procedure migration, auto-skip engine, `BidsOnlyBeforeNextScheduledDraft` logic, viewmodel reshaping, and the frontend `playStatus`→per-draft migration are all complete and correct. No critical/blocking defects found. The items below are a mix of a few genuine gaps, several intentionally-deferred pieces worth confirming, and polish.

---

## Already fixed during this review

- **Inline entities split out** of `MySQLConferenceRepo.cs` into `Entities/Conferences/` (`ConferencePublisherInfoEntity`, `ConferenceDraftInfoEntity`, `ConferenceDraftPositionEntity`), renamed to the `*Entity` convention per the architecture rule.
- **Removed 3 redundant `.ToDateTimeUnspecified()`** calls on `LeagueDraft.ScheduledDate` in `MySQLFantasyCriticRepo.cs` (the `LocalDate` Dapper handler is registered and sets the correct `MySqlDbType.Date`).
- Verified clean build (`FantasyCritic.MySQL`, 0 warnings with `TreatWarningsAsErrors`).

---

## Deferred / missing work — confirm these were intentional

This is the category you were most worried about. Good news: the markers are clean (`TODO(Phase2-MultiDraft)` fully removed; no stray TODO/FIXME in feature code). The items below are real absences — most are **explicitly deferred in the specs**, but listed here so you can make the call.

| # | Item | Status | Evidence |
|---|------|--------|----------|
| D1 | **`AddNewLeagueYear` (rollover) is single-draft only** — a multi-draft league rolling into a new year gets one "Initial Draft"; users re-add drafts via Manage Drafts. | **Explicitly deferred** in `create-league-multi-draft-design.md` ("user explicitly deferred") and `conference-clone-fix-design.md`. | `FantasyCriticService.cs:288-294`; `LeagueManagerController.cs:124-174` |
| D2 | **Overdue-draft soft warning not implemented** — community preview promises a "soft reminder to all players" when a scheduled draft's date has passed. No past-due logic exists in the ClientApp. | **Gap vs community preview** (no spec deferral found). | preview §"Starting a Draft"; `multi-draft-leagues-design.md:307`; no `overdue`/past-date code in ClientApp |
| D3 | **Publisher-list reorder by next-draft order not implemented (frontend)** — preview/spec say the league publisher list should reorder to the next draft's order once it's set. The backend exposes `DraftForPublisherDisplayOrder`, but `leagueGameSummary.vue` renders publishers as-is. | **Gap vs preview** — worth confirming whether backend ordering already covers it. | `multi-draft-leagues-design.md:309`; `leagueGameSummary.vue:23` |
| D4 | **Conference draft-setting sync (commissioner ↔ sub-league) not enforced** | **Explicitly out of scope** per `multi-draft-leagues-design.md:344`. | — |
| D5 | **Trailing auto-skips not persisted when a draft ends on skips only** — if the final turns of a draft are all skips, `GetDraftStatus` returns null and the queued `PicksToSkip` are discarded, so those final skip rows are never written to history. Draft still completes correctly. | Minor history-completeness gap. | `DraftFunctions.cs:72-76`; `DraftService.cs:221-246` |
| D6 | **Auto-persisting `PicksToSkip` was labeled "out of scope"** in `draft-skip-manager-action-design.md`, but **was actually implemented** in `AutoDraftForLeague`. Confirmed present — calling out only because the spec text is now stale. | Done (spec text stale). | `DraftService.cs:233-247` |

---

## Bugs / correctness (actionable)

### Medium

- **M1 — Create-as-Multi-Draft hides the "bids only before next draft" checkbox.** `createLeague.vue` never passes `:is-multi-draft` to `<leagueYearSettings>`, so the `bidsOnlyBeforeNextScheduledDraft` checkbox (gated `isMultiDraft && enableBids`) cannot appear during creation even when the user picks Multi Draft and enables bids. The edit flow passes it correctly.
  - `createLeague.vue:44`; `leagueYearSettings.vue:91,324`; `editLeague.vue:23`

- **M2 — `CreateLeagueDraftRequest` has no validation of `AdditionalStandardGames >= 0`.** The spec requires slot expansion be additive ("never decrease"). A negative value would shrink `StandardGames` via `WithNewDraftOptions`. The only guard is `NewSpecialGameSlots.Count > AdditionalStandardGames`.
  - `CreateLeagueDraftRequest.cs` (no `IsValid`); `DraftService.cs:392-396`; `LeagueOptions.cs:286-288`

- **M3 — `SkipCurrentDraftPick` doesn't push SignalR/Discord updates.** After a skip, the service runs auto-draft and may complete the draft, but the controller only calls `RefreshLeagueYear` — missing `PushDraftMessages` (the `DraftFinished` SignalR + next-pick Discord notifications other draft endpoints send). `UndoLastDraftAction` has the same omission.
  - `LeagueManagerController.cs:1176-1184`, `1145-1153`; compare `:861`, `:1098`

- **M4 — `OneShotMode` is the only `LeagueController` bid/drop gate, and it conflates one-shot with bids-off multi-draft.** Bids/drops are blocked when `leagueYear.OneShotMode` is true (`LeagueController.cs:930,1385`). For a multi-draft league with bids off whose per-draft game counts still sum to the league totals, `OneShotMode` evaluates true — which blocks **special-auction** bidding that the design says should work for bids-off leagues.
  - **Calibration:** the spec's `EnableBids` and `!IsAnyDraftInProgress` rules *are* enforced (line 936-939 for `EnableBids` with a special-auction carve-out; `RequiredYearStatus.YearNotFinishedNoDraftsActive` at line 907 for in-progress). So this is a narrow edge case, not "no gating." Consider replacing the `OneShotMode` checks with the explicit spec predicates, and add a `Drafts.Count > 1` short-circuit to `OneShotMode` so the 3-way category and the gate agree.
  - `LeagueYear.cs:79-86`; `LeagueController.cs:930-939,1385`

### Low

- **L1 — `pendingDraft` can point at a future draft while an earlier draft is active.** `pendingDraft = drafts.find(NotStartedDraft)` returns draft 2 while draft 1 is `Drafting`/`Paused`, so `draftReadinessBanner` can surface draft-2 readiness mid-draft-1. Consider guarding the banner with `!activeDraft`.
  - `leagueMixin.js:42-43`; `draftReadinessBanner.vue` (`v-if="pendingDraft"`)
- **L2 — `firstDraft()` throws instead of returning `null`** when `drafts` is empty/unloaded (spec wanted `?? null`). Defensible as fail-loud since every league-year has ≥1 draft, but a reactive eval before load could crash. `leagueMixin.js:36-40`
- **L3 — `GetConference` `AtLeastOneDraftStarted` still filters `ld.DraftNumber = 1`.** Functionally equivalent under sequential-draft rules (draft 1 always starts first) but diverges from `sp_getconferenceyeardata`, which aggregates all drafts. `MySQLConferenceRepo.cs:269-272`
- **L4 — `DeleteLeagueDraft` deletes publishers before skips.** FK `FK_draftpickskip_draftpublisher` would block if skip rows existed; the service only allows deleting `NotStartedDraft` drafts (no skips), so it's defensive hardening, not a live bug. Compare `ResetDraft` which deletes skips first. `MySQLFantasyCriticRepo.cs:1331-1339` vs `:1034-1043`
- **L5 — `AddNewConferenceYear` clears `ScheduledDate`** on cloned primary-league drafts, while `ConferenceService.AddNewLeagueYear` preserves it for satellite leagues. Confirm the rollover-clears-dates intent. `MySQLConferenceRepo.cs:175-179`
- **L6 — `conference.vue:167` uses bitwise `&`** (`x.draftStarted & !x.draftFinished`). Works in a `filter` (truthy/falsy) but should be `&&`. Cosmetic.

---

## Style / polish

- **Skip audit uses `LeagueAction(managerAction:true)`** rather than a `LeagueManagerAction` row (spec table said `LeagueManagerAction`/`"SkippedDraftTurn"`; impl uses `"Draft Pick Skipped"`). Tests assert the league-action feed. Confirm this is the intended audit surface. `DraftService.cs:140-142`
- **Value-tuple Dapper row shapes** in `MySQLFantasyCriticRepo.cs:76-77, 2136` — prefer `*Entity` types under `Entities/` per repo rules.
- **`PlayStatus` enum values interpolated into UPDATE SQL** (`MySQLFantasyCriticRepo.cs:1019,1027,1035,1054`) — not injectable (constants), but inconsistent with the parameterized `@playStatus` style elsewhere.
- **`LeagueDraftViewModel`** lives under `Models/Requests/League/` but has namespace `...Models.Responses`; `PublisherDraftInfo` is `List<>` not `IReadOnlyList<>`. `CreateLeagueDraftRequest.NewSpecialGameSlots` is `List<>`. (`StartDraftRequest` also lives in `StartPlayRequest.cs`.)
- **Dead branch:** `DraftPhase.Complete` is never produced by `DraftStatus.DraftPhase`. `DraftService.cs:228-231`
- **Leftover debug `console.log`** — `league.vue:377,389`; `setPauseModal.vue:31`.
- **Typo** — `manageDrafts.vue:115`: "you already all the slots".
- **Manage Drafts route** is `/manageDrafts/:leagueid/:year` vs spec's `/league/:leagueID/:year/manage-drafts`; the draft-management routes live in `LeagueManagerController` at `/api/leagueManager/...` vs the design doc's `/api/League/...`. Both consistent with existing conventions; specs are stale.
- **Pre-existing, multi-draft-adjacent:** `ActionProcessor` bid tiebreak uses `Publisher.FirstDraftInfo.DraftPosition` (`:491`), which can drift from a later draft's order. Not introduced by this branch.

---

## Documentation-vs-code drift (not defects)

- **`CurrentDraft` was never added** — deliberately split into `PendingDraft` + `ActiveDraft` (per `draft-status-per-draft-design.md`). No stale references exist. The Phase-2 spec text referring to `CurrentDraft` is the thing that's out of date, not the code.
- **`CreateLeague` takes a `drafts[]` array**, not the spec's `secondDraft` block — a deliberate later evolution (`create-league-multi-draft-design.md`), fully tested.
- **`SetDraftOrder` validates against `PendingDraft`** (correct — no draft is in progress when order is set) and **`ResetDraft` against `ActiveDraft`** (correct — gated `DuringDraft`).
- **`GetLeagueYearsInConferenceYear.DraftStarted` keyed to draft 1** is intentional per `conference-clone-fix-design.md` ("first draft started is the right signal for player-assignment locking").

---

## Confirmed correct / strong areas

- **Migrations complete:** `2026-06-27_000_multiDraftLeagues.sql` (`tbl_league_draft`, `tbl_league_draftpublisher`, `tbl_league_draftpickskip`, `EnableBids`, `DraftID` FK + dropped legacy columns), `2026-06-28_000_draftPickSkipIsManual.sql` (`IsManualSkip` ADD-default-then-drop), `2026-06-28_001_bidsOnlyBeforeNextScheduledDraft.sql`.
- **Stored-procedure `DraftNumber = 1` removal complete** across all idempotent SPs, each mapped to the correct primitive (A = AnyDraftStarted, B = most-recent-year category, C = DraftIsActiveOrPaused). Zero stray `DraftNumber = 1` in `Scripts/Idempotent/`. PlayStatus enum strings match between SQL and C#.
- **`BidsOnlyBeforeNextScheduledDraft`** matches the decision table exactly (non-overridable hard fail, `>=` boundary, null handling, skipped on drop/draft), threaded through every layer, with unit coverage asserting `Overridable == false`.
- **Auto-skip + manual skip + undo** all correct: skip when no open slot, skip rows persisted, `OverallPickNumber` not advanced on skip, snake double-skip handled, completion when no `NextPick`, and undo distinguishes "Draft Pick Undone" vs "Draft Skip Undone".
- **Draft CRUD guards** correct: delete rejects draft #1 and started drafts; edit allows Name always, other fields only when not started; create blocked while a draft is in progress.
- **ViewModels reshaped** to the always-present `Drafts` list; `CompleteFirstDraftPlayStatus` and `PlayStatusViewModel` deleted with zero references; `ConferenceLeagueYearViewModel.DraftFinished` aggregates all drafts; `ActiveDraftNumber` added; `OneShotMode` bool replaced by the 3-way `MostRecentYearType`; `AnyDraftStarted` replaces raw play status.
- **Frontend `playStatus` migration complete** — zero stale `leagueYear.playStatus` reads; all per-draft reads go through `firstDraft`/`pendingDraft`/`activeDraft` helpers.
- **Conference clone** copies all drafts (add-league, add-year, new-conference-year) and `AssignLeaguePlayers` rebuilds positions per-draft inside the transaction.
- **Authorization** on all new draft endpoints is `LeagueManager`-gated with correct `RequiredYearStatus`; manager actions are audited.

---

## Suggested priority order for follow-up

1. **M1** (create-flow checkbox hidden) and **M2** (negative slot validation) — quick, user-facing.
2. **M3** (skip/undo notifications) — affects live draft UX.
3. **D2 / D3** (overdue warning, publisher reorder) — confirm whether intended for this release vs deferred; they're promised in the community preview.
4. **M4 / L3** (OneShotMode conflation + conference `DraftNumber=1` consistency) — tidy the gating to use the explicit predicates.
5. Low/style items as time permits.
