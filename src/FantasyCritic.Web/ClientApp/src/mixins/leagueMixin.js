import { mapState } from 'vuex';
import BasicMixin from '@/mixins/basicMixin';

let leagueMixin = {
  mixins: [BasicMixin],
  computed: {
    ...mapState({
      leagueErrorInfo: (state) => state.league.errorInfo,
      forbidden: (state) => state.league.forbidden,
      inviteCode: (state) => state.league.inviteCode,
      leagueYear: (state) => state.league.leagueYear,
      userPublisher: (state) => state.league.userPublisher,
      leagueActions: (state) => state.league.leagueActions,
      leagueActionSets: (state) => state.league.leagueActionSets,
      historicalTrades: (state) => state.league.historicalTrades,
      advancedProjections: (state) => state.league.advancedProjections,
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
      return this.leagueYear.hasSpecialSlots;
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
    }
  },
  methods: {
    async notifyAction(message) {
      await this.$store.dispatch('refreshLeagueYear');
      if (message) {
        this.makeToast(message);
      }
    }
  }
};

export default leagueMixin;
