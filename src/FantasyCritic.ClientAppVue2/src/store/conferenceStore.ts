import { defineStore } from 'pinia';
import axios from 'axios';

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
    }
  }
});

async function fetchConferenceYear(store, conferencePageParams) {
  const queryURL = '/api/Conference/GetConferenceYear?conferenceid=' + conferencePageParams.conferenceID + '&year=' + conferencePageParams.year;

  try {
    const response = await axios.get(queryURL);
    store.conferenceYear = response.data;
    document.title = store.conferenceYear.conference.conferenceName + ' - Fantasy Critic';
  } catch {
    store.hasError = true;
  }
}
