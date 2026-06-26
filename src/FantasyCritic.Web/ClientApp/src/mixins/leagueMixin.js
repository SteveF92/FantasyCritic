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
    draftFinished() {
      return this.firstDraft.draftFinished;
    },
    postDraftPlayable() {
      return this.firstDraft.draftFinished && !this.leagueYear.supportedYear.finished;
    },
    postDraftEditable() {
      return this.firstDraft.draftFinished && (!this.leagueYear.supportedYear.finished || this.leagueYear.underReview);
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
