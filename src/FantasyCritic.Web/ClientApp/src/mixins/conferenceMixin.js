import { mapGetters } from 'vuex';

let conferenceMixin = {
  computed: {
    ...mapGetters(['conference', 'conferenceYear', 'hasError', 'conferenceInviteCode']),
    isConferenceManager() {
      return this.conference && this.conference.isManager;
    }
  },
  methods: {
    refreshConferenceYear() {
      return this.$store.dispatch('refreshConferenceYear');
    },
    async notifyAction(message) {
      await this.$store.dispatch('refreshConferenceYear');
      if (message) {
        this.makeToast(message);
      }
    }
  }
};

export default conferenceMixin;
