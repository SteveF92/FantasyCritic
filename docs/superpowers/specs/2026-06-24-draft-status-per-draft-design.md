# Design: Per-Draft Status on LeagueDraftViewModel

**Date:** 2026-06-24  
**Status:** Approved

## Problem

`CompleteFirstDraftPlayStatus` and its downstream `PlayStatusViewModel` were designed for a single-draft world. They aggregate draft readiness at the league-year level, implicitly (and now explicitly) anchored to the first draft. As multi-draft leagues are built out, this model breaks down: each draft has its own readiness state, and the frontend needs to display that information per-draft.

The three compiler errors that surfaced this issue (`PlayStatus`, `DraftOrderSet` moved from `LeagueYear` to `LeagueDraft`; `GetStartDraftResult` now requires a `LeagueDraft` argument) made the mismatch concrete.

## Decision

Remove `CompleteFirstDraftPlayStatus` and `PlayStatusViewModel` entirely. Move all draft-status information into `LeagueDraftViewModel` so each draft in the response carries its own readiness data. The frontend uses three computed helpers (`firstDraft`, `pendingDraft`, `activeDraft`) that mirror the backend's own domain concepts.

## Architecture

### Backend: Enriched `LeagueDraftViewModel`

**File:** `src/FantasyCritic.Web/Models/Requests/League/LeagueDraftViewModel.cs`  
(Note: namespace is `FantasyCritic.Web.Models.Responses` despite the file path)

Constructor signature changes from:
```csharp
public LeagueDraftViewModel(LeagueDraft domain)
```
To:
```csharp
public LeagueDraftViewModel(LeagueDraft domain, LeagueYear leagueYear, IEnumerable<FantasyCriticUser> activeUsers, bool isManager, bool conferenceDraftsNotEnabled)
```

**New fields added to every draft:**

| Field | Source | Notes |
|---|---|---|
| `PlayStarted` | `domain.PlayStatus.PlayStarted` | Convenience bool |
| `DraftIsActive` | `domain.PlayStatus.DraftIsActive` | Convenience bool |
| `DraftIsPaused` | `domain.PlayStatus.DraftIsPaused` | Convenience bool |
| `DraftFinished` | `domain.PlayStatus.DraftFinished` | Convenience bool |
| `DraftingCounterPicks` | `DraftFunctions.GetDraftStatus(leagueYear)` | True only if this draft is active AND in counter-picks phase |

**Fields added only for pending drafts** (`PlayStatus == NotStartedDraft`); otherwise default to empty/false:

| Field | Source |
|---|---|
| `ReadyToSetDraftOrder` | `DraftFunctions.LeagueIsReadyToSetDraftOrder(leagueYear.Publishers, activeUsers)` |
| `StartDraftErrors` | `DraftFunctions.GetStartDraftResult(leagueYear, domain, activeUsers, isManager, conferenceDraftsNotEnabled)` |
| `ReadyToDraft` | `!StartDraftErrors.Any()` |

### Backend: `LeagueYearViewModel` changes

- Remove `CompleteFirstDraftPlayStatus` constructor parameter
- Remove `PlayStatus` property (was `PlayStatusViewModel`)
- Change `activeUsers` parameter from `IReadOnlyList<MinimalFantasyCriticUser>` to `IEnumerable<FantasyCriticUser>`; derive minimal internally where needed
- Update `Drafts` construction to pass the new params: `leagueYear.Drafts.Select(d => new LeagueDraftViewModel(d, leagueYear, activeUsers, isManager, conferenceDraftsNotEnabled))`
- The `completePlayStatus.DraftStatus?.NextDraftPublisher` used when building `PublisherViewModel` instances (for the `nextToDraft` flag) moves to calling `DraftFunctions.GetDraftStatus(leagueYear)?.NextDraftPublisher` directly

### Backend: Files deleted

- `src/FantasyCritic.Lib/Domain/Draft/CompleteFirstDraftPlayStatus.cs`
- `src/FantasyCritic.Web/Models/Responses/PlayStatusViewModel.cs`

