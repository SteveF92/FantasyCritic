# Design: "Bids Only Before Next Scheduled Draft" Option

**Date:** 2026-06-28  
**Status:** Approved

---

## Overview

Add a new league year option, `BidsOnlyBeforeNextScheduledDraft`, that restricts the bidding system so players can only bid on games whose maximum known release date falls **before** the next scheduled draft. The intent is to ensure the bidding system is used exclusively for games that could not realistically be drafted — games that are guaranteed to release before anyone has another chance to pick them.

This option only has any effect when all three conditions are true at runtime:
- The league has a pending (not-yet-started) draft
- That pending draft has a scheduled date set
- The league has bids enabled

---

## Setting

**Name:** `BidsOnlyBeforeNextScheduledDraft`  
**Type:** `bool`  
**Default:** `false`

Naming follows the existing conventions: `EnableBids`, `DropOnlyDraftGames`, `CounterPicksBlockDrops`.

---

## Core Eligibility Logic

The check is added inside `GameEligibilityFunctions.GetGenericSlotMasterGameErrors`. It is a **non-overridable hard fail** (`ClaimError.Overridable = false`) — the same severity as "cannot claim a game someone already has."

The check is skipped entirely when `dropping` or `drafting` is true — it is purely a bidding constraint.

### Decision table

| `BidsOnlyBeforeNextScheduledDraft` | Pending draft exists? | Draft has a scheduled date? | `MaximumReleaseDate` | Result |
|---|---|---|---|---|
| `false` | any | any | any | No restriction |
| `true` | No | — | any | No restriction (all drafts done for the year) |
| `true` | Yes | No | any | **Hard fail** — unscheduled pending draft blocks all bids |
| `true` | Yes | Yes | `null` | **Hard fail** — unknown upper bound |
| `true` | Yes | Yes | `>= scheduledDate` | **Hard fail** — game may not release before draft |
| `true` | Yes | Yes | `< scheduledDate` | Allowed |

### Pseudocode

```
if !dropping && !drafting && leagueYear.Options.BidsOnlyBeforeNextScheduledDraft:
    pendingDraft = leagueYear.PendingDraft

    if pendingDraft is null:
        pass  // no more pending drafts — no restriction

    else if pendingDraft.ScheduledDate is null:
        → hard fail:
          "Your league only allows bids for games that release before the next
           scheduled draft, but your next draft does not have a scheduled date."

    else:
        scheduledDate = pendingDraft.ScheduledDate.Value
        maxRelease = masterGame.MaximumReleaseDate

        if maxRelease is null:
            → hard fail:
              "Your league only allows bids for games that release before the next
               scheduled draft ({scheduledDate:MMMM d, yyyy}), but this game has
               no known maximum release date."

        else if maxRelease >= scheduledDate:
            → hard fail:
              "Your league only allows bids for games that release before the next
               scheduled draft ({scheduledDate:MMMM d, yyyy})."
```

Because `GetGenericSlotMasterGameErrors` is called from both:
- `GameAcquisitionService.MakePickupBid` (placement, `acquiringNow: false`)
- `ActionProcessor.ProcessPickups` (processing, `acquiringNow: true`)

…the "instant fail at placement" and "re-checked at processing time" requirements are satisfied by this single addition.

---

## Data Flow — All Layers

### 1. Domain: `LeagueOptions`

- Add `bool BidsOnlyBeforeNextScheduledDraft { get; }` property.
- Add parameter to the primary constructor.
- Add to the `LeagueYearParameters` ctor overload.
- Update `GetDifferences`: add a difference entry when the value changes.
- Update `UpdateOptionsForYear`: thread the field through.
- Update `WithNewDraftOptions`: thread the field through.

### 2. Domain: `LeagueYearParameters`

- Add `bool BidsOnlyBeforeNextScheduledDraft` constructor parameter and property.

### 3. Persistence: `LeagueYearEntity`

- Add `bool BidsOnlyBeforeNextScheduledDraft { get; set; }` property.
- Write it in the constructor from `options.BidsOnlyBeforeNextScheduledDraft`.
- Read it in `ToDomain` when constructing `LeagueOptions`.

### 4. API: `LeagueYearSettingsViewModel`

- Add `bool BidsOnlyBeforeNextScheduledDraft` to both the `[JsonConstructor]` and the `LeagueYear`-based constructor.
- Thread through `ToDomain`.
- No changes to `IsValid()` — no cross-field validation needed at the API level.

---

## Frontend

### Settings UI (`leagueYearSettings.vue`)

Under the **Bidding Settings** section, add a new checkbox. It is only rendered when both `isMultiDraft` and `internalValue.enableBids` are `true`.

```html
<div v-if="isMultiDraft && internalValue.enableBids" class="form-group">
  <b-form-checkbox v-model="internalValue.bidsOnlyBeforeNextScheduledDraft">
    Only allow bids for games that release before the next scheduled draft
  </b-form-checkbox>
  <p>
    When enabled, bids can only be placed on games whose maximum known release
    date falls before your next draft's scheduled date. This ensures the bidding
    system is used only for games that no one will have a chance to draft.
  </p>
</div>
```

