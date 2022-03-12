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
    getBidTimes(context) {
      context.commit('setBusy', true);
      return new Promise(function (resolve, reject) {
        axios
          .get('/api/General/BidTimes')
          .then((response) => {
            context.commit('setBidTimes', response.data);
            context.commit('setBidsBusy', false);
            resolve();
          })
          .catch(() => {
            context.commit('setBidsBusy', false);
            reject();
          });
      });
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
