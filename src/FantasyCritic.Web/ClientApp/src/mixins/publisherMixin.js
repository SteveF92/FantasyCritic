let publisherMixin = {
  computed: {
    moveMode() {
      return this.$store.getters['publisher/moveMode'];
    },
    publisher() {
      return this.$store.getters['publisher/publisher'];
    },
    leagueYear() {
      return this.$store.getters['publisher/leagueYear'];
    },
    gameSlots() {
      return this.$store.getters['publisher/gameSlots'];
    },
    sortOrderMode() {
      return this.$store.getters['publisher/sortOrderMode'];
    },
    coverArtMode() {
      return this.$store.getters['publisher/coverArtMode'];
    },
    includeRemovedInSorted() {
      return this.$store.getters['publisher/includeRemovedInSorted'];
    },
    userIsPublisher() {
      return this.$store.getters['auth/userInfo'] && this.publisher.userID === this.$store.getters['auth/userInfo'].userID;
    },
    hasFormerGames() {
      return this.publisher && this.publisher.formerGames.length > 0;
    }
  },
  methods: {
    async notifyAction(message) {
      await this.$store.dispatch('refreshPublisher');
      if (message) {
        this.makeToast(message);
      }
    }
  }
};

export default publisherMixin;
