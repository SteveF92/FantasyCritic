import { mapGetters } from 'vuex';

let conferenceMixin = {
  computed: {
    ...mapGetters(['conference', 'conferenceYear', 'hasError']),
    isConferenceManager() {
      return this.conference && this.conference.isManager;
    }
  },
  methods: {}
};

export default conferenceMixin;
