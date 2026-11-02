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
      await fetchHistoryData(this);
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

        setLeagueYear(this, leagueYearPayload);
      } catch (err) {
        setError(this, err.response);
      }
    },
    clearLeagueStoreData() {
      this.hasError = false;
      this.forbidden = null;
      this.inviteCode = null;
      this.leagueYear = null;
      this.userPublisher = null;

      this.leagueActions = null;
      this.leagueActionSets = null;
      this.historicalTrades = null;
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

function fetchHistoryData(store) {
  return Promise.all([fetchLeagueActions(store), fetchLeagueActionSets(store), fetchHistoricalTrades(store)]);
}

function fetchLeagueActions(store) {
  const queryURL = '/api/League/GetLeagueActions?leagueID=' + store.leagueYear.leagueID + '&year=' + store.leagueYear.year;
  return axios
    .get(queryURL)
    .then((response) => {
      store.leagueActions = response.data;
    })
    .catch((err) => setError(store, err.response));
}

function fetchLeagueActionSets(store) {
  const queryURL = '/api/League/GetLeagueActionSets?leagueID=' + store.leagueYear.leagueID + '&year=' + store.leagueYear.year;
  return axios
    .get(queryURL)
    .then((response) => {
      store.leagueActionSets = response.data;
    })
    .catch((err) => setError(store, err.response));
}

function fetchHistoricalTrades(store) {
  const queryURL = '/api/League/TradeHistory?leagueID=' + store.leagueYear.leagueID + '&year=' + store.leagueYear.year;
  return axios
    .get(queryURL)
    .then((response) => {
      store.historicalTrades = response.data;
    })
    .catch((err) => setError(store, err.response));
}

function setError(store, response) {
  if (response.status === 401 || response.status === 403) {
    store.forbidden = true;
  } else {
    store.hasError = true;
  }
}

function setLeagueYear(store, leagueYearPayload) {
  store.leagueYear = leagueYearPayload.leagueYear;
  document.title = leagueYearPayload.leagueYear.league.leagueName + ' - Fantasy Critic';

  if (leagueYearPayload.userID) {
    store.userPublisher = leagueYearPayload.leagueYear.publishers.find((x) => x.userID === leagueYearPayload.userID);
  }
}
