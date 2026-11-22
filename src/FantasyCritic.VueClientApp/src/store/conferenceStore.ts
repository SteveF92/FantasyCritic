import { defineStore } from 'pinia';

import { conferenceClient, type ConferenceViewModel, type ConferenceYearViewModel } from '@/api/clients';
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
    requiredConferenceYear(state): ConferenceYearViewModel {
      if (!state.conferenceYear) {
        throw new Error('conferenceYear is not loaded. This component should only render under the conference page v-if.');
      }
      return state.conferenceYear;
    },
    requiredConference(state): ConferenceViewModel {
      if (!state.conferenceYear) {
        throw new Error('conferenceYear is not loaded. This component should only render under the conference page v-if.');
      }
      return state.conferenceYear.conference;
    },
    conference: (state): ConferenceViewModel | undefined => state.conferenceYear?.conference,
    isConferenceManager: (state): boolean => state.conferenceYear?.conference.isManager ?? false,
    playStarted: (state): boolean => state.conferenceYear?.leagueYears.some((x) => x.draftFinished) ?? false
  },
  actions: {
    async initializeConferencePage(conferencePageParams: ConferencePageParams): Promise<void> {
      this.hasError = false;
      this.conferenceYear = null;
      this.conferenceInviteCode = conferencePageParams.inviteCode;
      await this._fetchConferenceYear(conferencePageParams);
    },
    async refreshConferenceYear(): Promise<void> {
      if (!this.conferenceYear) {
        return;
      }
      await this._fetchConferenceYear({
        conferenceID: this.conferenceYear.conference.conferenceID,
        year: this.conferenceYear.year
      });
    },
    async notifyAction(message: string): Promise<void> {
      await this.refreshConferenceYear();
      if (message) {
        makeToast(message);
      }
    },
    async _fetchConferenceYear(conferencePageParams: ConferencePageParams): Promise<void> {
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
