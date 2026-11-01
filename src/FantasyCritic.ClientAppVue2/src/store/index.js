import Vue from 'vue';
import Vuex from 'vuex';
import admin from './adminStore';
import auth from './auth';
import league from './leagueStore';
import conference from './conferenceStore';
import interLeague from './interLeagueStore';
import publisher from './publisherStore';

Vue.use(Vuex);

const vuexStore = new Vuex.Store({
  modules: {
    admin,
    auth,
    league,
    conference,
    interLeague,
    publisher
  }
});

export { vuexStore };
