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
    advancedProjections: (state) => state.advancedProjectionsInternal,
    draftOrderView: (state) => state.draftOrderViewInternal
  },
  actions: {
    initializePage(context, leaguePageParams) {
      context.commit('cancelMoveMode');
      context.commit('clearLeagueSpecificData');
      context.commit('setInviteCode', leaguePageParams.inviteCode);
      return context.dispatch('fetchLeagueYear', leaguePageParams);
    },
    refreshLeagueYear(context) {
      const leaguePageParams = {
        leagueID: context.getters.leagueYear.leagueID,
        year: context.getters.leagueYear.year,
        inviteCode: context.getters.inviteCode
      };
      return context.dispatch('fetchLeagueYear', leaguePageParams);
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
          if (context.getters.userPublisher) {
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
      state.errorInfoInternal = null;
      state.forbiddenInternal = null;
      state.inviteCodeInternal = null;
      state.leagueYearInternal = null;
      state.userPublisherInternal = null;
      state.currentBidsInternal = null;
      state.currentDropsInternal = null;
      state.gameNewsInternal = null;
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
    setAdvancedProjections(state, advancedProjections) {
      state.advancedProjectionsInternal = advancedProjections;
    },
    setDraftOrderView(state, draftOrderView) {
      state.draftOrderViewInternal = draftOrderView;
    }
  }
};
