# Draft Skip — Status Header Display

## Summary

When draft picks are skipped — automatically (publisher has no open slots) or manually (league manager action) — the `draftStatusHeader.vue` should show a clear message explaining what happened. This requires bubbling up the ordered skip history from the draft engine to the frontend.

---

## Database

### Migration: `2026-06-28_000_draftPickSkipIsManual.sql`

Add `IsManual` to `tbl_league_draftpickskip`:

```sql
ALTER TABLE tbl_league_draftpickskip
    ADD COLUMN IsManual bit(1) NOT NULL DEFAULT 0;

ALTER TABLE tbl_league_draftpickskip
    ALTER COLUMN IsManual DROP DEFAULT;
```

The DEFAULT 0 is used only for the backfill of existing rows; it is immediately dropped so future inserts must always supply the value explicitly.

- `0` = auto-skip (written by the service layer when processing `PicksToSkip` after a real pick)
- `1` = manual skip (written by the controller action when the league manager explicitly skips a publisher)

---

## Domain Changes

### `PublisherDraftPickSkip`

Add `IsManual`:

```csharp
public record PublisherDraftPickSkip(bool CounterPick, int PickNumber, bool IsManual);
```

### `PastDraftPick`

Add `bool? IsManual` — meaningful only when `Skipped` is true; null for real picks:

```csharp
public record PastDraftPick(Publisher Publisher, bool CounterPick, int RoundNumber, PublisherGame? GameSelected, bool? IsManual = null)
{
    public bool Skipped => GameSelected is null;
    public int? OverallPickNumber => GameSelected?.OverallDraftPosition;
}
```

When `GetPastDraftPicks` creates a skipped `PastDraftPick`, it reads `IsManual` from the matching `PublisherDraftPickSkip` entry and passes it through.

### `DraftStatus`

Add `IReadOnlyList<PastDraftPick> SkippedPicksSinceLastRealPick`.

In `GetDraftStatus`, after `GetPastDraftPicks` returns `previousDraftPicks`, compute the trailing skipped entries by reading backwards from the end of the list:

```csharp
var skippedPicksSinceLastRealPick = previousDraftPicks
    .Reverse()
    .TakeWhile(x => x.Skipped)
    .Reverse()
    .ToList();
```

This avoids any object-identity comparison against `previousNonSkippedPick` and correctly handles the case where no real picks have been made yet (all trailing picks are skips).

Pass this to the `DraftStatus` constructor as `IReadOnlyList<PastDraftPick>`.

---

## Web Layer Changes

### `SkippedPickViewModel`

New small record in `FantasyCritic.Web.Models.Responses` (Web-only, no shared state):

```csharp
public record SkippedPickViewModel(string PublisherName, int RoundNumber, bool CounterPick, bool IsManual);
```

### `LeagueDraftViewModel`

Add `IReadOnlyList<SkippedPickViewModel> SkippedPicksSinceLastRealPick`.

Populated in the constructor when `domain.PlayStatus.DraftIsActiveOrPaused` and `draftStatus` is not null:

```csharp
SkippedPicksSinceLastRealPick = draftStatus.SkippedPicksSinceLastRealPick
    .Select(x => new SkippedPickViewModel(
        x.Publisher.GetPublisherAndUserDisplayName(),
        x.RoundNumber,
        x.CounterPick,
        x.IsManual!.Value))  // IsManual is always set on skipped picks; non-null guaranteed by construction
    .ToList();
```

---

## MySQL / Repo Changes

The MySQL repo that loads `PublisherDraftInfo` must read the new `IsManual` column when building `PublisherDraftPickSkip` records.

The service layer methods that insert skip rows must pass the correct `IsManual` value:
- Auto-skip insertion (after a real pick) → `IsManual = false`
- Manual skip insertion (manager action) → `IsManual = true`

---

## Frontend: `draftStatusHeader.vue`

### Data

`activeDraft.skippedPicksSinceLastRealPick` — array of `{ publisherName, roundNumber, counterPick, isManual }` objects.

### Computed property: `skippedPicksMessage`

Build the display string from the raw list:

1. **De-duplicate** by `publisherName`, counting occurrences.
2. **Single distinct publisher:**
   - All auto-skipped, skipped once → *"[Name]'s pick was auto-skipped because they have no open slots."*
   - All auto-skipped, skipped N times → *"[Name]'s pick was auto-skipped [N times] because they have no open slots."*
   - Any manual → *"[Name]'s pick was skipped by the league manager."*
3. **Multiple distinct publishers:**
   - *"The following players had their draft picks skipped: [A], [B]. See the History page for more information."*

The "any manual" rule means if a publisher was skipped both automatically and manually in the same streak, the manual label takes precedence — the edge case is too rare to warrant a combined message.

### Template

The message appears inline below the "Next to draft:" line inside the existing alert blocks (both the "not your turn" info alert and the "your turn" success alert). Rendered only when `skippedPicksMessage` is non-empty.

```html
<div v-if="skippedPicksMessage" class="text-muted small mt-1">{{ skippedPicksMessage }}</div>
```

---

## What Does NOT Change

- `PublisherDraftInfoViewModel` — does not need to expose skip records; the computed skip list flows through `DraftStatus` → `LeagueDraftViewModel`, not through the per-publisher info structure.
- The `PicksToSkip` field on `DraftStatus` (future auto-skips) — not surfaced in the header; those will be processed and become past skips automatically.
- Draft history page — out of scope for this feature.
