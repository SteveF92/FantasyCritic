import { defineStore } from 'pinia';
import { DateTime } from 'luxon';
import { useAuthStore } from './authStore';
import { usePublisherStore } from './publisherStore';
import { makeToast } from '@/utilities/toast';

import axios from 'axios';

export const useLeagueStore = defineStore('league', {
  state: () => ({
    hasError: false,
    forbidden: false,
    inviteCode: false,
    leagueYear: null,
    userPublisher: null,
    leagueActions: null,
    leagueActionSets: null,
    historicalTrades: null,
    showProjections: false,
    draftOrderView: false
  }),
  getters: {
    league: (state) => state.leagueYear?.league,
    publishers: (state) => state.leagueYear?.publishers,
    players: (state) => state.leagueYear?.players,
    hasSpecialSlots: (state) => state.leagueYear?.settings.hasSpecialSlots ?? false,
    supportedYear: (state) => state.leagueYear?.supportedYear,
    oneShotMode: (state) => state.leagueYear?.settings.oneShotMode ?? false,
    isManager: (state) => state.leagueYear?.league.isManager ?? false,
    firstDraft: (state) => state.leagueYear?.drafts?.[0] ?? null,
    pendingDraft: (state) => state.leagueYear?.drafts?.find((d) => d.playStatus === 'NotStartedDraft') ?? null,
    activeDraft: (state) => state.leagueYear?.drafts?.find((d) => d.draftIsActive || d.draftIsPaused) ?? null,
    nextPublisherUp: (state) => state.leagueYear?.publishers?.find((x) => x.nextToDraft) ?? null,
    // Publishers with no user stay in the standings, but can't win.
    publishersThatCanWin: (state) => (state.leagueYear?.publishers ?? []).filter((x) => !!x.userID),
    // The server picks it, so ties break the same way as the recorded winner.
    topPublisher: (state) => (state.leagueYear?.publishers ?? []).find((x) => x.publisherID === state.leagueYear?.winningPublisherID) ?? null,
    currentBids: (state) => state.leagueYear?.privatePublisherData?.myActiveBids ?? [],
    currentDrops: (state) => state.leagueYear?.privatePublisherData?.myActiveDrops ?? [],
    queuedGames: (state) => state.leagueYear?.privatePublisherData?.queuedGames ?? [],
    gameNews: (state) => state.leagueYear?.gameNews ?? [],
    userIsNextInDraft(): boolean {
      if (this.nextPublisherUp && this.userPublisher) {
        return this.nextPublisherUp.publisherID === this.userPublisher.publisherID;
      }

      return false;
    },
    draftIsPaused(): boolean {
      return this.activeDraft?.draftIsPaused ?? false;
    },
    playStarted(): boolean {
      return this.firstDraft?.playStarted ?? false;
    },
    readyToSetDraftOrder(): boolean {
      return this.pendingDraft?.readyToSetDraftOrder ?? false;
    },
    firstDraftFinished(): boolean {
      return this.firstDraft?.draftFinished ?? false;
    },
    hasPendingOrActiveDraft(): boolean {
      return this.pendingDraft !== null || this.activeDraft !== null;
    },
    pendingDraftIsFirst(): boolean {
      return this.pendingDraft?.draftNumber === 1;
    },
    pendingDraftIsWithinSevenDays(): boolean {
      if (!this.pendingDraft?.scheduledDate) return false;
      const scheduled = DateTime.fromISO(this.pendingDraft.scheduledDate).startOf('day');
      const today = DateTime.local().startOf('day');
      return scheduled.diff(today, 'days').days <= 7;
    },
    pendingDraftIsOverdue(): boolean {
      if (!this.pendingDraft?.scheduledDate) return false;
      const scheduled = DateTime.fromISO(this.pendingDraft.scheduledDate).startOf('day');
      const today = DateTime.local().startOf('day');
      return scheduled < today;
    },
    pendingDraftIsImminent(): boolean {
      if (!this.pendingDraft) return false;
      if (this.pendingDraftIsFirst) return true;
      if (this.pendingDraft.draftOrderSet && !this.pendingDraft.scheduledDate) return true;
      return this.pendingDraftIsWithinSevenDays;
    },
    pendingDraftCalloutVisible(): boolean {
      if (!this.pendingDraft || this.activeDraft) return false;
      if (this.pendingDraftIsFirst) return true;
      if (this.pendingDraftIsOverdue) return true;
      return this.pendingDraftIsWithinSevenDays;
    },
    draftIsActiveOrPaused(): boolean {
      return this.activeDraft !== null;
    },
    biddingAllowed(): boolean {
      return this.firstDraftFinished && !this.activeDraft && this.leagueYear.enableBids;
    },
    counterPickBiddingAllowed(): boolean {
      if (!this.biddingAllowed) {
        return false;
      }

      const publicBiddingGames = this.leagueYear.publicBiddingGames;
      if (this.leagueYear.settings.pickupSystem !== 'SemiPublicBiddingSemiPublicCounterPicks' || !publicBiddingGames) {
        return true;
      }

      // The reveal told the league only whether anything is being counter picked. While that answer is already 'yes',
      // another counter pick bid can't change it, so bidding stays open. While it's 'no', any counter pick bid would
      // flip it, so bidding is closed for the rest of the week.
      return publicBiddingGames.anyHiddenCounterPickBids;
    },
    tradesAllowed(): boolean {
      return this.firstDraftFinished && !this.activeDraft && this.leagueYear.settings.tradingSystem !== 'NoTrades';
    },
    dropsAllowed(): boolean {
      return this.firstDraftFinished && !this.activeDraft && !this.oneShotMode;
    },
    postDraftPlayable(): boolean {
      return this.firstDraftFinished && !this.leagueYear.supportedYear.finished;
    },
    postDraftEditable(): boolean {
      return this.firstDraftFinished && (!this.leagueYear.supportedYear.finished || this.leagueYear.underReview);
    }
  },
  actions: {
    async initializeLeaguePage(leaguePageParams) {
      const draftOrderView = localStorage.getItem('draftOrderView');
      const showProjections = localStorage.getItem('showProjections');
      this.setDraftOrderView(draftOrderView === 'true');
      this.setShowProjections(showProjections === 'true');
      const publisherStore = usePublisherStore();
      publisherStore.clearPublisherStoreData();
      this.clearLeagueStoreData();
      this.inviteCode = leaguePageParams.inviteCode;
      await this.fetchLeagueYear(leaguePageParams);
    },
    async initializeHistoryPage(leaguePageParams) {
      const publisherStore = usePublisherStore();
      publisherStore.clearPublisherStoreData();
      this.clearLeagueStoreData();
      await this.fetchLeagueYear(leaguePageParams);
      await fetchHistoryData(this);
    },
    async refreshLeagueYear() {
      const leaguePageParams = {
        leagueID: this.leagueYear.leagueID,
        year: this.leagueYear.year,
        inviteCode: this.inviteCode
      };
      await this.fetchLeagueYear(leaguePageParams);
    },
    async notifyAction(message) {
      await this.refreshLeagueYear();
      if (message) {
        makeToast(message);
      }
    },
    async fetchLeagueYear(leaguePageParams) {
      let queryURL = '/api/League/GetLeagueYear?leagueID=' + leaguePageParams.leagueID + '&year=' + leaguePageParams.year;
      if (leaguePageParams.inviteCode) {
        queryURL += '&inviteCode=' + leaguePageParams.inviteCode;
      }

      try {
        const response = await axios.get(queryURL);
        const leagueYearPayload = {
          leagueYear: response.data,
          userID: undefined
        };

        const authStore = useAuthStore();
        if (authStore.userInfo) {
          leagueYearPayload.userID = authStore.userInfo.userID;
        }

        setLeagueYear(this, leagueYearPayload);
      } catch (err) {
        setError(this, err.response);
      }
    },
    clearLeagueStoreData() {
      this.hasError = false;
      this.forbidden = null;
      this.inviteCode = null;
      this.leagueYear = null;
      this.userPublisher = null;

      this.leagueActions = null;
      this.leagueActionSets = null;
      this.historicalTrades = null;
    },
    setShowProjections(showProjections) {
      this.showProjections = showProjections;
      localStorage.setItem('showProjections', showProjections.toString());
    },
    setDraftOrderView(draftOrderView) {
      this.draftOrderView = draftOrderView;
      localStorage.setItem('draftOrderView', draftOrderView.toString());
    }
  }
});

