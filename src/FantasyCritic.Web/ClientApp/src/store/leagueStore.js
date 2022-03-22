import axios from 'axios';

const leagueErrorMessageText = 'Something went wrong with this league. Contact us on Twitter or Discord for support.';

export default {
  state: {
    errorInfoInternal: null,
    forbiddenInternal: false,
    inviteCodeInternal: false,
    leagueInternal: null,
    leagueYearInternal: null,
    currentBidsInternal: null,
    currentDropsInternal: null,
    gameNewsInternal: null,
    advancedProjectionsInternal: false,
    draftOrderViewInternal: false
  },
  getters: {
    errorInfo: (state) => state.errorInfoInternal,
    forbidden: (state) => state.forbiddenInternal,
    inviteCode: (state) => state.inviteCodeInternal,
    league: (state) => state.leagueInternal,
    leagueYear: (state) => state.leagueYearInternal,
    currentBids: (state) => state.currentBidsInternal,
    currentDrops: (state) => state.currentDropsInternal,
    gameNews: (state) => state.gameNewsInternal,
    advancedProjections: (state) => state.advancedProjectionsInternal,
    draftOrderView: (state) => state.draftOrderViewInternal
  },
  actions: {
    initializePage(context, leaguePageParams) {
      context.commit('cancelMoveMode');
      context.commit('clearLeagueSpecificData');
      context.commit('setInviteCode', leaguePageParams.inviteCode);
      return context.dispatch('fetchLeagueData', leaguePageParams);
    },
    refreshLeagueData(context) {
      const leaguePageParams = {
        leagueID: context.getters.leagueYear.leagueID,
        year: context.getters.leagueYear.year,
        inviteCode: context.getters.inviteCode
      };
      let leaguePromise = context.dispatch('fetchLeague', leaguePageParams);
      let leagueYearPromise = context.dispatch('fetchLeagueYear', leaguePageParams);
      return Promise.all([leaguePromise, leagueYearPromise]).then(() => {
        // All done
      });
    },
    fetchLeagueData(context, leaguePageParams) {
      let leaguePromise = context.dispatch('fetchLeague', leaguePageParams);
      let leagueYearPromise = context.dispatch('fetchLeagueYear', leaguePageParams);
      return Promise.all([leaguePromise, leagueYearPromise]).then(() => {
        // All done
      });
    },
    fetchLeague(context, leaguePageParams) {
      let queryURL = '/api/League/GetLeague/' + leaguePageParams.leagueID;
      if (leaguePageParams.inviteCode) {
        queryURL += '?inviteCode=' + leaguePageParams.inviteCode;
      }
      return axios
        .get(queryURL)
        .then((response) => {
          context.commit('setLeague', response.data);
        })
        .catch((returnedError) => {
          context.commit('setErrorInfo', leagueErrorMessageText);
          const forbidden = returnedError && returnedError.response && returnedError.response.status === 403;
          context.commit('setForbidden', forbidden);
        });
    },
    fetchLeagueYear(context, leaguePageParams) {
      let queryURL = '/api/League/GetLeagueYear?leagueID=' + leaguePageParams.leagueID + '&year=' + leaguePageParams.year;
      if (leaguePageParams.inviteCode) {
        queryURL += '&inviteCode=' + leaguePageParams.inviteCode;
      }
      return axios
        .get(queryURL)
        .then((response) => {
          context.commit('setLeagueYear', response.data);
          if (context.getters.leagueYear.userPublisher) {
            context.dispatch('fetchAdditionalLeagueData');
          }
          context.dispatch('fetchGameNews');
        })
        .catch(() => {
          context.commit('setErrorInfo', leagueErrorMessageText);
        });
    },
    fetchAdditionalLeagueData(context) {
      let currentBidsPromise = context.dispatch('fetchCurrentBids');
      let currentDropRequestsPromise = context.dispatch('fetchCurrentDropRequests');
      return Promise.all([currentBidsPromise, currentDropRequestsPromise]).then(() => {
        // All done
      });
    },
    fetchCurrentBids(context) {
      const queryURL = '/api/league/CurrentBids/' + context.getters.leagueYear.userPublisher.publisherID;
      return axios
        .get(queryURL)
        .then((response) => {
          context.commit('setCurrentBids', response.data);
        })
        .catch(() => {});
    },
    fetchCurrentDropRequests(context) {
      const queryURL = '/api/league/CurrentDropRequests/' + context.getters.leagueYear.userPublisher.publisherID;
      return axios
        .get(queryURL)
        .then((response) => {
          this.currentDrops = response.data;
          context.commit('setCurrentDrops', response.data);
        })
        .catch(() => {});
    },
    fetchGameNews(context) {
      const queryURL = '/api/League/LeagueGameNews?leagueID=' + context.getters.leagueYear.leagueID + '&year=' + context.getters.leagueYear.year;
      return axios
        .get(queryURL)
        .then((response) => {
          this.gameNews = response.data;
          context.commit('setGameNews', response.data);
        })
        .catch(() => {});
    }
  },
  mutations: {
    clearLeagueSpecificData(state) {
      state.leagueInternal = null;
      state.leagueYearInternal = null;
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
    setLeague(state, league) {
      state.leagueInternal = league;
      document.title = league.leagueName + ' - Fantasy Critic';
    },
    setLeagueYear(state, leagueYear) {
      state.leagueYearInternal = leagueYear;
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
    setAdvancedProjections(state, advancedProjections) {
      state.advancedProjectionsInternal = advancedProjections;
    },
    setDraftOrderView(state, draftOrderView) {
      state.draftOrderViewInternal = draftOrderView;
    }
  }
};
