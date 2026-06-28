# Design: Manager Skip Current Draft Pick

**Date:** 2026-06-27  
**Status:** Approved  
**Related:** [2026-06-27-draft-skip-handoff.md](./2026-06-27-draft-skip-handoff.md)

## Problem

The draft-status engine and `tbl_league_draftpickskip` table support skipped turns, but there is no way for a league manager to explicitly skip the current pick while a draft is paused. Without this, managers cannot advance the draft when a player is unavailable or stuck, and undo cannot reverse skip actions.

Auto-persisting `PicksToSkip` (publishers the system flags as `WillBeSkipped` because they have no open slots) is planned as separate follow-up work. The manual action is for cases the system does **not** auto-flag — e.g. a player with open slots who is AFK.

## Decision

Add a **Skip Current Draft Pick** manager action (Approach A): insert one `tbl_league_draftpickskip` row for the current `DraftStatus.NextPick`, gated on draft pause like undo. Extend **Undo Last Draft Action** so it reverses whichever action happened last — a drafted game **or** a recorded skip.

## Scope

### In scope

- `SkipCurrentDraftPick` API endpoint and `DraftService.SkipCurrentDraftPick`
- Repo methods to insert and delete skip rows
- Sidebar action + confirmation modal (type `SKIP TURN`)
- `LeagueDraftViewModel` fields so the modal can show who will be skipped while the draft is paused
- Undo extension for skip rows
- Unit and integration tests

### Out of scope

- Auto-persisting `DraftStatus.PicksToSkip` on live picks or auto-draft resume (separate chat)
- Bulk skip of multiple queued auto-skip turns
- Discord notifications on skip (existing push-on-resume behavior is sufficient for v1)

## Behavior

### SkipCurrentDraftPick

| Rule | Detail |
|---|---|
| Who | League manager only |
| When | Draft is **paused** (`RequiredYearStatus.DraftPaused`), same gate as undo |
| Target | **`DraftStatus.NextPick` only** — the publisher, phase, and round from `NextPick` |
| Slot check | **None** — manager may skip even if the publisher has open slots (voluntary skip) |
| `WillBeSkipped` / `PicksToSkip` | **Not used** for this action. Auto-skip follow-up will persist those before the manager sees them. |
| DB write | One row: `(DraftID, PublisherID, CounterPick, PickNumber)` where `PickNumber` = `NextPick.RoundNumber` |
| Audit | `LeagueManagerAction`, e.g. *"Alice was skipped for round 2 (standard game)."* |
| Broadcast | `RefreshLeagueYear` via SignalR |

**Validation failures** → `400 BadRequest`:

- No active draft, or `DraftID` does not match active draft
- `GetDraftStatus` returns null (draft complete)
- Skip row already exists for that turn

**Request:** `{ leagueID, year, draftID }` — same shape as undo; server derives the turn from `DraftStatus.NextPick`.

### UndoLastDraftAction (extended)

Use `DraftStatus.PreviousPick`:

- If `PreviousPick.Skipped` → delete the matching skip row from `tbl_league_draftpickskip`
- Else → existing behavior (remove the most recently drafted game)

One undo = one action, whether pick or skip.

## Architecture

### API

**File:** `src/FantasyCritic.Web/Controllers/API/LeagueManagerController.cs`

```csharp
[HttpPost]
public async Task<IActionResult> SkipCurrentDraftPick([FromBody] SkipCurrentDraftPickRequest request)
```

- `GetExistingLeagueYear(..., RequiredYearStatus.DraftPaused)`
- Validate active draft / `DraftID`
- Call `_draftService.SkipCurrentDraftPick(leagueYear)`
- `RefreshLeagueYear` on success

**Request type:** `src/FantasyCritic.Web/Models/Requests/LeagueManager/SkipCurrentDraftPickRequest.cs`

```csharp
public record SkipCurrentDraftPickRequest(Guid LeagueID, int Year, Guid DraftID);
```

### Service

**File:** `src/FantasyCritic.Lib/Services/DraftService.cs`

```csharp
public async Task<Result> SkipCurrentDraftPick(LeagueYear leagueYear)
public async Task<Result> UndoLastDraftAction(LeagueYear leagueYear)  // extended
```

`SkipCurrentDraftPick` flow:

