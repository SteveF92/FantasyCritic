import BasicMixin from '@/mixins/basicMixin';

let leagueMixin = {
  mixins: [BasicMixin],
  computed: {
    errorInfo() {
      return this.$store.getters.errorInfo;
    },
    forbidden() {
      return this.$store.getters.forbidden;
    },
    inviteCode() {
      return this.$store.getters.inviteCode;
    },
    league() {
      return this.$store.getters.league;
    },
    leagueYear() {
      return this.$store.getters.leagueYear;
    },
    currentBids() {
      return this.$store.getters.currentBids;
    },
    currentDrops() {
      return this.$store.getters.currentDrops;
    },
    gameNews() {
      return this.$store.getters.gameNews;
    },
    advancedProjections() {
      return this.$store.getters.advancedProjections;
    },
    draftOrderView() {
      return this.$store.getters.draftOrderView;
    }
  }
};

export default leagueMixin;
