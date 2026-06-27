# Draft Skip — Implementation Handoff

## What Was Built

This session designed and implemented the core draft-status engine that supports skipped turns in multi-draft leagues. The key insight driving the whole design: skips must be **explicitly stored** in the database (not inferred), so that past turns are always definitive facts and future turns can be projected forward cleanly.

---

## Database

### `tbl_league_draftpickskip`
```sql
DraftID     char(36) NOT NULL
PublisherID char(36) NOT NULL
CounterPick bit(1)   NOT NULL
PickNumber  int unsigned NOT NULL   -- round number (1-based, up to GamesToDraft / CounterPicksToDraft)
PRIMARY KEY (DraftID, CounterPick, PickNumber, PublisherID)
FK → tbl_league_draftpublisher (DraftID, PublisherID)
```

`PickNumber` is the **round number** in the snake draft (not the overall sequential pick number). Combined with `PublisherID`, `DraftID`, and `CounterPick`, it uniquely identifies one theoretical turn slot.

No `LeagueID`/`Year` columns — always accessed through the draft-publisher path and the FK chain provides full integrity.

### `tbl_league_draftpublisher`
Also added in this work: per-draft publisher order lives here instead of on `tbl_league_publisher`.

---

## Domain Model

### `PublisherDraftInfo` (on `Publisher`)
Each publisher carries one `PublisherDraftInfo` per draft they're in:
```csharp
public class PublisherDraftInfo
{
    public Guid DraftID { get; }
    public int DraftNumber { get; }
    public Guid PublisherID { get; }
    public int DraftPosition { get; }
    public IReadOnlyList<PublisherDraftPickSkip> PickSkips { get; }
}

public record PublisherDraftPickSkip(bool CounterPick, int PickNumber);
```

Skip records load with the publisher — they're always in memory when `GetDraftStatus` runs.

### `DraftStatus.cs` — the output model

```csharp
public class DraftStatus
{
    public LeagueDraft Draft { get; }
    public FutureDraftPick NextPick { get; }          // the next real turn
    public PastDraftPick? PreviousPick { get; }       // last resolved action (pick or skip)
    public PastDraftPick? PreviousNonSkippedPick { get; } // last actual pick (for Discord / UI "who just went")
    public IReadOnlyList<FutureDraftPick> PicksToSkip { get; } // consecutive skips before NextPick

    public DraftPhase DraftPhase => NextPick.CounterPick ? DraftPhase.CounterPicks : DraftPhase.StandardGames;
    public Publisher NextDraftPublisher => NextPick.Publisher;
    public Publisher? PreviousPublisherThatWasNotSkipped => PreviousNonSkippedPick?.Publisher;
    public int RoundNumber => NextPick.RoundNumber;
    public int OverallPickNumber => NextPick.OverallPickNumber ?? throw ...;
}

// Resolved past turn (definitive — either a real pick or a recorded skip)
public record PastDraftPick(Publisher Publisher, bool CounterPick, int RoundNumber, PublisherGame? GameSelected)
{
    public bool Skipped => GameSelected is null;
    public int? OverallPickNumber => GameSelected?.OverallDraftPosition;
}

// Projected future turn (not yet happened)
public record FutureDraftPick(Publisher Publisher, bool CounterPick, int RoundNumber, int? OverallPickNumber)
{
    public bool WillBeSkipped => OverallPickNumber is null;
}

public record PickProcessingResult(FutureDraftPick? NextPick, IReadOnlyList<FutureDraftPick> PicksToSkip);
```

Key design symmetry: `PastDraftPick.Skipped` and `FutureDraftPick.WillBeSkipped` are both derived from the meaningful value being null (`GameSelected` / `OverallPickNumber`), not stored as a separate flag.

---

## Algorithm: `GetPastDraftPicks`

Walks the snake draft sequence for the active draft (standard rounds first, then CP rounds), building a chronologically ordered list of all resolved turns.

**Setup:**
- `gameDictionary`: `(bool CounterPick, int OverallDraftPosition) → PublisherGame` — actual picks from DB
- `skipLookup`: `HashSet<(Guid PublisherID, bool CounterPick, int PickNumber)>` — from `PublisherDraftInfo.PickSkips`
- Validates that all games in `gameDictionary` have both `DraftPosition` and `OverallDraftPosition` set; throws `InvalidOperationException` loudly if not
- `publisherDictionary`: `Guid → Publisher` for O(1) lookups

