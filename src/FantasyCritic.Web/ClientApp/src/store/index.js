import { createStore } from 'vuex';
import authStore from './authStore.js';
import leagueStore from './leagueStore.js';
import interLeagueStore from './interLeagueStore.js';
import publisherStore from './publisherStore.js';

export default createStore({
  modules: {
    auth: authStore,
    league: leagueStore,
    interLeague: interLeagueStore,
    publisher: publisherStore
  }
});