### Settings validation (create league / edit league)

When `bidsOnlyBeforeNextScheduledDraft` is `true`, the UI must require that every non-first draft has a scheduled date set. A draft with this option enabled but no scheduled date will block all bidding.

Validation message: *"'Only allow bids before next scheduled draft' is enabled — all drafts after the first must have a scheduled date."*

This is a UI-level constraint. The backend gracefully handles the "pending draft exists, no date set" case with a hard fail on individual bids rather than a settings-save error, but the UI should prevent users from getting into that state in the first place.

### Bid form (`bidGameForm.vue`)

No structural changes needed. The server returns the error message through the existing `ClaimResult` → `PickupBidResultViewModel.errors` pipeline, which is already rendered in the bid form. The player sees the red error text explaining why the bid was rejected.

---

## Database Migration

New sequential script under `src/FantasyCritic.DatabaseUpdater/Scripts/Sequential/`:

```sql
ALTER TABLE tbl_leagueyear
  ADD COLUMN BidsOnlyBeforeNextScheduledDraft TINYINT(1) NOT NULL DEFAULT 0;

ALTER TABLE tbl_leagueyear
  ALTER COLUMN BidsOnlyBeforeNextScheduledDraft DROP DEFAULT;
```

`DEFAULT 0` backfills all existing rows (feature off for all existing leagues). The default is then dropped so future inserts must be explicit.

---

## Testing

### Unit tests (`FantasyCritic.Test/EligibilityTests.cs`)

These test `GameEligibilityFunctions.GetGenericSlotMasterGameErrors` directly, in isolation, without a running server. Because that function takes a `LeagueYear`, a minimal `LeagueYear` builder helper will be added to the test class (similar to the existing `CreateComplexMasterGame` helper) that accepts `bidsOnlyBeforeNextScheduledDraft` and an optional draft scheduled date.

The existing `CreateComplexMasterGame` helper already accepts a `maximumReleaseDate` parameter, so no new game factory is needed.

| # | Scenario | Expected result |
|---|---|---|
| U1 | Option on, `MaximumReleaseDate < scheduledDraftDate` | 0 errors from the new check |
| U2 | Option on, `MaximumReleaseDate == scheduledDraftDate` | 1 hard-fail error |
| U3 | Option on, `MaximumReleaseDate > scheduledDraftDate` | 1 hard-fail error |
| U4 | Option on, `MaximumReleaseDate` is `null` | 1 hard-fail error |
| U5 | Option on, pending draft has no scheduled date | 1 hard-fail error |
| U6 | Option on, no pending draft | 0 errors from the new check |
| U7 | Option off, `MaximumReleaseDate >= scheduledDraftDate` | 0 errors (option inactive) |
| U8 | Check skipped when `dropping = true` | 0 errors even with conflicting dates |
| U9 | Check skipped when `drafting = true` | 0 errors even with conflicting dates |

### Integration tests (`FantasyCritic.IntegrationTests/Tests/League/Actions/BidProcessingTests.cs`)

These exercise the full HTTP stack to confirm the check is enforced at bid placement time and re-enforced at processing time.

| # | Scenario | Expected result |
|---|---|---|
| I1 | Option on, valid game (`MaximumReleaseDate < draftDate`) | Bid placed and processes successfully |
| I2 | Option on, invalid game (`MaximumReleaseDate >= draftDate`) | Bid rejected at placement (hard fail) |
| I3 | Option on, bid placed while valid; game's release window expands past draft date by processing time | Bid fails at processing time |
| I4 | Option off | No restriction; invalid-date bids process normally |

---

## Files Affected

| File | Change |
|---|---|
| `src/FantasyCritic.Lib/Domain/LeagueOptions.cs` | New property, constructors, `GetDifferences`, `UpdateOptionsForYear`, `WithNewDraftOptions` |
| `src/FantasyCritic.Lib/Domain/Requests/LeagueYearParameters.cs` | New constructor parameter and property |
| `src/FantasyCritic.Lib/SharedSerialization/Database/LeagueYearEntity.cs` | New column property, read/write in `ToDomain` |
| `src/FantasyCritic.Web/Models/RoundTrip/LeagueYearSettingsViewModel.cs` | New field in both constructors, `JsonConstructor`, `ToDomain` |
| `src/FantasyCritic.Lib/BusinessLogicFunctions/GameEligibilityFunctions.cs` | New non-overridable check in `GetGenericSlotMasterGameErrors` |
| `src/FantasyCritic.Web/ClientApp/src/components/leagueYearSettings.vue` | New conditional checkbox in Bidding Settings section |
| `src/FantasyCritic.Web/ClientApp/src/views/createLeague.vue` (and/or related) | UI validation: require draft scheduled dates when option is on |
| `src/FantasyCritic.DatabaseUpdater/Scripts/Sequential/<next>.sql` | Migration: add + drop-default new column |
| `src/FantasyCritic.IntegrationTests/Tests/League/Actions/BidProcessingTests.cs` | New test cases (see table above) |
