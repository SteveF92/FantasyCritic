# Counter Picks Must Be From This Draft — Design Spec

**Date:** 2026-07-01  
**Status:** Approved for implementation

---

## Overview

Add a per-draft boolean setting, `CounterPicksMustBeFromThisDraft`, on `tbl_league_draft`. When enabled (the default), counter picks during that draft's live counter-pick phase may only target standard games whose `PublisherGame.DraftID` matches the active draft — i.e. games drafted in that draft's standard rounds, not games from earlier drafts or acquired via bid/trade (`DraftID = null`).

This is primarily useful for multi-draft leagues: Draft 2's counter-pick round should bet against games picked in Draft 2's standard rounds, not games carried over from Draft 1.

In single-draft leagues the setting is effectively a no-op (all standard draft picks share the same `DraftID`), so the UI checkbox is hidden entirely for single-draft leagues.

---

## Key Design Decisions

| Decision | Choice |
| -------- | ------ |
| Scope of enforcement | Draft counter-pick phase only (`PossibleCounterPicks`, `DraftGame`, auto-draft counter picks). Post-draft bidding counter picks are **not** affected. |
| Default value | `true` — backfill existing rows; new drafts default to checked in UI |
| Editability | Editable only while `PlayStatus == NotStartedDraft` (same lock as `GamesToDraft` / `CounterPicksToDraft`) |
| UI visibility | Multi-draft leagues only, and only when `counterPicksToDraft > 0` |
| UI copy | Placeholder (Lorem ipsum) — final prose TBD by product owner |
| Enforcement approach | Filter at list **and** validate at claim (defense in depth) |

---

## Database

New sequential script under `FantasyCritic.DatabaseUpdater/Scripts/Sequential/`:

```sql
ALTER TABLE tbl_league_draft
  ADD COLUMN CounterPicksMustBeFromThisDraft bit(1) NOT NULL DEFAULT b'1';

UPDATE tbl_league_draft SET CounterPicksMustBeFromThisDraft = b'1';

ALTER TABLE tbl_league_draft
  MODIFY COLUMN CounterPicksMustBeFromThisDraft bit(1) NOT NULL;
```

Pattern: add with default, backfill (redundant but explicit), drop default so new rows rely on application code.

---

## Domain

### `LeagueDraft` (`FantasyCritic.Lib/Domain/LeagueDraft.cs`)

Add `bool CounterPicksMustBeFromThisDraft` to:

- Constructor and property
- `UpdateDraft(int gamesToDraft, int counterPicksToDraft)` — preserve existing value
- `UpdateDraft(string name, LocalDate? scheduledDate, int gamesToDraft, int counterPicksToDraft)` — preserve existing value
- New overload or extended `UpdateDraft` that accepts the flag (for edit flows)
- `GetDifferences` — emit a line when the flag changes

### `LeagueDraftEntity` (`FantasyCritic.Lib/SharedSerialization/Database/LeagueDraftEntity.cs`)

Add `bool CounterPicksMustBeFromThisDraft`; map in `ToDomain`.

### Request / parameter types

| Type | Change |
| ---- | ------ |
| `DraftParameters` | Add `bool CounterPicksMustBeFromThisDraft` |
| `DraftSettingsRequest` | Add `bool CounterPicksMustBeFromThisDraft`; pass through in `ToDomain` |
| `EditLeagueDraftParameters` | Add `bool CounterPicksMustBeFromThisDraft` |
| `CreateLeagueDraftParameters` | Add `bool CounterPicksMustBeFromThisDraft` |
| `EditLeagueDraftRequest` | Add `bool CounterPicksMustBeFromThisDraft`; pass through in `ToDomain` |
| `CreateLeagueDraftRequest` | Add `bool CounterPicksMustBeFromThisDraft`; pass through in `ToDomain` |

Server-side default when the field is omitted from API requests: `true`.

---

## Persistence (`FantasyCritic.MySQL`)

Update all `tbl_league_draft` INSERT and UPDATE statements in `MySQLFantasyCriticRepo` to include `CounterPicksMustBeFromThisDraft`. Stored procedures that SELECT draft columns (`sp_getleagueyear`, etc.) pick up the new column automatically if they use `SELECT *` or explicit column lists — verify and update explicit lists if present.

---

## Enforcement (draft counter-pick phase only)

