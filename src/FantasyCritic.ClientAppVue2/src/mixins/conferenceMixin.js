import { mapState } from 'pinia';
import { useConferenceStore } from '@/store/conferenceStore';

const conferenceMixin = {
  computed: {
    ...mapState(useConferenceStore, ['conference', 'conferenceYear', 'hasError', 'conferenceInviteCode']),
    isConferenceManager() {
      return this.conference && this.conference.isManager;
    },
    playStarted() {
      return this.conferenceYear.leagueYears.some((x) => x.draftFinished);
    }
  },
  methods: {
    refreshConferenceYear() {
      const conferenceStore = useConferenceStore();
      conferenceStore.refreshConferenceYear();
    },
    async notifyAction(message) {
      const conferenceStore = useConferenceStore();
      conferenceStore.refreshConferenceYear();
      if (message) {
        this.makeToast(message);
      }
    }
  }
};

export default conferenceMixin;
