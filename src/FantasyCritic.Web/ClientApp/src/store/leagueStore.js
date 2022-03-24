import axios from 'axios';

const leagueErrorMessageText = 'Something went wrong with this league. Contact us on Twitter or Discord for support.';

export default {
  state: {
    errorInfoInternal: null,
    forbiddenInternal: false,
    inviteCodeInternal: false,
    leagueYearInternal: null,
    userPublisherInternal: null,
    currentBidsInternal: null,
    currentDropsInternal: null,
    gameNewsInternal: null,
    leagueActionsInternal: null,
    leagueActionSetsInternal: null,
    historicalTradesInternal: null,
    advancedProjectionsInternal: false,
    draftOrderViewInternal: false
  },
  getters: {
    errorInfo: (state) => state.errorInfoInternal,
    forbidden: (state) => state.forbiddenInternal,
    inviteCode: (state) => state.inviteCodeInternal,
    leagueYear: (state) => state.leagueYearInternal,
    userPublisher: (state) => state.userPublisherInternal,
    currentBids: (state) => state.currentBidsInternal,
    currentDrops: (state) => state.currentDropsInternal,
    gameNews: (state) => state.gameNewsInternal,
    leagueActions: (state) => state.leagueActionsInternal,
    leagueActionSets: (state) => state.leagueActionSetsInternal,
    historicalTrades: (state) => state.historicalTradesInternal,
    advancedProjections: (state) => state.advancedProjectionsInternal,
    draftOrderView: (state) => state.draftOrderViewInternal
  },
  actions: {
    async initializePage(context, leaguePageParams) {
      context.commit('cancelMoveMode');
      context.commit('clearLeagueSpecificData');
      context.commit('setInviteCode', leaguePageParams.inviteCode);
      await context.dispatch('fetchLeagueYear', leaguePageParams);
      if (context.getters.userPublisher) {
        context.dispatch('fetchAdditionalLeagueData');
      }
      context.dispatch('fetchGameNews');
    },
    async initializeHistoryPage(context, leaguePageParams) {
      context.commit('cancelMoveMode');
      context.commit('clearLeagueSpecificData');
      await context.dispatch('fetchLeagueYear', leaguePageParams);
      context.dispatch('fetchHistoryData');
    },
    async refreshLeagueYear(context) {
      const leaguePageParams = {
        leagueID: context.getters.leagueYear.leagueID,
        year: context.getters.leagueYear.year,
        inviteCode: context.getters.inviteCode
      };
      await context.dispatch('fetchLeagueYear', leaguePageParams);
      if (context.getters.userPublisher) {
        context.dispatch('fetchAdditionalLeagueData');
      }
      context.dispatch('fetchGameNews');
    },
    fetchLeagueYear(context, leaguePageParams) {
      let queryURL = '/api/League/GetLeagueYear?leagueID=' + leaguePageParams.leagueID + '&year=' + leaguePageParams.year;
      if (leaguePageParams.inviteCode) {
        queryURL += '&inviteCode=' + leaguePageParams.inviteCode;
      }
      return axios
        .get(queryURL)
        .then((response) => {
          const leagueYearPayload = {
            leagueYear: response.data,
            userID: context.getters.userInfo.userID
          };
          context.commit('setLeagueYear', leagueYearPayload);
        })
        .catch(() => {
          context.commit('setErrorInfo', leagueErrorMessageText);
        });
    },
    fetchAdditionalLeagueData(context) {
      let currentBidsPromise = context.dispatch('fetchCurrentBids');
      let currentDropRequestsPromise = context.dispatch('fetchCurrentDropRequests');
      return Promise.all([currentBidsPromise, currentDropRequestsPromise]);
    },
    fetchHistoryData(context) {
      let leagueActionsPromise = context.dispatch('fetchLeagueActions');
      let leagueActionSetsPromise = context.dispatch('fetchLeagueActionSets');
      let historicalTradesPromise = context.dispatch('fetchHistoricalTrades');
      return Promise.all([leagueActionsPromise, leagueActionSetsPromise, historicalTradesPromise]);
    },
    fetchCurrentBids(context) {
      const queryURL = '/api/league/CurrentBids/' + context.getters.userPublisher.publisherID;
      return axios
        .get(queryURL)
        .then((response) => {
          context.commit('setCurrentBids', response.data);
        })
        .catch(() => {});
    },
    fetchCurrentDropRequests(context) {
      const queryURL = '/api/league/CurrentDropRequests/' + context.getters.userPublisher.publisherID;
      return axios
        .get(queryURL)
        .then((response) => {
          context.commit('setCurrentDrops', response.data);
        })
        .catch(() => {});
    },
    fetchGameNews(context) {
      const queryURL = '/api/League/LeagueGameNews?leagueID=' + context.getters.leagueYear.leagueID + '&year=' + context.getters.leagueYear.year;
      return axios
        .get(queryURL)
        .then((response) => {
          context.commit('setGameNews', response.data);
        })
        .catch(() => {});
    },
    fetchLeagueActions(context) {
      const queryURL = '/api/League/GetLeagueActions?leagueID=' + context.getters.leagueYear.leagueID + '&year=' + context.getters.leagueYear.year;
      return axios
        .get(queryURL)
        .then((response) => {
          context.commit('setLeagueActions', response.data);
        })
        .catch((returnedError) => (this.error = returnedError));
    },
    fetchLeagueActionSets(context) {
      const queryURL = '/api/League/GetLeagueActionSets?leagueID=' + context.getters.leagueYear.leagueID + '&year=' + context.getters.leagueYear.year;
      return axios
        .get(queryURL)
        .then((response) => {
          context.commit('setLeagueActionSets', response.data);
        })
        .catch((returnedError) => (this.error = returnedError));
    },
    fetchHistoricalTrades(context) {
      const queryURL = '/api/League/TradeHistory?leagueID=' + context.getters.leagueYear.leagueID + '&year=' + context.getters.leagueYear.year;
      return axios
        .get(queryURL)
        .then((response) => {
          context.commit('setHistoricalTrades', response.data);
        })
        .catch((returnedError) => (this.error = returnedError));
    }
  },
  mutations: {
    clearLeagueSpecificData(state) {
      state.errorInfoInternal = null;
      state.forbiddenInternal = null;
      state.inviteCodeInternal = null;
      state.leagueYearInternal = null;
      state.userPublisherInternal = null;
      state.currentBidsInternal = null;
      state.currentDropsInternal = null;
      state.gameNewsInternal = null;

      state.leagueActionsInternal = null;
      state.leagueActionSetsInternal = null;
      state.historicalTradesInternal = null;
    },
    setErrorInfo(state, errorInfo) {
      state.errorInfoInternal = errorInfo;
    },
    setForbidden(state, forbidden) {
      state.forbiddenInternal = forbidden;
    },
    setInviteCode(state, inviteCode) {
      state.inviteCodeInternal = inviteCode;
    },
    setLeagueYear(state, leagueYearPayload) {
      state.leagueYearInternal = leagueYearPayload.leagueYear;
      document.title = leagueYearPayload.leagueYear.league.leagueName + ' - Fantasy Critic';
      let matchingPublishers = _.filter(leagueYearPayload.leagueYear.publishers, (x) => x.userID === leagueYearPayload.userID);
      state.userPublisherInternal = matchingPublishers[0];
    },
    setCurrentBids(state, currentBids) {
      state.currentBidsInternal = currentBids;
    },
    setCurrentDrops(state, currentDrops) {
      state.currentDropsInternal = currentDrops;
    },
    setGameNews(state, gameNews) {
      state.gameNewsInternal = gameNews;
    },
    setLeagueActions(state, leagueActions) {
      state.leagueActionsInternal = leagueActions;
    },
    setLeagueActionSets(state, leagueActionSets) {
      state.leagueActionSetsInternal = leagueActionSets;
    },
    setHistoricalTrades(state, historicalTrades) {
      state.historicalTradesInternal = historicalTrades;
    },
    setAdvancedProjections(state, advancedProjections) {
      state.advancedProjectionsInternal = advancedProjections;
    },
    setDraftOrderView(state, draftOrderView) {
      state.draftOrderViewInternal = draftOrderView;
    }
  }
};
