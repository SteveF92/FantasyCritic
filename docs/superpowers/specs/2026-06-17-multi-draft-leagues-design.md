# Multi-Draft Leagues — Phase 2 Design Spec

**Date:** 2026-06-17  
**Status:** Approved for implementation  
**Branch:** `multi-draft-leagues`  
**Prerequisite:** Phase 1 complete (see `.cursor/plans/multi-draft-phase1-complete.md`)

---

## Overview

Phase 2 makes the Phase 1 engine visible and usable. Leagues can now schedule and run multiple sequential drafts within a single year. The feature is delivered in five slices, each independently testable and reviewable.

### Key invariant

**ViewModels never vary between single and multi-draft leagues.** `LeagueYearViewModel` always exposes a `Drafts` list, even when it has exactly one entry. The frontend renders that list simply for single-draft leagues and with richer UI for multi-draft leagues. No conditional ViewModel shapes.

---

## Slices

| # | Slice | Deliverable |
|---|-------|-------------|
| 1 | Draft CRUD | API + Manage Drafts page to create/edit/delete scheduled drafts |
| 2 | Read path + UI | All read APIs handle N drafts; all UIs react correctly |
| 3 | Draft execution | Per-draft draft order, start/run, auto-skip mechanic |
| 4 | Create page presets | Standard / One-Shot / Multi-Draft shortcuts on league creation |
| 5 | Conference cloning | Verify draft rows clone correctly; fix `AssignLeaguePlayers` for N drafts |

---

## Domain Model Changes

### `LeagueYear` — new computed properties

**`CurrentDraft`** (`LeagueDraft?`)  
The first draft (by `DraftNumber`) whose `PlayStatus` is not `DraftFinal`. This is the authoritative target for all service operations that previously hard-coded `FirstDraft`. Returns `null` when all drafts are `DraftFinal`.

**`IsAnyDraftInProgress`** (`bool`)  
`CurrentDraft != null && CurrentDraft.PlayStatus != NotStartedDraft`.  
Used as the primary gate for bid, drop, and special auction blocking.

**One-shot → multi-draft conversion:**  
Adding a second draft to a league that started in one-shot mode is fully supported by the existing design. `OneShotMode` is a computed property on `LeagueYear`; once a second draft exists, the condition that made the league one-shot is no longer met and `OneShotMode` returns `false` automatically. No special migration or flag is needed.

