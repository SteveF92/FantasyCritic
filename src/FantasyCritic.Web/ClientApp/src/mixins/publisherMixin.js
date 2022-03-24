import BasicMixin from '@/mixins/basicMixin';

let publisherMixin = {
  mixins: [BasicMixin],
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
    userIsPublisher() {
      return this.$store.getters.userInfo && this.publisher.userID === this.$store.getters.userInfo.userID;
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