1. Require active draft
2. `var draftStatus = DraftFunctions.GetDraftStatus(leagueYear)` — fail if null
3. Read `nextPick = draftStatus.NextPick` (publisher, `CounterPick`, `RoundNumber`)
4. Fail if skip row already exists for that key
5. Build `LeagueManagerAction` description
6. `_fantasyCriticRepo.AddDraftPickSkip(activeDraft, nextPick.Publisher, nextPick.CounterPick, nextPick.RoundNumber, action)`

`UndoLastDraftAction` flow (skip branch):

1. Existing guards (active draft, games exist OR previous pick was skip)
2. If `PreviousPick?.Skipped == true` → `_fantasyCriticRepo.RemoveDraftPickSkip(...)` with matching key
3. Else → existing game-removal logic

### Repo

**File:** `src/FantasyCritic.Lib/Interfaces/IFantasyCriticRepo.cs`

```csharp
Task AddDraftPickSkip(LeagueDraft draft, Publisher publisher, bool counterPick, int pickNumber, LeagueManagerAction action);
Task RemoveDraftPickSkip(LeagueDraft draft, Publisher publisher, bool counterPick, int pickNumber);
```

**File:** `src/FantasyCritic.MySQL/MySQLFantasyCriticRepo.cs`

- `AddDraftPickSkip`: insert into `tbl_league_draftpickskip` + `AddLeagueManagerAction` in one transaction
- `RemoveDraftPickSkip`: delete by primary key `(DraftID, PublisherID, CounterPick, PickNumber)`
- Mirror in `FantasyCritic.FakeRepo` for unit tests

No schema migration — table already exists per handoff doc.

### View model (modal display)

**File:** `src/FantasyCritic.Web/Models/Requests/League/LeagueDraftViewModel.cs`

When `domain.PlayStatus.DraftIsActiveOrPaused` and this draft is the active draft, populate from `DraftStatus.NextPick`:

| Field | Source |
|---|---|
| `NextPickPublisherName` | `NextPick.Publisher` display name |
| `NextPickRoundNumber` | `NextPick.RoundNumber` |
| `NextPickIsCounterPick` | `NextPick.CounterPick` |

Null when draft is not active/paused or `GetDraftStatus` is null. Used only for the confirmation modal; no change to `NextToDraft` highlighting logic.

## Frontend

### Sidebar

**File:** `src/FantasyCritic.Web/ClientApp/src/components/leagueActions.vue`

Add under Draft Management, mirroring undo:

- **Paused:** clickable → opens `skipCurrentDraftPickModal`
- **Not paused:** greyed text + *(Pause Draft First)*

Label: **Skip Current Pick** (or similar; modal title can be more explicit).

### Modal

**File:** `src/FantasyCritic.Web/ClientApp/src/components/modals/skipCurrentDraftPickModal.vue`

Pattern from `resetDraftModal.vue`:

- User must type **`SKIP TURN`** (case-insensitive) to enable OK
- Body shows publisher name, round, and standard game vs counter-pick from `activeDraft.nextPickPublisherName`, etc.
- POST `/api/leagueManager/SkipCurrentDraftPick` with `{ leagueID, year, draftID }`
- Success toast: *"Turn was skipped."*

Register modal in `leagueActions.vue` like `UndoLastDraftActionModal`.

## Testing

### Unit (`FantasyCritic.Test`)

- `SkipCurrentDraftPick` inserts skip for `NextPick` coordinates
- Rejects when skip row already exists
- `UndoLastDraftAction` removes skip when `PreviousPick.Skipped`
- `UndoLastDraftAction` still removes last game when `PreviousPick` is a real pick

### Integration (`FantasyCritic.IntegrationTests`)

New fixture under `Tests/League/Draft/EdgeCases/`:

1. Pause → `SkipCurrentDraftPick` → verify new `NextPick` / skip row present
2. Pause → skip → undo → verify turn restored (skip row gone)
3. Voluntary skip: publisher with open slots can be skipped while paused

Regenerate NSwag client after adding the controller action.

## Error handling summary

| Condition | Response |
|---|---|
| Not manager / not paused | 403 / existing year-status guard |
| Wrong or missing draft | 400 |
| Draft complete | 400 |
| Duplicate skip | 400 |
| Undo with no prior pick or skip | 400 (existing message pattern) |

## Implementation notes

- Naming is **`SkipCurrentDraftPick`** everywhere (service method, controller action, request type, modal id).
- Manual skip and auto-skip share the same DB row shape; only the trigger differs.
- After this ships, the auto-persist follow-up should run on pick/resume so `PicksToSkip` never blocks the manager path in practice.
