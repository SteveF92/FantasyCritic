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
    isActionRunner: (state) => state.userInfo && state.userInfo.roles.includes('ActionRunner'),
    isFactChecker: (state) => state.userInfo && state.userInfo.roles.includes('FactChecker'),
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
        } else {
          context.commit('setUserInfo', response.data);
        }
      } catch (error) {
        context.commit('clearUserInfo');
      } finally {
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
