import { defineStore } from 'pinia';
import axios from 'axios';

export const useInterLeagueStore = defineStore('interLeague', {
  state: () => ({ tags: null, isBusy: false, possibleLeagueOptions: null, bidTimes: null, supportedYears: null, activeRoyaleQuarter: null, dataLoaded: false }),
  getters: {
    interLeagueIsBusy: (state) => state.isBusy,
    interLeagueDataLoaded: (state) => state.dataLoaded,
    allTags: (state) => state.tags
  },
  actions: {
    async fetchBasicData() {
      this.isBusy = true;

      try {
        const response = await axios.get('/api/CombinedData/BasicData');

        this.tags = response.data.masterGameTags;
        this.possibleLeagueOptions = response.data.leagueOptions;
        this.bidTimes = response.data.bidTimes;
        this.supportedYears = response.data.supportedYears;
        this.activeRoyaleQuarter = response.data.activeRoyaleQuarter;
        this.dataLoaded = true;
      } catch (error) {
        console.log(error);
      } finally {
        this.isBusy = false;
      }
    }
  }
});
