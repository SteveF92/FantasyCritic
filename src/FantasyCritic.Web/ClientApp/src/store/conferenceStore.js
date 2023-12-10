import axios from 'axios';

export default {
  state: {
    hasError: false,
    conferenceYear: null
  },
  getters: {
    hasError: (state) => state.hasError,
    conferenceYear: (state) => state.conferenceYear,
    conference: (state) => state.conferenceYear.conference
  },
  actions: {
    async initializeConferencePage(context, conferencePageParams) {
      context.commit('clearConferenceStoreData');
      await context.dispatch('fetchConferenceYear', conferencePageParams);
    },
    async fetchConferenceYear(context, conferencePageParams) {
      let queryURL = '/api/Conference/GetConferenceYear?conferenceid=' + conferencePageParams.conferenceID + '&year=' + conferencePageParams.year;

      try {
        const response = await axios.get(queryURL);
        context.commit('setConferenceYear', response.data);
      } catch (err) {
        context.commit('setConferenceError');
      }
    }
  },
  mutations: {
    clearConferenceStoreData(state) {
      state.hasError = false;
      state.conferenceYear = null;
    },
    setConferenceError(state) {
      state.hasError = true;
    },
    setConferenceYear(state, conferenceYear) {
      state.conferenceYear = conferenceYear;
      document.title = conferenceYear.conference.conferenceName + ' - Fantasy Critic';
    }
  }
};
