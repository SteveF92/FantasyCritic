# League Mixin Draft Helpers Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Rename the ambiguous `draftFinished` mixin computed to `firstDraftFinished` and introduce five new semantic helpers (`hasPendingOrActiveDraft`, `draftIsActiveOrPaused`, `biddingAllowed`, `tradesAllowed`, `dropsAllowed`) that correctly express multi-draft intent across the league UI.

**Architecture:** All helpers live in `leagueMixin.js` and are available to every component that mixes it in. Template files are updated to use the appropriate helper in place of the old `draftFinished` references, removing the implicit `!oneShotMode` wrapper in `leagueActions.vue` in favour of semantically correct per-item helpers.

**Tech Stack:** Vue 2, `leagueMixin.js` computed properties, Vuex store state (`leagueYear`).

---

## Files Modified

| File | What changes |
|------|-------------|
| `src/FantasyCritic.Web/ClientApp/src/mixins/leagueMixin.js` | Rename `draftFinished` → `firstDraftFinished`; add five new helpers; update `postDraftPlayable`/`postDraftEditable` to call `firstDraftFinished` |
| `src/FantasyCritic.Web/ClientApp/src/components/leagueActions.vue` | Draft Actions section uses `hasPendingOrActiveDraft`; game actions use `biddingAllowed`/`tradesAllowed`/`dropsAllowed`; remove `!oneShotMode` template wrapper |
| `src/FantasyCritic.Web/ClientApp/src/components/leagueYearStandings.vue` | Auto Draft badges and column selection use `hasPendingOrActiveDraft` |
| `src/FantasyCritic.Web/ClientApp/src/views/league.vue` | Post-draft block uses `postDraftPlayable`; bid countdowns gated on `biddingAllowed` |

---

## Task 1: Update `leagueMixin.js`

**Files:**
- Modify: `src/FantasyCritic.Web/ClientApp/src/mixins/leagueMixin.js`

