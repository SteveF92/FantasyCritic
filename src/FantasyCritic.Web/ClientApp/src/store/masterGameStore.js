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
    async getAllTags(context) {
      context.commit('setBusy', true);
      try {
        const response = await axios.get('/api/Game/GetMasterGameTags');
        context.commit('setTags', response.data);
      } catch (error) {
        console.log(error);
      } finally {
        context.commit('setBusy', false);
      }
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
