import axios from 'axios';

export default {
  state: {
    bidTimes: null,
    isBusy: false
  },
  getters: {
    bidTimes: (state) => state.bidTimes,
    bidTimesIsBusy: (state) => state.isBusy
  },
  actions: {
    async getBidTimes(context) {
      context.commit('setBusy', true);
      try {
        const response = await axios.get('/api/General/BidTimes');
        context.commit('setBidTimes', response.data);
      } catch (error) {
        console.log(error);
      } finally {
        context.commit('setBusy', false);
      }
    }
  },
  mutations: {
    setBidTimes(state, bidTimes) {
      state.bidTimes = bidTimes;
    },
    setBidsBusy(state, isBusyFlag) {
      state.isBusy = isBusyFlag;
    }
  }
};
