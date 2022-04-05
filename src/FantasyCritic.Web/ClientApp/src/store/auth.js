// auth.js
import axios from 'axios';

export default {
  state: {
    userInfo: null,
    isBusy: false
  },
  getters: {
    isAuth: (state) => !!state.userInfo,
    userInfo: (state) => state.userInfo,
    authIsBusy: (state) => state.isBusy,
    isAdmin: (state) => state.userInfo && state.userInfo.roles.includes('Admin'),
    isBetaTester: (state) => state.userInfo && state.userInfo.roles.includes('BetaTester'),
    isPlusUser: (state) => state.userInfo && state.userInfo.roles.includes('PlusUser')
  },
  actions: {
    async getUserInfo(context) {
      context.commit('setBusy', true);
      try {
        const response = await axios.get('/api/account/CurrentUser');
        if (!response.data.userID) {
          context.commit('clearUserInfo');
          context.commit('setBusy', false);
        } else {
          context.commit('setUserInfo', response.data);
          context.commit('setBusy', false);
        }
      } catch (error) {
        context.commit('clearUserInfo');
        context.commit('setBusy', false);
      }
    }
  },
  mutations: {
    setUserInfo(state, userInfo) {
      state.userInfo = userInfo;
    },
    clearUserInfo(state) {
      state.userInfo = null;
    },
    setBusy(state, isBusyFlag) {
      state.isBusy = isBusyFlag;
    }
  }
};
