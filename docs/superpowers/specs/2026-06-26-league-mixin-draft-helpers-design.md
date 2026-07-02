# Design: League Mixin Draft Helper Refactor

**Date:** 2026-06-26
**Status:** Approved

## Problem

The `leagueMixin.js` computed property `draftFinished` is a thin alias for `firstDraft.draftFinished`. In single-draft leagues this was harmless; in multi-draft leagues it causes incorrect UI behavior:

- Auto draft options disappear after draft 1 even when draft 2 is pending or active, because the section wrapper used `!draftFinished`.
- Bid/drop/trade actions appear during an active second draft (between bids going live and the draft finishing), because `draftFinished` only tests the first draft.
- The name `draftFinished` is ambiguous — it does not communicate _which_ draft finished.

The `biddingAllowed` concept was previously expressed as `!oneShotMode` template wrappers plus inline `draftFinished` checks, scattering the logic across the template and making it hard to reason about multi-draft + no-bids combinations.

## Decision

1. **Rename** `draftFinished` → `firstDraftFinished` in `leagueMixin.js`. Update all downstream references.
2. **Add four new computed helpers** to `leagueMixin.js` that express intent rather than mechanics.
3. **Refactor templates** in `leagueActions.vue`, `leagueYearStandings.vue`, and `league.vue` to use the new helpers, eliminating the `<template v-if="!oneShotMode">` wrapper in `leagueActions.vue`.

---

## New Helpers

All helpers are added to `leagueMixin.js`.

```js
firstDraftFinished() {
  return this.firstDraft.draftFinished;
},
hasPendingOrActiveDraft() {
  return this.pendingDraft !== null || this.activeDraft !== null;
},
draftIsActiveOrPaused() {
  return this.activeDraft !== null;
},
biddingAllowed() {
  return this.firstDraftFinished && !this.activeDraft && this.leagueYear.enableBids;
},
tradesAllowed() {
  return this.firstDraftFinished && !this.activeDraft && this.leagueYear.settings.tradingSystem !== 'NoTrades';
},
dropsAllowed() {
  return this.firstDraftFinished && !this.activeDraft && !this.oneShotMode;
},
```

**Rationale for each:**

- `firstDraftFinished` — identical behavior to old `draftFinished`, but the name communicates scope. Used by `postDraftPlayable`, `postDraftEditable`, and the "toggle under review" gate, which are all legitimately anchored to the first draft completing.
- `hasPendingOrActiveDraft` — true whenever any draft is upcoming or in progress. Used to show draft-phase UI (section visibility, auto draft option, auto draft badges in standings).
- `draftIsActiveOrPaused` — true whenever `activeDraft` is non-null. A named alias to avoid `!!activeDraft` inline. Available for future use; not used in the initial templates (templates read `activeDraft?.draftIsActive` directly for pick buttons since paused drafts should not show pick buttons).
- `biddingAllowed` — first draft done, no draft currently running, and the league has bids enabled. One-shot leagues have `enableBids = false` so `!oneShotMode` is implicit.
- `tradesAllowed` — first draft done, no draft running, trading is not disabled.
- `dropsAllowed` — first draft done, no draft running, not a one-shot league (one-shot leagues have no drops).

**Existing helpers updated (no behavior change):**

```js
// postDraftPlayable and postDraftEditable reference firstDraftFinished internally
postDraftPlayable() {
  return this.firstDraftFinished && !this.leagueYear.supportedYear.finished;
},
postDraftEditable() {
  return this.firstDraftFinished && (!this.leagueYear.supportedYear.finished || this.leagueYear.underReview);
},
```

---

## `leagueActions.vue` Changes

### Draft Actions section (player)

Gate the entire section on `hasPendingOrActiveDraft`. Pick buttons use `v-show` with `activeDraft?.draftIsActive` so they hide while paused without collapsing the section. "Set Auto Draft" lives inside the section — it is always visible when the section is shown (no additional condition).

```vue
<li v-if="hasPendingOrActiveDraft">
  <strong>Draft Actions</strong>
  <ul class="actions-list">
    <li v-show="activeDraft?.draftIsActive && !activeDraft?.draftingCounterPicks && userIsNextInDraft"
        v-b-modal="'playerDraftGameForm'" class="fake-link action">
      Draft Game
    </li>
    <li v-show="activeDraft?.draftIsActive && activeDraft?.draftingCounterPicks && userIsNextInDraft"
        v-b-modal="'playerDraftCounterPickForm'" class="fake-link action">
      Draft Counter Pick
    </li>
    <li v-b-modal="'editAutoDraftForm'" class="fake-link action">Set Auto Draft</li>
  </ul>
</li>
```

### Game Actions section — remove `<template v-if="!oneShotMode">` wrapper

Each item uses its semantic helper directly. The `!oneShotMode` guard is captured inside `biddingAllowed` and `dropsAllowed`.