**Bid / drop / special auction blocking rules** (replaces the old `OneShotMode` checks):
- Regular bid allowed: `leagueYear.Options.EnableBids && !leagueYear.IsAnyDraftInProgress`
- Drop allowed: `!leagueYear.IsAnyDraftInProgress`
- Special auction creation allowed: `!leagueYear.IsAnyDraftInProgress` (EnableBids is irrelevant — special auctions are commissioner-initiated and independent of the regular bid toggle; players can only bid on the specific special-auction game, so commissioners who want zero bids simply don't create special auctions)

(EnableBids is irrelevant to drops; if a commissioner wants to block drops between drafts they simply give players zero drop allowances.)

### `LeagueOptions` — `EnableBids` now exposed in UI

`EnableBids` (bool) already exists on `LeagueOptions` from Phase 1 but was never surfaced. Phase 2 exposes it in the league year settings UI. Default values by preset:

| Preset | `EnableBids` default |
|--------|----------------------|
| Standard League | `true` |
| One Shot League | `false` |
| Multi Draft League | `false` |

Changes to `EnableBids` are recorded as a manager action in league history (same as any other settings edit).

### Phase 1 `TODO(Phase2-MultiDraft)` markers

All service methods that currently hard-code `leagueYear.FirstDraft` for lifecycle operations switch to `leagueYear.CurrentDraft`:

- `StartDraft`, `PauseDraft`, `ResumeDraft`, `CompleteDraft`, `ResetDraft`
- `SetDraftOrder`
- `EditLeagueYear` (draft count update)
- `CreatePublisher`, `DeletePublisher` (draft position rows)
- `AddNewLeagueYear`, `AddNewConferenceYear`
- All stored procedures that use `AND ld.DraftNumber = 1` replace this filter with the appropriate current-draft logic

---

## Database Schema Changes

Phase 1 already contains all required schema. No new migrations needed for Phase 2.

The only SQL change is updating stored procedures to replace `AND ld.DraftNumber = 1` filters with either:
- A join to the current (first non-DraftFinal) draft where applicable, or
- An aggregate or ordered subquery where the procedure needs summary data across all drafts

---

## API: New Endpoints

All three endpoints are commissioner-only and record a manager action on success.

### `POST /api/League/CreateLeagueDraft`

Creates a new draft for an existing league year. Optionally expands the league's slot count in the same atomic transaction.

**Request:**
```json
{
  "leagueID": "...",
  "year": 2026,
  "name": "Winter Draft",
  "scheduledDate": "2026-02-01",        // nullable
  "gamesToDraft": 3,
  "counterPicksToDraft": 1,
  "additionalStandardGames": 2,          // 0 = no change; must be >= 0
  "newSpecialSlots": [ ... ]             // [] = no change; appended to existing
}
```

Server validates:
- `additionalStandardGames >= 0` (never decrease)
- No existing special slots are modified (append-only)
- The resulting `GamesToDraft` across all drafts does not exceed `StandardGames` (soft warning; not blocked, since managers may plan to add more slots later)

If `additionalStandardGames > 0` or `newSpecialSlots` is non-empty, the league year is updated atomically with the draft creation.

### `POST /api/League/EditLeagueDraft`

Edits an existing draft. `Name` is always editable. Other fields are only accepted if the draft is `NotStartedDraft`; the server returns an error otherwise.

**Request:**
```json
{
  "draftID": "...",
  "leagueID": "...",
  "year": 2026,
  "name": "Spring Draft",
  "scheduledDate": "2026-03-15",
  "gamesToDraft": 4,
  "counterPicksToDraft": 1
}
```

Only records a manager action if at least one meaningful field changed.

### `POST /api/League/DeleteLeagueDraft`

Deletes a scheduled draft. Rejected unless `DraftNumber > 1` AND `PlayStatus == NotStartedDraft`.

**Request:**
```json
{
  "draftID": "...",
  "leagueID": "...",
  "year": 2026
}
```

---

## API: Modified Endpoints

### `POST /api/League/CreateLeague` (and `CreateLeagueYear`)

The request body gains an optional `secondDraft` block:

```json
{
  "secondDraft": {
    "name": "Draft 2",
    "scheduledDate": null,
    "gamesToDraft": 5,
    "counterPicksToDraft": 1
  }
}
```

If present, the server creates the league year and the second draft **in a single DB transaction**. Internally this reuses the `CreateLeagueDraft` service logic, but from the user's perspective (and the HTTP layer) it is one atomic request. Slot counts for the second draft must fit within the already-specified `StandardGames` for the new league year.

### `POST /api/League/EditLeagueYear`

When the league has 2+ drafts, `GamesToDraft` and `CounterPicksToDraft` are ignored in the request body (draft counts are managed via `EditLeagueDraft`). The server does not error on their presence; it simply does not apply them.

### `POST /api/League/SetDraftOrder`

Gains an explicit `draftID` parameter. The server validates that the supplied `DraftID` matches `CurrentDraft` and rejects the request if not. The UI always passes the correct `DraftID` behind the scenes — users never choose it themselves. "Inverse Standings" option computes the order from current league year standings (by projected points) at the moment the endpoint is called.

### `POST /api/League/ResetDraft`

Gains an explicit `draftID` parameter. The server validates that the supplied `DraftID` matches `CurrentDraft`. Resetting allows a commissioner to then edit or delete a second draft that had already been started. The UI passes the `DraftID` behind the scenes.

---

## ViewModels

### New: `LeagueDraftViewModel`

Always included in `LeagueYearViewModel.Drafts` (a list, even for single-draft leagues):

```json
{
  "draftID": "...",
  "draftNumber": 1,
  "name": "Initial Draft",
  "scheduledDate": "2026-01-15",
  "gamesToDraft": 5,
  "counterPicksToDraft": 1,
  "playStatus": "DraftFinal",
  "draftStartedTimestamp": "2026-01-15T19:00:00Z",
  "draftOrderSet": true
}
```

### Modified: `LeagueYearViewModel`

- `drafts: LeagueDraftViewModel[]` — new field, always present
- `enableBids: bool` — new field
- `playStatus` — computed from `CurrentDraft` instead of `FirstDraft`. `DraftFinished = true` means all drafts are `DraftFinal` (i.e. `CurrentDraft == null`).

---

## Draft Execution: Auto-Skip

**Valid-to-start check** (for drafts 2+):
At least one publisher must have enough open standard game slots to draft at least one game. If `CounterPicksToDraft > 0`, at least one publisher must also have an open counter-pick slot. This is a publisher-level check, not purely a settings-level check.

**Auto-skip logic:**
When the draft engine advances to a publisher's turn:
1. Check if the publisher has an open slot of the required type (standard or counter-pick)
2. If not:
   - Create a `SkippedDraftTurn` league action (new type) recording: publisher, draft number, turn number
   - Advance to the next publisher without waiting for input
   - Repeat until a publisher with an open slot is found, or all remaining turns are exhausted (draft completes)
3. The UI reads the contiguous run of `SkippedDraftTurn` actions immediately preceding the current pick to display "Players B and C were skipped before Player D's turn"

**Snake-draft double-skip edge case:**
In a snake draft, the same publisher can appear at the end of one round and the start of the next (turns N and N+1 consecutively). If that publisher has no open slots, both turns are correctly emitted as separate `SkippedDraftTurn` actions by the backend — the skip-and-advance loop naturally handles this. The UI must deduplicate when rendering: if the same publisher appears more than once in the contiguous skipped-turn run, collapse them into a single "Player A was skipped (×2)" rather than listing Player A twice.

No additional DB columns are needed; the league action history is the source of truth.

---

## Frontend

### New page: Manage Drafts (`/league/:leagueID/:year/manage-drafts`)

Commissioner-only. Lists all drafts as cards showing: name, draft number, scheduled date, games/counter-picks to draft, play status.

Commissioner actions:
- **Create draft** — form with all `CreateLeagueDraft` fields including additive slot expansion. Available when no draft is currently in progress.
- **Edit** — inline form; `Name` always editable; other fields disabled once draft has started.
- **Delete** — only shown for `NotStartedDraft` drafts with `DraftNumber > 1`. Confirms before submitting.

### Modified: League Year Settings page

- `EnableBids` exposed as a checkbox: "Allow bids and pickups between drafts"
- When league has **exactly 1 draft**: `GamesToDraft` and `CounterPicksToDraft` remain inline as today
- When league has **2+ drafts**: those fields are hidden, replaced with: "To edit draft settings, visit the [Manage Drafts page]"

### Modified: League Home page

- If league has 2+ drafts: a **Drafts** button opens a modal listing all drafts with status and scheduled date (similar to the existing League Options modal)
- **Imminent draft** (scheduled within 7 days, OR draft order already set): surfaces a callout/banner at the top of the page, visible to all players
- **Overdue draft** (`ScheduledDate` passed, still `NotStartedDraft`): soft informational warning visible to all players
- Existing "Start Draft" / "Set Draft Order" buttons naturally target `CurrentDraft`
- **Publisher list ordering**: once the next scheduled draft's order is set, the publishers panel reorders to reflect that draft's order. Before the next draft's order is set, the panel stays in the previous draft's order. The frontend derives this by scanning `Drafts` for the first `NotStartedDraft` entry with `DraftOrderSet = true`; if none, it uses the most recent draft that has an order set.

### Modified: Create League page

Three preset buttons:
- **Standard League** — existing defaults, `EnableBids = true`, single draft section
- **One Shot League** — existing one-shot defaults, `EnableBids = false`, single draft section
- **Multi Draft League** — reveals second draft section below the first; `EnableBids = false`; pre-fills a sensible split (e.g. 10 standard games, 5 per draft)

---

## Conference Support (Slice 5)

When a new league is added to a conference year, the cloning action copies all `tbl_league_draft` rows from the primary league, assigning fresh `DraftID`s and the new `LeagueID`/`Year`. All draft settings (`GamesToDraft`, `CounterPicksToDraft`, `Name`, `ScheduledDate`) are cloned.

The main code fix is in `MySQLConferenceRepo.AssignLeaguePlayers` (marked `TODO(Phase2-MultiDraft)`): the draft-position fixup currently only handles Draft 1 and must be updated to handle N drafts.

No access-control gating between conference-level and league-level managers is enforced in this phase. That is out of scope and deferred.

---

## Integration Testing Strategy

| Slice | Tests |
|-------|-------|
| 1 | Create/edit/delete draft; verify `tbl_league_draft` rows; verify manager actions recorded |
| 2 | `GetLeagueYear` returns correct `Drafts` list; `PlayStatus` reflects `CurrentDraft`; bid/drop blocking rules enforced correctly (bids blocked during draft, bids blocked when `EnableBids=false`, drops blocked during draft but not by `EnableBids`) |
| 3 | Full two-draft scenario: set order → run draft 1 → run draft 2 with ≥1 publisher skipped; assert `SkippedDraftTurn` actions; assert final game counts |
| 4 | Create league via multi-draft preset in one request; assert two draft rows with correct settings |
| 5 | Conference clone; assert all draft rows present for each cloned league with correct `DraftID`s and settings |

---

## Out of Scope

- Enforcement of draft-setting sync between conference primary and child leagues (deferred)
- Per-draft `DraftSystem` (always Flexible at league-year level; no change)
- Decreasing slot counts via the Manage Drafts page (additive only; use League Year Settings for any reduction)