- [ ] **Step 1: Replace the `draftFinished` block and update `postDraftPlayable`/`postDraftEditable`**

  Replace lines 111–119 (the `draftFinished`, `postDraftPlayable`, and `postDraftEditable` computed properties) with the following:

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
      postDraftPlayable() {
        return this.firstDraftFinished && !this.leagueYear.supportedYear.finished;
      },
      postDraftEditable() {
        return this.firstDraftFinished && (!this.leagueYear.supportedYear.finished || this.leagueYear.underReview);
      },
  ```

  The complete computed block should read (lines 1–127 of the file for reference):

  ```js
  import { mapState } from 'vuex';
  import { maxBy } from '@/globalFunctions';

  let leagueMixin = {
    computed: {
      ...mapState({
        hasError: (state) => state.league.hasError,
        forbidden: (state) => state.league.forbidden,
        inviteCode: (state) => state.league.inviteCode,
        leagueYear: (state) => state.league.leagueYear,
        userPublisher: (state) => state.league.userPublisher,
        leagueActions: (state) => state.league.leagueActions,
        leagueActionSets: (state) => state.league.leagueActionSets,
        historicalTrades: (state) => state.league.historicalTrades,
        showProjections: (state) => state.league.showProjections,
        draftOrderView: (state) => state.league.draftOrderView
      }),
      league() {
        if (!this.leagueYear) {
          return;
        }
        return this.leagueYear.league;
      },
      publishers() {
        return this.leagueYear.publishers;
      },
      players() {
        return this.leagueYear.players;
      },
      hasSpecialSlots() {
        return this.leagueYear.settings.hasSpecialSlots;
      },
      supportedYear() {
        return this.leagueYear.supportedYear;
      },
      firstDraft() {
        if (!this.leagueYear?.drafts || this.leagueYear.drafts.length === 0) {
          throw new Error('Could not load drafts for this league.');
        }
        return this.leagueYear.drafts[0];
      },
      pendingDraft() {
        return this.leagueYear.drafts.find((d) => d.playStatus === 'NotStartedDraft') ?? null;
      },
      activeDraft() {
        return this.leagueYear.drafts.find((d) => d.draftIsActive || d.draftIsPaused) ?? null;
      },
      nextPublisherUp() {
        if (!this.leagueYear || !this.leagueYear.publishers) {
          return null;
        }
        let next = this.leagueYear.publishers.find((x) => x.nextToDraft);
        return next;
      },
      userIsNextInDraft() {
        if (this.nextPublisherUp && this.leagueYear && this.userPublisher) {
          return this.nextPublisherUp.publisherID === this.userPublisher.publisherID;
        }
        return false;
      },
      draftIsPaused() {
        return this.activeDraft?.draftIsPaused ?? false;
      },
      isManager() {
        return this.league && this.league.isManager;
      },
      topPublisher() {
        if (this.leagueYear.publishers && this.leagueYear.publishers.length > 0) {
          return maxBy(this.leagueYear.publishers, (x) => x.totalFantasyPoints);
        }
        return null;
      },
      currentBids() {
        if (!this.leagueYear || !this.leagueYear.privatePublisherData) {
          return [];
        }
        return this.leagueYear.privatePublisherData.myActiveBids;
      },
      currentDrops() {
        if (!this.leagueYear || !this.leagueYear.privatePublisherData) {
          return [];
        }
        return this.leagueYear.privatePublisherData.myActiveDrops;
      },
      queuedGames() {
        if (!this.leagueYear || !this.leagueYear.privatePublisherData) {
          return [];
        }
        return this.leagueYear.privatePublisherData.queuedGames;
      },
      gameNews() {
        if (!this.leagueYear || !this.leagueYear.gameNews) {
          return [];
        }
        return this.leagueYear.gameNews;
      },
      oneShotMode() {
        return this.leagueYear.settings.oneShotMode;
      },
      playStarted() {
        return this.firstDraft.playStarted ?? false;
      },
      readyToSetDraftOrder() {
        return this.pendingDraft?.readyToSetDraftOrder ?? false;
      },
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
      postDraftPlayable() {
        return this.firstDraftFinished && !this.leagueYear.supportedYear.finished;
      },
      postDraftEditable() {
        return this.firstDraftFinished && (!this.leagueYear.supportedYear.finished || this.leagueYear.underReview);
      },
      decimalsToShow() {
        if (this.userInfo?.showDecimalPoints) {
          return 1;
        }
        return 0;
      }
    },
    methods: {
      async notifyAction(message, refresh = true) {
        if (refresh) {
          await this.$store.dispatch('refreshLeagueYear');
        }
        if (message) {
          this.makeToast(message);
        }
      }
    }
  };

  export default leagueMixin;
  ```

- [ ] **Step 2: Commit**

  ```
  git add src/FantasyCritic.Web/ClientApp/src/mixins/leagueMixin.js
  git commit -m "leagueMixin: rename draftFinished to firstDraftFinished; add hasPendingOrActiveDraft, draftIsActiveOrPaused, biddingAllowed, tradesAllowed, dropsAllowed helpers."
  ```

---

## Task 2: Update `leagueActions.vue`

**Files:**
- Modify: `src/FantasyCritic.Web/ClientApp/src/components/leagueActions.vue`

Three separate edits in the template.

- [ ] **Step 1: Restructure the Draft Actions section and Game Actions section**

  Find the `<template v-if="!leagueYear.supportedYear.finished">` block inside the player `<div v-if="userPublisher">` section (starting around line 45). Replace the entire block:

  ```vue
          <template v-if="!leagueYear.supportedYear.finished">
            <li v-if="!draftFinished">
              <strong>Draft Actions</strong>
              <ul class="actions-list">
                <li v-show="activeDraft?.draftIsActive && !activeDraft?.draftingCounterPicks && userIsNextInDraft" v-b-modal="'playerDraftGameForm'" class="fake-link action">
                  Draft Game
                </li>
                <li v-show="activeDraft?.draftIsActive && activeDraft?.draftingCounterPicks && userIsNextInDraft" v-b-modal="'playerDraftCounterPickForm'" class="fake-link action">
                  Draft Counter Pick
                </li>
                <li v-if="!oneShotMode && !draftFinished" v-b-modal="'editAutoDraftForm'" class="fake-link action">Set Auto Draft</li>
              </ul>
            </li>
            <li>
              <strong>Game Actions</strong>
              <ul class="actions-list">
                <li v-b-modal="'gameQueueForm'" class="fake-link action">Watchlist</li>
                <template v-if="!oneShotMode">
                  <li v-if="draftFinished" v-b-modal="'bidGameForm'" class="fake-link action">Make a Bid</li>
                  <li v-if="draftFinished" v-b-modal="'bidCounterPickForm'" class="fake-link action">Make a Counter Pick Bid</li>
                  <li v-if="draftFinished" v-b-modal="'currentBidsForm'" class="fake-link action">My Current Bids</li>
                  <li v-if="draftFinished && leagueYear.settings.tradingSystem !== 'NoTrades'" v-b-modal="'proposeTradeForm'" class="fake-link action">Propose a Trade</li>
                  <li v-if="draftFinished && leagueYear.settings.tradingSystem !== 'NoTrades'" v-b-modal="'activeTradesModal'" class="fake-link action">Active Trades</li>
                  <li v-if="draftFinished" v-b-modal="'dropGameForm'" class="fake-link action">Drop a Game</li>
                  <li v-if="draftFinished && userPublisher.superDropsAvailable > 0" v-b-modal="'superDropGameForm'" class="fake-link action">Use a Super Drop</li>
                  <li v-if="draftFinished" v-b-modal="'currentDropsForm'" class="fake-link action">My Pending Drops</li>
                </template>
              </ul>
            </li>
          </template>
  ```

  With:

  ```vue
          <template v-if="!leagueYear.supportedYear.finished">
            <li v-if="hasPendingOrActiveDraft">
              <strong>Draft Actions</strong>
              <ul class="actions-list">
                <li v-show="activeDraft?.draftIsActive && !activeDraft?.draftingCounterPicks && userIsNextInDraft" v-b-modal="'playerDraftGameForm'" class="fake-link action">
                  Draft Game
                </li>
                <li v-show="activeDraft?.draftIsActive && activeDraft?.draftingCounterPicks && userIsNextInDraft" v-b-modal="'playerDraftCounterPickForm'" class="fake-link action">
                  Draft Counter Pick
                </li>
                <li v-b-modal="'editAutoDraftForm'" class="fake-link action">Set Auto Draft</li>
              </ul>
            </li>
            <li>
              <strong>Game Actions</strong>
              <ul class="actions-list">
                <li v-b-modal="'gameQueueForm'" class="fake-link action">Watchlist</li>
                <li v-if="biddingAllowed" v-b-modal="'bidGameForm'" class="fake-link action">Make a Bid</li>
                <li v-if="biddingAllowed" v-b-modal="'bidCounterPickForm'" class="fake-link action">Make a Counter Pick Bid</li>
                <li v-if="biddingAllowed" v-b-modal="'currentBidsForm'" class="fake-link action">My Current Bids</li>
                <li v-if="tradesAllowed" v-b-modal="'proposeTradeForm'" class="fake-link action">Propose a Trade</li>
                <li v-if="tradesAllowed" v-b-modal="'activeTradesModal'" class="fake-link action">Active Trades</li>
                <li v-if="dropsAllowed" v-b-modal="'dropGameForm'" class="fake-link action">Drop a Game</li>
                <li v-if="dropsAllowed && userPublisher.superDropsAvailable > 0" v-b-modal="'superDropGameForm'" class="fake-link action">Use a Super Drop</li>
                <li v-if="dropsAllowed" v-b-modal="'currentDropsForm'" class="fake-link action">My Pending Drops</li>
              </ul>
            </li>
          </template>
  ```

- [ ] **Step 2: Update manager section — "Edit Player Auto Draft" and "Toggle Under Review"**

  In the manager section find:

  ```vue
              <li v-if="!draftFinished" v-b-modal="'managerSetAutoDraftForm'" class="fake-link action">Edit Player Auto Draft</li>
  ```

  Replace with:

  ```vue
              <li v-if="hasPendingOrActiveDraft" v-b-modal="'managerSetAutoDraftForm'" class="fake-link action">Edit Player Auto Draft</li>
  ```

  Then find:

  ```vue
              <li v-if="draftFinished && leagueYear.supportedYear.finished" v-b-modal="'toggleUnderReview'" class="fake-link action">
  ```

  Replace with:

  ```vue
              <li v-if="firstDraftFinished && leagueYear.supportedYear.finished" v-b-modal="'toggleUnderReview'" class="fake-link action">
  ```

- [ ] **Step 3: Commit**

  ```
  git add src/FantasyCritic.Web/ClientApp/src/components/leagueActions.vue
  git commit -m "leagueActions: use hasPendingOrActiveDraft, biddingAllowed, tradesAllowed, dropsAllowed; remove oneShotMode template wrapper."
  ```

---

## Task 3: Update `leagueYearStandings.vue`

**Files:**
- Modify: `src/FantasyCritic.Web/ClientApp/src/components/leagueYearStandings.vue`

Three edits: two template badge lines and two script locations.

- [ ] **Step 1: Update Auto Draft badges in the template**

  Find (around lines 14–15):

  ```vue
          <span v-show="!firstDraft.draftFinished && data.item.publisher.autoDraftMode === 'All'" class="publisher-badge badge badge-pill badge-primary badge-info">Auto Draft</span>
          <span v-show="!firstDraft.draftFinished && data.item.publisher.autoDraftMode === 'StandardGamesOnly'" class="publisher-badge badge badge-pill badge-primary badge-info">
  ```

  Replace with:

  ```vue
          <span v-show="hasPendingOrActiveDraft && data.item.publisher.autoDraftMode === 'All'" class="publisher-badge badge badge-pill badge-primary badge-info">Auto Draft</span>
          <span v-show="hasPendingOrActiveDraft && data.item.publisher.autoDraftMode === 'StandardGamesOnly'" class="publisher-badge badge badge-pill badge-primary badge-info">
  ```

- [ ] **Step 2: Update `created()` sort initialisation**

  Find:

  ```js
    created() {
      if (!this.firstDraft.draftFinished) {
        this.sortBy = 'draftPosition';
        this.sortDesc = false;
      }
    },
  ```

  Replace with:

  ```js
    created() {
      if (this.hasPendingOrActiveDraft) {
        this.sortBy = 'draftPosition';
        this.sortDesc = false;
      }
    },
  ```

- [ ] **Step 3: Update `standingFields` computed**

  Find:

  ```js
      standingFields() {
        if (!this.firstDraft.draftFinished) {
          return this.draftNotFinishedStandingsFields;
        }
  ```

  Replace with:

  ```js
      standingFields() {
        if (this.hasPendingOrActiveDraft) {
          return this.draftNotFinishedStandingsFields;
        }
  ```

- [ ] **Step 4: Commit**

  ```
  git add src/FantasyCritic.Web/ClientApp/src/components/leagueYearStandings.vue
  git commit -m "leagueYearStandings: use hasPendingOrActiveDraft for auto draft badges and column selection."
  ```

---

## Task 4: Update `league.vue`

**Files:**
- Modify: `src/FantasyCritic.Web/ClientApp/src/views/league.vue`

- [ ] **Step 1: Update the post-draft info block**

  Find (around line 176):

  ```vue
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
  ```

  Replace with:

  ```vue
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

  Key differences:
  - Outer div uses `postDraftPlayable` (the mixin helper, equivalent to `firstDraftFinished && !year.finished`).
  - The `!oneShotMode` wrapper div is removed — `biddingAllowed` already captures it via `leagueYear.enableBids`.
  - Each bid countdown and the "this week's bids" block is individually gated on `biddingAllowed`.
  - Game news (`<gameNews>`) is not bid-specific and shows whenever `postDraftPlayable`, unaffected by whether bids are enabled.

- [ ] **Step 2: Commit**

  ```
  git add src/FantasyCritic.Web/ClientApp/src/views/league.vue
  git commit -m "league: use postDraftPlayable and biddingAllowed for post-draft info block."
  ```

---

## Verification

After all four tasks, do a quick smoke check:

- [ ] **Search for `draftFinished` in the ClientApp directory to confirm no stale references remain**

  ```
  rg "draftFinished" src/FantasyCritic.Web/ClientApp/src
  ```

  Expected: zero matches. (The spec doc and conference files are excluded by the path; `conference.vue`/`conferenceMixin.js` use `leagueYears[].draftFinished` which is a raw API field, not the mixin computed — those are intentionally untouched.)

- [ ] **Load a single-draft league in the browser and verify:**
  - Before first draft: "Draft Actions" section visible, "Set Auto Draft" visible, no bid/drop/trade actions
  - During active draft: same, pick buttons appear when it's your turn
  - After draft completes: "Draft Actions" disappears, bid/drop/trade actions appear (if league has bids/drops/trades enabled), auto draft badges hidden in standings

- [ ] **Load a multi-draft league and verify:**
  - After draft 1, before draft 2 starts: "Draft Actions" section visible (pending draft), "Set Auto Draft" visible, bid/drop/trade actions show if league enables them
  - During active draft 2: bid/drop/trade actions hidden
  - After all drafts complete: "Draft Actions" disappears