function fetchHistoryData(store) {
  return Promise.all([fetchLeagueActions(store), fetchLeagueActionSets(store), fetchHistoricalTrades(store)]);
}

function fetchLeagueActions(store) {
  const queryURL = '/api/League/GetLeagueActions?leagueID=' + store.leagueYear.leagueID + '&year=' + store.leagueYear.year;
  return axios
    .get(queryURL)
    .then((response) => {
      store.leagueActions = response.data;
    })
    .catch((err) => setError(store, err.response));
}

function fetchLeagueActionSets(store) {
  const queryURL = '/api/League/GetLeagueActionSets?leagueID=' + store.leagueYear.leagueID + '&year=' + store.leagueYear.year;
  return axios
    .get(queryURL)
    .then((response) => {
      store.leagueActionSets = response.data;
    })
    .catch((err) => setError(store, err.response));
}

function fetchHistoricalTrades(store) {
  const queryURL = '/api/League/TradeHistory?leagueID=' + store.leagueYear.leagueID + '&year=' + store.leagueYear.year;
  return axios
    .get(queryURL)
    .then((response) => {
      store.historicalTrades = response.data;
    })
    .catch((err) => setError(store, err.response));
}

function setError(store, response) {
  if (response.status === 401 || response.status === 403) {
    store.forbidden = true;
  } else {
    store.hasError = true;
  }
}

function setLeagueYear(store, leagueYearPayload) {
  store.leagueYear = leagueYearPayload.leagueYear;
  document.title = leagueYearPayload.leagueYear.league.leagueName + ' - Fantasy Critic';

  if (leagueYearPayload.userID) {
    store.userPublisher = leagueYearPayload.leagueYear.publishers.find((x) => x.userID === leagueYearPayload.userID);
  }
}
