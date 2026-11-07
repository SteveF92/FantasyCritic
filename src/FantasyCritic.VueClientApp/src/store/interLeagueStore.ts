import { defineStore } from 'pinia';
import { BidTimesViewModel, combinedDataClient, LeagueOptionsViewModel, MasterGameTagViewModel, RoyaleYearQuarterViewModel, SupportedYearViewModel } from '@/api/clients';

export interface InterLeagueStoreState {
  tags: MasterGameTagViewModel[];
  isBusy: boolean;
  possibleLeagueOptions: LeagueOptionsViewModel;
  bidTimes: BidTimesViewModel;
  supportedYears: SupportedYearViewModel[];
  activeRoyaleQuarter: RoyaleYearQuarterViewModel;
  dataLoaded: boolean;
}

export const useInterLeagueStore = defineStore('interLeague', {
  state: (): InterLeagueStoreState => ({ tags: null, isBusy: false, possibleLeagueOptions: null, bidTimes: null, supportedYears: null, activeRoyaleQuarter: null, dataLoaded: false }),
  getters: {
    interLeagueIsBusy: (state) => state.isBusy,
    interLeagueDataLoaded: (state) => state.dataLoaded,
    allTags: (state) => state.tags
  },
  actions: {
    async fetchBasicData() {
      this.isBusy = true;

      try {
        const response = await combinedDataClient.basicData();

        this.tags = response.masterGameTags;
        this.possibleLeagueOptions = response.leagueOptions;
        this.bidTimes = response.bidTimes;
        this.supportedYears = response.supportedYears;
        this.activeRoyaleQuarter = response.activeRoyaleQuarter;
        this.dataLoaded = true;
      } catch (error) {
        console.log(error);
      } finally {
        this.isBusy = false;
      }
    }
  }
});
