import axios from 'axios';

export default {
  state: {
    tags: null,
    isBusy: false,
    possibleLeagueOptions: null,
    bidTimes: null,
    supportedYears: null,
    dataLoaded: false
  },
  getters: {
    interLeagueIsBusy: (state) => state.isBusy,
    interLeagueDataLoaded: (state) => state.dataLoaded,
    allTags: (state) => state.tags,
    bidTimes: (state) => state.bidTimes
  },
  actions: {
    async fetchBasicData(context) {
      context.commit('setBusy', true);

      try {
        const response = await axios.get('/api/General/BasicData');
        context.commit('setTags', response.data.masterGameTags);
        context.commit('setPossibleLeagueOptions', response.data.leagueOptions);
        context.commit('setBidTimes', response.data.bidTimes);
        context.commit('setSupportedYears', response.data.supportedYears);
        context.commit('setDataLoaded', true);
      } catch (error) {
        console.log(error);
      } finally {
        context.commit('setBusy', false);
      }
    }
  },
  mutations: {
    setBusy(state, isBusyFlag) {
      state.isBusy = isBusyFlag;
    },
    setDataLoaded(state, dataLoaded) {
      state.dataLoaded = dataLoaded;
    },
    setTags(state, tags) {
      state.tags = tags;
    },
    setSupportedYears(state, supportedYears) {
      state.supportedYears = supportedYears;
    },
    setPossibleLeagueOptions(state, possibleLeagueOptions) {
      state.possibleLeagueOptions = possibleLeagueOptions;
    },
    setBidTimes(state, bidTimes) {
      state.bidTimes = bidTimes;
    }
  }
};
