import axios from 'axios';

export default {
  state: {
    errorInfoInternal: null,
    forbiddenInternal: false,
    inviteCodeInternal: false,
    leagueInternal: null,
    leagueYearInternal: null,
    advancedProjectionsInternal: false,
    draftOrderViewInternal: false
  },
  getters: {
    errorInfo: (state) => state.errorInfoInternal,
    forbidden: (state) => state.forbiddenInternal,
    inviteCode: (state) => state.inviteCodeInternal,
    league: (state) => state.leagueInternal,
    leagueYear: (state) => state.leagueYearInternal,
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
    fetchLeagueData(context, leaguePageParams) {
      let leaguePromise = context.dispatch('fetchLeague', leaguePageParams);
      let leagueYearPromise = context.dispatch('fetchLeagueYear', leaguePageParams);
      return Promise.all([leaguePromise, leagueYearPromise]).then(() => {
        // Both done
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
          context.commit('setErrorInfo', 'Something went wrong with this league. Contact us on Twitter or Discord for support.');
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
          // this.fetchCurrentBids();
          // this.fetchCurrentDropRequests();
          // this.fetchGameNews();
        })
        .catch(() => {
          context.commit('setErrorInfo', 'Something went wrong with this league. Contact us on Twitter or Discord for support.');
        });
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
    setAdvancedProjections(state, advancedProjections) {
      state.advancedProjectionsInternal = advancedProjections;
    },
    setDraftOrderView(state, draftOrderView) {
      state.draftOrderViewInternal = draftOrderView;
    }
  }
};
