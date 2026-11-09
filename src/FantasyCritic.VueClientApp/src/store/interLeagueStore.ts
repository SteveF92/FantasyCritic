import { defineStore } from 'pinia';

import { BasicDataViewModel, combinedDataClient } from '@/api/clients';

interface InterLeagueStoreState {
  basicData: BasicDataViewModel | null;
  loadPromise: Promise<void> | null;
}

function requireBasicData(state: InterLeagueStoreState): BasicDataViewModel {
  if (!state.basicData) {
    throw new Error('Inter-league data was read before it was loaded. The router guard should have loaded it.');
  }
  return state.basicData;
}

export const useInterLeagueStore = defineStore('interLeague', {
  state: (): InterLeagueStoreState => ({ basicData: null, loadPromise: null }),
  getters: {
    interLeagueDataLoaded: (state) => state.basicData !== null,
    allTags: (state) => requireBasicData(state).masterGameTags,
    possibleLeagueOptions: (state) => requireBasicData(state).leagueOptions,
    bidTimes: (state) => requireBasicData(state).bidTimes,
    supportedYears: (state) => requireBasicData(state).supportedYears,
    mostRecentYear: (state) => requireBasicData(state).supportedYears[requireBasicData(state).supportedYears.length - 1],
    activeRoyaleQuarter: (state) => requireBasicData(state).activeRoyaleQuarter
  },
  actions: {
    ensureLoaded(): Promise<void> {
      if (!this.loadPromise) {
        this.loadPromise = combinedDataClient.basicData().then(
          (response) => {
            this.basicData = response;
          },
          (error) => {
            this.loadPromise = null;
            throw error;
          }
        );
      }
      return this.loadPromise;
    },
    async refreshBasicData() {
      this.basicData = await combinedDataClient.basicData();
    }
  }
});
