import { defineStore } from 'pinia';
import axios from 'axios';

export const useLeagueStore = defineStore('league', {
  state: () => ({
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
  }),
  actions: {
    async initializeLeaguePage(leaguePageParams) {
      const draftOrderView = localStorage.getItem('draftOrderView');
      const showProjections = localStorage.getItem('showProjections');
      this.setDraftOrderView(draftOrderView === 'true');
      this.setShowProjections(showProjections === 'true');
      this.clearPublisherStoreData();
      this.clearLeagueStoreData();
      this.inviteCode = leaguePageParams.inviteCode;
      await this.fetchLeagueYear(leaguePageParams);
    },
    async initializeHistoryPage(leaguePageParams) {
      this.clearPublisherStoreData();
      this.clearLeagueStoreData();
      await this.fetchLeagueYear(leaguePageParams);
      await this.fetchHistoryData();
    },
    async refreshLeagueYear() {
      const leaguePageParams = {
        leagueID: this.leagueYear.leagueID,
        year: this.leagueYear.year,
        inviteCode: this.inviteCode
      };
      await this.fetchLeagueYear(leaguePageParams);
    },
    async fetchLeagueYear(leaguePageParams) {
      let queryURL = '/api/League/GetLeagueYear?leagueID=' + leaguePageParams.leagueID + '&year=' + leaguePageParams.year;
      if (leaguePageParams.inviteCode) {
        queryURL += '&inviteCode=' + leaguePageParams.inviteCode;
      }

      try {
        const response = await axios.get(queryURL);
        const leagueYearPayload = {
          leagueYear: response.data,
          userID: undefined
        };
        if (this.getters.userInfo) {
          leagueYearPayload.userID = this.getters.userInfo.userID;
        }

        this.setLeagueYear(leagueYearPayload);
      } catch (err) {
        this.setError(err.response);
      }
    },
    fetchHistoryData(context) {
      const leagueActionsPromise = context.dispatch('fetchLeagueActions');
      const leagueActionSetsPromise = context.dispatch('fetchLeagueActionSets');
      const historicalTradesPromise = context.dispatch('fetchHistoricalTrades');
      return Promise.all([leagueActionsPromise, leagueActionSetsPromise, historicalTradesPromise]);
    },
    fetchLeagueActions(context) {
      const queryURL = '/api/League/GetLeagueActions?leagueID=' + context.this.leagueYear.leagueID + '&year=' + context.this.leagueYear.year;
      return axios
        .get(queryURL)
        .then((response) => {
          context.commit('setLeagueActions', response.data);
        })
        .catch((err) => context.commit('setError', err.response));
    },
    fetchLeagueActionSets(context) {
      const queryURL = '/api/League/GetLeagueActionSets?leagueID=' + context.this.leagueYear.leagueID + '&year=' + context.this.leagueYear.year;
      return axios
        .get(queryURL)
        .then((response) => {
          context.commit('setLeagueActionSets', response.data);
        })
        .catch((err) => context.commit('setError', err.response));
    },
    fetchHistoricalTrades(context) {
      const queryURL = '/api/League/TradeHistory?leagueID=' + context.this.leagueYear.leagueID + '&year=' + context.this.leagueYear.year;
      return axios
        .get(queryURL)
        .then((response) => {
          context.commit('setHistoricalTrades', response.data);
        })
        .catch((err) => context.commit('setError', err.response));
    },
    clearLeagueStoreData() {
      this.hasError = false;
      this.forbidden = null;
      this.inviteCode = null;
      this.leagueYear = null;
      this.userPublisher = null;
      this.gameNews = null;

      this.leagueActions = null;
      this.leagueActionSets = null;
      this.historicalTrades = null;
    },
    setError(response) {
      if (response.status === 401 || response.status === 403) {
        this.forbidden = true;
      } else {
        this.hasError = true;
      }
    },
    setLeagueYear(leagueYearPayload) {
      this.leagueYear = leagueYearPayload.leagueYear;
      document.title = leagueYearPayload.leagueYear.league.leagueName + ' - Fantasy Critic';

      if (leagueYearPayload.userID) {
        this.userPublisher = leagueYearPayload.leagueYear.publishers.find((x) => x.userID === leagueYearPayload.userID);
      }
    },
    setShowProjections(showProjections) {
      this.showProjections = showProjections;
      localStorage.setItem('showProjections', showProjections.toString());
    },
    setDraftOrderView(draftOrderView) {
      this.draftOrderView = draftOrderView;
      localStorage.setItem('draftOrderView', draftOrderView.toString());
    }
  }
});
