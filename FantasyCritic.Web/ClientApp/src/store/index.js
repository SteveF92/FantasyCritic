import Vue from 'vue';
import Vuex from 'vuex';
import auth from './auth';
import league from './leagueStore';
import masterGame from './masterGameStore';

Vue.use(Vuex);

export default new Vuex.Store({
  modules: {
    auth,
    league,
    masterGame
  }
});
