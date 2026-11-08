import { defineStore } from 'pinia';
import { DateTime } from 'luxon';
import { useAuthStore } from '@/store/authStore';
import { usePublisherStore } from '@/store/publisherStore';
import { makeToast } from '@/utilities/toast';
import { ApiException, LeagueActionProcessingSetViewModel, LeagueActionViewModel, leagueClient, LeagueYearViewModel, TradeViewModel } from '@/api/clients';

export interface LeaguePageParams {
  leagueID: string;
  year: number;
  inviteCode?: string;
}

interface LeagueStoreState {
  hasError: boolean;
  forbidden: boolean;
  inviteCode?: string;
  leagueYear: LeagueYearViewModel | null;
  leagueActions: LeagueActionViewModel[] | null;
  leagueActionSets: LeagueActionProcessingSetViewModel[] | null;
  historicalTrades: TradeViewModel[] | null;
  showProjections: boolean;
  draftOrderView: boolean;
}

export const useLeagueStore = defineStore('league', {
  state: (): LeagueStoreState => ({
    hasError: false,
    forbidden: false,
    inviteCode: undefined,
    leagueYear: null,
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
    userPublisher(state) {
      const authStore = useAuthStore();
      if (!authStore.userInfo) {
        return null;
      }

      const userID = authStore.userInfo.userID;
      return state.leagueYear?.publishers.find((x) => x.userID === userID) ?? null;
    },
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
      return (this.firstDraftFinished && !this.activeDraft && this.leagueYear?.enableBids) ?? false;
    },
    counterPickBiddingAllowed(): boolean {
      if (!this.biddingAllowed || !this.leagueYear) {
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
      return this.firstDraftFinished && !this.activeDraft && this.leagueYear?.settings.tradingSystem !== 'NoTrades';
    },
    dropsAllowed(): boolean {
      return this.firstDraftFinished && !this.activeDraft && !this.oneShotMode;
    },
    postDraftPlayable(): boolean {
      return this.firstDraftFinished && !this.leagueYear?.supportedYear.finished;
    },
    postDraftEditable(): boolean {
      return this.firstDraftFinished && (!this.leagueYear?.supportedYear.finished || this.leagueYear.underReview);
    }
  },
  actions: {
    async initializeLeaguePage(leaguePageParams: LeaguePageParams) {
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
    async initializeHistoryPage(leaguePageParams: LeaguePageParams) {
      const publisherStore = usePublisherStore();
      publisherStore.clearPublisherStoreData();
      this.clearLeagueStoreData();
      await this.fetchLeagueYear(leaguePageParams);
      await Promise.all([this.fetchLeagueActions(leaguePageParams), this.fetchLeagueActionSets(leaguePageParams), this.fetchHistoricalTrades(leaguePageParams)]);
    },
    async refreshLeagueYear() {
      if (!this.leagueYear) {
        throw new Error('League store is not initialized');
      }

      await this.fetchLeagueYear({
        leagueID: this.leagueYear.leagueID,
        year: this.leagueYear.year,
        inviteCode: this.inviteCode
      });
    },
    async notifyAction(message: string) {
      await this.refreshLeagueYear();
      if (message) {
        makeToast(message);
      }
    },
    async fetchLeagueYear(leaguePageParams: LeaguePageParams) {
      try {
        const leagueYear = await leagueClient.getLeagueYear(leaguePageParams.leagueID, leaguePageParams.year, leaguePageParams.inviteCode);
        this.leagueYear = leagueYear;
        document.title = leagueYear.league.leagueName + ' - Fantasy Critic';
      } catch (error) {
        this.setError(error);
      }
    },
    async fetchLeagueActions(leaguePageParams: LeaguePageParams) {
      try {
        this.leagueActions = await leagueClient.getLeagueActions(leaguePageParams.leagueID, leaguePageParams.year);
      } catch (error) {
        this.setError(error);
      }
    },
    async fetchLeagueActionSets(leaguePageParams: LeaguePageParams) {
      try {
        this.leagueActionSets = await leagueClient.getLeagueActionSets(leaguePageParams.leagueID, leaguePageParams.year);
      } catch (error) {
        this.setError(error);
      }
    },
    async fetchHistoricalTrades(leaguePageParams: LeaguePageParams) {
      try {
        this.historicalTrades = await leagueClient.tradeHistory(leaguePageParams.leagueID, leaguePageParams.year);
      } catch (error) {
        this.setError(error);
      }
    },
    setError(error: unknown) {
      if (ApiException.isApiException(error) && (error.status === 401 || error.status === 403)) {
        this.forbidden = true;
      } else {
        this.hasError = true;
      }
    },
    clearLeagueStoreData() {
      this.hasError = false;
      this.forbidden = false;
      this.inviteCode = undefined;
      this.leagueYear = null;
      this.leagueActions = null;
      this.leagueActionSets = null;
      this.historicalTrades = null;
    },
    setShowProjections(showProjections: boolean) {
      this.showProjections = showProjections;
      localStorage.setItem('showProjections', showProjections.toString());
    },
    setDraftOrderView(draftOrderView: boolean) {
      this.draftOrderView = draftOrderView;
      localStorage.setItem('draftOrderView', draftOrderView.toString());
    }
  }
});
