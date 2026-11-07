import { defineStore } from 'pinia';
import { conferenceClient } from '@/api/clients';
import { makeToast } from '@/utilities/toast';

export const useConferenceStore = defineStore('conference', {
  state: () => ({ hasError: false, conferenceYear: null, conferenceInviteCode: false }),
  getters: {
    conference: (state) => state.conferenceYear?.conference,
    isConferenceManager: (state) => state.conferenceYear?.conference?.isManager ?? false,
    playStarted: (state) => state.conferenceYear?.leagueYears.some((x) => x.draftFinished) ?? false
  },
  actions: {
    async initializeConferencePage(conferencePageParams) {
      this.hasError = false;
      this.conferenceYear = null;
      this.conferenceInviteCode = conferencePageParams.inviteCode;
      await fetchConferenceYear(this, conferencePageParams);
    },
    async refreshConferenceYear() {
      const conferencePageParams = {
        conferenceID: this.conferenceYear.conference.conferenceID,
        year: this.conferenceYear.year
      };
      await fetchConferenceYear(this, conferencePageParams);
    },
    async notifyAction(message) {
      await this.refreshConferenceYear();
      if (message) {
        makeToast(message);
      }
    }
  }
});

async function fetchConferenceYear(store, conferencePageParams) {
  try {
    store.conferenceYear = await conferenceClient.getConferenceYear(conferencePageParams.conferenceID, conferencePageParams.year);
    document.title = store.conferenceYear.conference.conferenceName + ' - Fantasy Critic';
  } catch {
    store.hasError = true;
  }
}
