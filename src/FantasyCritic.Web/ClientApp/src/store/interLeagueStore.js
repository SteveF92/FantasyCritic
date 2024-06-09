import axios from 'axios';

export default {
  state: {
    authIsBusy: false,
    userInfo: null,
    interLeagueIsBusy: false,
    tags: null,
    possibleLeagueOptions: null,
    bidTimes: null,
    dataLoaded: false
  },
  getters: {
    isAuth: (state) => !!state.userInfo,
    userInfo: (state) => state.userInfo,
    authIsBusy: (state) => state.authIsBusy,
    isAdmin: (state) => state.userInfo && state.userInfo.roles.includes('Admin'),
    isActionRunner: (state) => state.userInfo && state.userInfo.roles.includes('ActionRunner'),
    isFactChecker: (state) => state.userInfo && state.userInfo.roles.includes('FactChecker'),
    isBetaTester: (state) => state.userInfo && state.userInfo.roles.includes('BetaTester'),
    isPlusUser: (state) => state.userInfo && state.userInfo.roles.includes('PlusUser'),
    interLeagueIsBusy: (state) => state.interLeagueIsBusy,
    interLeagueDataLoaded: (state) => state.dataLoaded,
    allTags: (state) => state.tags,
    bidTimes: (state) => state.bidTimes
  },
  actions: {
    async fetchBasicData(context) {
      context.commit('setInterLeagueBusy', true);
      context.commit('setAuthBusy', true);

      try {
        const response = await axios.get('/api/General/BasicData');

        if (!response.data.currentUser) {
          context.commit('clearUserInfo');
        } else {
          context.commit('setUserInfo', response.data.currentUser);
        }

        context.commit('setTags', response.data.masterGameTags);
        context.commit('setPossibleLeagueOptions', response.data.leagueOptions);
        context.commit('setBidTimes', response.data.bidTimes);
        context.commit('setDataLoaded', true);
      } catch (error) {
        context.commit('clearUserInfo');
        console.log(error);
      } finally {
        context.commit('setInterLeagueBusy', false);
        context.commit('setAuthBusy', false);
      }
    },
    async getUserInfo(context) {
      context.commit('setAuthBusy', true);
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
        context.commit('setAuthBusy', false);
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
    setAuthBusy(state, isBusyFlag) {
      state.authIsBusy = isBusyFlag;
    },
    setInterLeagueBusy(state, isBusyFlag) {
      state.interLeagueIsBusy = isBusyFlag;
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