**Per turn (publisher × round, in snake order):**
1. Check `skipLookup` for `(publisher.PublisherID, counterPick, roundNumber)`  
   → If found: `PastDraftPick(publisher, ..., GameSelected: null)` — skip does NOT advance `overallPickNumber`
2. Else: look up `gameDictionary[(counterPick, overallPickNumber)]`  
   → If found: `PastDraftPick(publisher, ..., GameSelected: game)` — advance `overallPickNumber`  
   → If not found: **frontier reached** — return list immediately (remaining turns are future)

The `overallPickNumber` counter does not advance for skipped turns, preserving the sequential numbering of actual picks.

---

## Algorithm: `GetFutureDraftPicks`

Continues the snake walk from where `GetPastDraftPicks` stopped, projecting forward to find the next real pick and any immediately preceding skips.

**Setup:**
- `resolvedTurns`: `HashSet<(bool CounterPick, int RoundNumber, Guid PublisherID)>` from all past picks
- `nextStandardPickNumber` = past non-skipped standard picks + 1
- `nextCounterPickNumber` = past non-skipped CP picks + 1

**Per turn (same snake order):**
1. If in `resolvedTurns` → `continue` (already resolved, past territory)
2. `ShouldSkipPublisher(publisher, counterPick, leagueYear)`:
   - Returns `true` if publisher has no open slot of the right type (`GetPublisherSlots(leagueYear).Any(x => x.CounterPick == counterPick && x.PublisherGame is null)` is false)
   - If skip: `FutureDraftPick(publisher, ..., OverallPickNumber: null)` → add to `picksToSkip`
   - If not skip: `FutureDraftPick(publisher, ..., OverallPickNumber: nextStandardPickNumber)` → **return immediately** as `NextPick`

Stops as soon as `NextPick` is found — no need to compute the whole draft. `PicksToSkip` contains only the consecutive turns that must be processed (recorded in DB) before `NextPick` can happen.

If the walk exhausts all theoretical turns without finding a `NextPick`: `GetDraftStatus` returns `null` (draft complete).

---

## Key Invariants

- A publisher is skipped if and only if a `tbl_league_draftpickskip` row exists for their `(DraftID, PublisherID, CounterPick, PickNumber)`.
- Skipped turns **do not** consume an `OverallPickNumber`. The counter holds at the same value; the next non-skipped publisher gets that number.
- Drops and bids are blocked while a draft is active, so a publisher's game count can only increase during a draft. This means a skip decision is monotonic: once you're full, you stay full for the rest of the draft.
- `PreviousPick` (last action, including skips) and `PreviousNonSkippedPick` (last real pick) serve different consumers: the draft history page uses the former, Discord / "who just went" UI uses the latter.

---

## What the Next Steps Need to Do

### Auto-persisting skip actions

`DraftStatus.PicksToSkip` is the output that drives this. When a pick is made (or the draft advances for any reason), the service layer must:

1. Get the new `DraftStatus`
2. If `PicksToSkip` is non-empty: write one `tbl_league_draftpickskip` row per entry **in the same transaction as the pick**
3. These rows immediately become part of `PublisherDraftInfo.PickSkips` on the next load, so `GetPastDraftPicks` sees them as definitive history

The `FutureDraftPick` in `PicksToSkip` carries `Publisher`, `CounterPick`, and `RoundNumber` — exactly the three columns needed for the DB insert (plus `DraftID` from `activeDraft`).

### Manager manual skip action

A new controller action that lets the league manager explicitly skip the current `NextPick.Publisher`. The action should:
1. Verify the publisher's turn is actually next (using current `DraftStatus`)
2. Insert a `tbl_league_draftpickskip` row for `(DraftID, Publisher.PublisherID, NextPick.CounterPick, NextPick.RoundNumber)`
3. Reload and rebroadcast draft status (which will now show a new `NextPick`)

The manual skip uses the exact same DB concept as the auto-skip — the only difference is that the trigger is human rather than the slot-fullness check. There is no separate "manual skip flag"; it's just a skip record.
