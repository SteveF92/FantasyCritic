import { mapState } from 'pinia';
import { useConferenceStore } from '@/store/conferenceStore';
import { makeToast } from '@/utilities/toast';

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
    async refreshConferenceYear() {
      const conferenceStore = useConferenceStore();
      await conferenceStore.refreshConferenceYear();
    },
    async notifyAction(message) {
      const conferenceStore = useConferenceStore();
      await conferenceStore.refreshConferenceYear();
      if (message) {
        makeToast(message);
      }
    }
  }
};

export default conferenceMixin;
