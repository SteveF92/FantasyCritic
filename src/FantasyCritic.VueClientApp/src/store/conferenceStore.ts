import { defineStore } from 'pinia';
import { conferenceClient, ConferenceYearViewModel } from '@/api/clients';
import { makeToast } from '@/utilities/toast';

export interface ConferenceStoreState {
  hasError: boolean;
  conferenceYear: ConferenceYearViewModel | null;
  conferenceInviteCode?: string;
}

export interface ConferencePageParams {
  conferenceID: string;
  year: number;
  inviteCode?: string;
}

export const useConferenceStore = defineStore('conference', {
  state: (): ConferenceStoreState => ({ hasError: false, conferenceYear: null, conferenceInviteCode: undefined }),
  getters: {
    conference: (state) => state.conferenceYear?.conference,
    isConferenceManager: (state) => state.conferenceYear?.conference?.isManager ?? false,
    playStarted: (state) => state.conferenceYear?.leagueYears.some((x) => x.draftFinished) ?? false
  },
  actions: {
    async initializeConferencePage(conferencePageParams: ConferencePageParams) {
      this.hasError = false;
      this.conferenceYear = null;
      this.conferenceInviteCode = conferencePageParams.inviteCode;
      await fetchConferenceYear(this, conferencePageParams);
    },
    async refreshConferenceYear() {
      if (!this.conferenceYear) {
        return;
      }
      const conferencePageParams = {
        conferenceID: this.conferenceYear.conference.conferenceID,
        year: this.conferenceYear.year
      };
      await fetchConferenceYear(this, conferencePageParams);
    },
    async notifyAction(message: string) {
      await this.refreshConferenceYear();
      if (message) {
        makeToast(message);
      }
    }
  }
});

async function fetchConferenceYear(store: any, conferencePageParams: ConferencePageParams) {
  try {
    store.conferenceYear = await conferenceClient.getConferenceYear(conferencePageParams.conferenceID, conferencePageParams.year);
    document.title = store.conferenceYear.conference.conferenceName + ' - Fantasy Critic';
  } catch {
    store.hasError = true;
  }
}