```vue
<li v-if="biddingAllowed" v-b-modal="'bidGameForm'" class="fake-link action">Make a Bid</li>
<li v-if="biddingAllowed" v-b-modal="'bidCounterPickForm'" class="fake-link action">Make a Counter Pick Bid</li>
<li v-if="biddingAllowed" v-b-modal="'currentBidsForm'" class="fake-link action">My Current Bids</li>
<li v-if="tradesAllowed" v-b-modal="'proposeTradeForm'" class="fake-link action">Propose a Trade</li>
<li v-if="tradesAllowed" v-b-modal="'activeTradesModal'" class="fake-link action">Active Trades</li>
<li v-if="dropsAllowed" v-b-modal="'dropGameForm'" class="fake-link action">Drop a Game</li>
<li v-if="dropsAllowed && userPublisher.superDropsAvailable > 0" v-b-modal="'superDropGameForm'" class="fake-link action">Use a Super Drop</li>
<li v-if="dropsAllowed" v-b-modal="'currentDropsForm'" class="fake-link action">My Pending Drops</li>
```

### Manager section

| Item | Old condition | New condition |
|------|---------------|---------------|
| Edit Player Auto Draft | `!draftFinished` | `hasPendingOrActiveDraft` |
| Toggle under review | `draftFinished && leagueYear.supportedYear.finished` | `firstDraftFinished && leagueYear.supportedYear.finished` |

---

## `leagueYearStandings.vue` Changes

### Auto Draft badges

```vue
<!-- before -->
<span v-show="!firstDraft.draftFinished && data.item.publisher.autoDraftMode === 'All'" ...>Auto Draft</span>
<span v-show="!firstDraft.draftFinished && data.item.publisher.autoDraftMode === 'StandardGamesOnly'" ...>Auto Draft (No CPKs)</span>

<!-- after -->
<span v-show="hasPendingOrActiveDraft && data.item.publisher.autoDraftMode === 'All'" ...>Auto Draft</span>
<span v-show="hasPendingOrActiveDraft && data.item.publisher.autoDraftMode === 'StandardGamesOnly'" ...>Auto Draft (No CPKs)</span>
```

### Standings columns (draft position vs points)

Use `hasPendingOrActiveDraft` (Option A): show draft-position columns whenever any draft is pending or active, treating the entire multi-draft window as "draft mode." Both the `created()` sort initialization and the `standingFields` computed are updated.

```js
// created()
if (this.hasPendingOrActiveDraft) {
  this.sortBy = 'draftPosition';
  this.sortDesc = false;
}

// standingFields computed
if (this.hasPendingOrActiveDraft) {
  return this.draftNotFinishedStandingsFields;
}
```

---

## `league.vue` Changes

The post-draft info block at line 176:

```vue
<!-- before -->
<div v-if="firstDraft.draftFinished && !leagueYear.supportedYear.finished">
  <gameNews :game-news="gameNews" mode="league" />
  <br />
  <div v-if="!oneShotMode">
    <bidCountdowns v-if="showPublicRevealCountdown" mode="NextPublic" @publicBidRevealTimeElapsed="revealPublicBids"></bidCountdowns>
    <bidCountdowns v-if="!showPublicRevealCountdown" mode="NextBid"></bidCountdowns>
  </div>
  <div v-if="leagueYear.publicBiddingGames">
    <h2>This week's bids</h2>
    <activeBids />
  </div>
</div>

<!-- after -->
<div v-if="postDraftPlayable">
  <gameNews :game-news="gameNews" mode="league" />
  <br />
  <bidCountdowns v-if="biddingAllowed && showPublicRevealCountdown" mode="NextPublic" @publicBidRevealTimeElapsed="revealPublicBids"></bidCountdowns>
  <bidCountdowns v-if="biddingAllowed && !showPublicRevealCountdown" mode="NextBid"></bidCountdowns>
  <div v-if="biddingAllowed && leagueYear.publicBiddingGames">
    <h2>This week's bids</h2>
    <activeBids />
  </div>
</div>
```

Notes:
- `postDraftPlayable` is equivalent to `firstDraft.draftFinished && !leagueYear.supportedYear.finished` — same behavior, uses the mixin helper.
- `!oneShotMode` wrapper is removed; `biddingAllowed` captures it via `leagueYear.enableBids`.
- `showPublicRevealCountdown` is computed in `league.vue` and is unaffected.
- The bid countdowns are now individually conditioned rather than nested under a shared `!oneShotMode` div, so game news always shows post-draft even if bids are not allowed.

---

## Files Changed

| File | Change |
|------|--------|
| `src/FantasyCritic.Web/ClientApp/src/mixins/leagueMixin.js` | Rename `draftFinished` → `firstDraftFinished`; add `hasPendingOrActiveDraft`, `draftIsActiveOrPaused`, `biddingAllowed`, `tradesAllowed`, `dropsAllowed`; update `postDraftPlayable` and `postDraftEditable` to reference `firstDraftFinished` |
| `src/FantasyCritic.Web/ClientApp/src/components/leagueActions.vue` | Restructure Draft Actions section; replace `draftFinished` with semantic helpers; remove `!oneShotMode` template wrapper |
| `src/FantasyCritic.Web/ClientApp/src/components/leagueYearStandings.vue` | Replace `firstDraft.draftFinished` with `hasPendingOrActiveDraft` for badges and column selection |
| `src/FantasyCritic.Web/ClientApp/src/views/league.vue` | Replace `firstDraft.draftFinished && !year.finished` with `postDraftPlayable`; gate bid countdowns and bid list on `biddingAllowed` |

## Out of Scope

- `conference.vue` / `conferenceMixin.js` — those reference `leagueYears[].draftFinished`, which is a field on the conference API response, not the mixin computed. No change.
- `draftCompleteModal.vue` — already references `leagueYear.enableBids` directly; no change needed.