### Backend: Controller guard checks

`LeagueManagerController` currently builds `CompleteFirstDraftPlayStatus` to guard the `StartDraft` and `SetDraftOrder` actions. These are replaced with direct `DraftFunctions` calls:

**StartDraft guard** (currently `ReadyToDraft`):
```csharp
var pendingDraft = leagueYear.PendingDraft;
if (pendingDraft is null) { return BadRequest(); }
var startDraftErrors = DraftFunctions.GetStartDraftResult(leagueYear, pendingDraft, activeUsers, isManager, false);
if (startDraftErrors.Any()) { return BadRequest(); }
```

**SetDraftOrder guard** (currently `ReadyToSetDraftOrder`):
```csharp
if (!DraftFunctions.LeagueIsReadyToSetDraftOrder(leagueYear.Publishers, activeUsers)) { return BadRequest(); }
```

### Backend: `ConsolidatedLeagueDataViewModel`

Currently constructs `CompleteFirstDraftPlayStatus` to get draft status for summary display. Replace by calling `DraftFunctions` directly or deriving from the enriched `LeagueDraftViewModel` construction.

## Frontend

### New computed helpers in `leagueMixin.js`

```js
firstDraft() {
  return this.leagueYear?.drafts?.[0] ?? null;
},
pendingDraft() {
  return this.leagueYear?.drafts?.find(d => d.playStatus === 'NotStartedDraft') ?? null;
},
activeDraft() {
  return this.leagueYear?.drafts?.find(d => d.draftIsActive || d.draftIsPaused) ?? null;
},
```

The existing `draftIsPaused` computed property in `leagueMixin.js` also updates:
```js
// Before:
draftIsPaused() { return this.leagueYear.playStatus.draftIsPaused; },
// After:
draftIsPaused() { return this.leagueYear.activeDraft?.draftIsPaused ?? false; },
```

### Field migration map

| Old (`leagueYear.playStatus.*`) | New |
|---|---|
| `playStarted` | `leagueYear.firstDraft.playStarted` |
| `draftFinished` | `leagueYear.firstDraft.draftFinished` |
| `readyToDraft` | `leagueYear.pendingDraft?.readyToDraft` |
| `startDraftErrors` | `leagueYear.pendingDraft?.startDraftErrors` |
| `readyToSetDraftOrder` | `leagueYear.pendingDraft?.readyToSetDraftOrder` |
| `draftIsActive` | `!!leagueYear.activeDraft?.draftIsActive` |
| `draftIsPaused` | `!!leagueYear.activeDraft?.draftIsPaused` |
| `draftingCounterPicks` | `!!leagueYear.activeDraft?.draftingCounterPicks` |

### Files with `leagueYear.playStatus` references to update

- `src/FantasyCritic.Web/ClientApp/src/views/league.vue`
- `src/FantasyCritic.Web/ClientApp/src/components/leagueActions.vue`
- `src/FantasyCritic.Web/ClientApp/src/components/leagueYearStandings.vue`
- `src/FantasyCritic.Web/ClientApp/src/components/modals/removePlayerModal.vue`
- `src/FantasyCritic.Web/ClientApp/src/mixins/leagueMixin.js`

## Why not keep `CompleteFirstDraftPlayStatus`?

The class was a stepping stone. It made the first draft explicit, which was better than the implicit `leagueYear.PlayStatus` shortcut, but it still produces a flat `playStatus` blob on the response — a single-draft-world shape that doesn't extend to multi-draft. Removing it entirely means the frontend always reads from the `drafts` array through named helpers, which works for any number of drafts.

## Intentional omissions

- `nextPublisherUp` is unaffected — it derives from `publishers.find(x => x.nextToDraft)`, which is driven by `PublisherViewModel`'s `nextToDraft` flag, not from `playStatus`.
- No new `activeDraftStatus` top-level object is needed. All active-draft information is accessible via `leagueYear.activeDraft.*` (the `LeagueDraftViewModel` for the active draft).
