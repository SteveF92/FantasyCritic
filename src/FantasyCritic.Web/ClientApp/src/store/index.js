import Vue from 'vue';
import Vuex from 'vuex';
import auth from './auth';
import league from './leagueStore';
import interLeague from './interLeagueStore';
import publisher from './publisherStore';

Vue.use(Vuex);

export default new Vuex.Store({
  modules: {
    auth,
    league,
    interLeague,
    publisher
  }
});
