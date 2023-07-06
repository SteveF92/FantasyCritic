import { createStore } from 'vuex';
import authStore from './authStore.js';
import leagueStore from './leagueStore.js';
import interLeagueStore from './interLeagueStore.js';
import publisherStore from './publisherStore.js';
import modalStore from './modalStore.js';

export default createStore({
  modules: {
    auth: authStore,
    league: leagueStore,
    interLeague: interLeagueStore,
    publisher: publisherStore,
    modal: modalStore
  }
});
