import { defineStore } from 'pinia';
import axios from 'axios';

export const useConferenceStore = defineStore('conference', {
  state: () => ({ hasError: false, conferenceYear: null, conferenceInviteCode: false }),
  getters: {
    hasError: (state) => state.hasError,
    conferenceYear: (state) => state.conferenceYear,
    conference: (state) => state.conferenceYear.conference,
    conferenceInviteCode: (state) => state.conferenceInviteCode
  },
  actions: {
    async initializeConferencePage(conferencePageParams) {
      this.hasError = false;
      this.conferenceYear = null;
      this.conferenceInviteCode = conferencePageParams.inviteCode;
      await this.fetchConferenceYear(conferencePageParams);
    },
    async refreshConferenceYear() {
      const conferencePageParams = {
        conferenceID: this.conferenceYear.conference.conferenceID,
        year: this.conferenceYear.year
      };
      await this.fetchConferenceYear(conferencePageParams);
    },
    async fetchConferenceYear(conferencePageParams) {
      const queryURL = '/api/Conference/GetConferenceYear?conferenceid=' + conferencePageParams.conferenceID + '&year=' + conferencePageParams.year;

      try {
        const response = await axios.get(queryURL);
        this.conferenceYear = response.data;
        document.title = this.conferenceYear.conference.conferenceName + ' - Fantasy Critic';
      } catch {
        this.hasError = true;
      }
    }
  }
});
