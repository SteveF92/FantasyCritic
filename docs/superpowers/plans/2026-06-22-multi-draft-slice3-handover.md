# Multi-Draft Slice 3 — Handover & Open Questions

**Date:** 2026-06-22
**Branch:** `multi-draft-leagues`
**Status:** Backend for draft execution is implemented and mostly working. Two known bugs remain (documented below, with failing tests already written). **We are intentionally pausing backend fixes to build the front end** so the multi-draft flow can be exercised by hand ("feel" the code) before finalizing the skip/completion logic.

Related docs:
- Plan: `docs/superpowers/plans/2026-06-17-multi-draft-phase2.md`
- This handover captures the unresolved decisions from the Slice 3 work.

---

## Where the code stands

### Relevant commits (most recent last)
| Commit | Summary |
| ------ | ------- |
| `5e144d7a` | Slice 3 multi-draft execution: `ActiveDraft`/`PendingDraft` split, DraftID-scoped draft functions, auto-skip mechanic, `StartDraft` multi-draft support |
| `ad914891` | Tweaks: make the operating draft **explicit** — `DraftStatus.Draft`, repo methods take a `LeagueDraft`, `ClaimGame(…, Guid? draftID, …)`, explicit null-throws |
| `95b2081a` | Integration tests: per-draft position reset (passing) + skip-execution tests (`[Ignore]`d, documenting the bugs) |

### What works
- **Per-draft `DraftPosition` / `OverallDraftPosition` reset to 1 for each draft.** Confirmed by passing tests (`MultiDraftPositionResetTests`). No change needed here.
- `PendingDraft` (first `NotStartedDraft`) drives `StartDraft`; `ActiveDraft` (`DraftIsActiveOrPaused`) drives in-flight operations.
- `SetDraftOrder` / `ResetDraft` take an explicit `DraftID`; `StartDraft` works for any subsequent draft via `RequiredYearStatus.PlayOpenWithPendingDraft`.
- `PublisherGame.DraftID` is stamped from the explicit draft when a pick is made (non-draft acquisitions stay `null`).
- A uniform two-draft flow runs to completion (`MultiDraftSecondDraftExecutionTests`, passing).

### Test coverage map
| Test fixture | Covers | State |
| ------------ | ------ | ----- |
| `MultiDraftPositionResetTests` | Concern 1 (positions reset per draft) | **Passing** |
| `MultiDraftSkipExecutionTests.AutoDraft_WithUnequalSlots_…` | Concerns 2 + 3 (auto path) | **`[Ignore]`d — confirmed failing** |
| `MultiDraftSkipExecutionTests.ManualDraft_WithUnequalSlots_…` | Concern 2 (manual path) | **`[Ignore]`d — confirmed failing** |

File: `src/FantasyCritic.IntegrationTests/Tests/League/MultiDraft/MultiDraftSkipAndPositionTests.cs`

---

## The open questions / unanswered decisions

These are the things we have **not** committed to and should resolve once the front end lets us feel the behavior.

### Q1. How should "no open slots" be defined for skipping a turn?
The current (committed) auto-skip compares **games drafted in this draft** against the draft's **`GamesToDraft` quota**:

```csharp
gamesInActiveDraft >= activeDraft.GamesToDraft   // current, suspect
```

This is **not** the same as "the publisher has no open roster slot." It fails to account for slots filled *outside* this draft (bids/trades/manager claims between drafts), which is exactly when a skip should happen. The intended definition is slot-based:

```csharp
publisher.GetPublisherSlots(leagueYear).Any(s => !s.CounterPick && s.PublisherGame is null)  // open standard slot?
```

**Open question:** confirm the slot-based definition is the behavior we actually want when we see it in the UI (e.g., does a player who won a between-draft bid correctly get fewer draft turns?).

