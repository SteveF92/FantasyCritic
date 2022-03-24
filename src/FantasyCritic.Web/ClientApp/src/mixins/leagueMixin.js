import { mapGetters } from 'vuex';
import BasicMixin from '@/mixins/basicMixin';

let leagueMixin = {
  mixins: [BasicMixin],
  computed: {
    ...mapGetters([
      'forbidden',
      'inviteCode',
      'leagueYear',
      'userPublisher',
      'currentBids',
      'currentDrops',
      'gameNews',
      'leagueActions',
      'leagueActionSets',
      'historicalTrades',
      'advancedProjections',
      'draftOrderView'
    ]),
    league() {
      if (!this.$store.getters.leagueYear) {
        return;
      }
      return this.$store.getters.leagueYear.league;
    },
    leagueErrorInfo() {
      return this.$store.getters.errorInfo;
    },
    publishers() {
      return this.leagueYear.publishers;
    },
    players() {
      return this.leagueYear.players;
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
