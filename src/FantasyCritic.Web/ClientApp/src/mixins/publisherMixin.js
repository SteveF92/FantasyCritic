let publisherMixin = {
  computed: {
    moveMode() {
      return this.$store.getters.moveMode;
    },
    publisher() {
      return this.$store.getters.publisher;
    },
    leagueYear() {
      return this.$store.getters.leagueYear;
    },
    gameSlots() {
      return this.$store.getters.gameSlots;
    },
    sortOrderMode() {
      return this.$store.getters.sortOrderMode;
    },
    coverArtMode() {
      return this.$store.getters.coverArtMode;
    },
    includeRemovedInSorted() {
      return this.$store.getters.includeRemovedInSorted;
    },
    userIsPublisher() {
      return this.$store.getters.userInfo && this.publisher.userID === this.$store.getters.userInfo.userID;
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
