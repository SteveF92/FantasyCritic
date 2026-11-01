import { defineStore } from 'pinia';
import { useAuthStore } from './authStore';
import { usePublisherStore } from './publisherStore';

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
      const publisherStore = usePublisherStore();
      publisherStore.clearPublisherStoreData();
      this.clearLeagueStoreData();
      this.inviteCode = leaguePageParams.inviteCode;
      await this.fetchLeagueYear(leaguePageParams);
    },
    async initializeHistoryPage(leaguePageParams) {
      const publisherStore = usePublisherStore();
      publisherStore.clearPublisherStoreData();
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

        const authStore = useAuthStore();
        if (authStore.userInfo) {
          leagueYearPayload.userID = authStore.userInfo.userID;
        }

        this.setLeagueYear(leagueYearPayload);
      } catch (err) {
        this.setError(err.response);
      }
    },
    fetchHistoryData() {
      return Promise.all([this.fetchLeagueActions(), this.fetchLeagueActionSets(), this.fetchHistoricalTrades()]);
    },
    fetchLeagueActions() {
      const queryURL = '/api/League/GetLeagueActions?leagueID=' + this.leagueYear.leagueID + '&year=' + this.leagueYear.year;
      return axios
        .get(queryURL)
        .then((response) => {
          this.leagueActions = response.data;
        })
        .catch((err) => this.setError(err.response));
    },
    fetchLeagueActionSets() {
      const queryURL = '/api/League/GetLeagueActionSets?leagueID=' + this.leagueYear.leagueID + '&year=' + this.leagueYear.year;
      return axios
        .get(queryURL)
        .then((response) => {
          this.leagueActionSets = response.data;
        })
        .catch((err) => this.setError(err.response));
    },
    fetchHistoricalTrades() {
      const queryURL = '/api/League/TradeHistory?leagueID=' + this.leagueYear.leagueID + '&year=' + this.leagueYear.year;
      return axios
        .get(queryURL)
        .then((response) => {
          this.historicalTrades = response.data;
        })
        .catch((err) => this.setError(err.response));
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
