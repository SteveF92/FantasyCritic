import { defineStore } from 'pinia';

import { conferenceClient, ConferenceYearViewModel } from '@/api/clients';
import { makeToast } from '@/utilities/toast';

export interface ConferencePageParams {
  conferenceID: string;
  year: number;
  inviteCode?: string;
}

interface ConferenceStoreState {
  hasError: boolean;
  conferenceYear: ConferenceYearViewModel | null;
  conferenceInviteCode?: string;
}

export const useConferenceStore = defineStore('conference', {
  state: (): ConferenceStoreState => ({ hasError: false, conferenceYear: null, conferenceInviteCode: undefined }),
  getters: {
    conference: (state) => state.conferenceYear?.conference,
    isConferenceManager: (state) => state.conferenceYear?.conference.isManager ?? false,
    playStarted: (state) => state.conferenceYear?.leagueYears.some((x) => x.draftFinished) ?? false
  },
  actions: {
    async initializeConferencePage(conferencePageParams: ConferencePageParams) {
      this.hasError = false;
      this.conferenceYear = null;
      this.conferenceInviteCode = conferencePageParams.inviteCode;
      await this._fetchConferenceYear(conferencePageParams);
    },
    async refreshConferenceYear() {
      if (!this.conferenceYear) {
        return;
      }
      await this._fetchConferenceYear({
        conferenceID: this.conferenceYear.conference.conferenceID,
        year: this.conferenceYear.year
      });
    },
    async notifyAction(message: string) {
      await this.refreshConferenceYear();
      if (message) {
        makeToast(message);
      }
    },
    async _fetchConferenceYear(conferencePageParams: ConferencePageParams) {
      try {
        const conferenceYear = await conferenceClient.getConferenceYear(conferencePageParams.conferenceID, conferencePageParams.year);
        this.conferenceYear = conferenceYear;
        document.title = conferenceYear.conference.conferenceName + ' - Fantasy Critic';
      } catch {
        this.hasError = true;
      }
    }
  }
});