### Q2. Skip visibility — silent exclusion vs. explicit `SkippedDraftTurn` record?
- **Option A (lean): transparent exclusion.** A publisher with no open slot is simply never "next to draft." Simplest; the shorter roster is self-evident in history. `GetDraftStatus` / `GetNextDraftPublisher` just never select them.
- **Option B: explicit record.** Keep a (corrected, slot-based) `SkippedDraftTurn` `LeagueManagerAction` so the skip is visible in league history (this was the original plan's intent).

**Open question:** Which do we want? This is partly a UX call — easier to decide once the draft UI exists and we can see whether a visible "X was skipped" line is useful.

### Q3. Snake-draft order during an uneven tail
When one publisher is full and others continue, the snake direction/parity would be computed over the **eligible** set. The resulting pick order in that tail is "reasonable" but not formally specified.

**Open question:** Do we need a strict ordering guarantee for the tail, or is "everyone gets their remaining picks, draft completes" sufficient? The current tests only assert completion + final roster sizes, not the precise interleaving.

### Q4. Does `CompleteDraft` need to change in lockstep with the skip definition? (Yes.)
`CompleteDraft` currently uses the same `GamesToDraft * PublisherCount` total, so a draft where any publisher short-drafts **never completes** (concern 3). Whatever we decide for Q1, completion must use the *same* slot-aware definition (proposal: reuse `GetDraftPhase(...) == Complete`). Not really an open question — just a constraint: **the skip definition and the completion definition must be the same function.**

### Q5. `EditLeagueYear` still updates only the first draft's game counts
`MySQLFantasyCriticRepo.EditLeagueYear` has a `TODO(Phase2-MultiDraft)`: it should only edit game counts that way for **single-draft** leagues. Multi-draft editing of per-draft counts is unresolved. Likely surfaces when the settings UI is built.

---

## Proposed backend fix (deferred — recorded so it isn't lost)

When we return to the backend, the agreed-upon shape was:

1. **`DraftFunctions` — slot-aware, centralized.** Add per-publisher predicates and use them in both phase and next-publisher selection:
   ```csharp
   bool NeedsStandardPick(Publisher p) =>
       p.PublisherGames.Count(g => !g.CounterPick && g.DraftID == activeDraft.DraftID) < activeDraft.GamesToDraft
       && p.GetPublisherSlots(leagueYear).Any(s => !s.CounterPick && s.PublisherGame is null);

   bool NeedsCounterPick(Publisher p) =>
       p.PublisherGames.Count(g => g.CounterPick && g.DraftID == activeDraft.DraftID) < activeDraft.CounterPicksToDraft
       && p.GetPublisherSlots(leagueYear).Any(s => s.CounterPick && s.PublisherGame is null);
   ```
   - `GetDraftPhase`: `StandardGames` if any `NeedsStandardPick`; else `CounterPicks` if any `NeedsCounterPick`; else `Complete`.
   - `GetNextDraftPublisher`: run the existing snake logic over **only the eligible candidates**. Identical to today when nobody is full.

2. **`CompleteDraft`** — drop the arithmetic; check `GetDraftPhase(autoDraftResult.UpdatedLeagueYear).Equals(DraftPhase.Complete)`. Pass the *fresh* league year; the `standardGamesAdded`/`counterPicksAdded` plumbing can go away.

3. **`AutoDraftForLeague`** — remove the now-redundant explicit skip block (lines ~179–193); with a slot-aware `GetNextDraftPublisher`, the full publisher is never returned and the loop terminates naturally at `Complete`. (Unless we choose Q2 Option B, in which case keep a corrected skip-recording step.)

To validate the fix later: un-ignore the two tests in `MultiDraftSkipExecutionTests` and re-run the full draft suite for regressions.

---

## Why we're switching to the front end now

We have enough backend (Slices 1–3) to drive the multi-draft flow end-to-end for the **uniform** case, and the non-uniform (skip) edge cases are well-characterized with failing tests. Building the draft/settings UI lets us:
- Exercise create-second-draft → set order → start → draft to completion by hand.
- "Feel" the skip behavior with real between-draft bids, which will directly inform Q1–Q3.
- Surface any further multi-draft assumptions (e.g., Q5 settings editing) through actual use.

The failing skip tests are safely `[Ignore]`d, so the suite stays green while we work on the UI.
