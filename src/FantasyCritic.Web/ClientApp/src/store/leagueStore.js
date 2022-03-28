import axios from 'axios';

const leagueErrorMessageText = 'Something went wrong with this league. Contact us on Twitter or Discord for support.';

export default {
  state: {
    errorInfo: null,
    forbidden: false,
    inviteCode: false,
    leagueYear: null,
    userPublisher: null,
    leagueActions: null,
    leagueActionSets: null,
    historicalTrades: null,
    advancedProjections: false,
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
        if (context.getters.userInfo) {
          leagueYearPayload.userID = context.getters.userInfo.userID;
        }

        context.commit('setLeagueYear', leagueYearPayload);
      } catch (err) {
        context.commit('setErrorInfo', leagueErrorMessageText);
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
    clearLeagueStoreData(state) {
      state.errorInfo = null;
      state.forbidden = null;
      state.inviteCode = null;
      state.leagueYear = null;
      state.userPublisher = null;
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
    setAdvancedProjections(state, advancedProjections) {
      state.advancedProjections = advancedProjections;
    },
    setDraftOrderView(state, draftOrderView) {
      state.draftOrderView = draftOrderView;
    }
  }
};
