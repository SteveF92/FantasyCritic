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
    }
  }
};

export default conferenceMixin;
