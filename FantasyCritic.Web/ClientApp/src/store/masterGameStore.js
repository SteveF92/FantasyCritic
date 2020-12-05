// auth.js
import axios from 'axios';

export default {
  state: {
    tags: null
  },
  getters: {
    allTags: (state) => state.tags
  },
  actions: {
    getAllTags(context) {
      return new Promise(function (resolve, reject) {
        axios
          .get('/api/Game/GetMasterGameTags')
          .then(response => {
            context.commit('setTags', response.data);
            resolve();
          })
          .catch(() => reject());
      });
    }
  },
  mutations: {
    setTags(state, tags) {
      state.tags = tags;
    },
    clearTags(state) {
      state.tags = null;
    }
  }
};