When the **active draft** has `CounterPicksMustBeFromThisDraft == true` and the operation is a counter pick during drafting:

### 1. `DraftService.GetAvailableCounterPicks`

Currently returns all other players' standard games. When the flag is on, filter to games where `publisherGame.DraftID == activeDraft.DraftID`.

Requires passing the active draft (or its `DraftID` + flag) into this method. Call sites:

- `LeagueController.PossibleCounterPicks`
- Auto-draft counter-pick selection in `DraftService`

### 2. `GameEligibilityFunctions.CanClaimGame`

When `drafting == true`, `request.CounterPick == true`, and the active draft's flag is on:

- Reject if no other player holds the target game as a standard pick with `DraftID == activeDraft.DraftID`.
- Existing check (`otherPlayerHasDraftGame`) remains but is insufficient — it does not filter by `DraftID`.

Resolve the active draft from `leagueYear.Drafts` using the `draftID` already passed to `GameAcquisitionService.ClaimGame`.

Placeholder error message: *"That game was not drafted in this draft."*

### Out of scope

- Post-draft counter-pick bids (`CanClaimGame` with `drafting: false`)
- `Publisher.GetCounterPickedGameIsValid` roster display
- League-year-level settings

---

## Web API

### Response

`LeagueDraftViewModel` — add `bool CounterPicksMustBeFromThisDraft`.

### Edit guard

In `DraftService.EditDraft` (or equivalent), reject changes to `CounterPicksMustBeFromThisDraft` when `PlayStatus != NotStartedDraft`, consistent with `GamesToDraft` / `CounterPicksToDraft`.

---

## Frontend

### Visibility rule

Show the checkbox when **both**:

1. League is multi-draft (`gameMode === 'Multi Draft'` or `drafts.length > 1`)
2. That draft's `counterPicksToDraft > 0`

Single-draft leagues never show the control.

### Surfaces

| File | Change |
| ---- | ------ |
| `DraftCreationSettings.vue` | Checkbox per draft (default `true`); emit in draft object |
| `manageDrafts.vue` | Read view displays flag; edit form includes checkbox (disabled after draft starts) |
| `leagueCreationPresets.js` / `getDefaultDraft` | Default `counterPicksMustBeFromThisDraft: true` on new draft objects |

### Copy

Placeholder label and help text (Lorem ipsum) until final prose is supplied.

---

## Clone / copy paths

Update all `LeagueDraft` construction sites that copy draft settings to include `CounterPicksMustBeFromThisDraft`:

- `ConferenceService` clone loops (add league to conference, renew league year)
- `MySQLConferenceRepo` clone
- Any other draft-copy logic found during implementation

Copy the value verbatim from the source draft.

---

## Testing

### Unit

- `GetAvailableCounterPicks`: flag on excludes Draft 1 games when Draft 2 is active; flag off includes them
- `CanClaimGame` with `drafting: true`: rejects counter pick when target game's `DraftID` doesn't match active draft and flag is on

### Integration

- Multi-draft league: complete Draft 1 standard + counter picks; start Draft 2 standard rounds; enter Draft 2 counter-pick phase
  - With flag on (default): `PossibleCounterPicks` returns only Draft 2 standard games
  - Attempt to counter-pick a Draft 1 game via API → rejected
- With flag off on Draft 2: Draft 1 roster games appear in counter-pick list
- Single-draft regression: counter-pick flow unchanged

---

## Files likely touched (implementation checklist)

**DB:** Sequential migration script

**Lib:** `LeagueDraft.cs`, `LeagueDraftEntity.cs`, `DraftParameters.cs`, `EditLeagueDraftParameters.cs`, `CreateLeagueDraftParameters.cs`, `DraftService.cs`, `GameEligibilityFunctions.cs`, `GameAcquisitionService.cs`, `ConferenceService.cs`, `FantasyCriticService.cs`

**MySQL:** `MySQLFantasyCriticRepo.cs`, stored procs if needed

**Web:** `DraftSettingsRequest.cs`, `EditLeagueDraftRequest.cs`, `CreateLeagueDraftRequest.cs`, `LeagueDraftViewModel.cs`, `LeagueController.cs`

**Frontend:** `DraftCreationSettings.vue`, `manageDrafts.vue`, `leagueCreationPresets.js`, `createLeague.vue`, `createConference.vue` (if draft defaults propagate)

**Tests:** New unit tests, new multi-draft integration test fixture
