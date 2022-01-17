import axios from 'axios';

export default {
  state: {
    tags: null,
    isBusy: false
  },
  getters: {
    allTags: (state) => state.tags,
    masterGamesIsBusy: (state) => state.isBusy
  },
  actions: {
    getAllTags(context) {
      context.commit('setBusy', true);
      return new Promise(function(resolve, reject) {
        axios
          .get('/api/Game/GetMasterGameTags')
          .then(response => {
            context.commit('setTags', response.data);
            context.commit('setBusy', false);
            resolve();
          })
          .catch(() => {
            context.commit('setBusy', false);
            reject();
          });
      });
    }
  },
  mutations: {
    setTags(state, tags) {
      state.tags = tags;
    },
    clearTags(state) {
      state.tags = null;
    },
    setBusy(state, isBusyFlag) {
      state.isBusy = isBusyFlag;
    }
  }
};
