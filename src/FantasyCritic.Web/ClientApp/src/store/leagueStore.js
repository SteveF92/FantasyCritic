import axios from 'axios';

const leagueErrorMessageText = 'Something went wrong with this league. Contact us on Twitter or Discord for support.';

export default {
  state: {
    errorInfo: null,
    forbidden: false,
    inviteCode: false,
    leagueYear: null,
    userPublisher: null,
    currentBids: null,
    currentDrops: null,
    gameNews: null,
    leagueActions: null,
    leagueActionSets: null,
    historicalTrades: null,
    advancedProjections: false,
    draftOrderView: false
  },
  actions: {
    async initializePage(context, leaguePageParams) {
      context.commit('cancelMoveMode');
      context.commit('clearLeagueSpecificData');
      context.commit('setInviteCode', leaguePageParams.inviteCode);
      await context.dispatch('fetchLeagueYear', leaguePageParams);
      if (context.state.userPublisher) {
        await context.dispatch('fetchAdditionalLeagueData');
      }
      await context.dispatch('fetchGameNews');
    },
    async initializeHistoryPage(context, leaguePageParams) {
      context.commit('cancelMoveMode');
      context.commit('clearLeagueSpecificData');
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
      if (context.state.userPublisher) {
        await context.dispatch('fetchAdditionalLeagueData');
      }
      await context.dispatch('fetchGameNews');
    },
    async fetchLeagueYear(context, leaguePageParams) {
      let queryURL = '/api/League/GetLeagueYear?leagueID=' + leaguePageParams.leagueID + '&year=' + leaguePageParams.year;
      if (leaguePageParams.inviteCode) {
        queryURL += '&inviteCode=' + leaguePageParams.inviteCode;
      }

      try {
        const response = await axios.get(queryURL);
        const leagueYearPayload = {
          leagueYear: response.data,
          userID: context.getters.userInfo.userID
        };
        context.commit('setLeagueYear', leagueYearPayload);
      } catch (err) {
        context.commit('setErrorInfo', leagueErrorMessageText);
      }
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
      const queryURL = '/api/league/CurrentBids/' + context.state.userPublisher.publisherID;
      return axios
        .get(queryURL)
        .then((response) => {
          context.commit('setCurrentBids', response.data);
        })
        .catch(() => {});
    },
    fetchCurrentDropRequests(context) {
      const queryURL = '/api/league/CurrentDropRequests/' + context.state.userPublisher.publisherID;
      return axios
        .get(queryURL)
        .then((response) => {
          context.commit('setCurrentDrops', response.data);
        })
        .catch(() => {});
    },
    fetchGameNews(context) {
      const queryURL = '/api/League/LeagueGameNews?leagueID=' + context.state.leagueYear.leagueID + '&year=' + context.state.leagueYear.year;
      return axios
        .get(queryURL)
        .then((response) => {
          context.commit('setGameNews', response.data);
        })
        .catch(() => {});
    },
    fetchLeagueActions(context) {
      const queryURL = '/api/League/GetLeagueActions?leagueID=' + context.state.leagueYear.leagueID + '&year=' + context.state.leagueYear.year;
      return axios
        .get(queryURL)
        .then((response) => {
          context.commit('setLeagueActions', response.data);
        })
        .catch(() => context.commit('setErrorInfo', leagueErrorMessageText));
    },
    fetchLeagueActionSets(context) {
      const queryURL = '/api/League/GetLeagueActionSets?leagueID=' + context.state.leagueYear.leagueID + '&year=' + context.state.leagueYear.year;
      return axios
        .get(queryURL)
        .then((response) => {
          context.commit('setLeagueActionSets', response.data);
        })
        .catch(() => context.commit('setErrorInfo', leagueErrorMessageText));
    },
    fetchHistoricalTrades(context) {
      const queryURL = '/api/League/TradeHistory?leagueID=' + context.state.leagueYear.leagueID + '&year=' + context.state.leagueYear.year;
      return axios
        .get(queryURL)
        .then((response) => {
          context.commit('setHistoricalTrades', response.data);
        })
        .catch(() => context.commit('setErrorInfo', leagueErrorMessageText));
    }
  },
  mutations: {
    clearLeagueSpecificData(state) {
      state.errorInfo = null;
      state.forbidden = null;
      state.inviteCode = null;
      state.leagueYear = null;
      state.userPublisher = null;
      state.currentBids = null;
      state.currentDrops = null;
      state.gameNews = null;

      state.leagueActions = null;
      state.leagueActionSets = null;
      state.historicalTrades = null;
    },
    setErrorInfo(state, errorInfo) {
      state.errorInfo = errorInfo;
    },
    setForbidden(state, forbidden) {
      state.forbidden = forbidden;
    },
    setInviteCode(state, inviteCode) {
      state.inviteCode = inviteCode;
    },
    setLeagueYear(state, leagueYearPayload) {
      state.leagueYear = leagueYearPayload.leagueYear;
      document.title = leagueYearPayload.leagueYear.league.leagueName + ' - Fantasy Critic';
      let matchingPublishers = _.filter(leagueYearPayload.leagueYear.publishers, (x) => x.userID === leagueYearPayload.userID);
      state.userPublisher = matchingPublishers[0];
    },
    setCurrentBids(state, currentBids) {
      state.currentBids = currentBids;
    },
    setCurrentDrops(state, currentDrops) {
      state.currentDrops = currentDrops;
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
    setAdvancedProjections(state, advancedProjections) {
      state.advancedProjections = advancedProjections;
    },
    setDraftOrderView(state, draftOrderView) {
      state.draftOrderView = draftOrderView;
    }
  }
};
