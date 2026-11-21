import { defineStore } from 'pinia';
import { DateTime } from 'luxon';

import {
  ApiException,
  leagueClient,
  type DropGameRequestViewModel,
  type GameNewsViewModel,
  type LeagueActionProcessingSetViewModel,
  type LeagueActionViewModel,
  type LeagueDraftViewModel,
  type LeagueViewModel,
  type LeagueYearViewModel,
  type PickupBidViewModel,
  type PlayerWithPublisherViewModel,
  type PublisherViewModel,
  type QueuedGameViewModel,
  type SupportedYearViewModel,
  type TradeViewModel
} from '@/api/clients';
import { useAuthStore } from '@/store/authStore';
import { usePublisherStore } from '@/store/publisherStore';
import { makeToast } from '@/utilities/toast';

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
    requiredLeagueYear(state): LeagueYearViewModel {
      if (!state.leagueYear) {
        throw new Error('leagueYear is not loaded. This component should only render under the league page v-if.');
      }
      return state.leagueYear;
    },
    requiredLeague(state): LeagueViewModel {
      if (!state.leagueYear?.league) {
        throw new Error('leagueYear is not loaded. This component should only render under the league page v-if.');
      }
      return state.leagueYear.league;
    },
    requiredSupportedYear(state): SupportedYearViewModel {
      if (!state.leagueYear?.supportedYear) {
        throw new Error('leagueYear is not loaded. This component should only render under the league page v-if.');
      }
      return state.leagueYear.supportedYear;
    },
    league: (state): LeagueViewModel | undefined => state.leagueYear?.league,
    publishers: (state): PublisherViewModel[] | undefined => state.leagueYear?.publishers,
    players: (state): PlayerWithPublisherViewModel[] | undefined => state.leagueYear?.players,
    hasSpecialSlots: (state): boolean => state.leagueYear?.settings.hasSpecialSlots ?? false,
    supportedYear: (state): SupportedYearViewModel | undefined => state.leagueYear?.supportedYear,
    oneShotMode: (state): boolean => state.leagueYear?.settings.oneShotMode ?? false,
    isManager: (state): boolean => state.leagueYear?.league.isManager ?? false,
    firstDraft: (state): LeagueDraftViewModel | null => state.leagueYear?.drafts?.[0] ?? null,
    pendingDraft: (state): LeagueDraftViewModel | null => state.leagueYear?.drafts?.find((d) => d.playStatus === 'NotStartedDraft') ?? null,
    activeDraft: (state): LeagueDraftViewModel | null => state.leagueYear?.drafts?.find((d) => d.draftIsActive || d.draftIsPaused) ?? null,
    nextPublisherUp: (state): PublisherViewModel | null => state.leagueYear?.publishers?.find((x) => x.nextToDraft) ?? null,
    // Publishers with no user stay in the standings, but can't win.
    publishersThatCanWin: (state): PublisherViewModel[] => (state.leagueYear?.publishers ?? []).filter((x) => !!x.userID),
    // The server picks it, so ties break the same way as the recorded winner.
    topPublisher: (state): PublisherViewModel | null => (state.leagueYear?.publishers ?? []).find((x) => x.publisherID === state.leagueYear?.winningPublisherID) ?? null,
    currentBids: (state): PickupBidViewModel[] => state.leagueYear?.privatePublisherData?.myActiveBids ?? [],
    currentDrops: (state): DropGameRequestViewModel[] => state.leagueYear?.privatePublisherData?.myActiveDrops ?? [],
    queuedGames: (state): QueuedGameViewModel[] => state.leagueYear?.privatePublisherData?.queuedGames ?? [],
    gameNews: (state): GameNewsViewModel | undefined => state.leagueYear?.gameNews,
    userPublisher(state): PublisherViewModel | null {
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
      await this._fetchLeagueYear(leaguePageParams);
    },
    async initializeHistoryPage(leaguePageParams: LeaguePageParams) {
      const publisherStore = usePublisherStore();
      publisherStore.clearPublisherStoreData();
      this.clearLeagueStoreData();
      await this._fetchLeagueYear(leaguePageParams);
      await Promise.all([this._fetchLeagueActions(leaguePageParams), this._fetchLeagueActionSets(leaguePageParams), this._fetchHistoricalTrades(leaguePageParams)]);
    },
    async refreshLeagueYear() {
      if (!this.leagueYear) {
        throw new Error('League store is not initialized');
      }

      await this._fetchLeagueYear({
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
    },
    async _fetchLeagueYear(leaguePageParams: LeaguePageParams) {
      try {
        const leagueYear = await leagueClient.getLeagueYear(leaguePageParams.leagueID, leaguePageParams.year, leaguePageParams.inviteCode);
        this.leagueYear = leagueYear;
        document.title = leagueYear.league.leagueName + ' - Fantasy Critic';
      } catch (error) {
        this._setError(error);
      }
    },
    async _fetchLeagueActions(leaguePageParams: LeaguePageParams) {
      try {
        this.leagueActions = await leagueClient.getLeagueActions(leaguePageParams.leagueID, leaguePageParams.year);
      } catch (error) {
        this._setError(error);
      }
    },
    async _fetchLeagueActionSets(leaguePageParams: LeaguePageParams) {
      try {
        this.leagueActionSets = await leagueClient.getLeagueActionSets(leaguePageParams.leagueID, leaguePageParams.year);
      } catch (error) {
        this._setError(error);
      }
    },
    async _fetchHistoricalTrades(leaguePageParams: LeaguePageParams) {
      try {
        this.historicalTrades = await leagueClient.tradeHistory(leaguePageParams.leagueID, leaguePageParams.year);
      } catch (error) {
        this._setError(error);
      }
    },
    _setError(error: unknown) {
      if (ApiException.isApiException(error) && (error.status === 401 || error.status === 403)) {
        this.forbidden = true;
      } else {
        this.hasError = true;
      }
    }
  }
});
