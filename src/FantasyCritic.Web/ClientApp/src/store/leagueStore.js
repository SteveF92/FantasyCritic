import axios from 'axios';
import _ from 'lodash';

export default {
  namespaced: true,
  state: {
    hasError: false,
    forbidden: false,
    inviteCode: false,
    leagueYear: null,
    userPublisher: null,
    leagueActions: null,
    leagueActionSets: null,
    historicalTrades: null,
    showProjections: false,
    draftOrderView: false
  },
  actions: {
    async initializeLeaguePage(context, leaguePageParams) {
      context.commit('clearPublisherStoreData');
      context.commit('clearLeagueStoreData');
      context.commit('setInviteCode', leaguePageParams.inviteCode);
      await context.dispatch('fetchLeagueYear', leaguePageParams);
    },
    async initializeHistoryPage(context, leaguePageParams) {
      context.commit('clearPublisherStoreData');
      context.commit('clearLeagueStoreData');
      await context.dispatch('fetchLeagueYear', leaguePageParams);
      await context.dispatch('fetchHistoryData');
    },
    async refreshLeagueYear(context) {
      const leaguePageParams = {
        leagueID: context.state.leagueYear.leagueID,
        year: context.state.leagueYear.year,
        inviteCode: context.state.inviteCode
      };
      await context.dispatch('fetchLeagueYear', leaguePageParams);
    },
    async fetchLeagueYear(context, leaguePageParams) {
      let queryURL = '/api/League/GetLeagueYear?leagueID=' + leaguePageParams.leagueID + '&year=' + leaguePageParams.year;
      if (leaguePageParams.inviteCode) {
        queryURL += '&inviteCode=' + leaguePageParams.inviteCode;
      }

      try {
        const response = await axios.get(queryURL);
        const leagueYearPayload = {
          leagueYear: response.data
        };
        if (context.getters['auth/userInfo']) {
          leagueYearPayload.userID = context.getters['auth/userInfo'].userID;
        }

        context.commit('setLeagueYear', leagueYearPayload);
      } catch (err) {
        context.commit('setError', err.response);
      }
    },
    fetchHistoryData(context) {
      let leagueActionsPromise = context.dispatch('fetchLeagueActions');
      let leagueActionSetsPromise = context.dispatch('fetchLeagueActionSets');
      let historicalTradesPromise = context.dispatch('fetchHistoricalTrades');
      return Promise.all([leagueActionsPromise, leagueActionSetsPromise, historicalTradesPromise]);
    },
    fetchLeagueActions(context) {
      const queryURL = '/api/League/GetLeagueActions?leagueID=' + context.state.leagueYear.leagueID + '&year=' + context.state.leagueYear.year;
      return axios
        .get(queryURL)
        .then((response) => {
          context.commit('setLeagueActions', response.data);
        })
        .catch((err) => context.commit('setError', err.response));
    },
    fetchLeagueActionSets(context) {
      const queryURL = '/api/League/GetLeagueActionSets?leagueID=' + context.state.leagueYear.leagueID + '&year=' + context.state.leagueYear.year;
      return axios
        .get(queryURL)
        .then((response) => {
          context.commit('setLeagueActionSets', response.data);
        })
        .catch((err) => context.commit('setError', err.response));
    },
    fetchHistoricalTrades(context) {
      const queryURL = '/api/League/TradeHistory?leagueID=' + context.state.leagueYear.leagueID + '&year=' + context.state.leagueYear.year;
      return axios
        .get(queryURL)
        .then((response) => {
          context.commit('setHistoricalTrades', response.data);
        })
        .catch((err) => context.commit('setError', err.response));
    }
  },
  mutations: {
    clearLeagueStoreData(state) {
      state.hasError = false;
      state.forbidden = null;
      state.inviteCode = null;
      state.leagueYear = null;
      state.userPublisher = null;
      state.gameNews = null;

      state.leagueActions = null;
      state.leagueActionSets = null;
      state.historicalTrades = null;
    },
    setError(state, response) {
      if (response.status === 401 || response.status === 403) {
        state.forbidden = true;
      } else {
        state.hasError = true;
      }
    },
    setInviteCode(state, inviteCode) {
      state.inviteCode = inviteCode;
    },
    setLeagueYear(state, leagueYearPayload) {
      state.leagueYear = leagueYearPayload.leagueYear;
      document.title = leagueYearPayload.leagueYear.league.leagueName + ' - Fantasy Critic';

      if (leagueYearPayload.userID) {
        let matchingPublishers = _.filter(leagueYearPayload.leagueYear.publishers, (x) => x.userID === leagueYearPayload.userID);
        state.userPublisher = matchingPublishers[0];
      }
    },
    setGameNews(state, gameNews) {
      state.gameNews = gameNews;
    },
    setLeagueActions(state, leagueActions) {
      state.leagueActions = leagueActions;
    },
    setLeagueActionSets(state, leagueActionSets) {
      state.leagueActionSets = leagueActionSets;
    },
    setHistoricalTrades(state, historicalTrades) {
      state.historicalTrades = historicalTrades;
    },
    setShowProjections(state, showProjections) {
      state.showProjections = showProjections;
    },
    setDraftOrderView(state, draftOrderView) {
      state.draftOrderView = draftOrderView;
    }
  }
};
