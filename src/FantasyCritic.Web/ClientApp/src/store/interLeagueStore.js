import axios from 'axios';

export default {
  namespaced: true,
  state: {
    tags: null,
    isBusy: false,
    possibleLeagueOptions: null,
    bidTimes: null,
    dataLoaded: false
  },
  getters: {
    interLeagueIsBusy: (state) => state.isBusy,
    interLeagueDataLoaded: (state) => state.dataLoaded,
    allTags: (state) => state.tags,
    bidTimes: (state) => state.bidTimes
  },
  actions: {
    async fetchInterLeagueData(context) {
      context.commit('setBusy', true);

      try {
        const tasks = [context.dispatch('getAllTags'), context.dispatch('getPossibleLeagueOptions'), context.dispatch('getBidTimes')];
        await Promise.all(tasks);
        context.commit('setDataLoaded', true);
      } finally {
        context.commit('setBusy', false);
      }
    },
    async getAllTags(context) {
      try {
        const response = await axios.get('/api/Game/GetMasterGameTags');
        context.commit('setTags', response.data);
      } catch (error) {
        console.log(error);
      }
    },
    async getPossibleLeagueOptions(context) {
      try {
        const response = await axios.get('/api/League/LeagueOptions');
        context.commit('setPossibleLeagueOptions', response.data);
      } catch (error) {
        console.log(error);
      }
    },
    async getBidTimes(context) {
      try {
        const response = await axios.get('/api/General/BidTimes');
        context.commit('setBidTimes', response.data);
      } catch (error) {
        console.log(error);
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
    clearTags(state) {
      state.tags = null;
    },
    setPossibleLeagueOptions(state, possibleLeagueOptions) {
      state.possibleLeagueOptions = possibleLeagueOptions;
    },
    setBidTimes(state, bidTimes) {
      state.bidTimes = bidTimes;
    }
  }
};
