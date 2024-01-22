import { mapState } from 'vuex';
import _ from 'lodash';

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
    nextPublisherUp() {
      if (!this.leagueYear || !this.leagueYear.publishers) {
        return null;
      }
      let next = _.find(this.leagueYear.publishers, ['nextToDraft', true]);
      return next;
    },
    userIsNextInDraft() {
      if (this.nextPublisherUp && this.leagueYear && this.userPublisher) {
        return this.nextPublisherUp.publisherID === this.userPublisher.publisherID;
      }

      return false;
    },
    draftIsPaused() {
      return this.leagueYear.playStatus.draftIsPaused;
    },
    isManager() {
      return this.league && this.league.isManager;
    },
    topPublisher() {
      if (this.leagueYear.publishers && this.leagueYear.publishers.length > 0) {
        return _.maxBy(this.leagueYear.publishers, 'totalFantasyPoints');
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
      return this.leagueYear.playStatus.playStarted;
    },
    readyToSetDraftOrder() {
      return this.leagueYear.playStatus.readyToSetDraftOrder;
    },
    draftFinished() {
      return this.leagueYear.playStatus.draftFinished;
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
